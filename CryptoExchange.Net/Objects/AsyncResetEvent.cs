using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Objects
{
    /// <summary>
    /// Async auto/manual reset event implementation
    /// </summary>
    public class AsyncResetEvent
    {
        private readonly Queue<TaskCompletionSource<bool>> _waiters = new();
        private readonly bool _autoReset;
        private bool _signaled;
#if NET9_0_OR_GREATER
        private readonly Lock _waitersLock = new Lock();
#else
        private readonly object _waitersLock = new object();
#endif

        /// <summary>
        /// ctor
        /// </summary>
        public AsyncResetEvent(bool initialState = false, bool autoReset = true)
        {
            _signaled = initialState;
            _autoReset = autoReset;
        }

        /// <summary>
        /// Wait for the set event
        /// </summary>
        /// <returns></returns>
        public async Task<bool> WaitAsync(
            TimeSpan? timeout = null,
            CancellationToken ct = default)
        {
            TaskCompletionSource<bool> tcs;

            lock (_waitersLock)
            {
                if (_signaled)
                {
                    // Already was signaled, can return immediately
                    if (_autoReset)
                        _signaled = false;

                    return true;
                }

                tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                _waiters.Enqueue(tcs);
            }

            CancellationTokenSource? delayCts = null;

            try
            {
                if (timeout.HasValue || ct.CanBeCanceled)
                {
                    delayCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

                    var delayTask = Task.Delay(
                        timeout ?? Timeout.InfiniteTimeSpan,
                        delayCts.Token);

                    var completedTask =
                        await Task.WhenAny(tcs.Task, delayTask)
                                  .ConfigureAwait(false);

                    if (completedTask != tcs.Task)
                    {
                        // This was a timeout or cancellation, need to remove tcs from waiters
                        // if the tcs was set instead it will be removed in the Set method
                        if (tcs.TrySetResult(false))
                        {
                            lock (_waitersLock)
                            {
                                // Dequeue and put in the back of the queue again except for the one we need to remove
                                int count = _waiters.Count;
                                for (int i = 0; i < count; i++)
                                {
                                    var w = _waiters.Dequeue();
                                    if (w != tcs)
                                        _waiters.Enqueue(w);
                                }
                            }
                        }

                        return false;
                    }
                }
                else
                {
                    await tcs.Task.ConfigureAwait(false);
                }

                return true;
            }
            finally
            {
                // Actively stop the delay if tcs.Task won
                delayCts?.Cancel();
                delayCts?.Dispose();
            }
        }

        /// <summary>
        /// Signal a waiter
        /// </summary>
        public void Set()
        {
            if (!_autoReset && _signaled)
                // Already signaled and not resetting
                return;

            lock (_waitersLock)
            {
                if (_autoReset)
                {
                    while (_waiters.Count > 0)
                    {
                        // Try to dequeue and set the result
                        // If result setting was not successful it means timeout/cancellation happened at the same time
                        // If this is the case this Set isn't the one setting the result and we need to continue
                        var w = _waiters.Dequeue();
                        if (w.TrySetResult(true))
                            return;
                    }

                    // No queued waiters, set signaled for next waiter
                    _signaled = true;
                }
                else
                {
                    _signaled = true;

                    // Signal all current waiters
                    while (_waiters.Count > 0)
                    {
                        var w = _waiters.Dequeue();
                        w.TrySetResult(true);
                    }
                }
            }
        }
    }
}
