namespace Zen.MachineLearning.Core.Genetic.Mutation
{
    using System;
    using Zen.MachineLearning.Core;
    using Zen.MachineLearning.Core.Genetic;

    public class EntityMutateMultiDrift : EntityMutate
    {
        protected override void OnMutate(Entity father, Entity son)
        {
            if (father == null)
            {
                throw new ArgumentNullException(nameof(father));
            }

            // Determine chromosome and mutation point
            var drift = MutateDriftMode.Up;
            if (RandomFactory.Next(2) == 1)
            {
                drift = MutateDriftMode.Down;
            }

            // Clone the father and mutate the child...
            foreach (var chromo in son.Chromosomes)
            {
                for (var point = 0; point < chromo.Count; ++point)
                {
                    if (RandomFactory.RandomProb(0.47))
                    {
                        chromo.MutateDrift(point, drift);
                    }
                }
            }
        }
    }
}
