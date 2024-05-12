using System.Numerics;
using Zen.MachineLearning.Core.Neural.SelfOrganising;

namespace Zen.MachineLearning.Core.Neural;

/// <summary>
/// Kohonen Self Organizing Map (SOM) learning algorithm.
/// </summary>
/// 
/// <remarks><para>This class implements Kohonen's SOM learning algorithm and
/// is widely used in clusterization tasks. The class allows to train
/// <see cref="DistanceNetwork">Distance Networks</see>.</para>
/// 
/// <para>Sample usage (clustering RGB colors):</para>
/// <code>
/// // set range for randomization neurons' weights
/// Neuron.RandRange = new Range( 0, 255 );
/// // create network
/// DistanceNetwork	network = new DistanceNetwork(
///         3, // thress inputs in the network
///         100 * 100 ); // 10000 neurons
/// // create learning algorithm
/// SOMLearning	trainer = new SOMLearning( network );
/// // network's input
/// double[] input = new double[3];
/// // loop
/// while ( !needToStop )
/// {
///     input[0] = rand.Next( 256 );
///     input[1] = rand.Next( 256 );
///     input[2] = rand.Next( 256 );
/// 
///     trainer.Run( input );
/// 
///     // ...
///     // update learning rate and radius continuously,
///     // so networks may come steady state
/// }
/// </code>
/// </remarks>
/// 
public class SomLearning<T> : IUnsupervisedLearning<T>
    where T : IFloatingPoint<T>, IRootFunctions<T>, ISignedNumber<T>, IUnaryNegationOperators<T, T>, IAdditionOperators<T, T, T>
{
    // neural network to train
    private readonly DistanceNetwork<T> _network;

    // network's dimension
    private readonly int _width;
    private int _height;

    // learning rate
    protected double learningRate = 0.1;
    // learning radius
    protected double learningRadius = 7;

    // squared learning radius multiplied by 2 (precalculated value to speed up computations)
    protected double SquaredRadius2 = 2 * 7 * 7;

    /// <summary>
    /// Initializes a new instance of the <see cref="SomLearning"/> class.
    /// </summary>
    /// <param name="network">Neural network to train.</param>
    /// <param name="verifySize">Indicate whether the network size is asserted in ctor.</param>
    /// <remarks><para>This constructor supposes that a square network will be passed for training -
    /// it should be possible to get square root of network's neurons amount.</para></remarks>
    /// <exception cref="ArgumentException">Invalid network size - square network is expected.</exception>
    public SomLearning(DistanceNetwork<T> network, bool verifySize = true)
    {
        // network's dimension was not specified, let's try to guess
        var neuronsCount = network.Layers[0].Neurons.Length;
        var width = (int)Math.Sqrt(neuronsCount);

        if (verifySize && width * width != neuronsCount)
        {
            throw new ArgumentException("Invalid network size.");
        }

        // ok, we got it
        _network = network;
        _width = width;
        _height = width;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SomLearning"/> class.
    /// </summary>
    /// <param name="network">Neural network to train.</param>
    /// <param name="width">Neural network width.</param>
    /// <param name="height">Neural network height.</param>
    /// <param name="depth">Neural network depth (default = 1).</param>
    /// <remarks>The constructor allows to pass network of arbitrary rectangular shape.
    /// The amount of neurons in the network should be equal to <b>width</b> * <b>height</b>.
    /// </remarks>
    /// <exception cref="ArgumentException">Invalid network size - network size does not correspond
    /// to specified width and height.</exception>
    public SomLearning(DistanceNetwork<T> network, int width, int height, int depth = 1)
    {
        // check network size
        if (network.Layers[0].Neurons.Length != width * height * depth)
        {
            throw new ArgumentException("Invalid network size.");
        }

        _network = network;
        _width = width;
        _height = height;
    }

    /// <summary>
    /// Learning rate, [0, 1].
    /// </summary>
    /// 
    /// <remarks><para>Determines speed of learning.</para>
    /// 
    /// <para>Default value equals to <b>0.1</b>.</para>
    /// </remarks>
    /// 
    public double LearningRate
    {
        get
        {
            return learningRate;
        }
        set
        {
            learningRate = Math.Max(0.0, Math.Min(1.0, value));
        }
    }

    /// <summary>
    /// Learning radius.
    /// </summary>
    /// 
    /// <remarks><para>Determines the amount of neurons to be updated around
    /// winner neuron. Neurons, which are in the circle of specified radius,
    /// are updated during the learning procedure. Neurons, which are closer
    /// to the winner neuron, get more update.</para>
    /// 
    /// <para><note>In the case if learning rate is set to 0, then only winner
    /// neuron's weights are updated.</note></para>
    /// 
    /// <para>Default value equals to <b>7</b>.</para>
    /// </remarks>
    /// 
    public double LearningRadius
    {
        get
        {
            return learningRadius;
        }
        set
        {
            learningRadius = Math.Max(0, value);
            SquaredRadius2 = 2 * learningRadius * learningRadius;
        }
    }

    /// <summary>
    /// Runs learning iteration.
    /// </summary>
    /// 
    /// <param name="input">Input vector.</param>
    /// 
    /// <returns>Returns learning error - summary absolute difference between neurons' weights
    /// and appropriate inputs. The difference is measured according to the neurons
    /// distance to the winner neuron.</returns>
    /// 
    /// <remarks><para>The method runs one learning iterations - finds winner neuron (the neuron
    /// which has weights with values closest to the specified input vector) and updates its weight
    /// (as well as weights of neighbor neurons) in the way to decrease difference with the specified
    /// input vector.</para></remarks>
    /// 
    public virtual T Run(Vector<T> input)
    {
        T error = T.Zero;

        // compute the network
        _network.Compute(input);
        var winner = _network.GetWinner();

        // get layer of the network
        var layer = _network.Layers[0];

        // check learning radius
        if (learningRadius == 0)
        {
            var neuron = layer.Neurons[winner];

            // Calculate difference between the input and the neuron weights
            var difference = Vector.Subtract(input, neuron.Weights);

            // Calculate absolute difference vector
            var absDifference = Vector.Abs(difference);

            // Determine the error value
            error = Vector.Sum(absDifference);

            // Calculate the weight delta vector
            var weightChange = Vector.Multiply(difference, T.CreateChecked(learningRate));

            // Update the neuron weights
            neuron.Weights = Vector.Add(neuron.Weights, weightChange);
        }
        else
        {
            // winner's X and Y
            var wx = winner % _width;
            var wy = winner / _width;

            // walk through all neurons of the layer
            for (var j = 0; j < layer.Neurons.Length; j++)
            {
                var neuron = layer.Neurons[j];

                var dx = j % _width - wx;
                var dy = j / _width - wy;

                // update factor ( Gaussian based )
                var factor = T.CreateChecked(Math.Exp(-(double)(dx * dx + dy * dy) / SquaredRadius2));

                // Calculate difference between the input and the neuron weights
                var difference = Vector.Subtract(input, neuron.Weights);
                var factoredDifference = Vector.Multiply(difference, factor);

                // Calculate absolute difference vector
                var absDifference = Vector.Abs(factoredDifference);

                // Determine the error value
                error += Vector.Sum(absDifference);

                // Calculate the weight delta vector
                var weightChange = Vector.Multiply(factoredDifference, T.CreateChecked(learningRate));

                // Update the neuron weights
                neuron.Weights = Vector.Add(neuron.Weights, weightChange);
            }
        }
        return error;
    }

    /// <summary>
    /// Runs learning epoch.
    /// </summary>
    /// 
    /// <param name="input">Array of input vectors.</param>
    /// 
    /// <returns>Returns summary learning error for the epoch. See <see cref="Run"/>
    /// method for details about learning error calculation.</returns>
    /// 
    /// <remarks><para>The method runs one learning epoch, by calling <see cref="Run"/> method
    /// for each vector provided in the <paramref name="input"/> array.</para></remarks>
    /// 
    public T RunEpoch(Vector<T>[] input)
    {
        T error = T.Zero;

        // walk through all training samples
        foreach (var sample in input)
        {
            error += Run(sample);
        }

        // return summary error
        return error;
    }
}

/// <summary>
/// Kohonen Self Organizing Map (SOM) learning algorithm.
/// </summary>
/// 
/// <remarks>
/// <para>
/// This class implements Kohonen's SOM learning algorithm and
/// is widely used in clusterization tasks. The class allows to train
/// <see cref="DistanceNetworkEx">Distance Networks</see>.
/// </para>
/// <para>
/// This class extends <see cref="SomLearning"/> to provide specialised
/// learning capability to SOM networks that use advanced SOM topologies\
/// such as; cube, toroidal (wrap-around) and hexagonal layouts.
/// </para>
/// 
/// <para>Sample usage (clustering RGB colors):</para>
/// <code>
/// // set range for randomization neurons' weights
/// Neuron.RandRange = new Range( 0, 255 );
/// // create network
/// DistanceNetwork	network = new DistanceNetwork(
///         3, // thress inputs in the network
///         100 * 100 ); // 10000 neurons
/// // create learning algorithm
/// SOMLearning	trainer = new SOMLearning( network );
/// // network's input
/// double[] input = new double[3];
/// // loop
/// while ( !needToStop )
/// {
///     input[0] = rand.Next( 256 );
///     input[1] = rand.Next( 256 );
///     input[2] = rand.Next( 256 );
/// 
///     trainer.Run( input );
/// 
///     // ...
///     // update learning rate and radius continuously,
///     // so networks may come steady state
/// }
/// </code>
/// </remarks>
/// 
public class SomLearningEx<T> : SomLearning<T> 
    where T : IFloatingPoint<T>, IRootFunctions<T>, ISignedNumber<T>, IUnaryNegationOperators<T, T>, IAdditionOperators<T, T, T>
{
    private readonly DistanceNetworkEx<T> _network;

    /// <summary>
    /// Initializes a new instance of the <see cref="SomLearning"/> class.
    /// </summary>
    /// <param name="network">The network.</param>
    public SomLearningEx(DistanceNetworkEx<T> network)
        : base(network, network.Width, network.Height, network.Depth)
    {
        _network = network;
    }

    /// <summary>
    /// Runs learning iteration.
    /// </summary>
    /// 
    /// <param name="input">Input vector.</param>
    /// 
    /// <returns>Returns learning error - summary absolute difference between neurons' weights
    /// and appropriate inputs. The difference is measured according to the neurons
    /// distance to the winner neuron.</returns>
    /// 
    /// <remarks>
    /// <para>
    /// The method runs one learning iteration - finds winner neuron (the
    /// neuron which has weights with values closest to the specified input
    /// vector) and updates its weight (as well as weights of neighbor
    /// neurons) in such a way as to decrease the difference with the 
    /// specified input vector.
    /// </para>
    /// <para>
    /// This method correctly deals with neural networks having toroidal or
    /// multi-dimensional neuron topologies.
    /// </para>
    /// </remarks>
    public override T Run(T[] input)
    {
        T error = T.Zero;

        // compute the network
        _network.Compute(input);
        var winner = _network.GetWinner();

        // get layer of the network
        var layer = _network.Layers[0];

        // check learning radius
        if (learningRadius == 0)
        {
            var neuron = layer.Neurons[winner];

            // update weight of the winner only
            for (var i = 0; i < neuron.Weights.Length; i++)
            {
                // calculate the error
                var e = input[i] - neuron.Weights[i];
                error += T.Abs(e);

                // update weights
                neuron.Weights[i] += e * T.CreateChecked(learningRate);
            }
        }
        else
        {
            var maxIterations = Math.Max(_network.Width, Math.Max(_network.Height, _network.Depth));
            var location = _network.GetLocationFromIndex(winner)!;

            // Build list of nodes processed
            var processed = new Dictionary<string, NeuronLocation<T>>();
            var currentSet = 
                new List<NeuronLocation<T>>
                {
                    location
                };

            for (var iteration = 0; iteration < maxIterations; ++iteration)
            {
                var factor = Math.Exp(-(double)(iteration * iteration) / SquaredRadius2);

                // Process current neuron set
                foreach (var workItem in currentSet)
                {
                    Neuron<T> neuron = workItem.Neuron;

                    // update weight of the neuron
                    for (var i = 0; i < neuron.Weights.Length; i++)
                    {
                        // calculate the error
                        T e = (input[i] - neuron.Weights[i]) * T.CreateChecked(factor);
                        error += T.Abs(e);

                        // update weight
                        neuron.Weights[i] += e * T.CreateChecked(learningRate);
                    }
                }

                // Add each node in current set to processed set
                foreach (var elem in currentSet)
                {
                    processed.Add(elem.Location, elem);
                }

                // Build list of nodes for next run
                var nextRun = new Dictionary<string, NeuronLocation<T>>();
                foreach (var elem in currentSet)
                {
                    var locationCount = elem.LocationCount;
                    for (var locationIndex = 0; locationIndex < locationCount; ++locationIndex)
                    {
                        var key = elem.GetLocationAt(locationIndex);
                        if (!string.IsNullOrEmpty(key) &&
                            !processed.ContainsKey(key) &&
                            !nextRun.ContainsKey(key))
                        {
                            nextRun.Add(key, _network.GetLocationAt(key)!);
                        }
                    }
                }

                // Setup for next run
                currentSet.Clear();
                currentSet.AddRange(nextRun.Values);

                // Terminate if we run out of elements
                if (currentSet.Count == 0)
                {
                    break;
                }
            }
        }
        return error;
    }
}
