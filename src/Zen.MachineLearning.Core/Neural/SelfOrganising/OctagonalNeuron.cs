//-----------------------------------------------------------------------
// <copyright file="OctagonalNeuron.cs" company="Zen Design Corp">
//     Copyright (c) Zen Design Corp 2008-2012. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Numerics;

namespace Zen.MachineLearning.Core.Neural.SelfOrganising;

/// <summary>
/// <c>OctagonalNeuron</c> defines a 2D octagonal neuron location.
/// </summary>
public class OctagonalNeuron<T> : NeuronLocation<T> where T : IFloatingPoint<T>, IRootFunctions<T>
{
    #region Private Fields
    #endregion

    #region Public Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="OctagonalNeuron"/> class.
    /// </summary>
    /// <param name="location">The location.</param>
    /// <param name="locations">The locations.</param>
    /// <param name="neuronWeights">The neuron weights.</param>
    public OctagonalNeuron(string location, string[] locations, T[] neuronWeights)
        : base(location, locations, neuronWeights)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OctagonalNeuron"/> class.
    /// </summary>
    /// <param name="location">The location.</param>
    /// <param name="locations">The locations.</param>
    /// <param name="neuron">The neuron.</param>
    public OctagonalNeuron(string location, string[] locations, DistanceNeuron<T> neuron)
        : base(location, locations, neuron)
    {
    }
    #endregion

    #region Public Properties
    public string LeftUp => GetLocationAt(0);

    public string Up => GetLocationAt(1);

    public string RightUp => GetLocationAt(2);

    public string Left => GetLocationAt(3);

    public string Right => GetLocationAt(4);

    public string LeftDown => GetLocationAt(5);

    public string Down => GetLocationAt(6);

    public string RightDown => GetLocationAt(7);

    #endregion

    #region Public Methods
    #endregion

    #region Protected Methods
    #endregion

    #region Private Methods
    #endregion
}
