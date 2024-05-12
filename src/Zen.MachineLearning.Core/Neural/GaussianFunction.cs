// -----------------------------------------------------------------------
// <copyright file="GaussianFunction.cs" company="Microsoft">
// Copyright © Zen Design Corp 2010 - 2012
// </copyright>
// -----------------------------------------------------------------------

using System.Numerics;
using System.Runtime.Serialization;

namespace Zen.MachineLearning.Core.Neural;

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
public class GaussianFunction<T> : IActivationFunction<T>, ISerializable
    where T : IFloatingPoint<T>
{
    /// <summary>
    /// The sigma parameter of the gaussian
    /// </summary>
    private T _sigma = T.CreateChecked(0.159155f);
    /// <summary>
    /// The mu parameter of the gaussian
    /// </summary>
    private T _mu = T.CreateChecked(0f);
    /// <summary>
    /// C parameter (usfull for computing function value)
    /// </summary>
    private T _c;
    /// <summary>
    /// C parameter (usfull for computing function value)
    /// </summary>
    private T _k;

    public GaussianFunction()
    {
        ComputeCk();
    }

    public GaussianFunction(T sigma, T mu)
    {
        _sigma = sigma;
        _mu = mu;
        ComputeCk();
    }

    public GaussianFunction(SerializationInfo info, StreamingContext context)
    {
        _sigma = T.CreateChecked(info.GetDouble("sigma"));
        _mu = T.CreateChecked(info.GetDouble("mu"));
        ComputeCk();
    }

    /// <summary>
    /// Get or set the sigma parameter of the function
    /// (sigma must be positive)
    /// </summary>
    public T Sigma
    {
        get
        {
            return _sigma;
        }
        set
        {
            if (value > T.CreateChecked(0.0))
            {
                _sigma = T.CreateChecked(value);
            }
            ComputeCk();
        }
    }
    /// <summary>
    /// Get or set the mu parameter of the function
    /// </summary>
    public T Mu
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
        _c = T.CreateChecked(1.0) / T.CreateChecked(Math.Sqrt(2.0 * Math.PI * Double.CreateChecked(_sigma)));
        _k = T.CreateChecked(1.0) / (T.CreateChecked(2.0) * _sigma * _sigma);
    }

    #region IActivationFunction Members

    public T Function(T x)
    {
        return _c * T.CreateChecked(Math.Exp(double.CreateChecked(-(x - _mu) * (x - _mu) * _k)));
    }

    public T Derivative(T x)
    {
        var y = Function(x);
        return T.CreateChecked(-2.0) * y * _k * (x - _mu);
    }

    public T Derivative2(T y)
    {
        return T.CreateChecked(-2.0) * y * _k/* * (x - mu)*/;
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
