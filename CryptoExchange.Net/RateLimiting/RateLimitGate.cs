using CryptoExchange.Net.Objects;
using CryptoExchange.Net.RateLimiting.Filters;
using CryptoExchange.Net.RateLimiting.Guards;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.RateLimiting
{
    /// <inheritdoc />
    public class RateLimitGate : IRateLimitGate
    {
        private readonly IRateLimitGuard _singleLimitGuard = new SingleLimitGuard();
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
            _semaphore = new SemaphoreSlim(1);
        }

        /// <inheritdoc />
        public async Task<CallResult> ProcessAsync(ILogger logger, RateLimitItemType type, RequestDefinition definition, string host, SecureString? apiKey, int requestWeight, RateLimitingBehaviour rateLimitingBehaviour, CancellationToken ct)
        {
            await _semaphore.WaitAsync(ct).ConfigureAwait(false);
            _waitingCount++;
            try
            {
                return await CheckGuardsAsync(_guards, logger, type, definition, host, apiKey, requestWeight, rateLimitingBehaviour, ct).ConfigureAwait(false);
            }
            finally
            {
                _waitingCount--;
                _semaphore.Release();
            }
        }

        /// <inheritdoc />
        public async Task<CallResult> ProcessSingleAsync(ILogger logger, RateLimitItemType type, RequestDefinition definition, string host, SecureString? apiKey, int requestWeight, RateLimitingBehaviour rateLimitingBehaviour, CancellationToken ct)
        {
            await _semaphore.WaitAsync(ct).ConfigureAwait(false);
            if (requestWeight == 0)
                requestWeight = 1;

            _waitingCount++;
            try
            {
                return await CheckGuardsAsync(new IRateLimitGuard[] { _singleLimitGuard }, logger, type, definition, host, apiKey, requestWeight, rateLimitingBehaviour, ct).ConfigureAwait(false);
            }
            finally
            {
                _waitingCount--;
                _semaphore.Release();
            }
        }

        private async Task<CallResult> CheckGuardsAsync(IEnumerable<IRateLimitGuard> guards, ILogger logger, RateLimitItemType type, RequestDefinition definition, string host, SecureString? apiKey, int requestWeight, RateLimitingBehaviour rateLimitingBehaviour, CancellationToken ct)
        {
            foreach (var guard in guards)
            {
                // Check if a wait is needed for this guard
                var result = guard.Check(type, definition, host, apiKey, requestWeight);
                if (result.Delay != TimeSpan.Zero && rateLimitingBehaviour == RateLimitingBehaviour.Fail)
                {
                    logger.LogWarning($"[{_name}] Call to {definition.Path} failed because of ratelimit guard {guard.Name}; {guard.Description}");
                    RateLimitTriggered?.Invoke(new RateLimitEvent(_name, guard.Description, definition, host, result.Current, requestWeight, result.Limit, result.Period, result.Delay, rateLimitingBehaviour));
                    return new CallResult(new ClientRateLimitError($"Rate limit check failed on guard {guard.Name}; {guard.Description}"));
                }

                if (result.Delay != TimeSpan.Zero)
                {
                    _semaphore.Release();

                    if (result.Limit == null)
                        logger.LogWarning($"[{_name}] Delaying call to {definition.Path} by {result.Delay} because of ratelimit guard {guard.Name}; {guard.Description}");
                    else
                        logger.LogWarning($"[{_name}] Delaying call to {definition.Path} by {result.Delay} because of ratelimit guard {guard.Name}; {guard.Description}, Request weight: {requestWeight}, Current: {result.Current}, Limit: {result.Limit}, requests now being limited: {_waitingCount}");

                    RateLimitTriggered?.Invoke(new RateLimitEvent(_name, guard.Description, definition, host, result.Current, requestWeight, result.Limit, result.Period, result.Delay, rateLimitingBehaviour));
                    await Task.Delay(result.Delay).ConfigureAwait(false);
                    await _semaphore.WaitAsync().ConfigureAwait(false);
                    return await CheckGuardsAsync(guards, logger, type, definition, host, apiKey, requestWeight, rateLimitingBehaviour, ct).ConfigureAwait(false);
                }
            }

            // Apply the weight on each guard
            foreach (var guard in guards)
            {
                var result = guard.ApplyWeight(type, definition, host, apiKey, requestWeight);
                if (result.IsApplied)
                    logger.LogTrace($"[{_name}] Call to {definition.Path} passed ratelimit guard {guard.Name}; {guard.Description}, New count: {result.Current}");
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
        public async Task SetRetryAfterGuardAsync(DateTime retryAfter)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                var retryAfterGuard = _guards.OfType<RetryAfterGuard>().SingleOrDefault();
                if (retryAfterGuard == null)
                    _guards.Add(new RetryAfterGuard(retryAfter));
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
