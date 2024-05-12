using System;
using System.Net;
using System.Numerics;

namespace Zen.MachineLearning.Core.Neural;

/// <summary>
/// Distance network.
/// </summary>
///
/// <remarks>Distance network is a neural network of only one <see cref="DistanceLayer">distance
/// layer</see>. The network is a base for such neural networks as SOM, Elastic net, etc.
/// </remarks>
///
[Serializable]
public class DistanceNetwork<T> : Network<T> where T : IFloatingPoint<T>, IRootFunctions<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DistanceNetwork"/> class.
    /// </summary>
    /// 
    /// <param name="inputCount">Network's inputs count.</param>
    /// <param name="neuronCount">Network's neurons count.</param>
    /// 
    /// <remarks>The new network is randomized (see <see cref="Neuron.Randomize"/>
    /// method) after it is created.</remarks>
    /// 
    public DistanceNetwork(int inputCount, int neuronCount)
        : base(inputCount, 1)
    {
        // create layer
        layers[0] = new DistanceLayer<T>(neuronCount, inputCount);
    }

    /// <summary>
    /// Get winner neuron.
    /// </summary>
    /// 
    /// <returns>Index of the winner neuron.</returns>
    /// 
    /// <remarks>The method returns index of the neuron, whos weights have
    /// the minimum distance from network's input.</remarks>
    /// 
    public int GetWinner()
    {
        // find the MIN value
        var min = output[0];
        var minIndex = 0;

        for (var i = 1; i < output.Length; i++)
        {
            if (output[i] < min)
            {
                // found new MIN value
                min = output[i];
                minIndex = i;
            }
        }

        return minIndex;
    }
}
