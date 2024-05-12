namespace Zen.MachineLearning.Core.Neural
{
    using System;

    /// <summary>
    /// Resilient Backpropagation learning algorithm.
    /// </summary>
    /// 
    /// <remarks><para>This class implements the resilient backpropagation (RProp)
    /// learning algorithm. The RProp learning algorithm is one of the fastest learning
    /// algorithms for feed-forward learning networks which use only first-order
    /// information.</para>
    /// 
    /// <para>Sample usage (training network to calculate XOR function):</para>
    /// <code>
    /// // initialize input and output values
    /// double[][] input = new double[4][] {
    ///     new double[] {0, 0}, new double[] {0, 1},
    ///     new double[] {1, 0}, new double[] {1, 1}
    /// };
    /// double[][] output = new double[4][] {
    ///     new double[] {0}, new double[] {1},
    ///     new double[] {1}, new double[] {0}
    /// };
    /// // create neural network
    /// ActivationNetwork   network = new ActivationNetwork(
    ///     SigmoidFunction( 2 ),
    ///     2, // two inputs in the network
    ///     2, // two neurons in the first layer
    ///     1 ); // one neuron in the second layer
    /// // create teacher
    /// ResilientBackpropagationLearning teacher = new ResilientBackpropagationLearning( network );
    /// // loop
    /// while ( !needToStop )
    /// {
    ///     // run epoch of learning procedure
    ///     double error = teacher.RunEpoch( input, output );
    ///     // check error value to see if we need to stop
    ///     // ...
    /// }
    /// </code>
    /// </remarks>
    /// 
    public class ResilientBackpropagationLearning : ISupervisedLearning
    {
        private readonly ActivationNetwork _network;

        private double _learningRate = 0.0125;
        private readonly double _deltaMax = 50.0;
        private readonly double _deltaMin = 1e-6;

        private const double EtaMinus = 0.5;
        private readonly double _etaPlus = 1.2;

        private readonly double[][] _neuronErrors = null;

        // update values, also known as deltas
        private readonly double[][][] _weightsUpdates = null;
        private readonly double[][] _thresholdsUpdates = null;

        // current and previous gradient values
        private readonly double[][][] _weightsDerivatives = null;
        private readonly double[][] _thresholdsDerivatives = null;

        private readonly double[][][] _weightsPreviousDerivatives = null;
        private readonly double[][] _thresholdsPreviousDerivatives = null;


        /// <summary>
        /// Learning rate.
        /// </summary>
        /// 
        /// <remarks><para>The value determines speed of learning.</para>
        /// 
        /// <para>Default value equals to <b>0.0125</b>.</para>
        /// </remarks>
        ///
        public double LearningRate
        {
            get
            {
                return _learningRate;
            }
            set
            {
                _learningRate = value;
                ResetUpdates(_learningRate);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResilientBackpropagationLearning"/> class.
        /// </summary>
        /// 
        /// <param name="network">Network to teach.</param>
        /// 
        public ResilientBackpropagationLearning(ActivationNetwork network)
        {
            _network = network;

            var layersCount = network.Layers.Length;

            _neuronErrors = new double[layersCount][];

            _weightsDerivatives = new double[layersCount][][];
            _thresholdsDerivatives = new double[layersCount][];

            _weightsPreviousDerivatives = new double[layersCount][][];
            _thresholdsPreviousDerivatives = new double[layersCount][];

            _weightsUpdates = new double[layersCount][][];
            _thresholdsUpdates = new double[layersCount][];

            // initialize errors, derivatives and steps
            for (var i = 0; i < network.Layers.Length; i++)
            {
                var layer = network.Layers[i];
                var neuronsCount = layer.Neurons.Length;

                _neuronErrors[i] = new double[neuronsCount];

                _weightsDerivatives[i] = new double[neuronsCount][];
                _weightsPreviousDerivatives[i] = new double[neuronsCount][];
                _weightsUpdates[i] = new double[neuronsCount][];

                _thresholdsDerivatives[i] = new double[neuronsCount];
                _thresholdsPreviousDerivatives[i] = new double[neuronsCount];
                _thresholdsUpdates[i] = new double[neuronsCount];

                // for each neuron
                for (var j = 0; j < layer.Neurons.Length; j++)
                {
                    _weightsDerivatives[i][j] = new double[layer.InputCount];
                    _weightsPreviousDerivatives[i][j] = new double[layer.InputCount];
                    _weightsUpdates[i][j] = new double[layer.InputCount];
                }
            }

            // intialize steps
            ResetUpdates(_learningRate);
        }

        /// <summary>
        /// Runs learning iteration.
        /// </summary>
        /// 
        /// <param name="input">Input vector.</param>
        /// <param name="output">Desired output vector.</param>
        /// 
        /// <returns>Returns squared error (difference between current network's output and
        /// desired output) divided by 2.</returns>
        /// 
        /// <remarks><para>Runs one learning iteration and updates neuron's
        /// weights.</para></remarks>
        ///
        public double Run(double[] input, double[] output)
        {
            // zero gradient
            ResetGradient();

            // compute the network's output
            _network.Compute(input);

            // calculate network error
            var error = CalculateError(output);

            // calculate weights updates
            CalculateGradient(input);

            // update the network
            UpdateNetwork();

            // return summary error
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
        public double RunEpoch(double[][] input, double[][] output)
        {
            // zero gradient
            ResetGradient();

            var error = 0.0;

            // run learning procedure for all samples
            for (var i = 0; i < input.Length; i++)
            {
                // compute the network's output
                _network.Compute(input[i]);

                // calculate network error
                error += CalculateError(output[i]);

                // calculate weights updates
                CalculateGradient(input[i]);
            }

            // update the network
            UpdateNetwork();

            // return summary error
            return error;
        }

        /// <summary>
        /// Resets current weight and threshold derivatives.
        /// </summary>
        /// 
        private void ResetGradient()
        {
            for (var i = 0; i < _weightsDerivatives.Length; i++)
            {
                for (var j = 0; j < _weightsDerivatives[i].Length; j++)
                {
                    Array.Clear(_weightsDerivatives[i][j], 0, _weightsDerivatives[i][j].Length);
                }
            }

            for (var i = 0; i < _thresholdsDerivatives.Length; i++)
            {
                Array.Clear(_thresholdsDerivatives[i], 0, _thresholdsDerivatives[i].Length);
            }
        }

        /// <summary>
        /// Resets the current update steps using the given learning rate.
        /// </summary>
        /// 
        private void ResetUpdates(double rate)
        {
            for (var i = 0; i < _weightsUpdates.Length; i++)
            {
                for (var j = 0; j < _weightsUpdates[i].Length; j++)
                {
                    for (var k = 0; k < _weightsUpdates[i][j].Length; k++)
                    {
                        _weightsUpdates[i][j][k] = rate;
                    }
                }
            }

            for (var i = 0; i < _thresholdsUpdates.Length; i++)
            {
                for (var j = 0; j < _thresholdsUpdates[i].Length; j++)
                {
                    _thresholdsUpdates[i][j] = rate;
                }
            }
        }

        /// <summary>
        /// Update network's weights.
        /// </summary>
        /// 
        private void UpdateNetwork()
        {
            double[][] layerWeightsUpdates;
            double[] layerThresholdUpdates;
            double[] neuronWeightUpdates;

            double[][] layerWeightsDerivatives;
            double[] layerThresholdDerivatives;
            double[] neuronWeightDerivatives;

            double[][] layerPreviousWeightsDerivatives;
            double[] layerPreviousThresholdDerivatives;
            double[] neuronPreviousWeightDerivatives;

            // for each layer of the network
            for (var i = 0; i < _network.Layers.Length; i++)
            {
                var layer = _network.Layers[i] as ActivationLayer;

                layerWeightsUpdates = _weightsUpdates[i];
                layerThresholdUpdates = _thresholdsUpdates[i];

                layerWeightsDerivatives = _weightsDerivatives[i];
                layerThresholdDerivatives = _thresholdsDerivatives[i];

                layerPreviousWeightsDerivatives = _weightsPreviousDerivatives[i];
                layerPreviousThresholdDerivatives = _thresholdsPreviousDerivatives[i];

                // for each neuron of the layer
                for (var j = 0; j < layer.Neurons.Length; j++)
                {
                    var neuron = layer.Neurons[j] as ActivationNeuron;

                    neuronWeightUpdates = layerWeightsUpdates[j];
                    neuronWeightDerivatives = layerWeightsDerivatives[j];
                    neuronPreviousWeightDerivatives = layerPreviousWeightsDerivatives[j];

                    double s = 0;

                    // for each weight of the neuron
                    for (var k = 0; k < neuron.InputCount; k++)
                    {
                        s = neuronPreviousWeightDerivatives[k] * neuronWeightDerivatives[k];

                        if (s > 0)
                        {
                            neuronWeightUpdates[k] = Math.Min(neuronWeightUpdates[k] * _etaPlus, _deltaMax);
                            neuron.Weights[k] -= Math.Sign(neuronWeightDerivatives[k]) * neuronWeightUpdates[k];
                            neuronPreviousWeightDerivatives[k] = neuronWeightDerivatives[k];
                        }
                        else if (s < 0)
                        {
                            neuronWeightUpdates[k] = Math.Max(neuronWeightUpdates[k] * EtaMinus, _deltaMin);
                            neuronPreviousWeightDerivatives[k] = 0;
                        }
                        else
                        {
                            neuron.Weights[k] -= Math.Sign(neuronWeightDerivatives[k]) * neuronWeightUpdates[k];
                            neuronPreviousWeightDerivatives[k] = neuronWeightDerivatives[k];
                        }
                    }

                    // update treshold
                    s = layerPreviousThresholdDerivatives[j] * layerThresholdDerivatives[j];

                    if (s > 0)
                    {
                        layerThresholdUpdates[j] = Math.Min(layerThresholdUpdates[j] * _etaPlus, _deltaMax);
                        neuron.Threshold -= Math.Sign(layerThresholdDerivatives[j]) * layerThresholdUpdates[j];
                        layerPreviousThresholdDerivatives[j] = layerThresholdDerivatives[j];
                    }
                    else if (s < 0)
                    {
                        layerThresholdUpdates[j] = Math.Max(layerThresholdUpdates[j] * EtaMinus, _deltaMin);
                        layerThresholdDerivatives[j] = 0;
                    }
                    else
                    {
                        neuron.Threshold -= Math.Sign(layerThresholdDerivatives[j]) * layerThresholdUpdates[j];
                        layerPreviousThresholdDerivatives[j] = layerThresholdDerivatives[j];
                    }
                }
            }
        }

        /// <summary>
        /// Calculates error values for all neurons of the network.
        /// </summary>
        /// 
        /// <param name="desiredOutput">Desired output vector.</param>
        /// 
        /// <returns>Returns summary squared error of the last layer divided by 2.</returns>
        /// 
        private double CalculateError(double[] desiredOutput)
        {
            double error = 0;
            var layersCount = _network.Layers.Length;

            // assume, that all neurons of the network have the same activation function
            var function = (_network.Layers[0].Neurons[0] as ActivationNeuron).ActivationFunction;

            // calculate error values for the last layer first
            var layer = _network.Layers[layersCount - 1] as ActivationLayer;
            var layerDerivatives = _neuronErrors[layersCount - 1];

            for (var i = 0; i < layer.Neurons.Length; i++)
            {
                var output = layer.Neurons[i].Output;

                var e = output - desiredOutput[i];
                layerDerivatives[i] = e * function.Derivative2(output);
                error += e * e;
            }

            // calculate error values for other layers
            for (var j = layersCount - 2; j >= 0; j--)
            {
                layer = _network.Layers[j] as ActivationLayer;
                layerDerivatives = _neuronErrors[j];

                var layerNext = _network.Layers[j + 1] as ActivationLayer;
                var nextDerivatives = _neuronErrors[j + 1];

                // for all neurons of the layer
                for (int i = 0, n = layer.Neurons.Length; i < n; i++)
                {
                    var sum = 0.0;

                    for (var k = 0; k < layerNext.Neurons.Length; k++)
                    {
                        sum += nextDerivatives[k] * layerNext.Neurons[k].Weights[i];
                    }

                    layerDerivatives[i] = sum * function.Derivative2(layer.Neurons[i].Output);
                }
            }

            // return squared error of the last layer divided by 2
            return error / 2.0;
        }

        /// <summary>
        /// Calculate weights updates
        /// </summary>
        /// 
        /// <param name="input">Network's input vector.</param>
        /// 
        private void CalculateGradient(double[] input)
        {
            // 1. calculate updates for the first layer
            var layer = _network.Layers[0] as ActivationLayer;
            var weightErrors = _neuronErrors[0];
            var layerWeightsDerivatives = _weightsDerivatives[0];
            var layerThresholdDerivatives = _thresholdsDerivatives[0];

            // So, for each neuron of the first layer:
            for (var i = 0; i < layer.Neurons.Length; i++)
            {
                var neuron = layer.Neurons[i] as ActivationNeuron;
                var neuronWeightDerivatives = layerWeightsDerivatives[i];

                // for each weight of the neuron:
                for (var j = 0; j < neuron.InputCount; j++)
                {
                    neuronWeightDerivatives[j] += weightErrors[i] * input[j];
                }
                layerThresholdDerivatives[i] += weightErrors[i];
            }

            // 2. for all other layers
            for (var k = 1; k < _network.Layers.Length; k++)
            {
                layer = _network.Layers[k] as ActivationLayer;
                weightErrors = _neuronErrors[k];
                layerWeightsDerivatives = _weightsDerivatives[k];
                layerThresholdDerivatives = _thresholdsDerivatives[k];

                var layerPrev = _network.Layers[k - 1] as ActivationLayer;

                // for each neuron of the layer
                for (var i = 0; i < layer.Neurons.Length; i++)
                {
                    var neuron = layer.Neurons[i] as ActivationNeuron;
                    var neuronWeightDerivatives = layerWeightsDerivatives[i];

                    // for each weight of the neuron
                    for (var j = 0; j < layerPrev.Neurons.Length; j++)
                    {
                        neuronWeightDerivatives[j] += weightErrors[i] * layerPrev.Neurons[j].Output;
                    }
                    layerThresholdDerivatives[i] += weightErrors[i];
                }
            }
        }
    }
}