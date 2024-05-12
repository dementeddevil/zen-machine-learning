namespace Zen.MachineLearning.Core.Genetic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Collections;

    [Serializable]
    public class Dna : ICollection<IChromosome>, ICloneable, IDisposable
    {
        #region Private Fields
        private OrderedDictionary<string, IChromosome> _lookup;
        #endregion

        #region Public Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Dna"/> class.
        /// </summary>
        public Dna()
        {
            _lookup = new OrderedDictionary<string, IChromosome>(
                StringComparer.InvariantCultureIgnoreCase);
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _lookup.Count;

        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets or sets the <see cref="Chromosome"/> with the specified name.
        /// </summary>
        /// <value></value>
        public IChromosome this[string name]
        {
            get
            {
                return _lookup[name];
            }
            set
            {
                _lookup[name] = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Chromosome"/> with the specified index.
        /// </summary>
        /// <value></value>
        public IChromosome this[int index]
        {
            get
            {
                return _lookup[index];
            }
            set
            {
                _lookup[index] = value;
            }
        }

        /// <summary>
        /// Gets the chromosome names.
        /// </summary>
        /// <value>The chromosome names.</value>
        public ICollection<string> ChromosomeNames => _lookup.Keys;

        /// <summary>
        /// Gets the chromosome values.
        /// </summary>
        /// <value>The chromosome values.</value>
        public ICollection<IChromosome> ChromosomeValues => _lookup.Values;

        #endregion

        #region Public Methods
        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(IChromosome item)
        {
            throw new NotImplementedException(
                "This Add method is not implemented, use variant with name instead.");
        }

        /// <summary>
        /// Adds the specified chromosome with the given name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="chromosome">The chromosome.</param>
        public void Add(string name, IChromosome chromosome)
        {
            if (_lookup.ContainsKey(name))
            {
                throw new ArgumentException(
                    "Named chromosome already added.", nameof(name));
            }
            _lookup.Add(name, chromosome);
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            _lookup.Clear();
        }

        /// <summary>
        /// Determines whether the dna contains the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// <c>true</c> if the dna contains the specified key; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(string key)
        {
            return _lookup.ContainsKey(key);
        }

        /// <summary>
        /// Determines whether the dna contains the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        /// <c>true</c> if the dna contains the specified value; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(IChromosome item)
        {
            return _lookup.Values.First(
                (chromo) => chromo == item) != null;
        }

        /// <summary>
        /// Copies the chromosome values to the specified array starting from
        /// the given index.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(IChromosome[] array, int arrayIndex)
        {
            _lookup.Values.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the key at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public string GetKeyAt(int index)
        {
            return _lookup.GetKeyAt(index);
        }

        /// <summary>
        /// Removes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public bool Remove(IChromosome item)
        {
            foreach (var key in _lookup.Keys)
            {
                if (_lookup[key] == item)
                {
                    _lookup.Remove(key);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Seeds the chromosomes in this instance.
        /// </summary>
        public void Seed()
        {
            Seed(0.5f);
        }

        /// <summary>
        /// Seeds the chromosomes in this instance using the specified
        /// probability factor.
        /// </summary>
        public void Seed(float probability)
        {
            foreach (var chromo in _lookup.Values)
            {
                chromo.Seed(probability);
            }
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public Dna Clone()
        {
            var copy = new Dna();
            foreach (var pair in _lookup)
            {
                copy.Add(pair.Key, (IChromosome)pair.Value.Clone());
            }
            return copy;
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<IChromosome> GetEnumerator()
        {
            return _lookup.Values.GetEnumerator();
        }
        #endregion

        #region ICloneable Members
        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion

        #region IEnumerable Members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region IDisposable Members
        ~Dna()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_lookup != null)
                {
                    foreach (var pair in _lookup)
                    {
                        var disp = pair.Value as IDisposable;
                        if (disp != null)
                        {
                            disp.Dispose();
                        }
                    }
                    _lookup.Clear();
                    _lookup = null;
                }
            }
        }
        #endregion
    }
}
