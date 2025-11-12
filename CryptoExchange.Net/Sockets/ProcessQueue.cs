using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Sockets
{

    /// <summary>
    /// Queue for processing items
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public class ProcessQueue<T>
    {
        private readonly Channel<T> _channel;
        private readonly Func<T, Task> _processor;
        private Task? _processTask;
        private CancellationTokenSource? _cts;
        private bool _processTillEmpty;

        /// <summary>
        /// Event for when an exception is thrown in the processing handler
        /// </summary>
        public event Action<Exception>? Exception;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="processor">The function to async handle the updates</param>
        /// <param name="maxQueuedItems">The max number of items to be queued up. When happens when the queue is full and a new write is attempted can be specified with <see>fullMode</see></param>
        /// <param name="fullBehavior">What should happen if the queue contains <see>maxQueuedItems</see> pending items. If no max is set this setting is ignored</param>
        public ProcessQueue(Func<T, Task> processor, int? maxQueuedItems = null, QueueFullBehavior? fullBehavior = null)
        {
            _processor = processor;
            if (maxQueuedItems == null)
            {
                _channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions
                {
                    AllowSynchronousContinuations = false,
                    SingleReader = true,
                    SingleWriter = true
                });
            }
            else
            {
                _channel = Channel.CreateBounded<T>(new BoundedChannelOptions(maxQueuedItems.Value)
                {
                    AllowSynchronousContinuations = false,
                    SingleReader = true,
                    SingleWriter = true,
                    FullMode = MapMode(fullBehavior)
                });
            }
        }

        private BoundedChannelFullMode MapMode(QueueFullBehavior? behavior) =>
            behavior switch
            {
                QueueFullBehavior.DropOldest => BoundedChannelFullMode.DropOldest,
                QueueFullBehavior.DropNewest => BoundedChannelFullMode.DropNewest,
                QueueFullBehavior.DropWrite => BoundedChannelFullMode.DropWrite,
               _ => BoundedChannelFullMode.DropWrite
            };

        /// <summary>
        /// Start the processing of the queue
        /// </summary>
        public Task StartAsync()
        {
            _cts = new CancellationTokenSource();
            _processTask = Task.Run(async () =>
            {
                try
                {
                    await foreach (var item in _channel.Reader.ReadAllAsync(_cts.Token).ConfigureAwait(false))
                    {
                        if (_cts.IsCancellationRequested && !_processTillEmpty) // Items might still be processed even if CT is canceled
                            return;

                        try
                        {
                            await _processor.Invoke(item).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Exception?.Invoke(ex);
                        }
                    }
                }
                catch (OperationCanceledException) { }                
            });

            return Task.CompletedTask;
        }

        /// <summary>
        /// Stop processing the queue
        /// </summary>
        /// <param name="discardPending">Whether updates still pending in the queue should be discarded</param>
        /// <returns></returns>
        public async Task StopAsync(bool discardPending = true)
        {
            if (_processTask == null)
                return;

            _processTillEmpty = !discardPending;
            _cts!.Cancel();

            await _processTask.ConfigureAwait(false);
            _channel.Writer.TryComplete(_processTask.Exception);
        }

        /// <summary>
        /// Write an update to queue
        /// </summary>
        public bool Write(T item)
        {
            if (_cts?.IsCancellationRequested == true)
                return false;

            var write = _channel.Writer.TryWrite(item);
            if (!write)
                LibraryHelpers.StaticLogger?.Log(LogLevel.Warning, "Failed to write item to process queue. Item will be discarded");

            return write;
        }
    }
}
