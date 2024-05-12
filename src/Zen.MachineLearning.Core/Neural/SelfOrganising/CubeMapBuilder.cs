//-----------------------------------------------------------------------
// <copyright file="CubeMapBuilder.cs" company="Zen Design Corp">
//     Copyright (c) Zen Design Corp 2008-2012. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Numerics;

namespace Zen.MachineLearning.Core.Neural.SelfOrganising
{
    public class CubeMapBuilder<T> : RectangularMapBuilder<T> where T : IFloatingPoint<T>, IRootFunctions<T>
    {
        private readonly int _depth;

        public CubeMapBuilder(int inputVectorSize, int width, int height, int depth, bool toroidal)
            : base(inputVectorSize, width, height, toroidal)
        {
            _depth = depth;
        }

        public override int Depth => _depth;

        public override int TotalNodes => base.TotalNodes * _depth;

        public override string GetLocationFromIndex(int index)
        {
            int x, y, z;
            return GetLocationFromIndex(index, out x, out y, out z);
        }

        public virtual string GetLocationFromIndex(int index, out int x, out int y, out int z)
        {
            // NOTE: index = x + (y * Width) + z * (Width * Height)

            // Calculate 2D area
            var area = Width * Height;

            // Get the z coordinate and adjust index
            z = index / area;
            index %= area;

            var baseLocation = GetLocationFromIndex(index, out x, out y);
            var zLocation = GetZLocation(z);
            return string.Join(",", baseLocation, zLocation);
        }

        public string GetLocationFromCoord(int x, int y, int z)
        {
            var baseLocation = GetLocationFromCoord(x, y);
            var zLocation = GetZLocation(z);
            return string.Join(",", baseLocation, zLocation);
        }

        private string GetZLocation(int z)
        {
            if (z < 0)
            {
                if (IsToroidal)
                {
                    z += _depth;
                }
                else
                {
                    return string.Empty;
                }
            }
            if (z >= _depth)
            {
                if (IsToroidal)
                {
                    z -= _depth;
                }
                else
                {
                    return string.Empty;
                }
            }
            return z.ToString();
        }

        protected override NeuronLocation<T> CreateLocation(
            string location,
            string[] locations,
            T[] neuronWeights)
        {
            return new CubeNeuron<T>(location, locations, neuronWeights);
        }

        protected override NeuronLocation<T> CreateLocation(
            string location,
            string[] locations,
            DistanceNeuron<T> neuron)
        {
            return new CubeNeuron<T>(location, locations, neuron);
        }

        internal override NeuronLocation<T> CreateNode(int index)
        {
            var weights = GetWeightsAtIndex(index);
            int x, y, z;
            var location = GetLocationFromIndex(index, out x, out y, out z);
            return CreateLocation(
                location,
                new[]
                {
                    GetLocationFromCoord (x, y - 1, z),
                    GetLocationFromCoord (x, y + 1, z),
                    GetLocationFromCoord (x- 1, y, z),
                    GetLocationFromCoord (x + 1, y, z),
                    GetLocationFromCoord (x, y, z + 1),
                    GetLocationFromCoord (x, y, z - 1),
                }, weights);
        }
    }
}
