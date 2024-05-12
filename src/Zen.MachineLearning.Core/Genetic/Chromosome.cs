namespace Zen.MachineLearning.Core.Genetic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using Zen.MachineLearning.Core;

    [Serializable]
    public abstract class Chromosome : IChromosome, ISerializable, IEquatable<Chromosome>
    {
        #region Private Members
        private int _length;
        private object[] _chromosome;
        #endregion

        #region Protected Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Chromosome"/> class.
        /// </summary>
        protected Chromosome()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Chromosome"/> class.
        /// </summary>
        /// <param name="length">The length.</param>
        protected Chromosome(int length)
        {
            SetLength(length);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Chromosome"/> class.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        protected Chromosome(SerializationInfo info, StreamingContext context)
        {
            _length = info.GetInt32("Length");
            _chromosome = (object[])info.GetValue("Chromosome", typeof(object[]));
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.ICollection"/>.
        /// </summary>
        /// <value></value>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.ICollection"/>.</returns>
        public int Count => _length;

        /// <summary>
        /// Gets the type of the gene.
        /// </summary>
        /// <value>The type of the gene.</value>
        public abstract Type GeneType
        {
            get;
        }

        /// <summary>
        /// Gets or sets the <see cref="object"/> at the specified index.
        /// </summary>
        /// <value></value>
        public virtual object this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                return _chromosome[index];
            }
            set
            {
                if (index < 0 || index >= Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                _chromosome[index] = GetBoundedValue(value);
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Seeds this instance.
        /// </summary>
        public virtual void Seed()
        {
            Seed(0.5f);
        }

        /// <summary>
        /// Seeds the specified probability.
        /// </summary>
        /// <param name="probability">The probability.</param>
        public abstract void Seed(float probability);

        /// <summary>
        /// Performs drift mutation on the specified gene.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="mode">The mode.</param>
        public abstract void MutateDrift(int index, MutateDriftMode mode);

        /// <summary>
        /// Performs random mutation on the specified gene.
        /// </summary>
        /// <param name="index">The index.</param>
        public abstract void MutateRandom(int index);

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is Chromosome)
            {
                return Equals((Chromosome)obj);
            }
            return base.Equals(obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            var hashCode = base.GetHashCode();

            // Hash code is based on chromosome contents
            for (var index = 0; index < Count; ++index)
            {
                hashCode |= hashCode << 5 | this[index].GetHashCode();
            }
            return hashCode;
        }

        /// <summary>
        /// Gets the inner enumerator.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator GetEnumerator()
        {
            return _chromosome.GetEnumerator();
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Gets the bounded value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        protected virtual object GetBoundedValue(object value)
        {
            return value;
        }
        #endregion

        #region ICloneable Members
        public Chromosome Clone()
        {
            return CloneChromosome();
        }
        object ICloneable.Clone()
        {
            return Clone();
        }

        protected virtual Chromosome CloneChromosome()
        {
            // Clone chromosome and ensure clone is unsited
            return (Chromosome)MemberwiseClone();
        }
        #endregion

        #region ISerializable Members
        /// <summary>
        /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization.</param>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Length", _length);
        }

        /// <summary>
        /// Sets the length.
        /// </summary>
        /// <param name="newLength">The new length.</param>
        protected virtual void SetLength(int newLength)
        {
            if (Count != 0)
            {
                var newChromo = new object[newLength];
                Array.Copy(_chromosome, 0, newChromo, 0,
                    Math.Min(Count, newLength));
                _chromosome = newChromo;
            }
            else if (newLength > 0)
            {
                _chromosome = new object[newLength];
            }
            else
            {
                _chromosome = null;
            }

            // Cache the new length
            _length = newLength;
        }
        #endregion

        #region IEquatable<Chromosome> Members
        /// <summary>
        /// Determines whether this chromosome is the same as the specified 
        /// object.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public bool Equals(Chromosome other)
        {
            for (var index = 0; index < Count; ++index)
            {
                if (this[index] != other[index])
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region ICollection Members
        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotSupportedException();
        }

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => null;

        #endregion
    }

    [Serializable]
    public abstract class Chromosome<TGeneType> : IChromosome<TGeneType>, ISerializable, IEquatable<Chromosome<TGeneType>>
    {
        #region Private Members
        private int _length;
        private TGeneType[] _chromosome;
        #endregion

        #region Protected Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ChromosomeBase&lt;TGeneType&gt;"/> class.
        /// </summary>
        protected Chromosome()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChromosomeBase&lt;TGeneType&gt;"/> class.
        /// </summary>
        /// <param name="length">The length.</param>
        protected Chromosome(int length)
        {
            SetLength(length);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Chromosome"/> class.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        protected Chromosome(SerializationInfo info, StreamingContext context)
        {
            _length = info.GetInt32("Length");
            _chromosome = (TGeneType[])info.GetValue("Chromosome", typeof(TGeneType[]));
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets the length.
        /// </summary>
        /// <value>The length.</value>
        public int Count
        {
            get
            {
                return _length;
            }
            set
            {
                if (_length != value)
                {
                    SetLength(value);
                }
            }
        }

        /// <summary>
        /// Gets the type of the gene.
        /// </summary>
        /// <value>The type of the gene.</value>
        public Type GeneType => typeof(TGeneType);

        /// <summary>
        /// Gets or sets the <see cref="object"/> at the specified index.
        /// </summary>
        /// <value></value>
        public virtual TGeneType this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                return _chromosome[index];
            }
            set
            {
                if (index < 0 || index >= Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                _chromosome[index] = GetBoundedValue(value);
            }
        }
        #endregion

        #region Protected Properties
        /// <summary>
        /// Gets the internal chromosome array.
        /// </summary>
        /// <value>The chromosome internal.</value>
        protected TGeneType[] ChromosomeInternal => _chromosome;

        #endregion

        #region Public Methods
        /// <summary>
        /// Determines whether the specified item is contained within this instance.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        /// 	<c>true</c> if [contains] [the specified item]; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool Contains(TGeneType item)
        {
            for (var index = 0; index < Count; ++index)
            {
                if (_chromosome[index].Equals(item))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Seeds this instance.
        /// </summary>
        public virtual void Seed()
        {
            Seed(0.5f);
        }

        /// <summary>
        /// Seeds the specified probability.
        /// </summary>
        /// <param name="probability">The probability.</param>
        public abstract void Seed(float probability);

        /// <summary>
        /// Performs drift mutation on the specified gene.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="mode">The mode.</param>
        public abstract void MutateDrift(int index, MutateDriftMode mode);

        /// <summary>
        /// Performs random mutation on the specified gene.
        /// </summary>
        /// <param name="index">The index.</param>
        public abstract void MutateRandom(int index);

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is Chromosome)
            {
                return Equals((Chromosome)obj);
            }
            return base.Equals(obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            var hashCode = base.GetHashCode();

            // Hash code is based on chromosome contents
            for (var index = 0; index < Count; ++index)
            {
                hashCode |= hashCode << 5 | this[index].GetHashCode();
            }
            return hashCode;
        }

        /// <summary>
        /// Gets the inner enumerator.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator<TGeneType> GetEnumerator()
        {
            return _chromosome.Cast<TGeneType>().GetEnumerator();
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Gets the bounded value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        protected virtual TGeneType GetBoundedValue(TGeneType value)
        {
            return value;
        }
        #endregion

        #region ICloneable Members
        public Chromosome<TGeneType> Clone()
        {
            return CloneChromosome();
        }
        object ICloneable.Clone()
        {
            return Clone();
        }

        protected virtual Chromosome<TGeneType> CloneChromosome()
        {
            // Clone chromosome and ensure clone is unsited
            var clone =
                (Chromosome<TGeneType>)MemberwiseClone();
            clone._chromosome = new TGeneType[Count];
            for (var index = 0; index < Count; ++index)
            {
                clone._chromosome[index] = _chromosome[index];
            }
            return clone;
        }
        #endregion

        #region ISerializable Members
        /// <summary>
        /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization.</param>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Length", _length);
            info.AddValue("Chromosome", _chromosome, typeof(TGeneType[]));
        }

        /// <summary>
        /// Sets the length.
        /// </summary>
        /// <param name="newLength">The new length.</param>
        protected virtual void SetLength(int newLength)
        {
            if (Count != 0)
            {
                var newChromo = new TGeneType[newLength];
                Array.Copy(_chromosome, 0, newChromo, 0,
                    Math.Min(Count, newLength));
                _chromosome = newChromo;
            }
            else if (newLength > 0)
            {
                _chromosome = new TGeneType[newLength];
            }
            else
            {
                _chromosome = null;
            }

            // Cache the new length
            _length = newLength;
        }
        #endregion

        #region IEquatable<Chromosome<TGeneType>> Members
        /// <summary>
        /// Determines whether this chromosome is the same as the specified 
        /// object.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public bool Equals(Chromosome<TGeneType> other)
        {
            for (var index = 0; index < Count; ++index)
            {
                if (!this[index].Equals(other[index]))
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region ICollection Members
        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotSupportedException();
        }

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => null;

        #endregion

        #region ICollection<TGeneType> Members
        void ICollection<TGeneType>.Add(TGeneType item)
        {
            throw new NotImplementedException();
        }

        void ICollection<TGeneType>.Clear()
        {
            throw new NotImplementedException();
        }

        void ICollection<TGeneType>.CopyTo(TGeneType[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        bool ICollection<TGeneType>.IsReadOnly => false;

        bool ICollection<TGeneType>.Remove(TGeneType item)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region IChromosome Members
        object IChromosome.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this[index] = (TGeneType)value;
            }
        }
        #endregion
    }

    [Serializable]
    public class BoolChromosome : Chromosome<bool>
    {
        #region Public Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BoolChromosome"/> class.
        /// </summary>
        public BoolChromosome()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoolChromosome"/> class.
        /// </summary>
        /// <param name="length">The length.</param>
        public BoolChromosome(int length)
            : base(length)
        {
        }
        #endregion

        #region Protected Constructors
        protected BoolChromosome(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Seeds the specified probability.
        /// </summary>
        /// <param name="probability">The probability.</param>
        public override void Seed(float probability)
        {
            if (probability < 0.0f || probability > 1.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(probability),
                    "Seeding probability must be between 0.0 and 1.0");
            }

            for (var index = 0; index < Count; ++index)
            {
                this[index] = RandomFactory.RandomProb(probability);
            }
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var temp = new StringBuilder(Count);
            for (var index = 0; index < Count; ++index)
            {
                temp.Append(this[index] ? "1" : "0");
            }
            return temp.ToString();
        }

        /// <summary>
        /// Performs drift mutation on the specified gene.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="mode">The mode.</param>
        public override void MutateDrift(int index, MutateDriftMode mode)
        {
            this[index] = !this[index];
        }

        /// <summary>
        /// Performs random mutation on the specified gene.
        /// </summary>
        /// <param name="index">The index.</param>
        public override void MutateRandom(int index)
        {
            this[index] = RandomFactory.RandomProb(0.5);
        }
        #endregion
    }

    [Serializable]
    public class CharChromosome : Chromosome<char>
    {
        #region Public Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CharChromosome"/> class.
        /// </summary>
        public CharChromosome()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CharChromosome"/> class.
        /// </summary>
        /// <param name="length">The length.</param>
        public CharChromosome(int length)
            : base(length)
        {
        }
        #endregion

        #region Protected Constructors
        protected CharChromosome(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Seeds the specified probability.
        /// </summary>
        /// <param name="probability">The probability.</param>
        public override void Seed(float probability)
        {
            for (var index = 0; index < Count; ++index)
            {
                this[index] = (char)(RandomFactory.Next('~' - ' ') + ' ');
            }
        }

        /// <summary>
        /// Performs drift mutation on the specified gene.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="mode">The mode.</param>
        public override void MutateDrift(int index, MutateDriftMode mode)
        {
            var c = this[index];
            switch (mode)
            {
                case MutateDriftMode.Up:
                    ++c;
                    if (c > '~')
                    {
                        c = ' ';
                    }
                    break;
                case MutateDriftMode.Down:
                    --c;
                    if (c < ' ')
                    {
                        c = '~';
                    }
                    break;
            }
            this[index] = c;
        }

        /// <summary>
        /// Performs random mutation on the specified gene.
        /// </summary>
        /// <param name="index">The index.</param>
        public override void MutateRandom(int index)
        {
            this[index] = (char)(RandomFactory.Next('~' - ' ') + ' ');
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return new string(ChromosomeInternal);
        }
        #endregion
    }

    [Serializable]
    public class ShortChromosome : Chromosome<short>
    {
        #region Private Fields
        private short _minValue = short.MinValue;
        private short _maxValue = short.MaxValue;
        #endregion

        #region Public Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ShortChromosome"/> class.
        /// </summary>
        public ShortChromosome()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShortChromosome"/> class.
        /// </summary>
        /// <param name="length">The length.</param>
        public ShortChromosome(int length)
            : base(length)
        {
        }
        #endregion

        #region Protected Constructors
        protected ShortChromosome(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets the min value.
        /// </summary>
        /// <value>The min value.</value>
        public short MinValue
        {
            get
            {
                return _minValue;
            }
            set
            {
                _minValue = value;
            }
        }

        /// <summary>
        /// Gets or sets the max value.
        /// </summary>
        /// <value>The max value.</value>
        public short MaxValue
        {
            get
            {
                return _maxValue;
            }
            set
            {
                _maxValue = value;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Seeds the specified probability.
        /// </summary>
        /// <param name="probability">The probability.</param>
        public override void Seed(float probability)
        {
            for (var index = 0; index < Count; ++index)
            {
                this[index] = (short)(RandomFactory.Next(MaxValue - MinValue + 1) + MinValue);
            }
        }

        /// <summary>
        /// Performs drift mutation on the specified gene.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="mode">The mode.</param>
        public override void MutateDrift(int index, MutateDriftMode mode)
        {
            var c = this[index];
            switch (mode)
            {
                case MutateDriftMode.Up:
                    ++c;
                    if (c > MaxValue)
                    {
                        c = MinValue;
                    }
                    break;
                case MutateDriftMode.Down:
                    --c;
                    if (c < MinValue)
                    {
                        c = MaxValue;
                    }
                    break;
            }
            this[index] = c;
        }

        /// <summary>
        /// Performs random mutation on the specified gene.
        /// </summary>
        /// <param name="index">The index.</param>
        public override void MutateRandom(int index)
        {
            this[index] = (short)(RandomFactory.Next(MaxValue - MinValue + 1) + MinValue);
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ChromosomeInternal.ToString();
        }
        #endregion
    }

    [Serializable]
    public class IntChromosome : Chromosome<int>
    {
        #region Private Fields
        private int _minValue = int.MinValue;
        private int _maxValue = int.MaxValue;
        #endregion

        #region Public Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="IntChromosome"/> class.
        /// </summary>
        public IntChromosome()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntChromosome"/> class.
        /// </summary>
        /// <param name="length">The length.</param>
        public IntChromosome(int length)
            : base(length)
        {
        }
        #endregion

        #region Protected Constructors
        protected IntChromosome(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion

        #region Public Properties
        public int MinValue
        {
            get
            {
                return _minValue;
            }
            set
            {
                _minValue = value;
            }
        }

        public int MaxValue
        {
            get
            {
                return _maxValue;
            }
            set
            {
                _maxValue = value;
            }
        }
        #endregion

        #region Public Methods
        public override void Seed(float probability)
        {
            for (var index = 0; index < Count; ++index)
            {
                this[index] = RandomFactory.Next(MaxValue - MinValue + 1) + MinValue;
            }
        }

        public override void MutateDrift(int index, MutateDriftMode mode)
        {
            var c = this[index];
            switch (mode)
            {
                case MutateDriftMode.Up:
                    ++c;
                    if (c > MaxValue)
                    {
                        c = MinValue;
                    }
                    break;
                case MutateDriftMode.Down:
                    --c;
                    if (c < MinValue)
                    {
                        c = MaxValue;
                    }
                    break;
            }
            this[index] = c;
        }

        public override void MutateRandom(int index)
        {
            this[index] = RandomFactory.Next(MaxValue - MinValue + 1) + MinValue;
        }

        public override string ToString()
        {
            return ChromosomeInternal.ToString();
        }
        #endregion
    }

    [Serializable]
    public class DoubleChromosome : Chromosome<double>
    {
        #region Private Members
        private double _minValue = -9.0;
        private double _maxValue = 9.0;
        private double _drift = 0.1;
        #endregion

        #region Public Constructors
        public DoubleChromosome()
        {
        }

        public DoubleChromosome(int length)
            : base(length)
        {
        }
        #endregion

        #region Protected Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Chromosome"/> class.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        protected DoubleChromosome(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _minValue = info.GetDouble("MinValue");
            _maxValue = info.GetDouble("MaxValue");
            _drift = info.GetDouble("Drift");
        }
        #endregion

        #region Public Properties
        public double MinValue
        {
            get
            {
                return _minValue;
            }
            set
            {
                _minValue = value;
            }
        }

        public double MaxValue
        {
            get
            {
                return _maxValue;
            }
            set
            {
                _maxValue = value;
            }
        }

        public double Drift
        {
            get
            {
                return _drift;
            }
            set
            {
                _drift = value;
            }
        }
        #endregion

        #region Public Methods
        public override void Seed(float probability)
        {
            for (var index = 0; index < Count; ++index)
            {
                this[index] = RandomFactory.NextDouble() * (MaxValue - MinValue) + MinValue;
            }
        }

        public override void MutateDrift(int index, MutateDriftMode mode)
        {
            switch (mode)
            {
                case MutateDriftMode.Up:
                    this[index] = this[index] + _drift;
                    break;
                case MutateDriftMode.Down:
                    this[index] = this[index] - _drift;
                    break;
            }
        }

        public override void MutateRandom(int index)
        {
            this[index] = RandomFactory.NextDouble() * (MaxValue - MinValue) + MinValue;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("MinValue", _minValue);
            info.AddValue("MaxValue", _maxValue);
            info.AddValue("Drift", _drift);
        }
        #endregion

        #region Protected Methods
        protected override double GetBoundedValue(double value)
        {
            return Math.Max(MinValue, Math.Min(MaxValue, value));
        }
        #endregion
    }
}
