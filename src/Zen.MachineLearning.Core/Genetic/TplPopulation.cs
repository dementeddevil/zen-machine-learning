namespace Zen.MachineLearning.Core.Genetic
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class TplPopulation : Population
    {
        #region Private Types
        private class TplCrossoverCandidate
        {
            public Entity Mother
            {
                get;
                set;
            }

            public Entity Father
            {
                get;
                set;
            }
        }

        private class TplMutateCandidate
        {
            public Entity Parent
            {
                get;
                set;
            }
        }
        #endregion

        #region Private Fields
        private ParallelOptions _parallelOptions;

        private readonly List<TplCrossoverCandidate> _crossoverCandidates =
            new List<TplCrossoverCandidate>();
        private bool _inCrossover;

        private readonly List<TplMutateCandidate> _mutateCandidates =
            new List<TplMutateCandidate>();
        private bool _inMutate;

        private CancellationToken _cancellationToken = CancellationToken.None;
        #endregion

        #region Public Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="TplPopulation"/> class.
        /// </summary>
        public TplPopulation(PopulationHost host)
            : base(host)
        {
        }
        #endregion

        #region Public Properties
        public override bool IsCancellationRequested => _cancellationToken.IsCancellationRequested;

        #endregion

        #region Public Methods
        /// <summary>
        /// Initialises the <see cref="T:Population"/> with the specified
        /// <see cref="T:PopulationSettings"/> information.
        /// </summary>
        /// <param name="settings"></param>
        /// <remarks>
        /// This method can be called during the execution of the GA if a
        /// restart request is issued by calling the
        /// <see cref="M:RestartEvolution"/> method. In this case the settings
        /// parameter will refer to the existing settings object.
        /// </remarks>
        public override void InitializePopulation(PopulationSettings settings)
        {
            base.InitializePopulation(settings);

            _parallelOptions =
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = 4,
                    CancellationToken = _cancellationToken
                };

            // TODO: Query for our derived settings class
            // For now just reuse the CcrPopulationSettings type
            var tplSettings = settings as TplPopulationSettings;
            if (tplSettings != null)
            {
                _parallelOptions.MaxDegreeOfParallelism = tplSettings.ThreadCount;
            }
        }

        /// <summary>
        /// Creates a Task that will complete when the GA evolution has finished
        /// </summary>
        /// <returns></returns>
        public Task EvolveAsync()
        {
            return Task.Factory.StartNew(
                () => Evolve(),
                TaskCreationOptions.LongRunning | TaskCreationOptions.AttachedToParent);
        }

        /// <summary>
        /// Creates a Task that will complete when the GA evolution has finished
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public Task EvolveAsync(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            return EvolveAsync();
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Throws if cancellation has been requested.
        /// </summary>
        /// <remarks>
        /// This method delegates to the associated cancellation token (if any)
        /// passed to the EvolveAsync method.
        /// </remarks>
        protected override void ThrowIfCancellationRequested()
        {
            _cancellationToken.ThrowIfCancellationRequested();
            base.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Ensures the fitness of every entity has been evaluated
        /// </summary>
        protected override void EnsureFitness()
        {
            var result = Parallel.ForEach<Entity>(
                from entity in Entities
                where entity.State != EntityStates.Ready
                select entity,
                _parallelOptions,
                (item) =>
                {
                    item.EnsureFitness();
                });
            Debug.Assert(result.IsCompleted);
        }

        /// <summary>
        /// Called by the framework to perform the actual crossover of every
        /// entity in this population instance.
        /// </summary>
        /// <remarks>
        /// This method uses the crossover ratio for probabilistic evaluation
        /// of whether this method will actually perform crossover.
        /// </remarks>
        protected override void OnCrossover()
        {
            if (_parallelOptions.MaxDegreeOfParallelism == 1)
            {
                // Parallel crossover logic is switched off...
                base.OnCrossover();
            }
            else
            {
                _inCrossover = true;
                try
                {
                    // Pass to base class
                    base.OnCrossover();

                    // Process the crossover elements in parallel
                    var newEntityQueue =
                        new ConcurrentQueue<Entity>();
                    var result = Parallel.ForEach(
                        _crossoverCandidates,
                        _parallelOptions,
                        (candidate) =>
                        {
                            Entity daughter, son;
                            base.OnCrossover(candidate.Mother, candidate.Father, out daughter, out son);
                            if (daughter != null)
                            {
                                newEntityQueue.Enqueue(daughter);
                            }
                            if (son != null)
                            {
                                newEntityQueue.Enqueue(son);
                            }
                        });
                    Debug.Assert(result.IsCompleted);

                    // Flush the candidate list now we have processed entities
                    _crossoverCandidates.Clear();

                    // Empty new entity queue into main entity array
                    Entity item;
                    while (newEntityQueue.TryDequeue(out item))
                    {
                        Entities.Add(item);
                    }
                }
                finally
                {
                    _inCrossover = false;
                }
            }
        }

        /// <summary>
        /// Performs crossover on the specified mother and father entities,
        /// producing the offspring daughter and son entities as a result.
        /// </summary>
        /// <param name="mother">The mother.</param>
        /// <param name="father">The father.</param>
        /// <param name="daughter">The daughter.</param>
        /// <param name="son">The son.</param>
        /// <remarks>
        /// This method allocates both daughter and son entities before
        /// delegating the actual crossover work to the crossover handler
        /// specified in the settings object.
        /// </remarks>
        protected override void OnCrossover(Entity mother, Entity father, out Entity daughter, out Entity son)
        {
            if (!_inCrossover)
            {
                base.OnCrossover(mother, father, out daughter, out son);
            }
            else
            {
                // Do not do the crossover now - just store the candidates
                _crossoverCandidates.Add(
                    new TplCrossoverCandidate
                    {
                        Mother = mother,
                        Father = father
                    });
                daughter = null;
                son = null;
            }
        }

        /// <summary>
        /// Called when [mutation].
        /// </summary>
        protected override void OnMutation()
        {
            if (_parallelOptions.MaxDegreeOfParallelism == 1)
            {
                base.OnMutation();
            }
            else
            {
                _inMutate = true;
                try
                {
                    // Pass to base class
                    base.OnMutation();

                    // Process the mutation elements in parallel
                    var newEntityQueue =
                        new ConcurrentQueue<Entity>();
                    var result = Parallel.ForEach(
                        _mutateCandidates,
                        _parallelOptions,
                        (candidate) =>
                        {
                            Entity child;
                            base.OnMutate(candidate.Parent, out child);
                            if (child != null)
                            {
                                newEntityQueue.Enqueue(child);
                            }
                        });
                    Debug.Assert(result.IsCompleted);

                    // Flush the candidate list now we have processed entities
                    _mutateCandidates.Clear();

                    // Empty new entity queue into main entity array
                    Entity item;
                    while (newEntityQueue.TryDequeue(out item))
                    {
                        Entities.Add(item);
                    }
                }
                finally
                {
                    _inMutate = false;
                }
            }
        }

        /// <summary>
        /// Called when [mutate].
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="child">The child.</param>
        protected override void OnMutate(Entity parent, out Entity child)
        {
            if (!_inMutate)
            {
                base.OnMutate(parent, out child);
            }
            else
            {
                _mutateCandidates.Add(
                    new TplMutateCandidate
                    {
                        Parent = parent
                    });
                child = null;
            }
        }
        #endregion
    }

    public class TplPopulationSettings : PopulationSettings
    {
        public int ThreadCount
        {
            get;
            set;
        }
    }
}
