//-----------------------------------------------------------------------
// <copyright file="RectangularMapBuilder.cs" company="Zen Design Corp">
//     Copyright (c) Zen Design Corp 2008-2012. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Numerics;

namespace Zen.MachineLearning.Core.Neural.SelfOrganising;

public class RectangularMapBuilder<T> : NeuronMapBuilder<T> where T : IFloatingPoint<T>, IRootFunctions<T>
{
    public RectangularMapBuilder(int inputVectorSize, int width, int height, bool toroidal)
        : base(inputVectorSize, width, height, toroidal)
    {
    }

    public override string GetLocationFromIndex(int index)
    {
        int x, y;
        return GetLocationFromIndex(index, out x, out y);
    }

    public virtual string GetLocationFromIndex(int index, out int x, out int y)
    {
        y = index / Width;
        x = index % Width;
        return $"{x},{y}";
    }

    public string GetLocationFromCoord(int x, int y)
    {
        if (x < 0)
        {
            if (IsToroidal)
            {
                x += Width;
            }
            else
            {
                return string.Empty;
            }
        }
        else if (x >= Width)
        {
            if (IsToroidal)
            {
                x -= Width;
            }
            else
            {
                return string.Empty;
            }
        }
        if (y < 0)
        {
            if (IsToroidal)
            {
                y += Height;
            }
            else
            {
                return string.Empty;
            }
        }
        else if (y >= Height)
        {
            if (IsToroidal)
            {
                y -= Height;
            }
            else
            {
                return string.Empty;
            }
        }
        return $"{x},{y}";
    }

    protected override NeuronLocation<T> CreateLocation(
        string location,
        string[] locations,
        T[] neuronWeights)
    {
        return new RectangularNeuron<T>(location, locations, neuronWeights);
    }

    protected override NeuronLocation<T> CreateLocation(
        string location,
        string[] locations,
        DistanceNeuron<T> neuron)
    {
        return new RectangularNeuron<T>(location, locations, neuron);
    }

    internal override NeuronLocation<T> CreateNode(int index)
    {
        var weights = GetWeightsAtIndex(index);
        int x, y;
        var location = GetLocationFromIndex(index, out x, out y);
        return (RectangularNeuron<T>)CreateLocation(
            location,
            new[]
            {
                GetLocationFromCoord (x, y - 1),
                GetLocationFromCoord (x, y + 1),
                GetLocationFromCoord (x- 1, y),
                GetLocationFromCoord (x + 1, y)
            }, weights);
    }
}
