using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Objects
{
    /// <summary>
    /// Async auto reset based on Stephen Toub`s implementation
    /// https://devblogs.microsoft.com/pfxteam/building-async-coordination-primitives-part-2-asyncautoresetevent/
    /// </summary>
    public class AsyncResetEvent : IDisposable
    {
        private static readonly Task<bool> _completed = Task.FromResult(true);
        private Queue<TaskCompletionSource<bool>> _waits = new Queue<TaskCompletionSource<bool>>();
        private bool _signaled;
        private readonly bool _reset;

        /// <summary>
        /// New AsyncResetEvent
        /// </summary>
        /// <param name="initialState"></param>
        /// <param name="reset"></param>
        public AsyncResetEvent(bool initialState = false, bool reset = true)
        {
            _signaled = initialState;
            _reset = reset;
        }

        /// <summary>
        /// Wait for the AutoResetEvent to be set
        /// </summary>
        /// <returns></returns>
        public Task<bool> WaitAsync(TimeSpan? timeout = null, CancellationToken ct = default)
        {
            lock (_waits)
            {
                if (_signaled)
                {
                    if(_reset)
                        _signaled = false;
                    return _completed;
                }
                else
                {
                    if (ct.IsCancellationRequested)
                        return _completed;

                    var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    if (timeout.HasValue)
                    {
                        var timeoutSource = new CancellationTokenSource(timeout.Value);
                        var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, ct);
                        ct = cancellationSource.Token;
                    }

                    var registration = ct.Register(() =>
                    {
                        lock (_waits)
                        {
                            tcs.TrySetResult(false);

                            // Not the cleanest but it works
                            _waits = new Queue<TaskCompletionSource<bool>>(_waits.Where(i => i != tcs));
                        }
                    }, useSynchronizationContext: false);
                    

                    _waits.Enqueue(tcs);
                    return tcs.Task;
                }
            }
        }

        /// <summary>
        /// Signal a waiter
        /// </summary>
        public void Set()
        {
            lock (_waits)
            {
                if (!_reset)
                {
                    // Act as ManualResetEvent. Once set keep it signaled and signal everyone who is waiting
                    _signaled = true;
                    while (_waits.Count > 0)
                    {
                        var toRelease = _waits.Dequeue();
                        toRelease.TrySetResult(true);
                    }
                }
                else
                {
                    // Act as AutoResetEvent. When set signal 1 waiter
                    if (_waits.Count > 0)
                    {
                        var toRelease = _waits.Dequeue();
                        toRelease.TrySetResult(true);
                    }
                    else if (!_signaled)
                        _signaled = true;
                }
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            _waits.Clear();
        }
    }
}
