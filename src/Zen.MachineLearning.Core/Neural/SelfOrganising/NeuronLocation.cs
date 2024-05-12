//-----------------------------------------------------------------------
// <copyright file="NeuronLocation.cs" company="Zen Design Corp">
//     Copyright (c) Zen Design Corp 2008-2012. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Numerics;

namespace Zen.MachineLearning.Core.Neural.SelfOrganising;

public class NeuronLocation<T> where T : IFloatingPoint<T>, IRootFunctions<T>
{
    #region Private Fields
    private readonly string _location;
    private readonly string[] _locations;
    private readonly DistanceNeuron<T> _neuron;
    #endregion

    #region Public Constructors
    /// <summary>
    /// Initialises an instance of <see cref="T:NeuronLocation"/>.
    /// </summary>
    /// <param name="location">The location.</param>
    /// <param name="locations">The locations.</param>
    /// <param name="neuronWeights">The neuron weights.</param>
    public NeuronLocation(string location, string[] locations, T[] neuronWeights)
    {
        _location = location;
        _locations = locations;
        _neuron = new DistanceNeuron<T>(neuronWeights.Length);
        _neuron.Weights = new Vector<T>(neuronWeights);
    }

    /// <summary>
    /// Initialises an instance of <see cref="T:NeuronLocation"/>.
    /// </summary>
    /// <param name="location">The location.</param>
    /// <param name="locations">The locations.</param>
    /// <param name="neuron">The neuron.</param>
    public NeuronLocation(string location, string[] locations, DistanceNeuron<T> neuron)
    {
        _location = location;
        _locations = locations;
        _neuron = neuron;
    }
    #endregion

    #region Public Properties
    public string Location => _location;

    public int LocationCount => _locations.Length;

    public DistanceNeuron<T> Neuron => _neuron;

    public Vector<T> Weights => _neuron.Weights;

    #endregion

    #region Public Methods
    /// <summary>
    /// Gets the location at.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns></returns>
    public string GetLocationAt(int index)
    {
        return _locations[index];
    }

    /// <summary>
    /// Gets the euclidean distance.
    /// </summary>
    /// <param name="weights">The weights.</param>
    /// <returns></returns>
    public T Compute(Vector<T> weights)
    {
        return _neuron.Compute(weights);
    }

    /*public void UpdateWeights(double[] weights, double scale)
		{
			_neuron.UpdateWeights(weights, scale);
		}*/
    #endregion

    #region Protected Methods
    #endregion

    #region Private Methods
    #endregion
}
