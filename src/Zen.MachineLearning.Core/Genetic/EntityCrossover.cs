namespace Zen.MachineLearning.Core.Genetic
{
    using System;

    /// <summary>
    /// Defines a GA entity crossover function.
    /// </summary>
    public interface IEntityCrossover
    {
        /// <summary>
        /// Produces son and daughter entities composed of a mish-mash of
        /// the mother and father chromosome chains.
        /// </summary>
        /// <param name="mother"></param>
        /// <param name="father"></param>
        /// <param name="daughter"></param>
        /// <param name="son"></param>
        void Crossover(Entity mother, Entity father,
            Entity daughter, Entity son);
    }

    /// <summary>
    /// <b>EntityCrossover</b> serves as a base class for all genetic crossover
    /// objects used in the AI class framework.
    /// </summary>
    public abstract class EntityCrossover : IEntityCrossover
    {
        /// <summary>
        /// Creates a new instance of <see cref="T:Zen.Aero.MachineLearning.Genetic.EntityCrossover"/>
        /// </summary>
        protected EntityCrossover()
        {
        }

        /// <summary>
        /// Abstract. Executes the crossover initialisation of son/daughter
        /// entities from the given mother and father entities.
        /// </summary>
        /// <param name="mother"></param>
        /// <param name="father"></param>
        /// <param name="daughter"></param>
        /// <param name="son"></param>
        /// <remarks>
        /// All entities supplied to this method must have chromosome chains
        /// of equal length and specification.
        /// </remarks>
        protected abstract void OnCrossover(Entity mother, Entity father,
            Entity daughter, Entity son);

        void IEntityCrossover.Crossover(Entity mother, Entity father,
            Entity daughter, Entity son)
        {
            // Sanity check
            if (mother.Chromosomes.Count != father.Chromosomes.Count)
            {
                throw new ArgumentException("Father and mother chromosome counts must be equal.");
            }

            OnCrossover(mother, father, daughter, son);
        }
    }
}
