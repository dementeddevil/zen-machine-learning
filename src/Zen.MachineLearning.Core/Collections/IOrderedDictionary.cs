namespace Zen.MachineLearning.Core.Collections
{
    using System.Collections.Generic;

    public interface IOrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        TValue this[int index]
        {
            get;
            set;
        }

        TKey GetKeyAt(int index);
        TValue GetValueAt(int index);
        void Insert(int index, TKey key, TValue value);
        void RemoveAt(int index);
    }
}
