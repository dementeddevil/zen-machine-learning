using System;
using System.Numerics;

namespace Zen.MachineLearning.Core.Neural;

/// <summary>
/// Base neural layer class.
/// </summary>
/// 
/// <remarks>This is a base neural layer class, which represents
/// collection of neurons.</remarks>
/// 
[Serializable]
public abstract class Layer<T> where T : IFloatingPoint<T>
{
    /// <summary>
    /// Layer's inputs count.
    /// </summary>
    protected int _inputCount = 0;

    /// <summary>
    /// Layer's neurons count.
    /// </summary>
    protected int NeuronCount = 0;

    /// <summary>
    /// Layer's neurons.
    /// </summary>
    protected Neuron<T>[] _neurons;

    /// <summary>
    /// Layer's output vector.
    /// </summary>
    protected Vector<T> _output;

    /// <summary>
    /// Layer's inputs count.
    /// </summary>
    public int InputCount => _inputCount;

    /// <summary>
    /// Layer's neurons.
    /// </summary>
    /// 
    public Neuron<T>[] Neurons => _neurons;

    /// <summary>
    /// Layer's output vector.
    /// </summary>
    /// 
    /// <remarks><para>The calculation way of layer's output vector is determined by neurons,
    /// which comprise the layer.</para>
    /// 
    /// <para><note>The property is not initialized (equals to <see langword="null"/>) until
    /// <see cref="Compute"/> method is called.</note></para>
    /// </remarks>
    /// 
    public Vector<T> Output => _output;

    /// <summary>
    /// Initializes a new instance of the <see cref="Layer"/> class.
    /// </summary>
    /// <param name="neuronCount">Number of neurons in this layer.</param>
    /// <param name="inputCount">Number of inputs feeding this layer.</param>
    /// <remarks>
    /// Protected contructor, which initializes <see cref="inputCount"/>,
    /// <see cref="neuronCount"/> and <see cref="_neurons"/> members.
    /// </remarks>
    protected Layer(int neuronCount, int inputCount)
    {
        _inputCount = Math.Max(1, inputCount);
        NeuronCount = Math.Max(1, neuronCount);
        _neurons = new Neuron<T>[NeuronCount];
    }

    /// <summary>
    /// Compute output vector of the layer.
    /// </summary>
    /// 
    /// <param name="input">Input vector.</param>
    /// 
    /// <returns>Returns layer's output vector.</returns>
    /// 
    /// <remarks><para>The actual layer's output vector is determined by neurons,
    /// which comprise the layer - consists of output values of layer's neurons.
    /// The output vector is also stored in <see cref="Output"/> property.</para>
    /// 
    /// <para><note>The method may be called safely from multiple threads to compute layer's
    /// output value for the specified input values. However, the value of
    /// <see cref="Output"/> property in multi-threaded environment is not predictable,
    /// since it may hold layer's output computed from any of the caller threads. Multi-threaded
    /// access to the method is useful in those cases when it is required to improve performance
    /// by utilizing several threads and the computation is based on the immediate return value
    /// of the method, but not on layer's output property.</note></para>
    /// </remarks>
    /// 
    public virtual Vector<T> Compute(Vector<T> input)
    {
        // local variable to avoid mutlithread conflicts
        var output = new T[NeuronCount];

        // compute each neuron
        for (var i = 0; i < _neurons.Length; i++)
        {
            output[i] = _neurons[i].Compute(input);
        }

        // assign output property as well (works correctly for single threaded usage)
        var outputVector = new Vector<T>(output);
        _output = outputVector;

        return outputVector;
    }

    /// <summary>
    /// Randomize neurons of the layer.
    /// </summary>
    /// 
    /// <remarks>Randomizes layer's neurons by calling <see cref="Neuron.Randomize"/> method
    /// of each neuron.</remarks>
    /// 
    public virtual void Randomize()
    {
        foreach (var neuron in _neurons)
        {
            neuron.Randomize();
        }
    }
}
