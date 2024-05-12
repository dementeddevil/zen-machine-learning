// -----------------------------------------------------------------------
// <copyright file="DistanceNetworkEx.cs" company="Microsoft">
// Copyright © Zen Design Corp 2010 - 2012
// </copyright>
// -----------------------------------------------------------------------

using System.Numerics;

namespace Zen.MachineLearning.Core.Neural.SelfOrganising;

/// <summary>
/// TODO: Update summary.
/// </summary>
public class DistanceNetworkEx<T> : DistanceNetwork<T> where T : IFloatingPoint<T>, IRootFunctions<T>
{
    #region Private Objects
    private class LocationHolder
    {
        private readonly DistanceNetworkEx<T> _owner;
        private readonly NeuronLocation<T> _location;
        private LocationHolder[]? _hardLocation;

        public LocationHolder(DistanceNetworkEx<T> owner, NeuronLocation<T> location)
        {
            _owner = owner;
            _location = location;
        }

        public NeuronLocation<T> Location => _location;

        public int LocationCount => _location.LocationCount;

        public LocationHolder GetLocationAt(int index)
        {
            if (index < 0 || index >= _location.LocationCount)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (_hardLocation == null)
            {
                _hardLocation = new LocationHolder[_location.LocationCount];
            }
            if (_hardLocation[index] == null)
            {
                var key = _location.GetLocationAt(index);
                var hardLocation = _owner.GetLocationHolderAt(key);
                if (hardLocation == null)
                {
                    throw new ArgumentException("Unable to resolve hard location for index", nameof(index));
                }
                _hardLocation[index] = hardLocation;
            }
            return _hardLocation[index];
        }
    }
    #endregion

    #region Private Fields
    private readonly NeuronMapBuilder<T> _builder;
    private readonly Dictionary<string, LocationHolder> _map =
        new Dictionary<string, LocationHolder>();
    #endregion

    public DistanceNetworkEx(NeuronMapBuilder<T> builder)
        : base(builder.InputVectorSize, builder.TotalNodes)
    {
        _builder = builder;
        for (var index = 0; index < builder.TotalNodes; ++index)
        {
            // Create location node and hookup neuron to network
            var node = builder.CreateNode(index);
            layers[0].Neurons[index] = node.Neuron;

            // Cache the location object in our map
            _map.Add(node.Location, new LocationHolder(this, node));
        }
    }

    public int Width => _builder.Width;

    public int Height => _builder.Height;

    public int Depth => _builder.Depth;

    public NeuronLocation<T>? GetLocationFromIndex(int index)
    {
        var location = _builder.GetLocationFromIndex(index);
        return GetLocationAt(location);
    }

    public NeuronLocation<T>? GetLocationAt(string location)
    {
        var holder = GetLocationHolderAt(location);
        return holder?.Location;
    }

    private LocationHolder? GetLocationHolderAt(string location)
    {
        if (string.IsNullOrEmpty(location) || !_map.ContainsKey(location))
        {
            return null;
        }
        return _map[location];
    }
}
