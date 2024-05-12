namespace Zen.MachineLearning.Core.Genetic
{
    public interface IEntityMutate
    {
        void Mutate(Entity father, Entity son);
    }

    /// <summary>
    /// Summary description for EntityCrossover.
    /// </summary>
    public abstract class EntityMutate : IEntityMutate
    {
        protected EntityMutate()
        {
        }

        protected abstract void OnMutate(Entity father, Entity son);

        void IEntityMutate.Mutate(Entity father, Entity son)
        {
            OnMutate(father, son);
        }
    }
}
