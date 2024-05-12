namespace Zen.MachineLearning.Core
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    /// <summary>
    /// <b>RandomEx</b> extends <see cref="T:Random"/> by adding a random
    /// probability function and lock-free thread safety.
    /// </summary>
    /// <remarks>
    /// All methods are safe to call from multiple threads.
    /// </remarks>
    public class RandomEx : Random
    {
        #region Private Fields
        private SpinLock _spinLock = new SpinLock(true);
        #endregion

        #region Public Constructors
        public RandomEx()
        {
        }
        #endregion

        #region Public Methods
        public override int Next()
        {
            var lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);
                return base.Next();
            }
            finally
            {
                if (lockTaken)
                {
                    _spinLock.Exit();
                }
            }
        }

        public override int Next(int maxValue)
        {
            var lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);
                return base.Next(maxValue);
            }
            finally
            {
                if (lockTaken)
                {
                    _spinLock.Exit();
                }
            }
        }

        public override int Next(int minValue, int maxValue)
        {
            var lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);
                return base.Next(minValue, maxValue);
            }
            finally
            {
                if (lockTaken)
                {
                    _spinLock.Exit();
                }
            }
        }

        public override void NextBytes(byte[] buffer)
        {
            var lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);
                base.NextBytes(buffer);
            }
            finally
            {
                if (lockTaken)
                {
                    _spinLock.Exit();
                }
            }
        }

        public override double NextDouble()
        {
            var lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);
                return base.NextDouble();
            }
            finally
            {
                if (lockTaken)
                {
                    _spinLock.Exit();
                }
            }
        }

        public virtual int NextExcept(int maxValue, params int[] exceptValues)
        {
            var value = Next(maxValue - exceptValues.Length);
            foreach (var except in exceptValues)
            {
                if (value >= except)
                {
                    ++value;
                }
            }
            return value;
        }

        public virtual int NextRangeExcept(int minValue, int maxValue, params int[] exceptValues)
        {
            var value = Next(maxValue - exceptValues.Length);
            foreach (var except in exceptValues)
            {
                if (value >= except)
                {
                    ++value;
                }
            }
            return value;
        }

        public virtual bool RandomProb(double probability)
        {
            Debug.Assert(probability >= 0.0 && probability <= 1.0);
            return NextDouble() < probability;
        }
        #endregion
    }

    public static class RandomFactory
    {
        private static RandomEx _theInstance;
        private static readonly object SyncRandom = new object();

        public static RandomEx Instance
        {
            get
            {
                if (_theInstance == null)
                {
                    lock (SyncRandom)
                    {
                        if (_theInstance == null)
                        {
                            _theInstance = new RandomEx();
                        }
                    }
                }
                return _theInstance;
            }
        }

        public static int Next()
        {
            return Instance.Next();
        }

        public static int Next(int maxValue)
        {
            return Instance.Next(maxValue);
        }

        public static int Next(int minValue, int maxValue)
        {
            return Instance.Next(minValue, maxValue);
        }

        public static void NextBytes(byte[] buffer)
        {
            Instance.NextBytes(buffer);
        }

        public static double NextDouble()
        {
            return Instance.NextDouble();
        }

        public static int NextExcept(int maxValue, params int[] exceptValues)
        {
            return Instance.NextExcept(maxValue, exceptValues);
        }

        public static int NextRangeExcept(int minValue, int maxValue, params int[] exceptValue)
        {
            return Instance.NextRangeExcept(minValue, maxValue, exceptValue);
        }

        public static bool RandomProb(double probability)
        {
            return Instance.RandomProb(probability);
        }
    }
}
