namespace Zen.MachineLearning.Core.Collections
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    public class OrderedDictionary<TKey, TValue> : IOrderedDictionary<TKey, TValue>
    {
        private readonly List<TKey> _arrayLookup;
        private readonly ConcurrentDictionary<TKey, TValue> _dictionaryLookup;
        private IEqualityComparer<TKey> _comparer;

        #region Public Constructors
        public OrderedDictionary()
        {
            _arrayLookup = new List<TKey>();
            _dictionaryLookup = new ConcurrentDictionary<TKey, TValue>();
            _comparer = EqualityComparer<TKey>.Default;
        }

        public OrderedDictionary(int capacity)
        {
            _arrayLookup = new List<TKey>(capacity);
            _dictionaryLookup = new ConcurrentDictionary<TKey, TValue>(1, capacity);
            _comparer = EqualityComparer<TKey>.Default;
        }

        public OrderedDictionary(IEqualityComparer<TKey> comparer)
        {
            _arrayLookup = new List<TKey>();
            _dictionaryLookup = new ConcurrentDictionary<TKey, TValue>(comparer);
            _comparer = comparer;
        }

        public OrderedDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            _arrayLookup = new List<TKey>(capacity);
            _dictionaryLookup = new ConcurrentDictionary<TKey, TValue>(1, capacity, comparer);
            _comparer = comparer;
        }
        #endregion

        #region Public Propereties
        public int Count => _arrayLookup.Count;

        public bool IsReadOnly => false;

        public ICollection<TKey> Keys => _arrayLookup;

        public ICollection<TValue> Values
        {
            get
            {
                return this.Select(
(item) => item.Value).ToList().AsReadOnly();
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                return _dictionaryLookup[key];
            }
            set
            {
                // Get the current value and determine the array index
                var current = _dictionaryLookup[key];
                _dictionaryLookup[key] = value;
            }
        }

        public TValue this[int index]
        {
            get
            {
                return _dictionaryLookup[_arrayLookup[index]];
            }
            set
            {
                var key = _arrayLookup[index];
                _dictionaryLookup[key] = value;
            }
        }
        #endregion

        #region Public Methods
        public void Add(TKey key, TValue value)
        {
            if (_dictionaryLookup.TryAdd(key, value))
            {
                _arrayLookup.Add(key);
            }
        }

        public void Clear()
        {
            _dictionaryLookup.Clear();
            _arrayLookup.Clear();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var key in _arrayLookup)
            {
                yield return new KeyValuePair<TKey, TValue>(
                    key, _dictionaryLookup[key]);
            }
        }

        public int GetKeyIndex(TKey key)
        {
            return _arrayLookup.IndexOf(key);
        }

        public TKey GetKeyAt(int index)
        {
            return _arrayLookup[index];
        }

        public TValue GetValueAt(int index)
        {
            return _dictionaryLookup[_arrayLookup[index]];
        }

        public void Insert(int index, TKey key, TValue value)
        {
            if (_dictionaryLookup.TryAdd(key, value))
            {
                _arrayLookup.Insert(index, key);
            }
        }

        public void RemoveAt(int index)
        {
            var key = _arrayLookup[index];
            TValue value;
            if (_dictionaryLookup.TryRemove(key, out value))
            {
                _arrayLookup.Remove(key);
            }
        }

        public bool ContainsKey(TKey key)
        {
            return _dictionaryLookup.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            TValue value;
            if (_dictionaryLookup.TryRemove(key, out value))
            {
                _arrayLookup.Remove(key);
                return true;
            }

            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionaryLookup.TryGetValue(key, out value);
        }
        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)_dictionaryLookup).Contains(item);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_dictionaryLookup).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            var result = _arrayLookup.Remove(item.Key);
            if (result)
            {
                TValue value;
                _dictionaryLookup.TryRemove(item.Key, out value);
            }
            return result;
        }
        #endregion

        #region IEnumerable Members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}
