namespace Zen.MachineLearning.Core.Genetic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Threading;

    public enum EntityStates
    {
        Created = 0,
        Initialised = 1,
        Loaded = 2,
        Ready = 3,
        Free = 4,
    }

    /// <summary>
    /// <b>Entity</b> encapsulates a genetic-algoritm entity.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An entity is a member of the population that is manipulated by the GA
    /// framework to produce or determine the fittest member.
    /// </para>
    /// <para>
    /// 
    /// </para>
    /// </remarks>
    [Serializable]
    public class Entity : IDisposable, ICloneable
    {
        #region Private Members
        private static readonly object EventInitObject = new object();
        private static readonly object EventLoadObject = new object();
        private static long _nextId = 0;

        [NonSerialized]
        private EventHandlerList _events;

        [NonSerialized]
        private bool _disposed = false;

        [NonSerialized]
        private Population _population;

        private Dna _dna;
        private float _fitness = float.MinValue;
        #endregion

        #region Public Events
        /// <summary>
        /// Occurs when [init].
        /// </summary>
        public event EventHandler Init
        {
            add
            {
                Events.AddHandler(EventInitObject, value);
            }
            remove
            {
                Events.RemoveHandler(EventInitObject, value);
            }
        }

        /// <summary>
        /// Occurs when [load].
        /// </summary>
        public event EventHandler Load
        {
            add
            {
                Events.AddHandler(EventLoadObject, value);
            }
            remove
            {
                Events.RemoveHandler(EventLoadObject, value);
            }
        }
        #endregion

        #region Public Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class.
        /// </summary>
        public Entity()
        {
            Id = Interlocked.Increment(ref _nextId);
            State = EntityStates.Created;
        }
        #endregion

        #region Public Properties
        public EntityStates State
        {
            get;
            internal set;
        }

        public long Id
        {
            get;
            private set;
        }

        public Dna Chromosomes
        {
            get
            {
                if (_dna == null)
                {
                    _dna = new Dna();
                }
                return _dna;
            }
            internal set
            {
                _dna = value;
                State = EntityStates.Initialised;
            }
        }

        /// <summary>
        /// Gets the entity fitness value.
        /// </summary>
        /// <remarks>
        /// If the entity has not been loaded then calls to this property
        /// will cause the entity to load.
        /// If the fitness of this entity has not yet been calculated then
        /// this will be performed via a call to EvaluateFitness.
        /// The fitness value calculated is cached.
        /// </remarks>
        public float Fitness
        {
            get
            {
                EnsureFitness();
                return _fitness;
            }
        }

        public object Data
        {
            get;
            set;
        }
        #endregion

        #region Protected Properties
        protected EventHandlerList Events
        {
            get
            {
                if (_events == null)
                {
                    _events = new EventHandlerList();
                }
                return _events;
            }
        }

        protected Population Population
        {
            get
            {
                return _population;
            }
            private set
            {
                _population = value;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Seeds this instance.
        /// </summary>
        public void Seed()
        {
            // Sanity checks
            CheckDisposed();
            Chromosomes.Seed();
        }

        /// <summary>
        /// Seeds this instance with the specified probability.
        /// </summary>
        /// <param name="probability">The probability.</param>
        public virtual void Seed(float probability)
        {
            // Sanity checks
            CheckDisposed();
            Chromosomes.Seed(probability);
        }

        /// <summary>
        /// Copies the dna from the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public virtual void CopyFrom(Entity entity)
        {
            // Sanity checks
            CheckDisposed();
            entity.CheckDisposed();

            // Shallow copy members
            State = entity.State;
            _fitness = entity._fitness;
            Data = entity.Data;

            // Make deep copy of chromosomes
            _dna = entity.Chromosomes.Clone();
        }

        /// <summary>
        /// Initialises this entity.
        /// </summary>
        public virtual void InitEntity()
        {
            // Sanity checks
            CheckDisposed();

            // Ensure we only create DNA once.
            if (State == EntityStates.Created)
            {
                CreateDna();
            }

            // Reset state and raise event
            State = EntityStates.Initialised;
            OnInit(EventArgs.Empty);
        }

        /// <summary>
        /// Loads the entity from its dna.
        /// </summary>
        public virtual void LoadEntity()
        {
            // Sanity checks
            CheckDisposed();
            if (State == EntityStates.Initialised)
            {
                LoadFromDna();
                State = EntityStates.Loaded;
                OnLoad(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Saves the entity to its dna.
        /// </summary>
        public virtual void SaveEntity()
        {
            // Sanity checks
            CheckDisposed();

            SaveToDna();
        }

        /// <summary>
        /// Ensures this instance has been initialised.
        /// </summary>
        public void EnsureInitialised()
        {
            if (State == EntityStates.Created)
            {
                InitEntity();
            }
        }

        /// <summary>
        /// Ensures this instance has been loaded.
        /// </summary>
        public void EnsureLoaded()
        {
            EnsureInitialised();
            if (State == EntityStates.Initialised)
            {
                LoadEntity();
            }
        }

        /// <summary>
        /// Ensures the fitness of this instance has been calculated.
        /// </summary>
        /// <remarks>
        /// This method performs all the steps necessary to get the entity
        /// to the appropriate state.
        /// </remarks>
        public void EnsureFitness()
        {
            EnsureLoaded();

            if (State == EntityStates.Loaded)
            {
                _fitness = EvaluateFitness();
                State = EntityStates.Ready;
            }
        }

        /// <summary>
        /// Sets the fitness of this instance explicitly.
        /// </summary>
        /// <param name="fitness">The fitness.</param>
        public void SetFitness(float fitness)
        {
            if (State != EntityStates.Ready)
            {
                _fitness = fitness;
                State = EntityStates.Ready;
            }
        }
        #endregion

        #region Internal Methods
        internal void Attach(Population population)
        {
            Population = population;
        }

        internal void Detach()
        {
            Population = null;
        }

        internal void MarkAsCreated()
        {
            Debug.Assert(State == EntityStates.Created || State == EntityStates.Free);
            State = EntityStates.Created;
        }

        internal void MarkAsFree()
        {
            Debug.Assert(State != EntityStates.Free);
            State = EntityStates.Free;

            // Discard our DNA
            if (_dna != null)
            {
                _dna.Dispose();
                _dna = null;
            }
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Creates the dna for this instance.
        /// </summary>
        /// <remarks>
        /// Derived classes will typically add chromosome objects to
        /// the entity instance at this point.
        /// </remarks>
        protected virtual void CreateDna()
        {
        }

        /// <summary>
        /// Loads the instance from its dna representation.
        /// </summary>
        protected virtual void LoadFromDna()
        {
        }

        /// <summary>
        /// Saves the instance to its dna representation.
        /// </summary>
        protected virtual void SaveToDna()
        {
        }

        /// <summary>
        /// Raises the <see cref="E:Init"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnInit(EventArgs e)
        {
            var eh = (EventHandler)Events[EventInitObject];
            if (eh != null)
            {
                eh(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:Load"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnLoad(EventArgs e)
        {
            var eh = (EventHandler)Events[EventLoadObject];
            if (eh != null)
            {
                eh(this, e);
            }
        }

        /// <summary>
        /// Evaluates the fitness.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// By default this method will call EnsureLoaded prior to returning
        /// a fitness score of zero.
        /// Override this method to evaluate the fitness of this entity.
        /// </remarks>
        protected virtual float EvaluateFitness()
        {
            EnsureLoaded();
            return 0.0f;
        }

        protected void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(
                    $"Entity [Id:{Id}]");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                //System.Diagnostics.Trace.WriteLine ("Disposed : Entity [" + ID.ToString () + "]");
                if (disposing)
                {
                    if (_dna != null)
                    {
                        _dna.Dispose();
                        _dna = null;
                    }
                    if (_events != null)
                    {
                        _events.Dispose();
                        _events = null;
                    }
                }

                _disposed = true;
            }
        }
        #endregion

        #region Implementation of ICloneable
        public Entity Clone()
        {
            // Sanity checks
            CheckDisposed();

            // Clone the object and assign a new ID.
            var entity = (Entity)MemberwiseClone();
            entity.Id = Interlocked.Increment(ref _nextId);

            // Invalidate the chromosomes
            entity._dna = Chromosomes.Clone();
            return entity;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion
    }

    /// <summary>
    /// <b>EntityFitnessComparer</b> performs fitness level equality and/or
    /// comparison of two entities.
    /// </summary>
    public class EntityFitnessComparer : IComparer<Entity>, IComparer,
        IEqualityComparer<Entity>, IEqualityComparer
    {
        #region Private Fields
        private readonly bool _fittestFirst;
        #endregion

        #region Public Constructors
        public EntityFitnessComparer()
        {
        }

        public EntityFitnessComparer(bool fittestFirst)
        {
            _fittestFirst = fittestFirst;
        }
        #endregion

        #region Public Methods
        public int Compare(Entity x, Entity y)
        {
            // Sanity checks...
            if (x == null && y == null)
            {
                return 0;
            }
            else if (x == null)
            {
                return 1;
            }
            else if (y == null)
            {
                return -1;
            }

            // Rank by fitness...
            if (_fittestFirst)
            {
                return y.Fitness.CompareTo(x.Fitness);
            }
            else
            {
                return x.Fitness.CompareTo(y.Fitness);
            }
        }

        public bool Equals(Entity x, Entity y)
        {
            // Sanity checks...
            if (x == null && y == null)
            {
                return true;
            }
            else if (x == null)
            {
                return false;
            }
            else if (y == null)
            {
                return false;
            }
            return x.Fitness == y.Fitness;
        }

        public int GetHashCode(Entity obj)
        {
            if (obj == null)
            {
                return 0;
            }
            return obj.Fitness.GetHashCode();
        }
        #endregion

        #region IComparer Members
        int IComparer.Compare(object x, object y)
        {
            return Compare(x as Entity, y as Entity);
        }
        #endregion

        #region IEqualityComparer Members
        bool IEqualityComparer.Equals(object x, object y)
        {
            return Equals(x as Entity, y as Entity);
        }

        int IEqualityComparer.GetHashCode(object obj)
        {
            return GetHashCode(obj as Entity);
        }
        #endregion
    }

    /// <summary>
    /// <b>EntityChromosomeEqualityComparer</b> performs chromosome level
    /// equality-only comparison of two entities.
    /// </summary>
    public class EntityChromosomeEqualityComparer :
        IEqualityComparer<Entity>, IEqualityComparer
    {
        #region Public Constructors
        public EntityChromosomeEqualityComparer()
        {
        }
        #endregion

        #region Public Methods
        public bool Equals(Entity x, Entity y)
        {
            // Sanity checks...
            if (x == null && y == null)
            {
                return true;
            }
            else if (x == null)
            {
                return false;
            }
            else if (y == null)
            {
                return false;
            }

            // Entities must have same number of chromosomes and 
            //	each chromosome must have matching length.
            for (var chromoIndex = 0; chromoIndex < x.Chromosomes.Count; ++chromoIndex)
            {
                var chromoX = x.Chromosomes[chromoIndex];
                var chromoY = y.Chromosomes[chromoIndex];
                for (var alleleIndex = 0; alleleIndex < chromoX.Count; ++alleleIndex)
                {
                    if (!Equals(chromoX[alleleIndex], chromoY[alleleIndex]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public int GetHashCode(Entity obj)
        {
            if (obj == null)
            {
                return 0;
            }
            return obj.GetHashCode();
        }
        #endregion

        #region IEqualityComparer Members
        bool IEqualityComparer.Equals(object x, object y)
        {
            return Equals(x as Entity, y as Entity);
        }

        int IEqualityComparer.GetHashCode(object obj)
        {
            return GetHashCode(obj as Entity);
        }
        #endregion
    }
}
