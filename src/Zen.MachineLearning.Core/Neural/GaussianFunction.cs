// -----------------------------------------------------------------------
// <copyright file="GaussianFunction.cs" company="Microsoft">
// Copyright © Zen Design Corp 2010 - 2012
// </copyright>
// -----------------------------------------------------------------------

namespace Zen.MachineLearning.Core.Neural
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// The gaussian activation function
    /// </summary>
    /// <remarks>
    /// <code>
    /// 
    ///                  1                -(x-mu)^2 / (2 * sigma^2)
    /// f(x)  =  -------------------- *  e
    ///          sqrt(2 * pi * sigma)
    /// 
    /// f'(x)  =  y(x) * -2*K*(x - mu) 
    /// </code>
    /// To implement a more efficient computation :
    /// <code>
    /// C = 1/sqrt(2 * pi * sigma)
    /// K = 1/(2 * sigma^2)
    /// </code>
    /// </remarks>
    [Serializable]
    public class GaussianFunction : IActivationFunction, ISerializable
    {
        /// <summary>
        /// The sigma parameter of the gaussian
        /// </summary>
        private double _sigma = 0.159155f;
        /// <summary>
        /// The mu parameter of the gaussian
        /// </summary>
        private double _mu = 0f;
        /// <summary>
        /// C parameter (usfull for computing function value)
        /// </summary>
        private double _c;
        /// <summary>
        /// C parameter (usfull for computing function value)
        /// </summary>
        private double _k;

        public GaussianFunction()
        {
            ComputeCk();
        }

        public GaussianFunction(float sigma, float mu)
        {
            _sigma = sigma;
            _mu = mu;
            ComputeCk();
        }

        public GaussianFunction(SerializationInfo info, StreamingContext context)
        {
            _sigma = info.GetDouble("sigma");
            _mu = info.GetDouble("mu");
            ComputeCk();
        }

        /// <summary>
        /// Get or set the sigma parameter of the function
        /// (sigma must be positive)
        /// </summary>
        public double Sigma
        {
            get
            {
                return _sigma;
            }
            set
            {
                _sigma = value > 0 ? value : _sigma;
                ComputeCk();
            }
        }
        /// <summary>
        /// Get or set the mu parameter of the function
        /// </summary>
        public double Mu
        {
            get
            {
                return _mu;
            }
            set
            {
                _mu = value;
            }
        }
        /// <summary>
        /// Compute C and K parameters from sigma
        /// </summary>
        private void ComputeCk()
        {
            _c = 1.0 / Math.Sqrt(2.0 * Math.PI * _sigma);
            _k = 1.0 / (2.0 * _sigma * _sigma);
        }

        #region IActivationFunction Members

        public double Function(double x)
        {
            return _c * (float)Math.Exp(-(x - _mu) * (x - _mu) * _k);
        }

        public double Derivative(double x)
        {
            var y = Function(x);
            return -2 * y * _k * (x - _mu);
        }

        public double Derivative2(double y)
        {
            return -2 * y * _k/* * (x - mu)*/;
        }

        #endregion

        #region ISerializable Members
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("sigma", _sigma);
            info.AddValue("mu", _mu);
        }
        #endregion
    }
}
