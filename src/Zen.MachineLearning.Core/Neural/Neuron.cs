using System.Numerics;

namespace Zen.MachineLearning.Core.Neural;

/// <summary>
/// Base neuron class.
/// </summary>
/// 
/// <remarks>This is a base neuron class, which encapsulates such
/// common properties, like neuron's input, output and weights.</remarks>
/// 
[Serializable]
public abstract class Neuron<T> where T : IFloatingPoint<T>
{
    /// <summary>
    /// Number of neuron inputs.
    /// </summary>
    protected int _inputCount = 0;

    /// <summary>
    /// Neuron weights.
    /// </summary>
    protected Vector<T> _weights;

    /// <summary>
    /// Current neuron output value.
    /// </summary>
    protected T _output = default;

    /// <summary>
    /// Random generator range.
    /// </summary>
    /// 
    /// <remarks>Sets the range of random generator. Affects initial values of neuron's weight.
    /// Default value is [0, 1].</remarks>
    /// 
    protected static Range<T> randRange = new Range<T>(T.Zero, T.One);

    /// <summary>
    /// Random generator range.
    /// </summary>
    /// 
    /// <remarks>Sets the range of random generator. Affects initial values of neuron's weight.
    /// Default value is [0, 1].</remarks>
    /// 
    public static Range<T> RandRange
    {
        get
        {
            return randRange;
        }
        set
        {
            randRange = value;
        }
    }

    /// <summary>
    /// Neuron's inputs count.
    /// </summary>
    public int InputCount => _inputCount;

    /// <summary>
    /// Neuron's output value.
    /// </summary>
    /// 
    /// <remarks>The calculation way of neuron's output value is determined by inherited class.</remarks>
    /// 
    public T Output => _output;


    /// <summary>
    /// Neuron's weights.
    /// </summary>
    public Vector<T> Weights
    {
        get => _weights;
        set => _weights = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Neuron"/> class.
    /// </summary>
    /// <param name="inputCount">Number of inputs feeding this neuron.</param>
    /// <remarks>
    /// The new neuron will be randomized (see <see cref="Randomize"/> method)
    /// after it is created.
    /// </remarks>
    protected Neuron(int inputCount)
    {
        // allocate weights
        _inputCount = Math.Max(1, inputCount);
        _weights = new Vector<T>(new T[_inputCount]);

        // randomize the neuron
        Randomize();
    }

    /// <summary>
    /// Randomize neuron.
    /// </summary>
    /// 
    /// <remarks>Initialize neuron's weights with random values within the range specified
    /// by <see cref="RandRange"/>.</remarks>
    /// 
    public virtual void Randomize()
    {
        T d = randRange.Length;

        // randomize weights
        var randomWeights = new T[_inputCount];
        for (var i = 0; i < _inputCount; i++)
        {
            randomWeights[i] = T.CreateChecked(RandomFactory.NextDouble()) * d + randRange.Min;
        }

        // Update weights vector
        _weights = new Vector<T>(randomWeights);
    }

    /// <summary>
    /// Computes output value of neuron.
    /// </summary>
    /// 
    /// <param name="input">Input vector.</param>
    /// 
    /// <returns>Returns neuron's output value.</returns>
    /// 
    /// <remarks>
    /// The actual neuron's output value is determined by inherited class.
    /// The output value is also stored in <see cref="Output"/> property.
    /// </remarks>
    public abstract T Compute(Vector<T> input);
}
