//-----------------------------------------------------------------------
// <copyright file="OctagonalNeuron.cs" company="Zen Design Corp">
//     Copyright (c) Zen Design Corp 2008-2012. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Numerics;

namespace Zen.MachineLearning.Core.Neural.SelfOrganising;

/// <summary>
/// <c>OctagonalPrismNeuron</c> defines a 3D octagonal neuron location.
/// </summary>
public class OctagonalPrismNeuron<T> : OctagonalNeuron<T> where T : IFloatingPoint<T>, IRootFunctions<T>
{
    #region Private Fields
    #endregion

    #region Public Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="OctagonalPrismNeuron"/> class.
    /// </summary>
    /// <param name="location">The location.</param>
    /// <param name="locations">The locations.</param>
    /// <param name="neuronWeights">The neuron weights.</param>
    public OctagonalPrismNeuron(string location, string[] locations, T[] neuronWeights)
        : base(location, locations, neuronWeights)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OctagonalPrismNeuron"/> class.
    /// </summary>
    /// <param name="location">The location.</param>
    /// <param name="locations">The locations.</param>
    /// <param name="neuron">The neuron.</param>
    public OctagonalPrismNeuron(string location, string[] locations, DistanceNeuron<T> neuron)
        : base(location, locations, neuron)
    {
    }
    #endregion

    #region Public Properties
    public string In => GetLocationAt(8);

    public string Out => GetLocationAt(9);

    #endregion

    #region Public Methods
    #endregion

    #region Protected Methods
    #endregion

    #region Private Methods
    #endregion
}
