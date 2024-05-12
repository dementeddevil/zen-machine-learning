//-----------------------------------------------------------------------
// <copyright file="HexagonalNeuron.cs" company="Zen Design Corp">
//     Copyright (c) Zen Design Corp 2008-2012. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Numerics;

namespace Zen.MachineLearning.Core.Neural.SelfOrganising;

/// <summary>
/// <c>HexagonalNeuron</c> defines a 2D hexagonal neuron location.
/// </summary>
public class HexagonalNeuron<T> : NeuronLocation<T> where T : IFloatingPoint<T>, IRootFunctions<T>
{
    #region Private Fields
    #endregion

    #region Public Constructors
    /// <summary>
    /// Initialises an instance of <see cref="T:HexagonalNeuron"/>.
    /// </summary>
    /// <param name="location">The location.</param>
    /// <param name="locations">The locations.</param>
    /// <param name="neuronWeights">The neuron weights.</param>
    public HexagonalNeuron(
        string location,
        string[] locations,
        T[] neuronWeights)
        : base(location, locations, neuronWeights)
    {
    }

    /// <summary>
    /// Initialises an instance of <see cref="T:HexagonalNeuron"/>.
    /// </summary>
    /// <param name="location">The location.</param>
    /// <param name="locations">The locations.</param>
    /// <param name="neuron">The neuron.</param>
    public HexagonalNeuron(
        string location,
        string[] locations,
        DistanceNeuron<T> neuron)
        : base(location, locations, neuron)
    {
    }
    #endregion

    #region Public Properties
    #endregion

    #region Public Methods
    public string LeftUp => GetLocationAt(0);

    public string Up => GetLocationAt(1);

    public string RightUp => GetLocationAt(2);

    public string LeftDown => GetLocationAt(3);

    public string Down => GetLocationAt(4);

    public string RightDown => GetLocationAt(5);

    #endregion

    #region Protected Methods
    #endregion

    #region Private Methods
    #endregion
}
