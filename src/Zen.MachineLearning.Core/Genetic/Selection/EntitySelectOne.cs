namespace Zen.MachineLearning.Core.Genetic.Selection
{
    /// <summary>
    /// Select One implementation which selects an entity at RandomFactory.Instanceom.
    /// </summary>
    public class SelectOneRandom : EntitySelectOneBase
    {
        protected override bool OnSelectOne(Population pop, out Entity mother)
        {
            if (pop.OriginalCount < 1)
            {
                mother = null;
                return true;
            }

            mother = pop.Entities[RandomFactory.Next(pop.OriginalCount)];
            ++State;
            return State > pop.OriginalCount * pop.Settings.MutationRatio ? true : false;
        }
    }

    /// <summary>
    /// Select one implementation which selects every entity.
    /// </summary>
    public class SelectOneEvery : EntitySelectOneBase
    {
        protected override bool OnSelectOne(Population pop, out Entity mother)
        {
            mother = null;
            if (State >= pop.OriginalCount)
            {
                return true;
            }

            mother = pop.Entities[State];
            ++State;
            return false;
        }
    }

    public class SelectOneRandomRank : EntitySelectOneBase
    {
        protected override bool OnSelectOne(Population pop, out Entity mother)
        {
            mother = null;
            if (State > pop.OriginalCount)
            {
                return true;
            }

            if (RandomFactory.RandomProb(pop.Settings.MutationRatio))
            {
                mother = pop.Entities[RandomFactory.Next(State)];
            }
            ++State;
            return false;
        }
    }

    public class SelectOneOfBestTwo : EntitySelectOneBase
    {
        protected override bool OnSelectOne(Population pop, out Entity mother)
        {
            if (pop.OriginalCount < 1)
            {
                mother = null;
                return true;
            }

            mother = pop.Entities[RandomFactory.Next(pop.OriginalCount)];
            var mother2 = pop.Entities[RandomFactory.Next(pop.OriginalCount)];
            if (mother2.Fitness > mother.Fitness)
            {
                mother = mother2;
            }
            ++State;
            return State > pop.OriginalCount * pop.Settings.MutationRatio ? true : false;
        }
    }

    public class SelectOneRoulette : EntitySelectOneBase
    {
        private SelectStats _stats;
        private double _totalExpectancy;
        private int _marker;
        private double _selectValue;

        protected override bool OnSelectOne(Population pop, out Entity mother)
        {
            mother = null;
            if (pop.OriginalCount < 1)
            {
                return true;
            }

            // Is this the first call for a given generation?
            if (State == 0)
            {
                _stats = GetPopulationStats(pop);
                _totalExpectancy = _stats.Sum / _stats.Average;
                _marker = RandomFactory.Next(pop.OriginalCount - 1);
            }

            _selectValue = RandomFactory.NextDouble() * _totalExpectancy * _stats.Average;
            do
            {
                _marker = (_marker + 1) % pop.OriginalCount;
                _selectValue -= pop.Entities[_marker].Fitness;
            } while (_selectValue > 0.0);

            State++;

            mother = pop.Entities[_marker];
            return State > pop.OriginalCount * pop.Settings.MutationRatio ? true : false;
        }
    }
}
