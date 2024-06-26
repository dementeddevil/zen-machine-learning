using System.Numerics;

namespace Zen.MachineLearning.Core.Neural;

/// <summary>
/// Activation function interface.
/// </summary>
/// 
/// <remarks>All activation functions, which are supposed to be used with
/// neurons, which calculate their output as a function of weighted sum of
/// their inputs, should implement this interfaces.
/// </remarks>
/// 
public interface IActivationFunction<T>
    where T : IFloatingPoint<T>
{
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
    T Function(T x);

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
    T Derivative(T x);

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
    T Derivative2(T y);
}
