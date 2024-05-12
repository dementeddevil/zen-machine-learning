//-----------------------------------------------------------------------
// <copyright file="OctagonalMapBuilder.cs" company="Zen Design Corp">
//     Copyright (c) Zen Design Corp 2008-2012. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Numerics;

namespace Zen.MachineLearning.Core.Neural.SelfOrganising;

/// <summary>
/// <c>OctagonalMapBuilder</c> builds 2D octagonal/rectagular maps.
/// </summary>
public class OctagonalMapBuilder<T> : RectangularMapBuilder<T> where T : IFloatingPoint<T>, IRootFunctions<T>
{
    public OctagonalMapBuilder(int inputVectorSize, int width, int height, bool toroidal)
        : base(inputVectorSize, width, height, toroidal)
    {
        // Width and height must be even numbers if a toroidal map is desired
        if (toroidal)
        {
            if (width % 2 == 1)
            {
                throw new ArgumentException("Width must be an even number for toroidal maps.", nameof(width));
            }
            if (height % 2 == 1)
            {
                throw new ArgumentException("Height must be an even number for toroidal maps.", nameof(height));
            }
        }
    }

    public override string GetLocationFromIndex(int index, out int x, out int y)
    {
        var location = base.GetLocationFromIndex(index, out x, out y);
        var isOct = false;
        if (y % 2 == 0)
        {
            isOct = x % 2 == 0;
        }
        else
        {
            isOct = x % 2 == 1;
        }
        return $"{location}:{(isOct ? ":O" : ":R")}";
    }

    protected override NeuronLocation<T> CreateLocation(
        string location,
        string[] locations,
        T[] neuronWeights)
    {
        if (location.EndsWith(":O"))
        {
            return new OctagonalNeuron<T>(location, locations, neuronWeights);
        }
        else
        {
            return base.CreateLocation(location, locations, neuronWeights);
        }
    }

    protected override NeuronLocation<T> CreateLocation(
        string location,
        string[] locations,
        DistanceNeuron<T> neuron)
    {
        if (location.EndsWith(":O"))
        {
            return new OctagonalNeuron<T>(location, locations, neuron);
        }
        else
        {
            return base.CreateLocation(location, locations, neuron);
        }
    }

    internal override NeuronLocation<T> CreateNode(int index)
    {
        // Get neuron weights
        var weights = GetWeightsAtIndex(index);

        // Determine location array
        int x, y;
        var location = GetLocationFromIndex(index, out x, out y);
        var isOct = location.EndsWith(":O");
        string[] locations;
        if (isOct)
        {
            locations = new[]
                {
                    GetLocationFromCoord (x - 1, y -1),
                    GetLocationFromCoord (x, y - 1),
                    GetLocationFromCoord (x + 1, y - 1),

                    GetLocationFromCoord (x - 1, y),
                    GetLocationFromCoord (x + 1, y),

                    GetLocationFromCoord (x - 1, y + 1),
                    GetLocationFromCoord (x, y + 1),
                    GetLocationFromCoord (x + 1, y + 1)
                };
        }
        else
        {
            locations = new[]
                {
                    GetLocationFromCoord (x, y - 1),
                    GetLocationFromCoord (x, y + 1),
                    GetLocationFromCoord (x - 1, y),
                    GetLocationFromCoord (x + 1, y)
                };
        }

        return CreateLocation(location, locations, weights);
    }
}
