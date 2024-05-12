namespace Zen.MachineLearning.Core.Genetic.Mutation
{
    using System;
    using Zen.MachineLearning.Core;
    using Zen.MachineLearning.Core.Genetic;

    public class EntityMutateSingleDrift : EntityMutate
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
            var drift = MutateDriftMode.Up;
            if (RandomFactory.Next(2) == 1)
            {
                drift = MutateDriftMode.Down;
            }

            // Clone the father and mutate the child...
            son.Chromosomes[chromo].MutateDrift(point, drift);
        }
    }
}
