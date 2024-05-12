namespace Zen.MachineLearning.Core.Neural
{
    using System;
    using System.IO;
    using System.Numerics;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    /// <summary>
    /// Base neural network class.
    /// </summary>
    /// 
    /// <remarks>This is a base neural netwok class, which represents
    /// collection of neuron's layers.</remarks>
    /// 
    [Serializable]
    public abstract class Network<T> where T : IFloatingPoint<T>
    {
        /// <summary>
        /// Network's inputs count.
        /// </summary>
        protected int inputsCount;

        /// <summary>
        /// Network's layers count.
        /// </summary>
        protected int LayersCount;

        /// <summary>
        /// Network's layers.
        /// </summary>
        protected Layer<T>[] layers;

        /// <summary>
        /// Network's output vector.
        /// </summary>
        protected T[] output;

        /// <summary>
        /// Network's inputs count.
        /// </summary>
        public int InputsCount => inputsCount;

        /// <summary>
        /// Network's layers.
        /// </summary>
        public Layer<T>[] Layers => layers;

        /// <summary>
        /// Network's output vector.
        /// </summary>
        /// 
        /// <remarks><para>The calculation way of network's output vector is determined by
        /// layers, which comprise the network.</para>
        /// 
        /// <para><note>The property is not initialized (equals to <see langword="null"/>) until
        /// <see cref="Compute"/> method is called.</note></para>
        /// </remarks>
        /// 
        public T[] Output => output;


        /// <summary>
        /// Initializes a new instance of the <see cref="Network"/> class.
        /// </summary>
        /// 
        /// <param name="inputsCount">Network's inputs count.</param>
        /// <param name="layersCount">Network's layers count.</param>
        /// 
        /// <remarks>Protected constructor, which initializes <see cref="inputsCount"/>,
        /// <see cref="LayersCount"/> and <see cref="layers"/> members.</remarks>
        /// 
        protected Network(int inputsCount, int layersCount)
        {
            this.inputsCount = Math.Max(1, inputsCount);
            LayersCount = Math.Max(1, layersCount);
            // create collection of layers
            layers = new Layer<T>[LayersCount];
        }

        /// <summary>
        /// Compute output vector of the network.
        /// </summary>
        /// 
        /// <param name="input">Input vector.</param>
        /// 
        /// <returns>Returns network's output vector.</returns>
        /// 
        /// <remarks><para>The actual network's output vecor is determined by layers,
        /// which comprise the layer - represents an output vector of the last layer
        /// of the network. The output vector is also stored in <see cref="Output"/> property.</para>
        /// 
        /// <para><note>The method may be called safely from multiple threads to compute network's
        /// output value for the specified input values. However, the value of
        /// <see cref="Output"/> property in multi-threaded environment is not predictable,
        /// since it may hold network's output computed from any of the caller threads. Multi-threaded
        /// access to the method is useful in those cases when it is required to improve performance
        /// by utilizing several threads and the computation is based on the immediate return value
        /// of the method, but not on network's output property.</note></para>
        /// </remarks>
        /// 
        public virtual Vector<T> Compute(Vector<T> input)
        {
            // local variable to avoid mutlithread conflicts
            var output = input;

            // compute each layer
            for (var i = 0; i < layers.Length; i++)
            {
                output = layers[i].Compute(output);
            }

            // assign output property as well (works correctly for single threaded usage)
            this.output = output;

            return output;
        }

        /// <summary>
        /// Randomize layers of the network.
        /// </summary>
        /// 
        /// <remarks>Randomizes network's layers by calling <see cref="Layer.Randomize"/> method
        /// of each layer.</remarks>
        /// 
        public virtual void Randomize()
        {
            foreach (var layer in layers)
            {
                layer.Randomize();
            }
        }

        /// <summary>
        /// Save network to specified file.
        /// </summary>
        /// 
        /// <param name="fileName">File name to save network into.</param>
        /// 
        /// <remarks><para>The neural network is saved using .NET serialization (binary formatter is used).</para></remarks>
        /// 
        public void Save(string fileName)
        {
            var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
            Save(stream);
            stream.Close();
        }

        /// <summary>
        /// Save network to specified file.
        /// </summary>
        /// 
        /// <param name="stream">Stream to save network into.</param>
        /// 
        /// <remarks><para>The neural network is saved using .NET serialization (binary formatter is used).</para></remarks>
        /// 
        public void Save(Stream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);
        }

        /// <summary>
        /// Load network from specified file.
        /// </summary>
        /// 
        /// <param name="fileName">File name to load network from.</param>
        /// 
        /// <returns>Returns instance of <see cref="Network"/> class with all properties initialized from file.</returns>
        /// 
        /// <remarks><para>Neural network is loaded from file using .NET serialization (binary formater is used).</para></remarks>
        /// 
        public static Network<T> Load(string fileName)
        {
            var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            var network = Load(stream);
            stream.Close();

            return network;
        }

        /// <summary>
        /// Load network from specified file.
        /// </summary>
        /// 
        /// <param name="stream">Stream to load network from.</param>
        /// 
        /// <returns>Returns instance of <see cref="Network"/> class with all properties initialized from file.</returns>
        /// 
        /// <remarks><para>Neural network is loaded from file using .NET serialization (binary formater is used).</para></remarks>
        /// 
        public static Network<T> Load(Stream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            var network = (Network<T>)formatter.Deserialize(stream);
            return network;
        }
    }
}
