using CryptoExchange.Net.Logging.Extensions;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.RateLimiting.Guards;
using CryptoExchange.Net.RateLimiting.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.RateLimiting
{
    /// <inheritdoc />
    public class RateLimitGate : IRateLimitGate
    {
        private readonly ConcurrentBag<IRateLimitGuard> _guards;
        private readonly SemaphoreSlim _semaphore;
        private readonly string _name;

        private int _waitingCount;

        /// <inheritdoc />
        public event Action<RateLimitEvent>? RateLimitTriggered;

        /// <summary>
        /// ctor
        /// </summary>
        public RateLimitGate(string name)
        {
            _name = name;
            _guards = new ConcurrentBag<IRateLimitGuard>();
            _semaphore = new SemaphoreSlim(1, 1);
        }

        /// <inheritdoc />
        public async Task<CallResult> ProcessAsync(ILogger logger, int itemId, RateLimitItemType type, RequestDefinition definition, string host, string? apiKey, int requestWeight, RateLimitingBehaviour rateLimitingBehaviour, CancellationToken ct)
        {
            await _semaphore.WaitAsync(ct).ConfigureAwait(false);
            bool release = true;
            _waitingCount++;
            try
            {
                return await CheckGuardsAsync(_guards, logger, itemId, type, definition, host, apiKey, requestWeight, rateLimitingBehaviour, ct).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                // The semaphore has already been released if the task was cancelled
                release = false;
                return new CallResult(new CancellationRequestedError());
            }
            finally
            {
                _waitingCount--;
                if (release)
                    _semaphore.Release();
            }
        }

        /// <inheritdoc />
        public async Task<CallResult> ProcessSingleAsync(
            ILogger logger,
            int itemId,
            IRateLimitGuard guard,
            RateLimitItemType type,
            RequestDefinition definition, 
            string host,
            string? apiKey,
            RateLimitingBehaviour rateLimitingBehaviour,
            CancellationToken ct)
        {
            await _semaphore.WaitAsync(ct).ConfigureAwait(false);
            bool release = true;
            _waitingCount++;
            try
            {
                return await CheckGuardsAsync(new IRateLimitGuard[] { guard }, logger, itemId, type, definition, host, apiKey, 1, rateLimitingBehaviour, ct).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                // The semaphore has already been released if the task was cancelled
                release = false;
                return new CallResult(new CancellationRequestedError());
            }
            finally
            {
                _waitingCount--;
                if (release)
                    _semaphore.Release();
            }
        }

        private async Task<CallResult> CheckGuardsAsync(IEnumerable<IRateLimitGuard> guards, ILogger logger, int itemId, RateLimitItemType type, RequestDefinition definition, string host, string? apiKey, int requestWeight, RateLimitingBehaviour rateLimitingBehaviour, CancellationToken ct)
        {
            foreach (var guard in guards)
            {
                // Check if a wait is needed for this guard
                var result = guard.Check(type, definition, host, apiKey, requestWeight);
                if (result.Delay != TimeSpan.Zero && rateLimitingBehaviour == RateLimitingBehaviour.Fail)
                {
                    // Delay is needed and limit behaviour is to fail the request
                    if (type == RateLimitItemType.Connection)
                        logger.RateLimitConnectionFailed(itemId, guard.Name, guard.Description);
                    else
                        logger.RateLimitRequestFailed(itemId, definition.Path, guard.Name, guard.Description);
                    
                    RateLimitTriggered?.Invoke(new RateLimitEvent(_name, guard.Description, definition, host, result.Current, requestWeight, result.Limit, result.Period, result.Delay, rateLimitingBehaviour));
                    return new CallResult(new ClientRateLimitError($"Rate limit check failed on guard {guard.Name}; {guard.Description}"));
                }

                if (result.Delay != TimeSpan.Zero)
                {
                    // Delay is needed and limit behaviour is to wait for the request to be under the limit
                    _semaphore.Release();

                    var description = result.Limit == null ? guard.Description : $"{guard.Description}, Request weight: {requestWeight}, Current: {result.Current}, Limit: {result.Limit}, requests now being limited: {_waitingCount}";
                    if (type == RateLimitItemType.Connection)
                        logger.RateLimitDelayingConnection(itemId, result.Delay, guard.Name, description);
                    else
                        logger.RateLimitDelayingRequest(itemId, definition.Path, result.Delay, guard.Name, description);

                    RateLimitTriggered?.Invoke(new RateLimitEvent(_name, guard.Description, definition, host, result.Current, requestWeight, result.Limit, result.Period, result.Delay, rateLimitingBehaviour));
                    await Task.Delay((int)result.Delay.TotalMilliseconds + 1, ct).ConfigureAwait(false);
                    await _semaphore.WaitAsync(ct).ConfigureAwait(false);
                    return await CheckGuardsAsync(guards, logger, itemId, type, definition, host, apiKey, requestWeight, rateLimitingBehaviour, ct).ConfigureAwait(false);
                }
            }

            // Apply the weight on each guard
            foreach (var guard in guards)
            {
                var result = guard.ApplyWeight(type, definition, host, apiKey, requestWeight);
                if (result.IsApplied)
                {
                    if (type == RateLimitItemType.Connection)
                        logger.RateLimitAppliedConnection(itemId, guard.Name, guard.Description, result.Current);
                    else
                        logger.RateLimitAppliedRequest(itemId, definition.Path, guard.Name, guard.Description, result.Current);
                }
            }

            return new CallResult(null);
        }

        /// <inheritdoc />
        public IRateLimitGate AddGuard(IRateLimitGuard guard)
        {
            _guards.Add(guard);
            return this;
        }

        /// <inheritdoc />
        public async Task SetRetryAfterGuardAsync(DateTime retryAfter, RateLimitItemType type)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                var retryAfterGuard = _guards.OfType<RetryAfterGuard>().SingleOrDefault();
                if (retryAfterGuard == null)
                    _guards.Add(new RetryAfterGuard(retryAfter, type));
                else
                    retryAfterGuard.UpdateAfter(retryAfter);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <inheritdoc />
        public async Task<DateTime?> GetRetryAfterTime()
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                var retryAfterGuard = _guards.OfType<RetryAfterGuard>().SingleOrDefault();
                return retryAfterGuard?.After;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
