namespace Zen.MachineLearning.Core.Genetic
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Threading;
    using Zen.MachineLearning.Core;

    #region Generation Event Arguments and Delegates
    [Serializable]
    public class GenerationEventArgs : CancelEventArgs
    {
        #region Private Members
        private readonly int _generation;
        #endregion

        #region Public Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GenerationEventArgs"/> class.
        /// </summary>
        /// <param name="generation">The generation.</param>
        public GenerationEventArgs(int generation)
        {
            _generation = generation;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets the generation.
        /// </summary>
        /// <value>The generation.</value>
        public int Generation => _generation;

        #endregion
    }
    public delegate bool GenerationHandler(object sender, GenerationEventArgs e);
    #endregion

    #region Entity Event Arguments and Delegates
    [Serializable]
    public class EntityEventArgs : EventArgs
    {
        #region Private Members
        private readonly Entity _entity;
        #endregion

        #region Public Constructors
        public EntityEventArgs(Entity entity)
        {
            _entity = entity;
        }
        #endregion

        #region Public Properties
        public Entity Entity => _entity;

        #endregion
    }
    #endregion

    /// <summary>
    /// Summary description for Population.
    /// </summary>
    public class Population : MarshalByRefObject, ICloneable, IDisposable
    {
        #region Private Fields
        private readonly ConcurrentQueue<MigratingEntity> _inboundMigration =
            new ConcurrentQueue<MigratingEntity>();
        private readonly ConcurrentQueue<Entity> _freeEntityPool =
            new ConcurrentQueue<Entity>();
        private readonly int _maxFreeEntityPoolSize = 5000;
        private bool _disposed = false;
        private PopulationSettings _settings;
        private int _running;                           // Lock count
        private EntityArray _entities;                  // population sorted on fitness
        private int _generation;                        // current generation index
        private int _restartPending;
        #endregion

        #region Public Events
        public event EventHandler<GenerationEventArgs> GenerationEvent;
        #endregion

        #region Public Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Population"/> class.
        /// </summary>
        public Population(PopulationHost host)
        {
            IslandId = Guid.NewGuid();
            Host = host;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets the host.
        /// </summary>
        /// <value>The host.</value>
        public PopulationHost Host
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <value>The settings.</value>
        public PopulationSettings Settings => _settings;

        /// <summary>
        /// Gets a value indicating whether this instance is running.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is running; otherwise, <c>false</c>.
        /// </value>
        public bool IsRunning => _running > 0;

        /// <summary>
        /// Gets or sets the island id.
        /// </summary>
        /// <value>The island id.</value>
        public Guid IslandId
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the entities.
        /// </summary>
        /// <value>The entities.</value>
        public IList<Entity> Entities => _entities;

        /// <summary>
        /// Gets the generation.
        /// </summary>
        /// <value>The generation.</value>
        public int Generation => _generation;

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _entities.Count;

        /// <summary>
        /// Gets or sets the original count.
        /// </summary>
        /// <value>The original count.</value>
        public int OriginalCount
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether cancellation has been requested.
        /// </summary>
        /// <value>
        /// <c>true</c> if cancellation requested; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// By default this property returns false. Derived classes should 
        /// override in order to implement cancellation semantics.
        /// </remarks>
        public virtual bool IsCancellationRequested => false;

        #endregion

        #region Protected Properties
        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
        /// </value>
        protected bool IsDisposed => _disposed;

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
        public virtual void InitializePopulation(PopulationSettings settings)
        {
            // If settings are different then clone
            if (_settings != settings)
            {
                _settings = settings.Clone();
            }

            // Reset the count
            OriginalCount = 0;

            // Discard any current entities and recreate array
            if (_entities != null)
            {
                _entities.Dispose();
            }
            _entities = new EntityArray();

            // Seed population members
            Seed();
        }

        /// <summary>
        /// Restarts the evolution.
        /// </summary>
        public virtual void RestartEvolution()
        {
            Interlocked.Increment(ref _restartPending);
        }

        /// <summary>
        /// Adds the given entity to the inbound migration queue.
        /// </summary>
        /// <param name="entity"></param>
        public virtual bool AddMigratingEntity(MigratingEntity entity)
        {
            if (entity.IslandId != IslandId)
            {
                _inboundMigration.Enqueue(entity);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Main genetic algorithm routine. Performs GA-based optimisation
        /// on the population of entities.
        /// This is a generation-based GA.
        /// </summary>
        public void Evolve()
        {
            // Clear any restart requests and update run status.
            Interlocked.Exchange(ref _restartPending, 0);
            Interlocked.Increment(ref _running);
            try
            {
                _generation = 0;
                while (OnGeneration(new GenerationEventArgs(_generation)) &&
                    (Settings.SteadyState || _generation < Settings.MaxGenerations))
                {
                    ThrowIfCancellationRequested();

                    // Setup generation parameters and initial population size
                    ResetSelectState();
                    _generation++;
                    OriginalCount = Count;

                    ThrowIfCancellationRequested();
                    OnEnterGeneration();

                    ThrowIfCancellationRequested();
                    OnCrossoverMutate();

                    ThrowIfCancellationRequested();
                    OnAdaptAndEvaluate();

                    ThrowIfCancellationRequested();
                    OnSurvival();

                    // Steady state update
                    /*if (steadyState)
					{
						if (son != null)
						{
							OnReplaceEntity (son);
						}
						if (daughter != null)
						{
							OnReplaceEntity (daughter);
						}
						if (child != null)
						{
							OnReplaceEntity (child);
						}
					}*/

                    ThrowIfCancellationRequested();
                    OnMigrate();

                    ThrowIfCancellationRequested();
                    OnLeaveGeneration();

                    // Check for restart request
                    if (_restartPending > 0)
                    {
                        InitializePopulation(_settings);
                        OnRestartEvolve();
                        _generation = 0;
                        Interlocked.Exchange(ref _restartPending, 0);
                    }
                }
            }
            catch (Exception error)
            {
                Debug.WriteLine($"Exception caught in Population.Evolve\n\t{error.Message}\n\t{error.StackTrace}");
                throw error;
            }
            finally
            {
                Interlocked.Decrement(ref _running);
            }
        }

        /// <summary>
        /// Creates a clone of the population.
        /// </summary>
        /// <returns></returns>
        public virtual Population Clone()
        {
            CheckDisposed();

            // Allow derived classes to override the population type
            var pop = (Population)MemberwiseClone();

            // Clone settings and entities
            pop._settings = _settings.Clone();
            pop._entities = new EntityArray();
            foreach (var entity in _entities)
            {
                var newEntity = pop.CreateEntity(false);
                newEntity.CopyFrom(entity);
                pop._entities.Add(newEntity);
            }

            return pop;
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Throws if cancellation has been requested.
        /// </summary>
        /// <remarks>
        /// By default this method will throw an OperationCanceledException
        /// if the IsCancellationRequested property returns true.
        /// </remarks>
        protected virtual void ThrowIfCancellationRequested()
        {
            if (IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }
        }

        /// <summary>
        /// Called when the currently executing evolution has been restarted.
        /// </summary>
        /// <remarks>
        /// When this method is called the entity array will have been 
        /// regenerated and reseeded with random data.
        /// </remarks>
        protected virtual void OnRestartEvolve()
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_entities != null)
                    {
                        _entities.Dispose();
                        _entities = null;
                    }
                }
                _disposed = true;
            }
        }

        protected void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        /// <summary>
        /// Creates a <see cref="T:Zen.Aero.MachineLearning.Genetic.Entity"/> for this
        /// <see cref="T:Zen.Aero.MachineLearning.Genetic.Population"/> object.
        /// </summary>
        /// <returns><see cref="T:Zen.Aero.MachineLearning.Genetic.Entity"/>.</returns>
        public Entity CreateEntity(bool initialiseEntity)
        {
            return CreateEntityFromFreePool(-1, initialiseEntity);
        }

        protected Entity CreateEntityFromFreePool(int populationIndex, bool initialiseEntity)
        {
            Entity entity = null;

            // Dequeue entity from free-pool if we can
            if (_freeEntityPool.TryDequeue(out entity))
            {
                // Put entity into correct state
                entity.MarkAsCreated();
            }
            else
            {
                // Create a new physical entity and attach
                entity = CreateEntity(populationIndex);
                entity.Attach(this);
            }

            // (Re)Initialise entity as appropriate
            if (initialiseEntity)
            {
                entity.InitEntity();
#if DEBUG
                // Sanity check entity is valid
                Debug.Assert(entity.Chromosomes.Count > 0);
                foreach (var chromo in entity.Chromosomes)
                {
                    Debug.Assert(chromo.Count > 0);
                }
#endif
            }
            return entity;
        }

        /// <summary>
        /// Creates a <see cref="T:Zen.Aero.MachineLearning.Genetic.Entity"/> for this
        /// <see cref="T:Zen.Aero.MachineLearning.Genetic.Population"/> object.
        /// </summary>
        /// <param name="populationIndex">
        /// Population index.
        /// </param>
        /// <returns><see cref="T:Zen.Aero.MachineLearning.Genetic.Entity"/>.</returns>
        /// <remarks>
        /// Override this method in derived classes to control the type
        /// of population inhabitants.
        /// </remarks>
        protected virtual Entity CreateEntity(int populationIndex)
        {
            return new Entity();
        }

        protected void Seed()
        {
            // Sanity checks
            CheckDisposed();

            _entities.Clear();
            for (var index = 0; index < _settings.StableSize; ++index)
            {
                var entity = CreateEntityFromFreePool(index, true);
                entity.Seed();
                _entities.Add(entity);
            }
        }

        protected void ResetSelectState()
        {
            if (_settings.SelectOne != null)
            {
                _settings.SelectOne.Init();
            }
            if (_settings.SelectTwo != null)
            {
                _settings.SelectTwo.Init();
            }
            if (_settings.MigrationSelector != null)
            {
                _settings.MigrationSelector.Init();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#")]
        protected virtual bool OnSelectOne(out Entity mother)
        {
            if (_settings.SelectOne == null)
            {
                throw new InvalidOperationException("Select one handler not wired up.");
            }
            return _settings.SelectOne.SelectOne(this, out mother);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#")]
        protected virtual bool OnSelectOneForMigration(out Entity mother)
        {
            if (_settings.MigrationSelector == null)
            {
                throw new InvalidOperationException("Migration select one handler not wired up.");
            }
            return _settings.MigrationSelector.SelectOne(this, out mother);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        protected virtual bool OnSelectTwo(out Entity mother, out Entity father)
        {
            if (_settings.SelectTwo == null)
            {
                throw new InvalidOperationException("Select two handler not wired up.");
            }
            return _settings.SelectTwo.SelectTwo(this, out mother, out father);
        }

        /// <summary>
        /// Called when entering generation.
        /// </summary>
        protected virtual void OnEnterGeneration()
        {
            Debug.WriteLine($"Generation: {_generation}, FreePool Size: {_freeEntityPool.Count}");
        }

        /// <summary>
        /// Called when leaving generation.
        /// </summary>
        protected virtual void OnLeaveGeneration()
        {
        }

        /// <summary>
        /// Called by framework to perform crossover and mutations on the
        /// entities in this population instance.
        /// </summary>
        protected virtual void OnCrossoverMutate()
        {
            OnCrossover();
            OnMutation();
        }

        /// <summary>
        /// Called by the framework to perform the actual crossover of every
        /// entity in this population instance.
        /// </summary>
        /// <remarks>
        /// This method uses the crossover ratio for probabilistic evaluation
        /// of whether this method will actually perform crossover.
        /// </remarks>
        protected virtual void OnCrossover()
        {
            if (_settings.Crossover == null ||
                _settings.CrossoverRatio <= 0.0 ||
                !RandomFactory.RandomProb(_settings.CrossoverRatio))
            {
                return;
            }

            Entity mother, father, daughter, son;
            while (!OnSelectTwo(out mother, out father))
            {
                if (mother != null && father != null)
                {
                    OnCrossover(mother, father, out daughter, out son);

                    if (daughter != null)
                    {
                        _entities.Add(daughter);
                    }
                    if (son != null)
                    {
                        _entities.Add(son);
                    }
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "3#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
        protected virtual void OnCrossover(Entity mother, Entity father,
            out Entity daughter, out Entity son)
        {
            if (_settings.Crossover == null)
            {
                throw new InvalidOperationException("Crossover handler not wired up.");
            }

            // Create children and copy data from parents
            daughter = CreateEntity(false);
            daughter.CopyFrom(mother);
            son = CreateEntity(false);
            son.CopyFrom(father);

            // Execute cross over function
            _settings.Crossover.Crossover(mother, father, daughter, son);
            daughter.State = EntityStates.Initialised;
            son.State = EntityStates.Initialised;
        }

        protected virtual void OnMutation()
        {
            if (_settings.Mutate == null ||
                _settings.MutationRatio <= 0.0 ||
                !RandomFactory.RandomProb(_settings.MutationRatio))
            {
                return;
            }

            Entity parent, child;
            while (!OnSelectOne(out parent))
            {
                if (parent != null)
                {
                    OnMutate(parent, out child);
                    if (child != null)
                    {
                        _entities.Add(child);
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        protected virtual void OnMutate(Entity parent, out Entity child)
        {
            if (_settings.Mutate == null)
            {
                throw new InvalidOperationException("Mutation handler not wired up.");
            }

            // Create clone of parent
            child = CreateEntity(false);
            child.CopyFrom(parent);
            _settings.Mutate.Mutate(parent, child);
            child.State = EntityStates.Initialised;
        }

        protected virtual void OnMigrate()
        {
            if (!Host.CanMigrate ||
                _settings.MigrationRatio <= 0.0f ||
                !RandomFactory.RandomProb(_settings.MigrationRatio))
            {
                return;
            }

            // Do outbound migration
            Entity migrateEntity;
            OnSelectOneForMigration(out migrateEntity);
            if (migrateEntity != null &&
                OnMigrate(new MigratingEntity(migrateEntity, IslandId)))
            {
                migrateEntity.Detach();
                _entities.Remove(migrateEntity);
            }

            // Do inbound migration
            // TODO: Check settings to determine the manner we do inbound
            //	migration. For now we empty the inbound queue.
            MigratingEntity inEntity;
            while (_inboundMigration.TryDequeue(out inEntity))
            {
                if (inEntity.IslandId != IslandId)
                {
                    inEntity.Entity.Attach(this);
                    _entities.Add(inEntity.Entity);
                }
            }
        }

        protected virtual bool OnMigrate(MigratingEntity entity)
        {
            return Host.MigrateEntity(entity);
        }

        protected virtual bool OnGeneration(GenerationEventArgs e)
        {
            // Pass to generation handler in settings
            var run = true;
            if (_settings.GenerationHandler != null)
            {
                run = _settings.GenerationHandler(this, e);
            }

            // If we have attached event handlers then 
            if (GenerationEvent != null &&
                (_settings.EvolutionEventInterval <= 1 ||
                e.Generation % _settings.EvolutionEventInterval == 0))
            {
                e.Cancel = false;
                GenerationEvent(this, e);
                run = !e.Cancel;
            }
            return run;
        }

        protected virtual Entity OnAdaptEntity(Entity entity)
        {
            if (_settings.Adaption != null)
            {
                _settings.Adaption.OptimiseEntity(this, ref entity,
                    _settings.MaxAdaptionIterations);
            }
            return entity;
        }

        protected void OnAdaptAndEvaluate()
        {
            if (_settings.SteadyState)
            {
                if (_settings.Evolution != EvolutionScheme.Darwin)
                {
                    var newSize = Count;
                    switch (_settings.Evolution)
                    {
                        case EvolutionScheme.BaldwinChildren:
                            // Baldwinian evolution for children only
                            for (var index = OriginalCount; index < newSize; ++index)
                            {
                                var adult = OnAdaptEntity(_entities[index]);
                                if (adult != null)
                                {
                                    _entities[index] = adult;
                                }
                            }
                            break;
                        case EvolutionScheme.BaldwinAll:
                            // Baldwinian evolution for entire population
                            // Not recommended but here for completeness
                            for (var index = 0; index < newSize; ++index)
                            {
                                var adult = OnAdaptEntity(_entities[index]);
                                if (adult != null)
                                {
                                    _entities[index] = adult;
                                }
                            }
                            break;
                        case EvolutionScheme.LamarckChildren:
                            while (newSize > OriginalCount)
                            {
                                --newSize;
                                var adult = OnAdaptEntity(_entities[newSize]);
                                _entities.RemoveAt(newSize);
                            }
                            break;
                        case EvolutionScheme.LamarckAll:
                            while (newSize > 0)
                            {
                                --newSize;
                                var adult = OnAdaptEntity(_entities[newSize]);
                                _entities.RemoveAt(newSize);
                            }
                            break;
                    }
                }
            }
            else
            {
                if (_settings.Evolution != EvolutionScheme.Darwin)
                {
                    // Some kind of adaptation is required.
                    // First reevaluate parents, as needed then children
                    if ((_settings.Evolution & EvolutionScheme.BaldwinParents) != 0)
                    {
                        for (var index = 0; index < OriginalCount; ++index)
                        {
                            var adult = OnAdaptEntity(_entities[index]);
                            _entities[index] = adult;
                        }
                    }
                    else if ((_settings.Evolution & EvolutionScheme.LamarckParents) != 0)
                    {
                        for (var index = 0; index < OriginalCount; ++index)
                        {
                            var adult = OnAdaptEntity(_entities[index]);
                            _entities[index] = adult;
                        }
                    }
                    if ((_settings.Evolution & EvolutionScheme.BaldwinChildren) != 0)
                    {
                        for (var index = OriginalCount; index < Count; ++index)
                        {
                            var adult = OnAdaptEntity(_entities[index]);
                            _entities[index] = adult;
                        }
                    }
                    else if ((_settings.Evolution & EvolutionScheme.LamarckChildren) != 0)
                    {
                        for (var index = OriginalCount; index < Count; ++index)
                        {
                            var adult = OnAdaptEntity(_entities[index]);
                            _entities[index] = adult;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Survival of the fittest.
        /// Enforce elitism, apply crowding operator, reduce population
        /// back to its stable size and rerank entities as required.
        /// </summary>
        protected void OnSurvival()
        {
            // Need to kill parents or rescore parents?
            if (_settings.Elitism == ElitismType.ParentsDie ||
                _settings.Elitism == ElitismType.OneParentSurvives)
            {
                _entities.RemoveRange(
                    _settings.Elitism == ElitismType.OneParentSurvives ? 1 : 0,
                    _settings.Elitism == ElitismType.OneParentSurvives ? OriginalCount - 1 : OriginalCount);
            }

            // Ensure population fitness has been evaluated
            EnsureFitness();

            // Sort population members by fitness
            Sort();

            // TODO: Enforce type of crowding desired
            // Rough crowding doesn't do binary compare of chromosomes to
            //	determine equality it simply assumes identical fitness.
            // Exact elitism does make the full check

            // Least fit population members die to restore
            //	the population size to stable size
            OnGenocide();
        }

        /// <summary>
        /// Ensures the fitness of every entity has been evaluated
        /// </summary>
        protected virtual void EnsureFitness()
        {
            foreach (var entity in _entities)
            {
                entity.EnsureFitness();
            }
        }

        /// <summary>
        /// Sorts population members in descending fitness order.
        /// </summary>
        protected virtual void Sort()
        {
            _entities.Sort(new EntityFitnessComparer(true));
        }

        protected virtual void OnReplaceEntity(Entity entity)
        {
            var fitness = entity.Fitness;

            // Insert entity into sorted location within the list
            for (var index = 0; index < _entities.Count; ++index)
            {
                if (_entities[index].Fitness < fitness)
                {
                    _entities.Insert(index, entity);
                    _entities.RemoveAt(OriginalCount - 1);
                    break;
                }
            }
        }

        protected virtual void OnGenocide()
        {
            while (Count > _settings.StableSize)
            {
                // Remove last entity from our array
                var entity = _entities[Count - 1];
                _entities.RemoveAt(Count - 1);

                // Attempt to push entity onto free entity pool
                var dispose = true;
                if (_freeEntityPool.Count < _maxFreeEntityPoolSize)
                {
                    entity.MarkAsFree();
                    _freeEntityPool.Enqueue(entity);
                    dispose = false;
                }

                if (dispose)
                {
                    entity.Dispose();
                }
            }
        }
        #endregion

        #region Internal Methods
        internal void MutateInternal(Entity parent, out Entity child)
        {
            OnMutate(parent, out child);
        }
        #endregion

        #region Implementation of IDisposable
        ~Population()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Implementation of ICloneable
        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion
    }
}
