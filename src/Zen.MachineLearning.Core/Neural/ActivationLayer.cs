namespace Zen.MachineLearning.Core.Neural
{
    using System;

    /// <summary>
    /// Activation layer.
    /// </summary>
    /// 
    /// <remarks>Activation layer is a layer of <see cref="ActivationNeuron">activation neurons</see>.
    /// The layer is usually used in multi-layer neural networks.</remarks>
    ///
    [Serializable]
    public class ActivationLayer : Layer
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
        public ActivationLayer(int neuronCount, int inputCount, IActivationFunction function)
            : base(neuronCount, inputCount)
        {
            // create each neuron
            for (var i = 0; i < _neurons.Length; i++)
            {
                _neurons[i] = new ActivationNeuron(inputCount, function);
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
        public void SetActivationFunction(IActivationFunction function)
        {
            foreach (var neuron in _neurons)
            {
                ((ActivationNeuron)neuron).ActivationFunction = function;
            }
        }
    }
}
