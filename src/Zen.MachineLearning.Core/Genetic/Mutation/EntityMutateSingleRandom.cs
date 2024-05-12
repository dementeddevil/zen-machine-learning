namespace Zen.MachineLearning.Core.Genetic.Mutation
{
    using System;
    using Zen.MachineLearning.Core;
    using Zen.MachineLearning.Core.Genetic;

    public class EntityMutateSingleRandom : EntityMutate
    {
        protected override void OnMutate(Entity father, Entity son)
        {
            if (father == null)
            {
                throw new ArgumentNullException(nameof(father));
            }

            // Determine chromosome and mutation point
            var chromo = RandomFactory.Next(father.Chromosomes.Count);
            var point = RandomFactory.Next(father.Chromosomes[chromo].Count);

            // Clone the father and mutate the child...
            son.Chromosomes[chromo].MutateRandom(point);
        }
    }
}
