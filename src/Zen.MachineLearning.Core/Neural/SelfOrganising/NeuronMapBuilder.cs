//-----------------------------------------------------------------------
// <copyright file="NeuronMapBuilder.cs" company="Zen Design Corp">
//     Copyright (c) Zen Design Corp 2008-2012. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Zen.MachineLearning.Core.Neural.SelfOrganising
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;

    /// <summary>
    /// <c>NeuronMapBuilder</c> defines the base class for building neuron maps.
    /// </summary>
    public class NeuronMapBuilder<T> where T : IFloatingPoint<T>, IRootFunctions<T>
    {
        private readonly int _inputVectorSize;
        private readonly int _width;
        private readonly int _height;
        private readonly bool _isToroidal;

        private T _minValue;
        private T _maxValue;

        private bool _randomiseSpace;
        private Random _random;

        public NeuronMapBuilder(int inputVectorSize, int width, int height, bool toroidal)
        {
            _inputVectorSize = inputVectorSize;
            _width = width;
            _height = height;
            _isToroidal = toroidal;
            _minValue = T.NegativeOne;
            _maxValue = T.One;
        }

        public int Width => _width;

        public int Height => _height;

        public virtual int Depth => 1;

        public bool IsToroidal => _isToroidal;

        public int InputVectorSize => _inputVectorSize;

        public virtual int TotalNodes => _width * _height;

        public T MinValue
        {
            get
            {
                return _minValue;
            }
            set
            {
                _minValue = value;
            }
        }

        public T MaxValue
        {
            get
            {
                return _maxValue;
            }
            set
            {
                _maxValue = value;
            }
        }

        public bool RandomiseSpace
        {
            get
            {
                return _randomiseSpace;
            }
            set
            {
                _randomiseSpace = value;
            }
        }

        public DistanceNetworkEx<T> Initialise()
        {
            return new DistanceNetworkEx<T>(this);
        }

        public virtual string GetLocationFromIndex(int index)
        {
            return index.ToString();
        }

        protected virtual T[] GetWeightsAtIndex(int index)
        {
            var weights = new List<T>(_inputVectorSize);
            if (!_randomiseSpace)
            {
                var weight = _minValue +
                    T.CreateChecked(index) * (_maxValue - _minValue) / T.CreateChecked(TotalNodes);
                for (var weightIndex = 0; weightIndex < _inputVectorSize; ++weightIndex)
                {
                    weights.Add(weight);
                }
            }
            else
            {
                if (_random == null)
                {
                    _random = new Random();
                }
                for (var weightIndex = 0; weightIndex < _inputVectorSize; ++weightIndex)
                {
                    weights.Add(_minValue +
                        T.CreateChecked(_random.NextDouble()) * (_maxValue - _minValue));
                }
            }
            return weights.ToArray();
        }

        protected virtual NeuronLocation<T> CreateLocation(string location,
            string[] locations, T[] neuronWeights)
        {
            throw new NotImplementedException();
        }

        protected virtual NeuronLocation<T> CreateLocation(string location,
            string[] locations, DistanceNeuron<T> neuron)
        {
            throw new NotImplementedException();
        }

        internal virtual NeuronLocation<T> CreateNode(int index)
        {
            throw new NotImplementedException();
        }
    }
}
