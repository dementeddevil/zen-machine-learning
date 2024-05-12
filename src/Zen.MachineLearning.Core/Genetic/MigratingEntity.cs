namespace Zen.MachineLearning.Core.Genetic
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Wraps an entity migrating from one population to another.
    /// This object takes care of serializing an entity into another thread
    /// </summary>
    [Serializable]
    public class MigratingEntity : IDisposable, ISerializable
    {
        #region Public Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MigratingEntity" /> class.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="island">The island.</param>
        public MigratingEntity(Entity entity, Guid island)
        {
            Entity = entity;
            IslandId = island;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        public Entity Entity
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the island id.
        /// </summary>
        /// <value>
        /// The island id.
        /// </value>
        public Guid IslandId
        {
            get;
            private set;
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="MigratingEntity"/> is reclaimed by garbage collection.
        /// </summary>
        ~MigratingEntity()
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
            if (disposing && Entity != null)
            {
                Entity.Dispose();
                Entity = null;
            }
        }
        #endregion

        #region ISerializable Members
        protected MigratingEntity(SerializationInfo info, StreamingContext context)
        {
            IslandId = (Guid)info.GetValue("Island", typeof(Guid));
            Entity = new Entity();
            Entity.Chromosomes = (Dna)info.GetValue("Dna", typeof(Dna));
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Save the island
            info.AddValue("Island", IslandId);

            // Force the entity to update DNA
            Entity.SaveEntity();

            // Save entity DNA only
            info.AddValue("Dna", Entity.Chromosomes);
        }
        #endregion
    }
}
