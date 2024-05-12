namespace Zen.MachineLearning.Core.Genetic.Adaption
{
    using System;
    using Zen.MachineLearning.Core;
    using Zen.MachineLearning.Core.Genetic;

    /// <summary>
    /// <b>EntityAnnealingAscent</b> serves as a base class for all
    /// adaption handlers based on the Simulated Annealing algorithm.
    /// </summary>
    public class EntityAnnealingAscent : EntityAdaption
    {
        #region Public Constructors
        /// <summary>
        /// Initialises a new instance of the <see cref="T:EntityAnnealingAscent"/> class.
        /// </summary>
        /// <param name="initialTemperature"></param>
        /// <param name="finalTemperature"></param>
        /// <param name="temperatureStep"></param>
        public EntityAnnealingAscent(double initialTemperature,
            double finalTemperature, double temperatureStep)
            : this(initialTemperature, finalTemperature, temperatureStep, -1,
            AnnealingAcceptance.Linear)
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="T:EntityAnnealingAscent"/> class.
        /// </summary>
        /// <param name="initialTemperature"></param>
        /// <param name="finalTemperature"></param>
        /// <param name="temperatureStep"></param>
        /// <param name="acceptance"></param>
        public EntityAnnealingAscent(double initialTemperature,
            double finalTemperature, double temperatureStep,
            AnnealingAcceptance acceptance)
            : this(initialTemperature, finalTemperature, temperatureStep, -1,
            acceptance)
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="T:EntityAnnealingAscent"/> class.
        /// </summary>
        /// <param name="initialTemperature"></param>
        /// <param name="finalTemperature"></param>
        /// <param name="temperatureStep"></param>
        /// <param name="temperatureFrequency"></param>
        public EntityAnnealingAscent(double initialTemperature,
            double finalTemperature, double temperatureStep,
            int temperatureFrequency)
            : this(initialTemperature, finalTemperature, temperatureStep,
                temperatureFrequency, AnnealingAcceptance.Linear)
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="T:EntityAnnealingAscent"/> class.
        /// </summary>
        /// <param name="initialTemperature"></param>
        /// <param name="finalTemperature"></param>
        /// <param name="temperatureStep"></param>
        /// <param name="temperatureFrequency"></param>
        /// <param name="acceptance"></param>
        public EntityAnnealingAscent(double initialTemperature,
            double finalTemperature, double temperatureStep,
            int temperatureFrequency,
            AnnealingAcceptance acceptance)
        {
            InitialTemperature = initialTemperature;
            FinalTemperature = finalTemperature;
            TemperatureStep = temperatureStep;
            TemperatureFrequency = temperatureFrequency;
            Acceptance = acceptance;
        }
        #endregion

        #region Protected Properties
        protected double InitialTemperature
        {
            get;
            private set;
        }

        protected double FinalTemperature
        {
            get;
            private set;
        }

        protected double CurrentTemperature
        {
            get;
            set;
        }

        protected double TemperatureStep
        {
            get;
            private set;
        }

        protected double TemperatureFrequency
        {
            get;
            private set;
        }

        protected AnnealingAcceptance Acceptance
        {
            get;
            private set;
        }
        #endregion

        #region Protected Methods
        protected override void OnInitialiseOptimisation(
            Population population, Entity bestEntity)
        {
            CurrentTemperature = InitialTemperature;
            base.OnInitialiseOptimisation(population, bestEntity);
        }

        protected override bool OnIteration(int currentIteration, int maxIterations)
        {
            // Calculate new current temperature
            if (TemperatureFrequency == -1)
            {
                CurrentTemperature = InitialTemperature +
                    (double)currentIteration / maxIterations *
                    (FinalTemperature - InitialTemperature);
            }
            else
            {
                if (CurrentTemperature > FinalTemperature &&
                    currentIteration % TemperatureFrequency == 0)
                {
                    CurrentTemperature -= TemperatureStep;
                }
            }

            return base.OnIteration(currentIteration, maxIterations);
        }

        protected override void OnOptimiseEntity(Population population,
            Entity bestEntity, out Entity putative)
        {
            population.MutateInternal(bestEntity, out putative);
        }

        protected override bool EvaluateFitness(Population population,
            Entity bestEntity, Entity putative)
        {
            switch (Acceptance)
            {
                case AnnealingAcceptance.Linear:
                    return bestEntity.Fitness < putative.Fitness + CurrentTemperature;

                case AnnealingAcceptance.Boltzman:
                    return bestEntity.Fitness < putative.Fitness ||
                           RandomFactory.RandomProb(
                               Math.Exp((putative.Fitness - bestEntity.Fitness) /
                                        (MathEx.BoltzmannFactor * CurrentTemperature)));

                default:
                    return base.EvaluateFitness(population, bestEntity, putative);
            }
        }
        #endregion
    }
}
