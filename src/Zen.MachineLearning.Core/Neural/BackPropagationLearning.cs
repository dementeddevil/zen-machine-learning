namespace Zen.MachineLearning.Core.Neural
{
    using System;
    using System.Numerics;

    /// <summary>
    /// Back propagation learning algorithm.
    /// </summary>
    /// 
    /// <remarks><para>The class implements back propagation learning algorithm,
    /// which is widely used for training multi-layer neural networks with
    /// continuous activation functions.</para>
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
    /// BackPropagationLearning teacher = new BackPropagationLearning( network );
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
    /// <seealso cref="EvolutionaryLearning"/>
    /// 
    public class BackPropagationLearning<T> : ISupervisedLearning<T>
        where T : IFloatingPoint<T>
    {
        // network to teach
        private readonly ActivationNetwork<T> _network;
        // learning rate
        private double _learningRate = 0.1;
        // momentum
        private double _momentum = 0.0;

        private readonly double[][] _neuronErrors = null;

        // update values, also known as deltas
        private readonly double[][][] _weightsUpdates = null;
        private readonly double[][] _thresholdsUpdates = null;

        /// <summary>
        /// Learning rate, [0, 1].
        /// </summary>
        /// 
        /// <remarks><para>The value determines speed of learning.</para>
        /// 
        /// <para>Default value equals to <b>0.1</b>.</para>
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
                var min = 0.0;
                var max = 1.0;
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
        /// Momentum, [0, 1].
        /// </summary>
        /// 
        /// <remarks><para>The value determines the portion of previous weight's update
        /// to use on current iteration. Weight's update values are calculated on
        /// each iteration depending on neuron's error. The momentum specifies the amount
        /// of update to use from previous iteration and the amount of update
        /// to use from current iteration. If the value is equal to 0.1, for example,
        /// then 0.1 portion of previous update and 0.9 portion of current update are used
        /// to update weight's value.</para>
        /// 
        /// <para>Default value equals to <b>0.0</b>.</para>
        ///	</remarks>
        /// 
        public double Momentum
        {
            get
            {
                return _momentum;
            }
            set
            {
                var min = 0.0;
                var max = 1.0;
                if (value < min)
                {
                    _momentum = min;
                }
                else if (value > max)
                {
                    _momentum = max;
                }
                else
                {
                    _momentum = value;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackPropagationLearning"/> class.
        /// </summary>
        /// 
        /// <param name="network">Network to teach.</param>
        /// 
        public BackPropagationLearning(ActivationNetwork<T> network)
        {
            _network = network;

            // create error and deltas arrays
            _neuronErrors = new double[network.Layers.Length][];
            _weightsUpdates = new double[network.Layers.Length][][];
            _thresholdsUpdates = new double[network.Layers.Length][];

            // initialize errors and deltas arrays for each layer
            for (var i = 0; i < network.Layers.Length; i++)
            {
                var layer = network.Layers[i];

                _neuronErrors[i] = new double[layer.Neurons.Length];
                _weightsUpdates[i] = new double[layer.Neurons.Length][];
                _thresholdsUpdates[i] = new double[layer.Neurons.Length];

                // for each neuron
                for (var j = 0; j < _weightsUpdates[i].Length; j++)
                {
                    _weightsUpdates[i][j] = new double[layer.InputCount];
                }
            }
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
        public T Run(Vector<T> input, Vector<T> output)
        {
            // compute the network's output
            _network.Compute(input);

            // calculate network error
            var error = CalculateError(output);

            // calculate weights updates
            CalculateUpdates(input);

            // update the network
            UpdateNetwork();

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

        /// <summary>
        /// Calculates error values for all neurons of the network.
        /// </summary>
        /// 
        /// <param name="desiredOutput">Desired output vector.</param>
        /// 
        /// <returns>Returns summary squared error of the last layer divided by 2.</returns>
        /// 
        private T CalculateError(Vector<T> desiredOutput)
        {
            // current and the next layers
            Layer<T> layer, layerNext;
            // current and the next errors arrays
            double[] errors, errorsNext;
            // error values
            T error = T.Zero, e, sum;
            // neuron's output value
            T output;
            // layers count
            var layersCount = _network.Layers.Length;

            // assume, that all neurons of the network have the same activation function
            var function = (_network.Layers[0].Neurons[0] as ActivationNeuron<T>).ActivationFunction;

            // calculate error values for the last layer first
            layer = _network.Layers[layersCount - 1];
            errors = _neuronErrors[layersCount - 1];

            for (var i = 0; i < layer.Neurons.Length; i++)
            {
                output = layer.Neurons[i].Output;

                // error of the neuron
                e = desiredOutput[i] - output;

                // error multiplied with activation function's derivative
                errors[i] = Double.CreateChecked(e * function.Derivative2(output));

                // squre the error and sum it
                error += e * e;
            }

            // calculate error values for other layers
            for (var j = layersCount - 2; j >= 0; j--)
            {
                layer = _network.Layers[j];
                layerNext = _network.Layers[j + 1];
                errors = _neuronErrors[j];
                errorsNext = _neuronErrors[j + 1];

                // for all neurons of the layer
                for (var i = 0; i < layer.Neurons.Length; i++)
                {
                    sum = T.CreateChecked(0.0);

                    // for all neurons of the next layer
                    for (var k = 0; k < layerNext.Neurons.Length; k++)
                    {
                        sum += T.CreateChecked(errorsNext[k]) * layerNext.Neurons[k].Weights[i];
                    }

                    errors[i] = Double.CreateChecked(sum * function.Derivative2(layer.Neurons[i].Output));
                }
            }

            // return squared error of the last layer divided by 2
            return error / T.CreateChecked(2.0);
        }

        /// <summary>
        /// Calculate weights updates.
        /// </summary>
        /// 
        /// <param name="input">Network's input vector.</param>
        /// 
        private void CalculateUpdates(Vector<T> input)
        {
            // current neuron
            Neuron<T> neuron;
            // current and previous layers
            Layer<T> layer, layerPrev;
            // layer's weights updates
            double[][] layerWeightsUpdates;
            // layer's thresholds updates
            double[] layerThresholdUpdates;
            // layer's error
            double[] errors;
            // neuron's weights updates
            double[] neuronWeightUpdates;
            // error value
            // double		error;

            // 1 - calculate updates for the first layer
            layer = _network.Layers[0];
            errors = _neuronErrors[0];
            layerWeightsUpdates = _weightsUpdates[0];
            layerThresholdUpdates = _thresholdsUpdates[0];

            // cache for frequently used values
            var cachedMomentum = _learningRate * _momentum;
            var cached1MMomentum = _learningRate * (1.0 - _momentum);
            double cachedError;

            // for each neuron of the layer
            for (var i = 0; i < layer.Neurons.Length; i++)
            {
                neuron = layer.Neurons[i];
                cachedError = errors[i] * cached1MMomentum;
                neuronWeightUpdates = layerWeightsUpdates[i];

                // for each weight of the neuron
                for (var j = 0; j < neuronWeightUpdates.Length; j++)
                {
                    // calculate weight update
                    neuronWeightUpdates[j] = 
                        cachedMomentum * neuronWeightUpdates[j] +
                        cachedError * Double.CreateChecked(input[j]);
                }

                // calculate treshold update
                layerThresholdUpdates[i] = cachedMomentum * layerThresholdUpdates[i] + cachedError;
            }

            // 2 - for all other layers
            for (var k = 1; k < _network.Layers.Length; k++)
            {
                layerPrev = _network.Layers[k - 1];
                layer = _network.Layers[k];
                errors = _neuronErrors[k];
                layerWeightsUpdates = _weightsUpdates[k];
                layerThresholdUpdates = _thresholdsUpdates[k];

                // for each neuron of the layer
                for (var i = 0; i < layer.Neurons.Length; i++)
                {
                    neuron = layer.Neurons[i];
                    cachedError = errors[i] * cached1MMomentum;
                    neuronWeightUpdates = layerWeightsUpdates[i];

                    // for each synapse of the neuron
                    for (var j = 0; j < neuronWeightUpdates.Length; j++)
                    {
                        // calculate weight update
                        neuronWeightUpdates[j] =
                            cachedMomentum * neuronWeightUpdates[j] +
                            cachedError * Double.CreateChecked( layerPrev.Neurons[j].Output);
                    }

                    // calculate treshold update
                    layerThresholdUpdates[i] = cachedMomentum * layerThresholdUpdates[i] + cachedError;
                }
            }
        }

        /// <summary>
        /// Update network'sweights.
        /// </summary>
        /// 
        private void UpdateNetwork()
        {
            // for each layer of the network
            for (var i = 0; i < _network.Layers.Length; i++)
            {
                var layer = _network.Layers[i];
                var layerWeightsUpdates = _weightsUpdates[i];
                var layerThresholdUpdates = _thresholdsUpdates[i];

                // for each neuron of the layer
                for (var j = 0; j < layer.Neurons.Length; j++)
                {
                    var neuron = layer.Neurons[j] as ActivationNeuron<T>;
                    var neuronWeightUpdates = layerWeightsUpdates[j];

                    // for each weight of the neuron
                    for (var k = 0; k < neuron.InputCount; k++)
                    {
                        // update weight
                        neuron.Weights = neuron.Weights.WithElement(
                            k, neuron.Weights[k] + T.CreateChecked(neuronWeightUpdates[k]));
                    }

                    // update treshold
                    neuron.Threshold += T.CreateChecked(layerThresholdUpdates[j]);
                }
            }
        }
    }
}
