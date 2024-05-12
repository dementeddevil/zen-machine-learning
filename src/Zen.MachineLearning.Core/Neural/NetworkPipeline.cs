// -----------------------------------------------------------------------
// <copyright file="AsyncActivationNetwork.cs" company="Microsoft">
// Copyright © Zen Design Corp 2010 - 2012
// </copyright>
// -----------------------------------------------------------------------

using System.Numerics;
using System.Threading.Tasks.Dataflow;

namespace Zen.MachineLearning.Core.Neural;

/// <summary>
/// TODO: Update summary.
/// </summary>
public class NetworkPipeline<T>
    where T : IFloatingPoint<T>
{
    private readonly Network<T> _network;
    private readonly int _maxDegreeOfParallelismPerLayer;

    private ITargetBlock<LayerData<T>> _pipelineHead;
    private ISourceBlock<LayerData<T>> _pipelineTail;

    private List<IPropagatorBlock<LayerData<T>, LayerData<T>>> _pipelineSegments;
    private List<IDisposable> _pipelineLinks;

    private IPropagatorBlock<LayerData<T>, LayerData<T>> _pipeline;

    public NetworkPipeline(
        Network<T> network, int maxDegreeOfParallelismPerLayer = 2)
    {
        _network = network;
        _maxDegreeOfParallelismPerLayer = maxDegreeOfParallelismPerLayer;
        CreatePipeline();
    }

    public IPropagatorBlock<LayerData<T>, LayerData<T>> Transformer => _pipeline;

    private void CreatePipeline()
    {
        _pipelineSegments = new List<IPropagatorBlock<LayerData<T>, LayerData<T>>>();
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

    private TransformBlock<LayerData<T>, LayerData<T>> CreatePipelineLayer(Layer<T> layer)
    {
        return new TransformBlock<LayerData<T>, LayerData<T>>(
            (state) =>
            {
                var tcs =
                    new TaskCompletionSource<LayerData<T>>();
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

public class LayerData<T>
    where T : IFloatingPoint<T>
{
    public LayerData()
    {
    }

    public LayerData(Vector<T> data, object state)
    {
        Data = data;
        State = state;
    }

    public Vector<T> Data
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
