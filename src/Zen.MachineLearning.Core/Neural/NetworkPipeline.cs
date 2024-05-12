// -----------------------------------------------------------------------
// <copyright file="AsyncActivationNetwork.cs" company="Microsoft">
// Copyright © Zen Design Corp 2010 - 2012
// </copyright>
// -----------------------------------------------------------------------

namespace Zen.MachineLearning.Core.Neural
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class NetworkPipeline
    {
        private readonly Network _network;
        private readonly int _maxDegreeOfParallelismPerLayer;

        private ITargetBlock<LayerData> _pipelineHead;
        private ISourceBlock<LayerData> _pipelineTail;

        private List<IPropagatorBlock<LayerData, LayerData>> _pipelineSegments;
        private List<IDisposable> _pipelineLinks;

        private IPropagatorBlock<LayerData, LayerData> _pipeline;

        public NetworkPipeline(
            Network network, int maxDegreeOfParallelismPerLayer = 2)
        {
            _network = network;
            _maxDegreeOfParallelismPerLayer = maxDegreeOfParallelismPerLayer;
            CreatePipeline();
        }

        public IPropagatorBlock<LayerData, LayerData> Transformer => _pipeline;

        private void CreatePipeline()
        {
            _pipelineSegments = new List<IPropagatorBlock<LayerData, LayerData>>();
            for (var index = 0; index < _network.Layers.Length; ++index)
            {
                // Create pipeline segment and add to our list
                var pipelineBlock = CreatePipelineLayer(_network.Layers[index]);
                _pipelineSegments.Add(pipelineBlock);

                // Link segment to any existing segments
                if (_pipelineHead == null)
                {
                    _pipelineHead = pipelineBlock;
                    _pipelineTail = pipelineBlock;
                }
                else
                {
                    // Ensure we have linkage holder
                    if (_pipelineLinks == null)
                    {
                        _pipelineLinks = new List<IDisposable>();
                    }

                    // Link new pipeline block to tail and update tail
                    _pipelineLinks.Add(_pipelineTail.LinkTo(
                        pipelineBlock,
                        new DataflowLinkOptions
                        {
                            Append = true,
                            PropagateCompletion = true
                        }));
                    _pipelineTail = pipelineBlock;
                }
            }

            // Finally create the pipeline transform block
            _pipeline = DataflowBlock.Encapsulate(_pipelineHead, _pipelineTail);
        }

        private TransformBlock<LayerData, LayerData> CreatePipelineLayer(Layer layer)
        {
            return new TransformBlock<LayerData, LayerData>(
                (state) =>
                {
                    var tcs =
                        new TaskCompletionSource<LayerData>();
                    try
                    {
                        state.Data = layer.Compute(state.Data);
                        tcs.SetResult(state);
                    }
                    catch (Exception exception)
                    {
                        tcs.TrySetException(exception);
                    }
                    return tcs.Task;
                },
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = _maxDegreeOfParallelismPerLayer
                });
        }
    }

    public class LayerData
    {
        public LayerData()
        {
        }

        public LayerData(double[] data, object state)
        {
            Data = data;
            State = state;
        }

        public double[] Data
        {
            get;
            set;
        }

        public object State
        {
            get;
            set;
        }
    }
}
