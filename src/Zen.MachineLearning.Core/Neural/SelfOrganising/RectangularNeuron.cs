//-----------------------------------------------------------------------
// <copyright file="RectangularNeuron.cs" company="Zen Design Corp">
//     Copyright (c) Zen Design Corp 2008-2012. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Numerics;

namespace Zen.MachineLearning.Core.Neural.SelfOrganising;

/// <summary>
/// <c>RectangularNeuron</c> defines a 2D rectangular neuron location.
/// </summary>
public class RectangularNeuron<T> : NeuronLocation<T> where T : IFloatingPoint<T>, IRootFunctions<T>
{
    #region Private Fields
    #endregion

    #region Public Constructors
    /// <summary>
    /// Initialises an instance of <see cref="T:RectangularNeuron"/>.
    /// </summary>
    /// <param name="weight">The weight.</param>
    public RectangularNeuron(string location, string[] locations, T[] neuronWeights)
        : base(location, locations, neuronWeights)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RectangularNeuron"/> class.
    /// </summary>
    /// <param name="neuron">The neuron.</param>
    public RectangularNeuron(string location, string[] locations, DistanceNeuron<T> neuron)
        : base(location, locations, neuron)
    {
    }
    #endregion

    #region Public Properties
    public string Up => GetLocationAt(0);

    public string Down => GetLocationAt(1);

    public string Left => GetLocationAt(2);

    public string Right => GetLocationAt(3);

    #endregion

    #region Public Methods
    #endregion

    #region Protected Methods
    #endregion

    #region Private Methods
    #endregion
}
