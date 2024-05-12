//-----------------------------------------------------------------------
// <copyright file="HexagonalMapBuilder.cs" company="Zen Design Corp">
//     Copyright (c) Zen Design Corp 2008-2012. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Numerics;

namespace Zen.MachineLearning.Core.Neural.SelfOrganising;

public class HexagonalMapBuilder<T> : RectangularMapBuilder<T> where T : IFloatingPoint<T>, IRootFunctions<T>
{
    public HexagonalMapBuilder(int inputVectorSize, int width, int height, bool toroidal)
        : base(inputVectorSize, width, height, toroidal)
    {
    }

    protected override NeuronLocation<T> CreateLocation(
        string location,
        string[] locations,
        T[] neuronWeights)
    {
        return new HexagonalNeuron<T>(location, locations, neuronWeights);
    }

    protected override NeuronLocation<T> CreateLocation(
        string location,
        string[] locations,
        DistanceNeuron<T> neuron)
    {
        return new HexagonalNeuron<T>(location, locations, neuron);
    }

    internal override NeuronLocation<T> CreateNode(int index)
    {
        var weights = GetWeightsAtIndex(index);
        int x, y;
        var location = GetLocationFromIndex(index, out x, out y);

        var isOdd = index % 2 != 0;
        string[] locations;
        if (isOdd)
        {
            locations = new[]
                {
                    GetLocationFromCoord (x - 1, y),
                    GetLocationFromCoord (x, y - 1),
                    GetLocationFromCoord (x + 1, y),
                    GetLocationFromCoord (x + 1, y + 1),
                    GetLocationFromCoord (x, y + 1),
                    GetLocationFromCoord (x - 1, y + 1)
                };
        }
        else
        {
            locations = new[]
                {
                    GetLocationFromCoord (x - 1, y - 1),
                    GetLocationFromCoord (x, y - 1),
                    GetLocationFromCoord (x + 1, y - 1),
                    GetLocationFromCoord (x + 1, y),
                    GetLocationFromCoord (x, y + 1),
                    GetLocationFromCoord (x - 1, y)
                };
        }

        return CreateLocation(location, locations, weights);
    }
}
