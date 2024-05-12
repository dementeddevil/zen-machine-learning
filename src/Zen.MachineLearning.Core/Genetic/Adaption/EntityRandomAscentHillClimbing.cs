using Zen.MachineLearning.Core;
using Zen.MachineLearning.Core.Genetic;

namespace Zen.MachineLearning.Core.Genetic.Adaption
{
    /// <summary>
    /// <b>EntityRandomAscentHillClimbing</b> serves as a base class for
    /// all Random Ascent Hill Climbing (RAHC) algorithms.
    /// </summary>
    public class EntityRandomAscentHillClimbing : EntityAscentHillClimbing
    {
        #region Public Constructors
        public EntityRandomAscentHillClimbing()
        {
        }
        #endregion

        #region Protected Methods
        protected override void UpdateHillClimbParameters(Entity entity)
        {
            ChromosomeIndex = RandomFactory.Next(entity.Chromosomes.Count);
            AlleleIndex = RandomFactory.Next(entity.Chromosomes[ChromosomeIndex].Count);
        }
        #endregion
    }
}
