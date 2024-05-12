namespace Zen.MachineLearning.Core.Genetic.Crossover
{
    using System;
    using Zen.MachineLearning.Core;
    using Zen.MachineLearning.Core.Genetic;

    public class EntityCrossoverSinglePoint : EntityCrossover
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
                var geneLength = motherChromo.Count;

                var location = RandomFactory.Next(geneLength - 2) + 1;
                for (var index = 0; index < geneLength; ++index)
                {
                    if (index < location)
                    {
                        son.Chromosomes[chromo][index] = motherChromo[index];
                        daughter.Chromosomes[chromo][index] = fatherChromo[index];
                    }
                    else
                    {
                        son.Chromosomes[chromo][index] = fatherChromo[index];
                        daughter.Chromosomes[chromo][index] = motherChromo[index];
                    }
                }
            }
        }
    }
}
