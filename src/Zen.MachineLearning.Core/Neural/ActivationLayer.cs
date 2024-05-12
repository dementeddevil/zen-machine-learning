using System.Numerics;

namespace Zen.MachineLearning.Core.Neural;

/// <summary>
/// Activation layer.
/// </summary>
/// <remarks>
/// Activation layer is a layer of <see cref="ActivationNeuron">activation neurons</see>.
/// The layer is usually used in multi-layer neural networks.
/// </remarks>
[Serializable]
public class ActivationLayer<T> : Layer<T>
    where T : IFloatingPoint<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ActivationLayer"/> class.
    /// </summary>
    /// <param name="neuronCount">Number of neurons in this layer.</param>
    /// <param name="inputCount">Number of inputs feeding this layer.</param>
    /// <param name="function">Activation function of neurons of the layer.</param>
    /// <remarks>
    /// The new layer is randomized (see <see cref="ActivationNeuron.Randomize"/>
    /// method) after it is created.
    /// </remarks>
    public ActivationLayer(int neuronCount, int inputCount, IActivationFunction<T> function)
        : base(neuronCount, inputCount)
    {
        // create each neuron
        for (var i = 0; i < _neurons.Length; i++)
        {
            _neurons[i] = new ActivationNeuron<T>(inputCount, function);
        }
    }

    /// <summary>
    /// Set new activation function for all neurons of the layer.
    /// </summary>
    /// <param name="function">Activation function to set.</param>
    /// <remarks>
    /// <para>The methods sets new activation function for each neuron by setting
    /// their <see cref="ActivationNeuron.ActivationFunction"/> property.</para>
    /// </remarks>
    public void SetActivationFunction(IActivationFunction<T> function)
    {
        foreach (var neuron in _neurons)
        {
            ((ActivationNeuron<T>)neuron).ActivationFunction = function;
        }
    }
}
