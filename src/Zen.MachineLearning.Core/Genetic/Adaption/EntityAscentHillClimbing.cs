namespace Zen.MachineLearning.Core.Genetic.Adaption
{
    /// <summary>
    /// <b>EntityAscentHillClimbing</b> serves as a base class for all
    /// adaption algorithms based on Hill-Climbing algorithms.
    /// </summary>
    public abstract class EntityAscentHillClimbing : EntityAdaption
    {
        #region Protected Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityAscentHillClimbing"/> class.
        /// </summary>
        protected EntityAscentHillClimbing()
        {
        }
        #endregion

        #region Protected Properties
        /// <summary>
        /// Gets or sets the index of the chromosome.
        /// </summary>
        /// <value>The index of the chromosome.</value>
        protected int ChromosomeIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the index of the allele.
        /// </summary>
        /// <value>The index of the allele.</value>
        protected int AlleleIndex
        {
            get;
            set;
        }
        #endregion

        #region Protected Methods
        protected override void OnOptimiseEntity(
            Population population, Entity bestEntity, out Entity putative)
        {
            UpdateHillClimbParameters(bestEntity);
            OnMutateAllele(population, bestEntity, out putative);
        }

        /// <summary>
        /// Updates the hill-climb parameters.
        /// </summary>
        /// <param name="entity">The entity.</param>
        protected abstract void UpdateHillClimbParameters(Entity entity);

        /// <summary>
        /// Called during optimisation to mutate the specified entity.
        /// </summary>
        /// <param name="population">The population.</param>
        /// <param name="bestEntity">The best entity.</param>
        /// <param name="putative">The putative.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
        protected virtual void OnMutateAllele(
            Population population, Entity bestEntity, out Entity putative)
        {
            population.MutateInternal(bestEntity, out putative);
        }
        #endregion
    }
}
