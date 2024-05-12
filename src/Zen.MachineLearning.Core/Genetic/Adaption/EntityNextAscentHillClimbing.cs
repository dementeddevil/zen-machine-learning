using Zen.MachineLearning.Core.Genetic;

namespace Zen.MachineLearning.Core.Genetic.Adaption
{
    /// <summary>
    /// <b>EntityNextAscentHillClimbing</b> serves as a base class for all
    /// Next Ascent Hill Climbing (NAHC) algorithms.
    /// </summary>
    public class EntityNextAscentHillClimbing : EntityAscentHillClimbing
    {
        #region Public Constructors
        public EntityNextAscentHillClimbing()
        {
        }
        #endregion

        #region Protected Methods
        protected override void UpdateHillClimbParameters(Entity entity)
        {
            ++AlleleIndex;
            if (AlleleIndex >= entity.Chromosomes[ChromosomeIndex].Count)
            {
                AlleleIndex = 0;
                ++ChromosomeIndex;
                if (ChromosomeIndex >= entity.Chromosomes.Count)
                {
                    ChromosomeIndex = 0;
                }
            }
        }
        #endregion
    }
}
