using System.Numerics;

namespace Zen.MachineLearning.Core.Neural;

/// <summary>
/// Activation neuron.
/// </summary>
/// <remarks>
/// <para>
/// Activation neuron computes weighted sum of its inputs, adds
/// threshold value and then applies <see cref="ActivationFunction">activation function</see>.
/// The neuron isusually used in multi-layer neural networks.
/// </para>
/// </remarks>
/// <seealso cref="IActivationFunction"/>
[Serializable]
public class ActivationNeuron<T> : Neuron<T>
    where T : IFloatingPoint<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ActivationNeuron"/> class.
    /// </summary>
    /// <param name="inputs">Neuron's inputs count.</param>
    /// <param name="function">Neuron's activation function.</param>
    public ActivationNeuron(int inputs, IActivationFunction<T> function)
        : base(inputs)
    {
        ActivationFunction = function;
    }

    /// <summary>
    /// Threshold value.
    /// </summary>
    /// <remarks>
    /// The value is added to inputs weighted sum before it is passed to
    /// activation function.
    /// </remarks>
    public T Threshold { get; set; }

    /// <summary>
    /// Neuron's activation function.
    /// </summary>
    /// <remarks>
    /// The function is applied to inputs weighted sum plus threshold value.
    /// </remarks>
    public IActivationFunction<T> ActivationFunction { get; set; }

    /// <summary>
    /// Randomize neuron.
    /// </summary>
    /// 
    /// <remarks>Calls base class <see cref="Neuron.Randomize">Randomize</see> method
    /// to randomize neuron's weights and then randomizes threshold's value.</remarks>
    /// 
    public override void Randomize()
    {
        // randomize weights
        base.Randomize();

        // randomize threshold
        Threshold = T.CreateChecked(RandomFactory.NextDouble()) * randRange.Length + randRange.Min;
    }

    /// <summary>
    /// Computes output value of neuron.
    /// </summary>
    /// <param name="input">Input vector.</param>
    /// <returns>Returns neuron's output value.</returns>
    /// <remarks>
    /// <para>
    /// The output value of activation neuron is equal to value
    /// of nueron's activation function, which parameter is weighted sum
    /// of its inputs plus threshold value. The output value is also stored
    /// in <see cref="Neuron.Output">Output</see> property.
    /// </para>
    /// <para>
    /// <note>The method may be called safely from multiple threads to compute neuron's
    /// output value for the specified input values. However, the value of
    /// <see cref="Neuron.Output"/> property in multi-threaded environment is not predictable,
    /// since it may hold neuron's output computed from any of the caller threads. Multi-threaded
    /// access to the method is useful in those cases when it is required to improve performance
    /// by utilizing several threads and the computation is based on the immediate return value
    /// of the method, but not on neuron's output property.</note>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Wrong length of the input vector, which is not equal to the
    /// <see cref="Neuron.InputCount">expected value</see>.
    /// </exception>
    public override T Compute(Vector<T> input)
    {
        // initial sum value (dot product)
        T sum = Vector.Dot(input, _weights);

        // compute weighted sum of inputs
        sum += Threshold;

        // local variable to avoid mutlithreaded conflicts
        var output = ActivationFunction.Function(sum);

        // assign output property as well (works correctly for single threaded usage)
        _output = output;

        return output;
    }
}
