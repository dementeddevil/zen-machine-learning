namespace Zen.MachineLearning.Core.Genetic
{
    using System;

    public interface IEntitySelectOne
    {
        void Init();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        bool SelectOne(Population pop, out Entity mother);
    }

    public interface IEntitySelectTwo
    {
        void Init();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
        bool SelectTwo(Population pop, out Entity mother, out Entity father);
    }

    public class EntitySelectHelper
    {
        #region Nested Select Statistics class
        public class SelectStats
        {
            private readonly double _average;
            private readonly double _standardDeviation;
            private readonly double _sum;

            public SelectStats(double average, double stddev,
                double sum)
            {
                _average = average;
                _standardDeviation = stddev;
                _sum = sum;
            }

            public double Average => _average;

            public double StandardDeviation => _standardDeviation;

            public double Sum => _sum;
        }
        #endregion

        public static SelectStats GetPopulationStats(Population pop)
        {
            double fsum = 0.0, fsumsq = 0.0;
            for (var index = 0; index < pop.OriginalCount; ++index)
            {
                fsum += pop.Entities[index].Fitness;
                fsumsq += pop.Entities[index].Fitness * pop.Entities[index].Fitness;
            }

            return new SelectStats(
                fsum / pop.OriginalCount,
                (fsumsq - fsum) * (fsum / pop.OriginalCount) / pop.OriginalCount,
                fsum);
        }

        public static double GetPopulationSum(Population pop)
        {
            var fsum = 0.0;
            for (var index = 0; index < pop.OriginalCount; ++index)
            {
                fsum += pop.Entities[index].Fitness;
            }
            return fsum;
        }
    }

    /// <summary>
    /// Summary description for EntitySelect.
    /// </summary>
    public abstract class EntitySelectOneBase : EntitySelectHelper, IEntitySelectOne
    {
        #region Private Members
        private int _state;
        #endregion

        #region Protected Constructors
        protected EntitySelectOneBase()
        {
        }
        #endregion

        #region Protected Properties
        protected int State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
            }
        }
        #endregion

        #region Protected Methods
        protected virtual void OnInit()
        {
            _state = 0;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        protected abstract bool OnSelectOne(Population pop, out Entity mother);
        #endregion

        #region Implementation of IEntitySelectOne
        void IEntitySelectOne.Init()
        {
            OnInit();
        }

        bool IEntitySelectOne.SelectOne(Population pop, out Entity mother)
        {
            if (pop == null)
            {
                throw new ArgumentNullException(nameof(pop));
            }

            return OnSelectOne(pop, out mother);
        }
        #endregion
    }

    /// <summary>
    /// Summary description for EntitySelect.
    /// </summary>
    public abstract class EntitySelectTwoBase : EntitySelectHelper, IEntitySelectTwo
    {
        #region Private Members
        private int _state;
        #endregion

        #region Protected Constructors
        protected EntitySelectTwoBase()
        {
        }
        #endregion

        #region Protected Properties
        protected int State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
            }
        }
        #endregion

        #region Protected Methods
        protected virtual void OnInit()
        {
            _state = 0;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
        protected abstract bool OnSelectTwo(Population pop, out Entity mother, out Entity father);
        #endregion

        #region Implementation of IEntitySelectTwo
        void IEntitySelectTwo.Init()
        {
            OnInit();
        }

        bool IEntitySelectTwo.SelectTwo(Population pop, out Entity mother, out Entity father)
        {
            if (pop == null)
            {
                throw new ArgumentNullException(nameof(pop));
            }

            return OnSelectTwo(pop, out mother, out father);
        }
        #endregion
    }
}
