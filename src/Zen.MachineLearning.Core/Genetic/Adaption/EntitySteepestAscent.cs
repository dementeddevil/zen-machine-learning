using Zen.MachineLearning.Core;

namespace Zen.MachineLearning.Core.Genetic.Adaption
{
    /// <summary>
    /// <b>EntitySteepestAscent</b> serves as a base class for all adaption
    /// handlers based on the Steepest Ascent algorithm.
    /// </summary>
    /// <remarks>
    /// Since this class is derived from <see cref="T:EntityAscentGradient"/>
    /// it requires the entity chromosomes to have a suitable type-converter
    /// for providing a reversable conversion to arrays of double.
    /// </remarks>
    public class EntitySteepestAscent : EntityAscentGradient
    {
        #region Private Fields
        private double[] _bestEntityState;
        private double[] _putativeEntityState;
        private double[] _currentGradient;
        private double _grms;
        private double _currentStepSize;
        #endregion

        #region Public Constructors
        public EntitySteepestAscent(int dimensions, double stepSize)
            : base(dimensions, stepSize)
        {
        }

        public EntitySteepestAscent(int dimensions, double stepSize,
            double alpha, double beta)
            : base(dimensions, stepSize, alpha, beta)
        {
        }
        #endregion

        #region Protected Methods
        protected override void OnInitialiseOptimisation(Population population,
            Entity bestEntity)
        {
            // Initialise state machine
            _currentGradient = new double[Dimensions];
            _putativeEntityState = new double[Dimensions];
            _bestEntityState = new double[Dimensions];

            EntityToDoubleArray(bestEntity, ref _bestEntityState);
            _grms = CalculateGradient(population, bestEntity, _bestEntityState, _currentGradient);
            _currentStepSize = StepSize;
            base.OnInitialiseOptimisation(population, bestEntity);
        }

        protected override void OnOptimiseEntity(Population population, Entity bestEntity, out Entity putative)
        {
            // Adjust parameters for building next best guess
            for (var index = 0; index < Dimensions; ++index)
            {
                _putativeEntityState[index] =
                    _bestEntityState[index] + _currentStepSize * _currentGradient[index];
            }

            // Load the putative entity from the revised data
            putative = population.CreateEntity(true);
            EntityFromDoubleArray(putative, _putativeEntityState);
        }

        protected override bool EvaluateFitness(Population population,
            Entity bestEntity, Entity putative)
        {
            if (bestEntity.Fitness > putative.Fitness)
            {
                // New solution is worse...
                do
                {
                    // Adjust step size and re-optimise
                    _currentStepSize *= Alpha;
                    putative.Dispose();
                    OnOptimiseEntity(population, bestEntity, out putative);
                }
                while (bestEntity.Fitness > putative.Fitness && !_currentStepSize.IsApproximatelyZero());

                if (_currentStepSize.IsApproximatelyZero() && _grms.IsApproximatelyZero())
                {
                    ForceTerminate();
                }
            }
            else
            {
                // New solution is better...
                _currentStepSize *= Beta;
            }
            return true;
        }

        protected override void OnUpdateFittest(Population population, Entity bestEntity)
        {
            // Update state arrays
            var temp = _bestEntityState;
            _bestEntityState = _putativeEntityState;
            _putativeEntityState = temp;

            // Recalculate gradient
            _grms = CalculateGradient(population, bestEntity, _bestEntityState, _currentGradient);
            base.OnUpdateFittest(population, bestEntity);
        }
        #endregion
    }
}
