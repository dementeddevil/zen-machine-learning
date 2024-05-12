namespace Zen.MachineLearning.Core.Genetic.Selection
{
    using System.Diagnostics;
    using Zen.MachineLearning.Core;
    using Zen.MachineLearning.Core.Genetic;

    /// <summary>
    /// Select two implementation that selects two different entities at random.
    /// </summary>
    public class SelectTwoRandom : EntitySelectTwoBase
    {
        protected override bool OnSelectTwo(Population pop, out Entity mother, out Entity father)
        {
            if (pop.OriginalCount < 2)
            {
                mother = null;
                father = null;
                return true;
            }

            var motherIndex = RandomFactory.Next(pop.OriginalCount);
            var fatherIndex = RandomFactory.NextExcept(pop.OriginalCount, motherIndex);
            Debug.Assert(fatherIndex != motherIndex);
            mother = pop.Entities[motherIndex];
            father = pop.Entities[fatherIndex];

            ++State;
            return State > pop.OriginalCount * pop.Settings.CrossoverRatio ? true : false;
        }
    }

    /// <summary>
    /// Select two implementation which selects every entity.
    /// </summary>
    public class SelectTwoEvery : EntitySelectTwoBase
    {
        protected override bool OnSelectTwo(Population pop, out Entity mother, out Entity father)
        {
            mother = null;
            father = null;
            if (State >= pop.OriginalCount * pop.OriginalCount)
            {
                return true;
            }

            mother = pop.Entities[State % pop.OriginalCount];
            father = pop.Entities[State / pop.OriginalCount];

            ++State;
            return false;
        }
    }

    public class SelectTwoRandomRank : EntitySelectTwoBase
    {
        protected override void OnInit()
        {
            State = 1;
        }

        protected override bool OnSelectTwo(Population pop, out Entity mother, out Entity father)
        {
            mother = null;
            father = null;
            if (State >= pop.OriginalCount || pop.OriginalCount < 2)
            {
                return true;
            }

            if (RandomFactory.RandomProb(pop.Settings.CrossoverRatio))
            {
                var fatherIndex = State;
                var motherIndex = RandomFactory.Next(State);
                Debug.Assert(fatherIndex != motherIndex);
                father = pop.Entities[fatherIndex];
                mother = pop.Entities[motherIndex];
            }
            ++State;
            return false;
        }
    }

    public class SelectTwoOfBestTwo : EntitySelectTwoBase
    {
        protected override bool OnSelectTwo(Population pop, out Entity mother, out Entity father)
        {
            if (pop.OriginalCount < 2)
            {
                mother = null;
                father = null;
                return true;
            }

            var motherIndex = RandomFactory.Next(pop.OriginalCount);
            var challengerIndex = RandomFactory.NextExcept(pop.OriginalCount, motherIndex);
            mother = pop.Entities[motherIndex];
            var challenger = pop.Entities[challengerIndex];

            if (challenger.Fitness > mother.Fitness)
            {
                motherIndex = challengerIndex;
                mother = challenger;
            }

            var fatherIndex = RandomFactory.NextExcept(pop.OriginalCount, motherIndex);
            challengerIndex = RandomFactory.NextExcept(pop.OriginalCount, motherIndex, fatherIndex);
            father = pop.Entities[fatherIndex];
            challenger = pop.Entities[challengerIndex];

            if (challenger.Fitness > father.Fitness)
            {
                fatherIndex = challengerIndex;
                father = challenger;
            }

            Debug.Assert(motherIndex != fatherIndex);

            ++State;
            return State > pop.OriginalCount * pop.Settings.CrossoverRatio ? true : false;
        }
    }

    public class SelectTwoRoulette : EntitySelectTwoBase
    {
        #region Private Properties
        private SelectStats Stats
        {
            get;
            set;
        }

        private double TotalExpectancy
        {
            get;
            set;
        }

        private int Marker
        {
            get;
            set;
        }

        private double SelectValue
        {
            get;
            set;
        }
        #endregion

        protected override bool OnSelectTwo(Population pop, out Entity mother, out Entity father)
        {
            mother = null;
            father = null;
            if (pop.OriginalCount < 1)
            {
                return true;
            }

            // Is this the first call for a given generation?
            if (State == 0)
            {
                Stats = GetPopulationStats(pop);
                TotalExpectancy = Stats.Sum / Stats.Average;
                Marker = RandomFactory.Next(pop.OriginalCount);
            }

            SelectValue = RandomFactory.NextDouble() * TotalExpectancy * Stats.Average;
            do
            {
                Marker = (Marker + 1) % pop.OriginalCount;
                SelectValue -= pop.Entities[Marker].Fitness;
            } while (SelectValue > 0.0);
            mother = pop.Entities[Marker];

            SelectValue = RandomFactory.NextDouble() * TotalExpectancy * Stats.Average;
            do
            {
                Marker = (Marker + 1) % pop.OriginalCount;
                SelectValue -= pop.Entities[Marker].Fitness;
            } while (SelectValue > 0.0);
            father = pop.Entities[Marker];

            State++;
            return State > pop.OriginalCount * pop.Settings.CrossoverRatio ? true : false;
        }
    }
}
