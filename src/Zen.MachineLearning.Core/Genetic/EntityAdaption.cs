namespace Zen.MachineLearning.Core.Genetic
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEntityAdaption
    {
        int OptimiseEntity(
            Population population, ref Entity bestEntity, int maxIterations);
    }

    /// <summary>
    /// <c>EntityAdaption</c> serves as a base class for all entity population
    /// adaption algorithms.
    /// </summary>
    public abstract class EntityAdaption : IEntityAdaption
    {
        #region Private Fields

        #endregion

        #region Public Constructors

        #endregion

        #region Public Properties

        #endregion

        #region Public Methods
        /// <summary>
        /// Optimises the specified entity over a fixed number of iterations.
        /// </summary>
        /// <param name="population">Current population.</param>
        /// <param name="bestEntity">Best entity found so far (this will be overwritten).</param>
        /// <param name="maxIterations">Max iterations</param>
        /// <returns>Number of iterations performed.</returns>
        public virtual int OptimiseEntity(Population population,
            ref Entity bestEntity, int maxIterations)
        {
            // Create putative entity
            OnInitialiseOptimisation(population, bestEntity);

            int iteration;
            for (iteration = 0; OnIteration(iteration, maxIterations); ++iteration)
            {
                // Derived class optimisation step
                Entity putative;
                OnOptimiseEntity(population, bestEntity, out putative);

                // Is new solution better than current best?
                if (EvaluateFitness(population, bestEntity, putative))
                {
                    bestEntity = putative;
                    OnUpdateFittest(population, bestEntity);
                }
            }

            OnTerminateOptimisation(population, iteration);
            return iteration;
        }
        #endregion

        #region Protected Methods
        protected virtual void OnInitialiseOptimisation(
            Population population, Entity bestEntity)
        {
        }

        /// <summary>
        /// Determines whether optimisation should continue.
        /// </summary>
        /// <param name="currentIteration"></param>
        /// <returns></returns>
        protected virtual bool OnIteration(int currentIteration, int maxIterations)
        {
            return currentIteration < maxIterations;
        }

        /// <summary>
        /// Performs an optimisation of the specified entity.
        /// </summary>
        /// <param name="population"></param>
        /// <param name="bestEntity"></param>
        /// <param name="putative"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
        protected abstract void OnOptimiseEntity(
            Population population, Entity bestEntity, out Entity putative);

        /// <summary>
        /// Evaluates the fitness of the putative entity.
        /// </summary>
        /// <param name="population">The population.</param>
        /// <param name="bestEntity">The best entity.</param>
        /// <param name="putative">The putative.</param>
        /// <returns>
        /// Boolean <c>true</c> if the putative entity has a better fitness
        /// than the best entity.
        /// </returns>
        protected virtual bool EvaluateFitness(
            Population population, Entity bestEntity, Entity putative)
        {
            return putative.Fitness > bestEntity.Fitness;
        }

        protected virtual void OnUpdateFittest(
            Population population, Entity bestEntity)
        {
        }

        protected virtual void OnTerminateOptimisation(
            Population population, int iterations)
        {
        }
        #endregion
    }
}
