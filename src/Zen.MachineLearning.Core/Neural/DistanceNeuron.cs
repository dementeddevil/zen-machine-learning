using System;
using System.Numerics;

namespace Zen.MachineLearning.Core.Neural;

/// <summary>
/// Distance neuron.
/// </summary>
/// <remarks>
/// <para>
/// Distance neuron computes its output as distance between
/// its weights and inputs - sum of absolute differences between weights'
/// values and corresponding inputs' values. The neuron is usually used in Kohonen
/// Self Organizing Map.
/// </para>
/// </remarks>
[Serializable]
public class DistanceNeuron<T> : Neuron<T> where T : IFloatingPoint<T>, IRootFunctions<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DistanceNeuron"/> class.
    /// </summary>
    /// <param name="inputCount">Neuron's inputs count.</param>
    public DistanceNeuron(int inputCount)
        : base(inputCount)
    {
    }

    /// <summary>
    /// Computes output value of neuron.
    /// </summary>
    /// <param name="input">Input vector.</param>
    /// <returns>Returns neuron's output value.</returns>
    /// <remarks><para>The output value of distance neuron is equal to the distance
    /// between its weights and inputs - sum of absolute differences.
    /// The output value is also stored in <see cref="Neuron.Output">Output</see>
    /// property.</para>
    /// <para><note>The method may be called safely from multiple threads to compute neuron's
    /// output value for the specified input values. However, the value of
    /// <see cref="Neuron.Output"/> property in multi-threaded environment is not predictable,
    /// since it may hold neuron's output computed from any of the caller threads. Multi-threaded
    /// access to the method is useful in those cases when it is required to improve performance
    /// by utilizing several threads and the computation is based on the immediate return value
    /// of the method, but not on neuron's output property.</note></para>
    /// </remarks>
    /// <exception cref="ArgumentException">Wrong length of the input vector, which is not
    /// equal to the <see cref="Neuron.InputCount">expected value</see>.</exception>
    public override T Compute(Vector<T> input)
    {
        var difference = Vector.Subtract(_weights, input);
        var error = Vector.Abs(difference);
        var squareError = Vector.Multiply(error, error);
        var sumOfSquareError = Vector.Sum(squareError);
        var output = T.Sqrt(sumOfSquareError);
        _output = output;

        return output;
    }
}
