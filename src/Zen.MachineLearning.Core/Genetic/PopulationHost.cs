namespace Zen.MachineLearning.Core.Genetic
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// <c>PopulationHost</c> provides the root implementation of a
    /// Population Host object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default implementation provides extension points for supporting
    /// entity migration and multiple population execution.
    /// </para>
    /// <para>
    /// The ability to restart all running population instances is exposed
    /// to support synchronising population objects.
    /// </para>
    /// <para>
    /// Each population running on the host can be given different settings
    /// or inherit the default settings of the host.
    /// </para>
    /// <para>
    /// The base implementation is fully synchronous and not thread-safe.
    /// </para>
    /// </remarks>
    public class PopulationHost : IDisposable
    {
        #region Internal Objects
        private class HostMigratingEntity : MigratingEntity
        {
            private readonly Guid _sourceHostId;

            public HostMigratingEntity(Guid sourceHostId, MigratingEntity entity)
                : base(entity.Entity, entity.IslandId)
            {
                _sourceHostId = sourceHostId;
            }

            public Guid SourceHostId => _sourceHostId;
        }
        #endregion

        #region Private Fields
        private bool _disposed;
        private readonly PopulationSettings _defaultSettings;
        private Queue<HostMigratingEntity> _sharedMigrationQueue;
        private Dictionary<Guid, Population> _populations;
        #endregion

        #region Public Constructors
        /// <summary>
        /// Creates a new <see cref="T:Zen.Aero.MachineLearning.Genetic.PopulationHost"/>
        /// </summary>
        /// <param name="settings"></param>
        public PopulationHost(PopulationSettings defaultSettings)
        {
            _defaultSettings = defaultSettings.Clone();
            _populations = new Dictionary<Guid, Population>();
            _sharedMigrationQueue = new Queue<HostMigratingEntity>();
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets the default settings.
        /// </summary>
        /// <value>The default settings.</value>
        public PopulationSettings DefaultSettings => _defaultSettings;

        /// <summary>
        /// Gets the host id.
        /// </summary>
        /// <value>The host id.</value>
        public Guid HostId
        {
            get;
            private set;
        }

        public bool CanMigrate
        {
            get;
            protected set;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Updates the host ID for the host and restarts all current 
        /// populations.
        /// </summary>
        /// <param name="newId"></param>
        /// <remarks>
        /// Use this as a mechanism to restart all active populations if an
        /// external event occurs that would invalidate the GAs in progress.
        /// </remarks>
        public virtual void UpdateHostId(Guid newHostId)
        {
            CheckDisposed();
            if (HostId != newHostId)
            {
                HostId = newHostId;
                RestartAllEvolutions();
            }
        }

        public Entity[] Evolve(PopulationSettings settings)
        {
            // If no settings provided; use default settings.
            if (settings == null)
            {
                // If no default settings then throw exception
                if (_defaultSettings == null)
                {
                    throw new ArgumentNullException(nameof(settings));
                }

                settings = _defaultSettings;
            }

            Entity[] entities = null;

            // Create population
            using (var pop = CreatePopulation())
            {
                // Initialise population
                pop.InitializePopulation(settings);

                // Add population to map
                AddPopulation(pop);
                pop.GenerationEvent += OnPopulationGenerationInternal;
                try
                {
                    // Start evolution process
                    pop.Evolve();

                    // Copy entities from population
                    if (pop.Entities.Count > 0)
                    {
                        entities = new Entity[pop.Entities.Count];
                        for (var index = 0; index < pop.Entities.Count; ++index)
                        {
                            entities[index] = pop.Entities[index].Clone();
                        }
                    }
                }
                finally
                {
                    // Disconnect from event handler and remove
                    pop.GenerationEvent -= OnPopulationGenerationInternal;
                    RemovePopulation(pop.IslandId);
                }
            }

            return entities;
        }

        /// <summary>
        /// Queues the specified entity for migration.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>
        /// <c>true</c> if the entity was successfully queued for migration.
        /// See remarks.
        /// </returns>
        /// <remarks>
        /// <para>
        /// If the source host id does not match this host's id then the
        /// entity will be rejected. This means that PopulationHost objects
        /// running on different machines must synchronise their host ids
        /// from a centralised location - ie: database or broker service.
        /// For hosted entities the population count must be greater than one
        /// for the entity to be queued.
        /// For non-hosted entities the population count must be greater than
        /// zero for the entity to be queued.
        /// </para>
        /// <para>
        /// Calls to this method are not thread-safe and this must be
        /// protected by the caller.
        /// </para>
        /// </remarks>
        public bool MigrateEntity(MigratingEntity entity)
        {
            CheckDisposed();
            return MigrateEntity(HostId, entity);
        }

        /// <summary>
        /// Queues the specified entity for migration using the specified
        /// host id as a source of the entity.
        /// </summary>
        /// <param name="sourceHostId">The source host id.</param>
        /// <param name="entity">The entity.</param>
        /// <returns>
        /// <c>true</c> if the entity was successfully queued for migration.
        /// See remarks.
        /// </returns>
        /// <remarks>
        /// <para>
        /// If the source host id does not match this host's id then the
        /// entity will be rejected. This means that PopulationHost objects
        /// running on different machines must synchronise their host ids
        /// from a centralised location - ie: database or broker service.
        /// For hosted entities the population count must be greater than one
        /// for the entity to be queued.
        /// For non-hosted entities the population count must be greater than
        /// zero for the entity to be queued.
        /// </para>
        /// <para>
        /// Calls to this method are not thread-safe and this must be
        /// protected by the caller.
        /// </para>
        /// </remarks>
        public virtual bool MigrateEntity(Guid sourceHostId, MigratingEntity entity)
        {
            CheckDisposed();
            var migrated = false;
            if (sourceHostId == HostId &&
                (IsHostedEntity(entity) && _populations.Count > 1 ||
                !IsHostedEntity(entity) && _populations.Count > 0))
            {
                _sharedMigrationQueue.Enqueue(
                    new HostMigratingEntity(sourceHostId, entity));
                migrated = true;
            }
            return migrated;
        }

        /// <summary>
        /// Determines whether the specified entity is hosted by this host.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>
        /// <c>true</c> if this is a hosted entity; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Calls to this method are not thread-safe and this must be
        /// protected by the caller.
        /// </remarks>
        public virtual bool IsHostedEntity(MigratingEntity entity)
        {
            CheckDisposed();
            return _populations.ContainsKey(entity.IslandId);
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Checks whether this object has been disposed.
        /// </summary>
        protected void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        /// <summary>
        /// Adds the population.
        /// </summary>
        /// <param name="population">The population.</param>
        /// <remarks>
        /// Calls to this method are not thread-safe and this must be
        /// protected by the caller.
        /// </remarks>
        protected virtual void AddPopulation(Population population)
        {
            CheckDisposed();
            _populations.Add(population.IslandId, population);
        }

        /// <summary>
        /// Removes the population.
        /// </summary>
        /// <param name="island">The island.</param>
        /// <remarks>
        /// Calls to this method are not thread-safe and this must be
        /// protected by the caller.
        /// </remarks>
        protected virtual void RemovePopulation(Guid island)
        {
            CheckDisposed();
            _populations.Remove(island);
        }

        /// <summary>
        /// Restarts the evaluations on every active population.
        /// </summary>
        /// <remarks>
        /// Calls to this method are not thread-safe and this must be
        /// protected by the caller.
        /// </remarks>
        protected virtual void RestartAllEvolutions()
        {
            CheckDisposed();
            foreach (var pop in _populations.Values)
            {
                pop.RestartEvolution();
            }
        }

        /// <summary>
        /// Gets the next migrating entity from the migration queue.
        /// </summary>
        /// <returns>
        /// A <see cref="T:MigratingEntity"/> object if one is available;
        /// otherwise, <c>null</c>.
        /// </returns>
        /// <remarks>
        /// Entities originating from a host whos id does not match this
        /// host are silently discarded.
        /// Calls to this method are not thread-safe and this must be
        /// protected by the caller.
        /// </remarks>
        protected virtual MigratingEntity GetNextMigratingEntity()
        {
            CheckDisposed();
            HostMigratingEntity entity = null;
            do
            {
                if (_sharedMigrationQueue.Count > 0)
                {
                    entity = _sharedMigrationQueue.Dequeue();
                }
                else
                {
                    entity = null;
                }
            }
            while (entity != null && entity.SourceHostId != HostId);
            return entity;
        }

        /// <summary>
        /// Adds the entity to a population
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>
        /// <c>true</c> if the entity is migrated to a population; otherwise,
        /// <c>false</c>.
        /// </returns>
        protected virtual bool AddMigratingEntityToPopulation(MigratingEntity entity)
        {
            CheckDisposed();
            var migrated = false;

            // If this is a hosted entity then we don't want to add the
            //	entity to the same island...
            foreach (var pop in _populations.Values)
            {
                if (pop.IslandId != entity.IslandId)
                {
                    pop.AddMigratingEntity(entity);
                    migrated = true;
                }
            }
            return migrated;
        }

        /// <summary>
        /// Called during evolution process when a new generation is started.
        /// </summary>
        /// <param name="population">The population.</param>
        /// <param name="generation">The generation.</param>
        /// <returns>
        /// <c>true</c> to continue processing; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool OnPopulationGeneration(Population population, int generation)
        {
            return true;
        }

        /// <summary>
        /// Creates the population.
        /// </summary>
        /// <returns></returns>
        protected virtual Population CreatePopulation()
        {
            return new Population(this);
        }

        protected T CreatePopulation<T>()
            where T : Population
        {
            return (T)CreatePopulation();
        }
        #endregion

        #region Private Methods
        private void OnPopulationGenerationInternal(object sender, GenerationEventArgs e)
        {
            e.Cancel = !OnPopulationGeneration((Population)sender, e.Generation);
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="PopulationHostBase"/> is reclaimed by garbage collection.
        /// </summary>
        ~PopulationHost()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, 
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; 
        /// <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_sharedMigrationQueue != null)
                {
                    while (_sharedMigrationQueue.Count > 0)
                    {
                        _sharedMigrationQueue.Dequeue().Dispose();
                    }
                    _sharedMigrationQueue = null;
                }
                if (_populations != null)
                {
                    foreach (var island in _populations.Keys)
                    {
                        _populations[island].Dispose();
                    }
                    _populations.Clear();
                    _populations = null;
                }
                _disposed = true;
            }
        }
        #endregion
    }

    /// <summary>
    /// <c>AsyncPopulationHost</c> provides an asynchronous implementation of
    /// a Population Host using .NET APM pattern.
    /// </summary>
    /// <remarks>
    /// The default host supports local migration of entities between
    /// population objects running on this host instance.
    /// Each population is executed on a thread from the .NET Thread pool.
    /// </remarks>
    public class AsyncPopulationHost : PopulationHost
    {
        #region Internal Objects
        private class AsyncWorker : IAsyncResult
        {
            private readonly AsyncPopulationHost _owner;
            private readonly PopulationSettings _settings;
            private readonly ManualResetEvent _asyncWaitHandle;
            private readonly AsyncCallback _callback;
            private bool _isCompleted;
            private readonly object _asyncState;
            private Entity[] _entities;
            private Exception _error;

            public AsyncWorker(AsyncPopulationHost owner,
                PopulationSettings settings, AsyncCallback callback,
                object state)
            {
                _owner = owner;
                _settings = settings;
                _asyncWaitHandle = new ManualResetEvent(false);
                _callback = callback;
                _asyncState = state;

                ThreadPool.QueueUserWorkItem(new WaitCallback(OnEvolveThread), _settings);
            }

            public Entity[] EndEvolve()
            {
                _asyncWaitHandle.WaitOne();
                if (_error != null)
                {
                    throw _error;
                }
                return _entities;
            }

            private void OnEvolveThread(object state)
            {
                var settings = (PopulationSettings)state;
                Interlocked.Increment(ref _owner._runningThreads);
                try
                {
                    // Run the synchronous evolution method
                    _entities = _owner.Evolve(settings);
                }
                catch (Exception e)
                {
                    _error = e;
                }
                finally
                {
                    // Decrease number of running threads now
                    Interlocked.Decrement(ref _owner._runningThreads);

                    // Mark handler as complete
                    _isCompleted = true;
                    _asyncWaitHandle.Set();

                    // Call through callback function
                    if (_callback != null)
                    {
                        _callback(this);
                    }
                }
            }

            internal AsyncPopulationHost Owner => _owner;

            object IAsyncResult.AsyncState => _asyncState;

            WaitHandle IAsyncResult.AsyncWaitHandle => _asyncWaitHandle;

            bool IAsyncResult.CompletedSynchronously => false;

            bool IAsyncResult.IsCompleted => _isCompleted;
        }
        #endregion

        #region Private Fields
        private bool _supportsExternalMigration;
        private int _runningThreads;
        private Thread _migrationThread;
        private readonly Semaphore _syncMigrationQueue = new Semaphore(0, 1);
        private readonly object _syncPopulations = new object();
        private readonly ManualResetEvent _shutdownRequestEvent = new ManualResetEvent(false);
        #endregion

        #region Public Constructors
        /// <summary>
        /// Creates a new <see cref="T:Zen.Aero.MachineLearning.Genetic.PopulationHost"/>
        /// for the given population type and settings.
        /// </summary>
        /// <param name="settings"></param>
        public AsyncPopulationHost(PopulationSettings defaultSettings)
            : base(defaultSettings)
        {
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Begins asynchronous evolutions for each of the supplied population
        /// settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        /// <returns>
        /// A matching array of IAsyncResult objects to track each evolution.
        /// </returns>
        public IAsyncResult[] BeginEvolve(PopulationSettings[] settings,
            AsyncCallback callback, object state)
        {
            if (settings == null || settings.Length == 0)
            {
                throw new ArgumentException("settings");
            }

            var results = new IAsyncResult[settings.Length];
            for (var index = 0; index < settings.Length; ++index)
            {
                results[index] = BeginEvolve(settings[index], callback, state);
            }
            return results;
        }

        /// <summary>
        /// Begins an asychronous evolution against this host
        /// using the default population settings.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        /// <returns>
        /// <see cref="T:System.IAsyncResult"/> representing the async
        /// operation.
        /// </returns>
        public IAsyncResult BeginEvolve(AsyncCallback callback, object state)
        {
            CheckDisposed();
            return BeginEvolve(DefaultSettings, callback, state);
        }

        /// <summary>
        /// Begins an asychronous evolution against this host.
        /// </summary>
        /// <param name="maxGenerations"></param>
        /// <param name="steadyState"></param>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        /// <returns>
        /// <see cref="T:System.IAsyncResult"/> representing the async
        /// operation.
        /// </returns>
        public virtual IAsyncResult BeginEvolve(PopulationSettings settings,
            AsyncCallback callback, object state)
        {
            StartMigrationThread();
            return new AsyncWorker(this, settings, callback, state);
        }

        /// <summary>
        /// Ends an asynchronous evolve.
        /// </summary>
        /// <param name="ar"></param>
        /// <returns>A collection of evolved entities.</returns>
        /// <remarks>
        /// If the operation is still ongoing then this method will block until
        /// the operation has completed.
        /// </remarks>
        /// <exception cref="T:System.ArgumentException">
        /// Thrown if ar parameter was not obtained from BeginEvolve
        /// </exception>
        public virtual Entity[] EndEvolve(IAsyncResult ar)
        {
            if (!(ar is AsyncWorker))
            {
                throw new ArgumentException("ar",
                    "IAsyncResult not raised by this object.");
            }

            var worker = ar as AsyncWorker;
            var result = worker.EndEvolve();
            //StopMigrationThread();
            return result;
        }

        public override bool MigrateEntity(Guid sourceHostId, MigratingEntity entity)
        {
            var migrated = false;
            migrated = base.MigrateEntity(sourceHostId, entity);
            if (migrated)
            {
                _syncMigrationQueue.Release();
            }
            return migrated;
        }
        #endregion

        #region Protected Properties
        protected bool SupportsExternalMigration
        {
            get
            {
                return _supportsExternalMigration;
            }
            set
            {
                _supportsExternalMigration = value;
            }
        }

        protected bool ShutdownMigrationThread
        {
            get
            {
                if (SupportsExternalMigration && _runningThreads > 0 ||
                    !SupportsExternalMigration && _runningThreads > 1)
                {
                    return false;
                }
                return true;
            }
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Adds the population.
        /// </summary>
        /// <param name="population">The population.</param>
        /// <remarks>
        /// Calls to this method are thread-safe.
        /// </remarks>
        protected override void AddPopulation(Population population)
        {
            lock (_syncPopulations)
            {
                base.AddPopulation(population);
            }
        }

        /// <summary>
        /// Removes the population.
        /// </summary>
        /// <param name="island">The island.</param>
        /// <remarks>
        /// Calls to this method are thread-safe.
        /// </remarks>
        protected override void RemovePopulation(Guid island)
        {
            lock (_syncPopulations)
            {
                base.RemovePopulation(island);
            }
        }

        /// <summary>
        /// Restarts the evaluations on every active population.
        /// </summary>
        /// <remarks>
        /// Calls to this method are thread-safe.
        /// </remarks>
        protected override void RestartAllEvolutions()
        {
            lock (_syncPopulations)
            {
                base.RestartAllEvolutions();
            }
        }
        #endregion

        #region Private Methods
        private void OnMigrateEntityThread()
        {
            try
            {
                while (true)
                {
                    // Wait for next migrating entity
                    var eventIndex = WaitHandle.WaitAny(new WaitHandle[] { _syncMigrationQueue, _shutdownRequestEvent });

                    // Check for termination event
                    if (eventIndex == 1)
                    {
                        break;
                    }

                    // Retrieve next entity from the queue and release lock
                    var entity = GetNextMigratingEntity();
                    _syncMigrationQueue.Release();

                    // If we have population we can send to then do it
                    if (entity != null)
                    {
                        bool migrated;
                        lock (_syncPopulations)
                        {
                            migrated = AddMigratingEntityToPopulation(entity);
                        }
                        if (!migrated)
                        {
                            entity.Dispose();
                        }
                    }
                }
            }
            finally
            {
            }
        }

        private void StartMigrationThread()
        {
            if (_migrationThread == null)
            {
                lock (this)
                {
                    if (_migrationThread == null)
                    {
                        _migrationThread = new Thread(new ThreadStart(OnMigrateEntityThread));
                        _migrationThread.Name = "Migration Thread";
                        _migrationThread.Start();
                    }
                }
            }
        }

        private void StopMigrationThread()
        {
            if (ShutdownMigrationThread && _migrationThread != null)
            {
                _shutdownRequestEvent.Set();
                _migrationThread = null;
            }
        }
        #endregion
    }
}
