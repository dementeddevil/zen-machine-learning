//-----------------------------------------------------------------------
// <copyright file="OctagonalPrismMapBuilder.cs" company="Zen Design Corp">
//     Copyright (c) Zen Design Corp 2008-2012. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Numerics;

namespace Zen.MachineLearning.Core.Neural.SelfOrganising;

/// <summary>
/// <c>OctagonalPrismMapBuilder</c> builds 3D octagonal prism/cube maps.
/// </summary>
public class OctagonalPrismMapBuilder<T> : CubeMapBuilder<T> where T : IFloatingPoint<T>, IRootFunctions<T>
{
    public OctagonalPrismMapBuilder(int inputVectorSize, int width, int height, int depth, bool toroidal)
        : base(inputVectorSize, width, height, depth, toroidal)
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
            if (depth % 2 == 1)
            {
                throw new ArgumentException("Depth must be an even number for toroidal maps.", nameof(depth));
            }
        }
    }

    public override string GetLocationFromIndex(int index, out int x, out int y, out int z)
    {
        var location = base.GetLocationFromIndex(index, out x, out y, out z);
        var isOct = false;
        if (y % 2 == 0)
        {
            isOct = x % 2 == 0;
        }
        else
        {
            isOct = x % 2 == 1;
        }
        return $"{location}:{(isOct ? "O" : "R")}";
    }

    protected override NeuronLocation<T> CreateLocation(
        string location,
        string[] locations,
        T[] neuronWeights)
    {
        if (location.EndsWith(":O"))
        {
            return new OctagonalPrismNeuron<T>(location, locations, neuronWeights);
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
            return new OctagonalPrismNeuron<T>(location, locations, neuron);
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
        int x, y, z;
        var location = GetLocationFromIndex(index, out x, out y, out z);
        var isOct = location.EndsWith(":O");
        string[] locations;
        if (isOct)
        {
            locations = new[]
                {
                    GetLocationFromCoord (x - 1, y -1, z),
                    GetLocationFromCoord (x, y - 1, z),
                    GetLocationFromCoord (x + 1, y - 1, z),
                    GetLocationFromCoord (x - 1, y, z),
                    GetLocationFromCoord (x + 1, y, z),
                    GetLocationFromCoord (x - 1, y + 1, z),
                    GetLocationFromCoord (x, y + 1, z),
                    GetLocationFromCoord (x + 1, y + 1, z),
                    GetLocationFromCoord (x, y, z - 1),
                    GetLocationFromCoord (x, y, z + 1)
                };
        }
        else
        {
            locations = new[]
                {
                    GetLocationFromCoord (x, y - 1, z),
                    GetLocationFromCoord (x, y + 1, z),
                    GetLocationFromCoord (x - 1, y, z),
                    GetLocationFromCoord (x + 1, y, z),
                    GetLocationFromCoord (x, y, z - 1),
                    GetLocationFromCoord (x, y, z + 1)
                };
        }

        return CreateLocation(location, locations, weights);
    }
}
