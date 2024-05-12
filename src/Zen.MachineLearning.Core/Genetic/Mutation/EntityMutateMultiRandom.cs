namespace Zen.MachineLearning.Core.Genetic.Mutation
{
    using System;
    using Zen.MachineLearning.Core;
    using Zen.MachineLearning.Core.Genetic;

    public class EntityMutateMultiRandom : EntityMutate
    {
        protected override void OnMutate(Entity father, Entity son)
        {
            if (father == null)
            {
                throw new ArgumentNullException(nameof(father));
            }

            // Clone the father and mutate the child...
            foreach (var chromo in son.Chromosomes)
            {
                for (var point = 0; point < chromo.Count; ++point)
                {
                    switch (RandomFactory.Next(3))
                    {
                        case 0:
                            chromo.MutateDrift(point, MutateDriftMode.Up);
                            break;
                        case 1:
                            chromo.MutateDrift(point, MutateDriftMode.Down);
                            break;
                        case 2:
                            break;
                    }
                }
            }
        }
    }
}
