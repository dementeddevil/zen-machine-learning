using System.Numerics;

namespace Zen.MachineLearning.Core.Neural;

/// <summary>
/// Activation network.
/// </summary>
/// <remarks>
/// <para>
/// Activation network is a base for multi-layer neural network
/// with activation functions. It consists of <see cref="ActivationLayer">activation
/// layers</see>.
/// </para>
/// <para>Sample usage:</para>
/// <code>
/// // create activation network
///	ActivationNetwork network = new ActivationNetwork(
///		new SigmoidFunction( ), // sigmoid activation function
///		3,                      // 3 inputs
///		4, 1 );                 // 2 layers:
///                             // 4 neurons in the firs layer
///                             // 1 neuron in the second layer
///	</code>
/// </remarks>
[Serializable]
public class ActivationNetwork<T> : Network<T>
    where T : IFloatingPoint<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ActivationNetwork"/> class.
    /// </summary>
    /// 
    /// <param name="function">Activation function for neurons of the network.</param>
    /// <param name="inputCount">Number of inputs feeding this network.</param>
    /// <param name="neuronCountPerLayer">Array, which specifies the number of neurons in
    /// each layer of the neural network.</param>
    /// <remarks>The new network is randomized (see <see cref="ActivationNeuron.Randomize"/>
    /// method) after it is created.</remarks>
    /// 
    public ActivationNetwork(
        IActivationFunction<T> function,
        int inputCount,
        params int[] neuronCountPerLayer)
        : base(inputCount, neuronCountPerLayer.Length)
    {
        // create each layer
        for (var index = 0; index < layers.Length; index++)
        {
            layers[index] = new ActivationLayer<T>(
                neuronCountPerLayer[index],
                index == 0 ? inputCount : neuronCountPerLayer[index - 1],
                function);
        }
        outputsCount = neuronCountPerLayer.Last();
    }

    /// <summary>
    /// Set new activation function for all neurons of the network.
    /// </summary>
    /// <param name="function">Activation function to set.</param>
    /// <remarks>
    /// <para>
    /// The method sets new activation function for all neurons by calling
    /// <see cref="ActivationLayer.SetActivationFunction"/> method for each layer of the network.
    /// </para>
    /// </remarks>
    public void SetActivationFunction(IActivationFunction<T> function)
    {
        foreach (var layer in layers)
        {
            ((ActivationLayer<T>)layer).SetActivationFunction(function);
        }
    }
}
