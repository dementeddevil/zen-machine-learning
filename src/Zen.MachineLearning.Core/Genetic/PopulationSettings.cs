namespace Zen.MachineLearning.Core.Genetic
{
    using System;
    using Crossover;
    using Mutation;
    using Selection;

    public enum GenesisType
    {
        Unknown,
        Random,
        Soup,
        User
    }

    [Flags]
    public enum EvolutionScheme
    {
        None = 0,
        LamarckParents = 1,
        LamarckChildren = 2,
        LamarckAll = 3,
        BaldwinParents = 4,
        BaldwinChildren = 8,
        BaldwinAll = 12,
        Darwin = 16,
    }

    [Flags]
    public enum ElitismType
    {
        None = 0,
        ParentsSurvive = 1,
        OneParentSurvives = 2,
        ParentsDie = 3,
        RescoreParents = 4
    }

    public class PopulationSettings : ICloneable
    {
        public PopulationSettings()
        {
            StableSize = 100;
            MaxGenerations = 100;
            SelectOne = new SelectOneRandom();
            SelectTwo = new SelectTwoRandom();
            Crossover = new EntityCrossoverDoublePoint();
            Mutate = new EntityMutateSingleDrift();
            EvolutionEventInterval = 10;
            MaxAdaptionIterations = 20;
            CrossoverRatio = 0.75;
            MutationRatio = 0.2;
            MigrationRatio = 0.1;
            Genesis = GenesisType.Soup;
            Evolution = EvolutionScheme.Darwin;
            Elitism = ElitismType.None;
        }

        #region Public Properties
        public int StableSize
        {
            get;
            set;
        }

        public int MaxGenerations
        {
            get;
            set;
        }

        public bool SteadyState
        {
            get;
            set;
        }

        public int EvolutionEventInterval
        {
            get;
            set;
        }

        public double CrossoverRatio
        {
            get;
            set;
        }

        public double MutationRatio
        {
            get;
            set;
        }

        public double MigrationRatio
        {
            get;
            set;
        }

        public GenesisType Genesis
        {
            get;
            set;
        }

        public EvolutionScheme Evolution
        {
            get;
            set;
        }

        public ElitismType Elitism
        {
            get;
            set;
        }

        public EventHandler<EntityEventArgs> FitnessHandler
        {
            get;
            set;
        }

        public int MaxAdaptionIterations
        {
            get;
            set;
        }

        public GenerationHandler GenerationHandler
        {
            get;
            set;
        }

        public IEntityAdaption Adaption
        {
            get;
            set;
        }

        public IEntitySelectOne SelectOne
        {
            get;
            set;
        }

        public IEntitySelectTwo SelectTwo
        {
            get;
            set;
        }

        public IEntitySelectOne MigrationSelector
        {
            get;
            set;
        }

        public IEntityMutate Mutate
        {
            get;
            set;
        }

        public IEntityCrossover Crossover
        {
            get;
            set;
        }

        public object UserState
        {
            get;
            set;
        }
        #endregion

        public PopulationSettings Clone()
        {
            return (PopulationSettings)MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }
    }
}
