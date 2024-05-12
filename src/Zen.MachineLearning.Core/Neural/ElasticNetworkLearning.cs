namespace Zen.MachineLearning.Core.Neural
{
    using System;

    /// <summary>
    /// Elastic network learning algorithm.
    /// </summary>
    ///
    /// <remarks><para>This class implements elastic network's learning algorithm and
    /// allows to train <see cref="DistanceNetwork">Distance Networks</see>.</para>
    /// </remarks> 
    ///
    public class ElasticNetworkLearning : IUnsupervisedLearning
    {
        // neural network to train
        private readonly DistanceNetwork _network;

        // array of distances between neurons
        private readonly double[] _distance;

        // learning rate
        private double _learningRate = 0.1;
        // learning radius
        private double _learningRadius = 0.5;

        // squared learning radius multiplied by 2 (precalculated value to speed up computations)
        private double _squaredRadius2 = 2 * 7 * 7;

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
                return _learningRate;
            }
            set
            {
                _learningRate = Math.Max(0.0, Math.Min(1.0, value));
            }
        }

        /// <summary>
        /// Learning radius, [0, 1].
        /// </summary>
        /// 
        /// <remarks><para>Determines the amount of neurons to be updated around
        /// winner neuron. Neurons, which are in the circle of specified radius,
        /// are updated during the learning procedure. Neurons, which are closer
        /// to the winner neuron, get more update.</para>
        /// 
        /// <para>Default value equals to <b>0.5</b>.</para>
        /// </remarks>
        /// 
        public double LearningRadius
        {
            get
            {
                return _learningRadius;
            }
            set
            {
                _learningRadius = Math.Max(0, Math.Min(1.0, value));
                _squaredRadius2 = 2 * _learningRadius * _learningRadius;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElasticNetworkLearning"/> class.
        /// </summary>
        /// 
        /// <param name="network">Neural network to train.</param>
        /// 
        public ElasticNetworkLearning(DistanceNetwork network)
        {
            _network = network;

            // precalculate distances array
            var neurons = network.Layers[0].Neurons.Length;
            var deltaAlpha = Math.PI * 2.0 / neurons;
            var alpha = deltaAlpha;

            _distance = new double[neurons];
            _distance[0] = 0.0;

            // calculate all distance values
            for (var i = 1; i < neurons; i++)
            {
                var dx = 0.5 * Math.Cos(alpha) - 0.5;
                var dy = 0.5 * Math.Sin(alpha);

                _distance[i] = dx * dx + dy * dy;

                alpha += deltaAlpha;
            }
        }

        /// <summary>
        /// Runs learning iteration.
        /// </summary>
        /// 
        /// <param name="input">Input vector.</param>
        /// 
        /// <returns>Returns learning error - summary absolute difference between neurons'
        /// weights and appropriate inputs. The difference is measured according to the neurons
        /// distance to the winner neuron.</returns>
        /// 
        /// <remarks><para>The method runs one learning iterations - finds winner neuron (the neuron
        /// which has weights with values closest to the specified input vector) and updates its weight
        /// (as well as weights of neighbor neurons) in the way to decrease difference with the specified
        /// input vector.</para></remarks>
        /// 
        public double Run(double[] input)
        {
            var error = 0.0;

            // compute the network
            _network.Compute(input);
            var winner = _network.GetWinner();

            // get layer of the network
            var layer = _network.Layers[0];

            // walk through all neurons of the layer
            for (var j = 0; j < layer.Neurons.Length; j++)
            {
                var neuron = layer.Neurons[j];

                // update factor
                var factor = Math.Exp(-_distance[Math.Abs(j - winner)] / _squaredRadius2);

                // update weights of the neuron
                for (var i = 0; i < neuron.Weights.Length; i++)
                {
                    // calculate the error
                    var e = (input[i] - neuron.Weights[i]) * factor;
                    error += Math.Abs(e);
                    // update weight
                    neuron.Weights[i] += e * _learningRate;
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
        public double RunEpoch(double[][] input)
        {
            var error = 0.0;

            // walk through all training samples
            foreach (var sample in input)
            {
                error += Run(sample);
            }

            // return summary error
            return error;
        }
    }
}
