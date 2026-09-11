using CryptoExchange.Net.Logging.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Objects
{
    /// <summary>
    /// Request coalescer, used to detect multiple identical requests and wait for and return only a single actual request result
    /// </summary>
    public class RequestCoalescer
    {
        private sealed class RequestEntry
        {
            public RequestCoalescingKey Key { get; }

            public CancellationTokenSource CancellationSource { get; } = new();

            public Lazy<Task<object>> Request { get; set; } = null!;

            public int ListenerCount { get; set; }

            public bool Completed { get; set; }

            public bool CancellationInProgress { get; set; }

            public bool Disposed { get; set; }

            public RequestEntry(RequestCoalescingKey key)
            {
                Key = key;
            }
        }

        private readonly object _sync = new();
        private readonly ILogger _logger;

        private readonly Dictionary<RequestCoalescingKey, RequestEntry> _requests = new();

        /// <summary>
        /// ctor
        /// </summary>
        public RequestCoalescer(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute a request, coalescing identical requests into a single request
        /// </summary>
        public Task<TResult> ExecuteAsync<TResult>(
            RequestCoalescingKey key,
            Func<CancellationToken, Task<TResult>> requestFactory,
            Func<TResult> cancellationResultFactory,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromResult(cancellationResultFactory());

            RequestEntry entry;
            bool joined;

            lock (_sync)
            {
                joined = _requests.TryGetValue(key, out entry!);

                if (!joined)
                {
                    entry = CreateEntry(key, requestFactory);
                    _requests.Add(key, entry);
                }

                entry.ListenerCount++;
            }

            if (joined)
                _logger.RestApiRequestsJoined(key.Method, key.Url);

            return WaitAsync(
                entry,
                cancellationResultFactory,
                cancellationToken);
        }

        private RequestEntry CreateEntry<TResult>(
            RequestCoalescingKey key,
            Func<CancellationToken, Task<TResult>> requestFactory)
        {
            var entry = new RequestEntry(key);

            entry.Request = new Lazy<Task<object>>(
                () => ExecuteCoreAsync(
                    entry,
                    async requestCancellationToken =>
                        (await requestFactory(requestCancellationToken).ConfigureAwait(false))!),
                LazyThreadSafetyMode.ExecutionAndPublication);

            return entry;
        }

        private async Task<object> ExecuteCoreAsync(
            RequestEntry entry,
            Func<CancellationToken, Task<object>> requestFactory)
        {
            try
            {
                return await requestFactory(entry.CancellationSource.Token).ConfigureAwait(false);
            }
            finally
            {
                CompleteRequest(entry);
            }
        }

        private async Task<TResult> WaitAsync<TResult>(
            RequestEntry entry,
            Func<TResult> cancellationResultFactory,
            CancellationToken cancellationToken)
        {
            try
            {
                var requestTask = entry.Request.Value;

                if (!cancellationToken.CanBeCanceled)
                    return (TResult)await requestTask.ConfigureAwait(false);

                var cancellationCompletion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

                using (cancellationToken.Register(
                    state => ((TaskCompletionSource<bool>)state!).TrySetResult(true),
                    cancellationCompletion))
                {
                    var completedTask = await Task.WhenAny(requestTask, cancellationCompletion.Task).ConfigureAwait(false);
                    if (completedTask != requestTask)
                        return cancellationResultFactory();
                }

                return (TResult)await requestTask.ConfigureAwait(false);
            }
            finally
            {
                ReleaseListener(entry);
            }
        }

        private void ReleaseListener(RequestEntry entry)
        {
            var cancelRequest = false;
            var disposeCancellationSource = false;

            lock (_sync)
            {
                entry.ListenerCount--;

                if (entry.ListenerCount == 0)
                {
                    if (!entry.Completed)
                    {
                        // Remove the entry before canceling it. A caller arriving
                        // after this point must create a new physical request
                        // instead of joining one that is being canceled.
                        RemoveEntry(entry);

                        entry.CancellationInProgress = true;
                        cancelRequest = true;
                    }
                    else if (!entry.Disposed)
                    {
                        entry.Disposed = true;
                        disposeCancellationSource = true;
                    }
                }
            }

            if (cancelRequest)
            {
                try
                {
                    entry.CancellationSource.Cancel();
                }
                catch (Exception exception)
                {
                    // Cancellation callbacks are external to the coalescer and
                    // must not cause an exception to escape the library.
                    _logger.LogWarning(
                        exception,
                        "Error canceling in-flight request {Method} {Url}",
                        entry.Key.Method,
                        entry.Key.Url);
                }
                finally
                {
                    CompleteCancellation(entry);
                }
            }
            else if (disposeCancellationSource)
            {
                entry.CancellationSource.Dispose();
            }
        }

        private void CompleteCancellation(RequestEntry entry)
        {
            var disposeCancellationSource = false;

            lock (_sync)
            {
                entry.CancellationInProgress = false;

                if (entry.Completed
                    && entry.ListenerCount == 0
                    && !entry.Disposed)
                {
                    entry.Disposed = true;
                    disposeCancellationSource = true;
                }
            }

            if (disposeCancellationSource)
                entry.CancellationSource.Dispose();
        }

        private void CompleteRequest(RequestEntry entry)
        {
            var disposeCancellationSource = false;

            lock (_sync)
            {
                entry.Completed = true;

                // Only remove this specific entry. If all listeners canceled,
                // a replacement request may already have been created for the
                // same key.
                RemoveEntry(entry);

                if (entry.ListenerCount == 0
                    && !entry.CancellationInProgress
                    && !entry.Disposed)
                {
                    entry.Disposed = true;
                    disposeCancellationSource = true;
                }
            }

            if (disposeCancellationSource)
                entry.CancellationSource.Dispose();
        }

        /// <summary>
        /// Remove an entry when it is still the current entry for its key.
        /// Must only be called while holding <see cref="_sync"/>.
        /// </summary>
        private void RemoveEntry(RequestEntry entry)
        {
            if (_requests.TryGetValue(entry.Key, out var current)
                && ReferenceEquals(current, entry))
            {
                _requests.Remove(entry.Key);
            }
        }
    }

    /// <summary>
    /// Key used to identify identical requests for coalescing
    /// </summary>
    /// <param name="Method">Request method</param>
    /// <param name="Url">Request URL</param>
    /// <param name="Parameters">Request parameter string</param>
    /// <param name="ResponseType">Response type</param>
    public sealed record RequestCoalescingKey(
        string Method,
        string Url,
        string? Parameters,
        Type ResponseType);
}