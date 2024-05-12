namespace Zen.MachineLearning.Core.Genetic
{
    using System;
    using System.Runtime.Serialization;
    using System.Text;
    using Zen.MachineLearning.Core;

    [Serializable]
    public abstract class LockingChromosome<TGeneType> : Chromosome<TGeneType>
    {
        #region Private Fields
        private bool[] _lockState;
        #endregion

        #region Protected Constructors
        protected LockingChromosome()
        {
        }

        protected LockingChromosome(int length)
            : base(length)
        {
        }

        protected LockingChromosome(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets the gene at the specified index.
        /// </summary>
        /// <value></value>
        public override TGeneType this[int index]
        {
            get
            {
                return base[index];
            }
            set
            {
                if (!_lockState[index])
                {
                    base[index] = value;
                }
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets the lock-state of the chromosome index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public bool GetLockState(int index)
        {
            return _lockState[index];
        }

        /// <summary>
        /// Sets the lock-state of the chromosome index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="isLocked">if set to <c>true</c> [is locked].</param>
        public void SetLockState(int index, bool isLocked)
        {
            _lockState[index] = isLocked;
        }

        /// <summary>
        /// Mutates the drift.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="mode">The mode.</param>
        public sealed override void MutateDrift(int index, MutateDriftMode mode)
        {
            if (!_lockState[index])
            {
                OnMutateDrift(index, mode);
            }
        }

        /// <summary>
        /// Mutates the random.
        /// </summary>
        /// <param name="index">The index.</param>
        public sealed override void MutateRandom(int index)
        {
            if (!_lockState[index])
            {
                OnMutateRandom(index);
            }
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Clones the chromosome.
        /// </summary>
        /// <returns></returns>
        protected override Chromosome<TGeneType> CloneChromosome()
        {
            var clone =
                (LockingChromosome<TGeneType>)base.CloneChromosome();
            clone._lockState = (bool[])_lockState.Clone();
            return clone;
        }

        /// <summary>
        /// Called when [mutate drift].
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="mode">The mode.</param>
        protected abstract void OnMutateDrift(int index, MutateDriftMode mode);

        /// <summary>
        /// Called when [mutate random].
        /// </summary>
        /// <param name="index">The index.</param>
        protected abstract void OnMutateRandom(int index);

        /// <summary>
        /// Sets the length.
        /// </summary>
        /// <param name="newLength">The new length.</param>
        protected override void SetLength(int newLength)
        {
            if (Count != 0)
            {
                var newLockState = new bool[newLength];
                Array.Copy(_lockState, 0, newLockState, 0,
                    Math.Min(Count, newLength));
                _lockState = newLockState;
            }
            else if (newLength > 0)
            {
                _lockState = new bool[newLength];
            }
            else
            {
                _lockState = null;
            }
            base.SetLength(newLength);
        }
        #endregion
    }

    [Serializable]
    public class BoolLockingChromosome : LockingChromosome<bool>
    {
        #region Public Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BoolLockingChromosome"/> class.
        /// </summary>
        public BoolLockingChromosome()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoolLockingChromosome"/> class.
        /// </summary>
        /// <param name="length">The length.</param>
        public BoolLockingChromosome(int length)
            : base(length)
        {
        }
        #endregion

        #region Protected Constructors
        protected BoolLockingChromosome(SerializationInfo info, StreamingContext context)
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
        #endregion

        #region Protected Methods
        /// <summary>
        /// Performs drift mutation on the specified gene.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="mode">The mode.</param>
        protected override void OnMutateDrift(int index, MutateDriftMode mode)
        {
            this[index] = !this[index];
        }

        /// <summary>
        /// Performs random mutation on the specified gene.
        /// </summary>
        /// <param name="index">The index.</param>
        protected override void OnMutateRandom(int index)
        {
            this[index] = RandomFactory.RandomProb(0.5);
        }
        #endregion
    }

    [Serializable]
    public class CharLockingChromosome : LockingChromosome<char>
    {
        #region Public Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CharLockingChromosome"/> class.
        /// </summary>
        public CharLockingChromosome()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CharLockingChromosome"/> class.
        /// </summary>
        /// <param name="length">The length.</param>
        public CharLockingChromosome(int length)
            : base(length)
        {
        }
        #endregion

        #region Protected Constructors
        protected CharLockingChromosome(SerializationInfo info, StreamingContext context)
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

        #region Protected Methods
        /// <summary>
        /// Performs drift mutation on the specified gene.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="mode">The mode.</param>
        protected override void OnMutateDrift(int index, MutateDriftMode mode)
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
        protected override void OnMutateRandom(int index)
        {
            this[index] = (char)(RandomFactory.Next('~' - ' ') + ' ');
        }
        #endregion
    }

    [Serializable]
    public class ShortLockingChromosome : LockingChromosome<short>
    {
        #region Private Fields
        private short _minValue = short.MinValue;
        private short _maxValue = short.MaxValue;
        #endregion

        #region Public Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ShortLockingChromosome"/> class.
        /// </summary>
        public ShortLockingChromosome()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShortLockingChromosome"/> class.
        /// </summary>
        /// <param name="length">The length.</param>
        public ShortLockingChromosome(int length)
            : base(length)
        {
        }
        #endregion

        #region Protected Constructors
        protected ShortLockingChromosome(SerializationInfo info, StreamingContext context)
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

        #region Protected Methods
        /// <summary>
        /// Performs drift mutation on the specified gene.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="mode">The mode.</param>
        protected override void OnMutateDrift(int index, MutateDriftMode mode)
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
        protected override void OnMutateRandom(int index)
        {
            this[index] = (short)(RandomFactory.Next(MaxValue - MinValue + 1) + MinValue);
        }
        #endregion
    }

    [Serializable]
    public class IntLockingChromosome : LockingChromosome<int>
    {
        #region Private Fields
        private int _minValue = int.MinValue;
        private int _maxValue = int.MaxValue;
        #endregion

        #region Public Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="IntLockingChromosome"/> class.
        /// </summary>
        public IntLockingChromosome()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntLockingChromosome"/> class.
        /// </summary>
        /// <param name="length">The length.</param>
        public IntLockingChromosome(int length)
            : base(length)
        {
        }
        #endregion

        #region Protected Constructors
        protected IntLockingChromosome(SerializationInfo info, StreamingContext context)
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

        public override string ToString()
        {
            return ChromosomeInternal.ToString();
        }
        #endregion

        #region Protected Methods
        protected override void OnMutateDrift(int index, MutateDriftMode mode)
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

        protected override void OnMutateRandom(int index)
        {
            this[index] = RandomFactory.Next(MaxValue - MinValue + 1) + MinValue;
        }
        #endregion
    }

    [Serializable]
    public class DoubleLockingChromosome : LockingChromosome<double>
    {
        #region Private Members
        private double _minValue = -9.0;
        private double _maxValue = 9.0;
        private double _drift = 0.1;
        #endregion

        #region Public Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleLockingChromosome"/> class.
        /// </summary>
        public DoubleLockingChromosome()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleLockingChromosome"/> class.
        /// </summary>
        /// <param name="length">The length.</param>
        public DoubleLockingChromosome(int length)
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
        protected DoubleLockingChromosome(SerializationInfo info, StreamingContext context)
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

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("MinValue", _minValue);
            info.AddValue("MaxValue", _maxValue);
            info.AddValue("Drift", _drift);
        }
        #endregion

        #region Protected Methods
        protected override void OnMutateDrift(int index, MutateDriftMode mode)
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

        protected override void OnMutateRandom(int index)
        {
            this[index] = RandomFactory.NextDouble() * (MaxValue - MinValue) + MinValue;
        }

        protected override double GetBoundedValue(double value)
        {
            return Math.Max(MinValue, Math.Min(MaxValue, value));
        }
        #endregion
    }
}
