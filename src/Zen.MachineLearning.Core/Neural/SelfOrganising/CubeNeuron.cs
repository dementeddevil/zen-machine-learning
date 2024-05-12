//-----------------------------------------------------------------------
// <copyright file="CubeNeuron.cs" company="Zen Design Corp">
//     Copyright (c) Zen Design Corp 2008-2012. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Numerics;

namespace Zen.MachineLearning.Core.Neural.SelfOrganising
{
    /// <summary>
    /// <c>CubeNeuron</c> defines a six sided neuron cube.
    /// </summary>
    public class CubeNeuron<T> : RectangularNeuron<T> where T : IFloatingPoint<T>, IRootFunctions<T>
    {
        #region Private Fields
        #endregion

        #region Public Constructors
        /// <summary>
        /// Initialises an instance of <see cref="T:CubeNeuron"/>.
        /// </summary>
        /// <param name="weight">The weight.</param>
        public CubeNeuron(
            string location,
            string[] locations,
            T[] neuronWeights)
            : base(location, locations, neuronWeights)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:CubeNeuron"/> class.
        /// </summary>
        /// <param name="neuron">The neuron.</param>
        public CubeNeuron(
            string location,
            string[] locations,
            DistanceNeuron<T> neuron)
            : base(location, locations, neuron)
        {
        }
        #endregion

        #region Public Properties
        public string In => GetLocationAt(4);

        public string Out => GetLocationAt(5);

        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        #endregion
    }
}
