// -----------------------------------------------------------------------
// <copyright file="ArraySubsetEnumerator.cs" company="Zen Design Corp">
// Copyright © Zen Design Corp 2010 - 2012
// </copyright>
// -----------------------------------------------------------------------

namespace Zen.MachineLearning.Core.Collections
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    internal class ArraySubsetEnumerator<T> : IEnumerator<T>
        where T : class
    {
        private readonly T[] _array;
        private readonly int _count;
        private int _index = -1;

        public ArraySubsetEnumerator(T[] array, int count)
        {
            _array = array;
            _count = count;
        }

        public T Current
        {
            get
            {
                if (_index < 0 || _index >= _count)
                {
                    return null;
                }
                else
                {
                    return _array[_index];
                }
            }
        }

        public bool MoveNext()
        {
            if (++_index < _count)
            {
                return true;
            }
            return false;
        }

        public void Reset()
        {
            _index = -1;
        }

        #region IDisposable Members
        public void Dispose()
        {
        }
        #endregion

        #region IEnumerator Members
        object IEnumerator.Current => Current;

        #endregion
    }
}
