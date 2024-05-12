namespace Zen.MachineLearning.Core.Genetic
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///	A strongly-typed collection of <see cref="Entity"/> objects.
    /// </summary>
    [Serializable]
    public class EntityArray : List<Entity>, IDisposable
    {
        #region Private Fields
        private bool _disposed;
        #endregion

        #region Public Constructors
        public EntityArray()
        {
        }
        #endregion

        #region Implementation (IDisposable)
        ~EntityArray()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///	Frees managed and unmanaged resources
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    foreach (var obj in this)
                    {
                        try
                        {
                            var disp = (IDisposable)obj;
                            disp?.Dispose();
                        }
                        catch
                        {
                        }
                    }
                    Clear();
                }
                _disposed = true;
            }
        }
        #endregion
    }
}
