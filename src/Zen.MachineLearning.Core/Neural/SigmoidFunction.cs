using System.Numerics;

namespace Zen.MachineLearning.Core.Neural;

/// <summary>
/// Sigmoid activation function.
/// </summary>
///
/// <remarks><para>The class represents sigmoid activation function with
/// the next expression:
/// <code lang="none">
///                1
/// f(x) = ------------------
///        1 + exp(-alpha * x)
///
///           alpha * exp(-alpha * x )
/// f'(x) = ---------------------------- = alpha * f(x) * (1 - f(x))
///           (1 + exp(-alpha * x))^2
/// </code>
/// </para>
///
/// <para>Output range of the function: <b>[0, 1]</b>.</para>
/// 
/// <para>Functions graph:</para>
/// <img src="img/neuro/sigmoid.bmp" width="242" height="172" />
/// </remarks>
/// 
[Serializable]
public class SigmoidFunction<T> : IActivationFunction<T>
    where T : IFloatingPoint<T>
{
    /// <summary>
    /// Sigmoid's alpha value.
    /// </summary>
    /// 
    /// <remarks><para>The value determines steepness of the function. Increasing value of
    /// this property changes sigmoid to look more like a threshold function. Decreasing
    /// value of this property makes sigmoid to be very smooth (slowly growing from its
    /// minimum value to its maximum value).</para>
    ///
    /// <para>Default value is set to <b>2</b>.</para>
    /// </remarks>
    /// 
    public T Alpha { get; set; } = T.CreateChecked(2);

    /// <summary>
    /// Initializes a new instance of the <see cref="SigmoidFunction"/> class.
    /// </summary>
    public SigmoidFunction()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SigmoidFunction"/> class.
    /// </summary>
    /// 
    /// <param name="alpha">Sigmoid's alpha value.</param>
    /// 
    public SigmoidFunction(T alpha)
    {
        Alpha = alpha;
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
        return T.CreateChecked(1.0) / (T.CreateChecked(1.0) + T.CreateChecked(Math.Exp(-Double.CreateChecked(Alpha * x))));
    }

    /// <summary>
    /// Calculates function derivative.
    /// </summary>
    /// 
    /// <param name="x">Function input value.</param>
    /// 
    /// <returns>Function derivative, <i>f'(x)</i>.</returns>
    /// 
    /// <remarks>The method calculates function derivative at point <paramref name="x"/>.</remarks>
    ///
    public T Derivative(T x)
    {
        var y = Function(x);

        return Alpha * y * (T.CreateChecked(1) - y);
    }

    /// <summary>
    /// Calculates function derivative.
    /// </summary>
    /// 
    /// <param name="y">Function output value - the value, which was obtained
    /// with the help of <see cref="Function"/> method.</param>
    /// 
    /// <returns>Function derivative, <i>f'(x)</i>.</returns>
    /// 
    /// <remarks><para>The method calculates the same derivative value as the
    /// <see cref="Derivative"/> method, but it takes not the input <b>x</b> value
    /// itself, but the function value, which was calculated previously with
    /// the help of <see cref="Function"/> method.</para>
    /// 
    /// <para><note>Some applications require as function value, as derivative value,
    /// so they can save the amount of calculations using this method to calculate derivative.</note></para>
    /// </remarks>
    /// 
    public T Derivative2(T y)
    {
        return Alpha * y * (T.CreateChecked(1.0) - y);
    }
}
