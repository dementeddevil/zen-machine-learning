using System.Numerics;

namespace Zen.MachineLearning.Core.Neural;

/// <summary>
/// Perceptron learning algorithm.
/// </summary>
/// 
/// <remarks><para>This learning algorithm is used to train one layer neural
/// network of <see cref="ActivationNeuron">Activation Neurons</see>
/// with the <see cref="ThresholdFunction">Threshold</see>
/// activation function.</para>
/// 
/// <para>See information about <a href="http://en.wikipedia.org/wiki/Perceptron">Perceptron</a>
/// and its learning algorithm.</para>
/// </remarks>
/// 
public class PerceptronLearning<T> : ISupervisedLearning<T>
    where T : IFloatingPoint<T>
{
    // network to teach
    private readonly ActivationNetwork<T> _network;
    // learning rate
    private T _learningRate = T.CreateChecked(0.1);

    /// <summary>
    /// Learning rate, [0, 1].
    /// </summary>
    /// 
    /// <remarks><para>The value determines speed of learning.</para>
    /// 
    /// <para>Default value equals to <b>0.1</b>.</para>
    /// </remarks>
    /// 
    public T LearningRate
    {
        get
        {
            return _learningRate;
        }
        set
        {
            var min = T.CreateChecked(0.0);
            var max = T.CreateChecked(1.0);
            if (value < min)
            {
                _learningRate = min;
            }
            else if (value > max)
            {
                _learningRate = max;
            }
            else
            {
                _learningRate = value;
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PerceptronLearning"/> class.
    /// </summary>
    /// 
    /// <param name="network">Network to teach.</param>
    /// 
    /// <exception cref="ArgumentException">Invalid nuaral network. It should have one layer only.</exception>
    /// 
    public PerceptronLearning(ActivationNetwork<T> network)
    {
        // check layers count
        if (network.Layers.Length != 1)
        {
            throw new ArgumentException("Invalid nuaral network. It should have one layer only.");
        }

        _network = network;
    }

    /// <summary>
    /// Runs learning iteration.
    /// </summary>
    /// 
    /// <param name="input">Input vector.</param>
    /// <param name="output">Desired output vector.</param>
    /// 
    /// <returns>Returns absolute error - difference between current network's output and
    /// desired output.</returns>
    /// 
    /// <remarks><para>Runs one learning iteration and updates neuron's
    /// weights in the case if neuron's output is not equal to the
    /// desired output.</para></remarks>
    /// 
    public T Run(Vector<T> input, Vector<T> output)
    {
        // compute output of network
        var networkOutput = _network.Compute(input);

        // get the only layer of the network
        var layer = _network.Layers[0];

        // summary network absolute error
        var error = T.CreateChecked(0.0);

        // check output of each neuron and update weights
        for (var j = 0; j < layer.Neurons.Length; j++)
        {
            var e = output[j] - networkOutput[j];

            if (e != T.CreateChecked(0.0))
            {
                var perceptron = layer.Neurons[j] as ActivationNeuron<T>;

                // update weights
                for (var i = 0; i < perceptron.InputCount; i++)
                {
                    perceptron.Weights = perceptron.Weights.WithElement(
                        i, perceptron.Weights[i] + _learningRate * e * input[i]);
                }

                // update threshold value
                perceptron.Threshold += _learningRate * e;

                // make error to be absolute
                error += T.CreateChecked(Math.Abs(Double.CreateChecked(e)));
            }
        }

        return error;
    }

    /// <summary>
    /// Runs learning epoch.
    /// </summary>
    /// 
    /// <param name="input">Array of input vectors.</param>
    /// <param name="output">Array of output vectors.</param>
    /// 
    /// <returns>Returns summary learning error for the epoch. See <see cref="Run"/>
    /// method for details about learning error calculation.</returns>
    /// 
    /// <remarks><para>The method runs one learning epoch, by calling <see cref="Run"/> method
    /// for each vector provided in the <paramref name="input"/> array.</para></remarks>
    /// 
    public T RunEpoch(Vector<T>[] input, Vector<T>[] output)
    {
        var error = T.CreateChecked(0.0);

        // run learning procedure for all samples
        for (int i = 0, n = input.Length; i < n; i++)
        {
            error += Run(input[i], output[i]);
        }

        // return summary error
        return error;
    }
}
