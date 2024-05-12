using System.Numerics;

namespace Zen.MachineLearning.Core.Neural;

/// <summary>
/// Threshold activation function.
/// </summary>
///
/// <remarks><para>The class represents threshold activation function with
/// the next expression:
/// <code lang="none">
/// f(x) = 1, if x >= 0, otherwise 0
/// </code>
/// </para>
/// 
/// <para>Output range of the function: <b>[0, 1]</b>.</para>
/// 
/// <para>Functions graph:</para>
/// <img src="img/neuro/threshold.bmp" width="242" height="172" />
/// </remarks>
///
[Serializable]
public class ThresholdFunction<T> : IActivationFunction<T>
    where T : IFloatingPoint<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ThresholdFunction"/> class.
    /// </summary>
    public ThresholdFunction()
    {
    }

    /// <summary>
    /// Calculates function value.
    /// </summary>
    ///
    /// <param name="x">Function input value.</param>
    /// 
    /// <returns>Function output value, <i>f(x)</i>.</returns>
    ///
    /// <remarks>The method calculates function value at point <paramref name="x"/>.</remarks>
    ///
    public T Function(T x)
    {
        return x >= T.CreateChecked(0)
            ? T.CreateChecked(1)
            : T.CreateChecked(0);
    }

    /// <summary>
    /// Calculates function derivative (not supported).
    /// </summary>
    /// 
    /// <param name="x">Input value.</param>
    /// 
    /// <returns>Always returns 0.</returns>
    /// 
    /// <remarks><para><note>The method is not supported, because it is not possible to
    /// calculate derivative of the function.</note></para></remarks>
    ///
    public T Derivative(T x)
    {
        return T.CreateChecked(0);
    }

    /// <summary>
    /// Calculates function derivative (not supported).
    /// </summary>
    /// 
    /// <param name="y">Input value.</param>
    /// 
    /// <returns>Always returns 0.</returns>
    /// 
    /// <remarks><para><note>The method is not supported, because it is not possible to
    /// calculate derivative of the function.</note></para></remarks>
    /// 
    public T Derivative2(T y)
    {
        return T.CreateChecked(0);
    }
}
