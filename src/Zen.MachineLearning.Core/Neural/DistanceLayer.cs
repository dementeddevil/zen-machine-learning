using System.Numerics;

namespace Zen.MachineLearning.Core.Neural;

/// <summary>
/// Distance layer.
/// </summary>
/// 
/// <remarks>Distance layer is a layer of <see cref="DistanceNeuron">distance neurons</see>.
/// The layer is usually a single layer of such networks as Kohonen Self
/// Organizing Map, Elastic Net, Hamming Memory Net.</remarks>
/// 
[Serializable]
public class DistanceLayer<T> : Layer<T> where T : IFloatingPoint<T>, IRootFunctions<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DistanceLayer"/> class.
    /// </summary>
    /// <param name="neuronCount">Layer's neurons count.</param>
    /// <param name="inputCount">Layer's inputs count.</param>
    /// <remarks>The new layer is randomized (see <see cref="Neuron.Randomize"/>
    /// method) after it is created.</remarks>
    public DistanceLayer(int neuronCount, int inputCount)
        : base(neuronCount, inputCount)
    {
        // create each neuron
        for (var i = 0; i < neuronCount; i++)
        {
            _neurons[i] = new DistanceNeuron<T>(inputCount);
        }
    }
}
