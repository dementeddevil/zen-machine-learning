namespace Zen.MachineLearning.Core.Genetic.Adaption
{
    /// <summary>
    /// <b>EntityAscentGradient</b> serves as a base class for all
    /// Ascent Gradient algorithms.
    /// </summary>
    public abstract class EntityAscentGradient : EntityAdaption
    {
        #region Private Fields
        private readonly int _dimensions;
        private double _stepSize;
        private double _alpha = 0.5f;   // Step size scale down factor
        private double _beta = 1.2f;    // Step size scale up factor
        private bool _forceTerminate;
        #endregion

        #region Protected Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityAscentGradient"/> class.
        /// </summary>
        /// <param name="dimensions">The dimensions.</param>
        /// <param name="stepSize">Size of the step.</param>
        protected EntityAscentGradient(int dimensions, double stepSize)
        {
            _dimensions = dimensions;
            _stepSize = stepSize;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityAscentGradient"/> class.
        /// </summary>
        /// <param name="dimensions">The dimensions.</param>
        /// <param name="stepSize">Size of the step.</param>
        /// <param name="alpha">The alpha.</param>
        /// <param name="beta">The beta.</param>
        protected EntityAscentGradient(
            int dimensions, double stepSize, double alpha, double beta)
        {
            _dimensions = dimensions;
            _stepSize = stepSize;
            _alpha = alpha;
            _beta = beta;
        }
        #endregion

        #region Protected Properties
        protected int Dimensions => _dimensions;

        /// <summary>
        /// Gets/sets the step-size scale down factor.
        /// </summary>
        protected double Alpha
        {
            get
            {
                return _alpha;
            }
            set
            {
                _alpha = value;
            }
        }

        /// <summary>
        /// Gets/sets the step-size scale up factor.
        /// </summary>
        protected double Beta
        {
            get
            {
                return _beta;
            }
            set
            {
                _beta = value;
            }
        }

        public double StepSize
        {
            get
            {
                return _stepSize;
            }
            set
            {
                _stepSize = value;
            }
        }
        #endregion

        #region Protected Methods
        protected void ForceTerminate()
        {
            _forceTerminate = true;
        }

        protected override void OnInitialiseOptimisation(Population population,
            Entity bestEntity)
        {
            _forceTerminate = false;
            base.OnInitialiseOptimisation(population, bestEntity);
        }

        protected override bool OnIteration(int currentIteration, int maxIterations)
        {
            if (_forceTerminate)
            {
                return false;
            }
            return base.OnIteration(currentIteration, maxIterations);
        }

        protected virtual void EntityToDoubleArray(Entity entity, ref double[] array)
        {
            var arrayIndex = 0;
            for (var index = 0; index < entity.Chromosomes.Count; ++index)
            {
                var chromo = (DoubleChromosome)entity.Chromosomes[index];
                for (var allele = 0; allele < chromo.Count; ++allele)
                {
                    array[arrayIndex++] = chromo[allele];
                }
            }
        }

        protected virtual void EntityFromDoubleArray(Entity entity, double[] array)
        {
            var arrayIndex = 0;
            for (var index = 0; index < entity.Chromosomes.Count; ++index)
            {
                var chromo = (DoubleChromosome)entity.Chromosomes[index];
                for (var allele = 0; allele < chromo.Count; ++allele)
                {
                    chromo[allele] = array[arrayIndex++];
                }
            }
        }

        protected virtual double CalculateGradient(Population population,
            Entity bestEntity, double[] solution, double[] gradient)
        {
            //for (
            return 0;
        }
        #endregion
    }
}
