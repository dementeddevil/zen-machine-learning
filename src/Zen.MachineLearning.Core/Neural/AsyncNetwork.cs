// -----------------------------------------------------------------------
// <copyright file="AsyncActivationNetwork.cs" company="Microsoft">
// Copyright © Zen Design Corp 2010 - 2012
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.ObjectPool;

namespace Zen.MachineLearning.Core.Neural
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class AsyncNetwork : IDisposable
    {
        private NetworkPipeline _pipeline;
        private IPropagatorBlock<LayerData, LayerData> _networkPipeline;
        private ActionBlock<LayerData> _resultSink;
        private IDisposable _pipelineToSink;
        private ObjectPool<LayerData> _layerDataPool;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncNetwork"/> class.
        /// </summary>
        /// <param name="network">The network.</param>
        /// <param name="maxDegreeOfParallelismPerLayer">The max degree of parallelism per layer.</param>
        public AsyncNetwork(
            Network network, int maxDegreeOfParallelismPerLayer = 2)
        {
            _pipeline = new NetworkPipeline(network, maxDegreeOfParallelismPerLayer);
            _networkPipeline = _pipeline.Transformer;
            _resultSink = new ActionBlock<LayerData>(
                (result) =>
                {
                    var tcs =
                        (TaskCompletionSource<double[]>)result.State;
                    tcs.TrySetResult(result.Data);
                    _layerDataPool.Return(result);
                    return tcs.Task;
                });
            _pipelineToSink = _networkPipeline.LinkTo(
                _resultSink,
                new DataflowLinkOptions
                {
                    Append = true,
                    PropagateCompletion = true
                });
            _layerDataPool = ObjectPool.Create<LayerData>();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AsyncNetwork"/> class.
        /// </summary>
        ~AsyncNetwork()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets the task that is signalled when all work in the pipeline has
        /// been completed after a call to <see cref="Complete"/>.
        /// </summary>
        /// <value>
        /// The completion <see cref="Task{TResult}"/>.
        /// </value>
        public Task Completion
        {
            get
            {
                if (_resultSink == null)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
                return _resultSink.Completion;
            }
        }

        /// <summary>
        /// Computes the neural output for the specified input asynchronously.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public Task<double[]> ComputeAsync(double[] input)
        {
            ThrowIfDisposed();
            var tcs = new TaskCompletionSource<double[]>();
            var layerData = _layerDataPool.Get();
            layerData.Data = input;
            layerData.State = tcs;
            _networkPipeline.Post(layerData);
            return tcs.Task;
        }

        /// <summary>
        /// Notifies the network that no further calls to <see cref="ComputeAsync"/>
        /// will be made and that the <see cref="Completion"/> task should be
        /// signalled when the last item in the queue has been processed.
        /// </summary>
        public void Complete()
        {
            ThrowIfDisposed();
            _networkPipeline.Complete();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Throws if disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// Thrown if the object has been disposed.
        /// </exception>
        protected void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_isDisposed)
            {
                _isDisposed = true;

                // Ensure the pipeline is completed
                // TODO: Add a timeout to the wait operation
                _networkPipeline.Complete();
                _resultSink.Completion.Wait();

                // Discard and dispose
                _pipelineToSink.Dispose();
                _pipelineToSink = null;
                _networkPipeline = null;
                _resultSink = null;
                _pipeline = null;
                _layerDataPool = null;
            }
        }
    }
}
