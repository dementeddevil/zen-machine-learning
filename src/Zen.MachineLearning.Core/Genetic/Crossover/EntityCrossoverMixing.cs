namespace Zen.MachineLearning.Core.Genetic.Crossover
{
    using System;
    using Zen.MachineLearning.Core;
    using Zen.MachineLearning.Core.Genetic;

    public class EntityCrossoverMixing : EntityCrossover
    {
        protected override void OnCrossover(Entity mother, Entity father,
            Entity daughter, Entity son)
        {
            // Now mangle entities
            for (var chromo = 0; chromo < mother.Chromosomes.Count; ++chromo)
            {
                // Get chromosomes to cross
                var motherChromo = mother.Chromosomes[chromo];
                var fatherChromo = father.Chromosomes[chromo];

                // Sanity check
                if (motherChromo.Count != fatherChromo.Count)
                {
                    throw new InvalidOperationException($"Mother and father chromosome {chromo} have differing gene lengths.");
                }

                if (RandomFactory.Next(4) > 2)
                {
                    son.Chromosomes[chromo] = (IChromosome)father.Chromosomes[chromo].Clone();
                    daughter.Chromosomes[chromo] = (IChromosome)mother.Chromosomes[chromo].Clone();
                }
                else
                {
                    son.Chromosomes[chromo] = (IChromosome)mother.Chromosomes[chromo].Clone();
                    daughter.Chromosomes[chromo] = (IChromosome)father.Chromosomes[chromo].Clone();
                }
            }
        }
    }
}
