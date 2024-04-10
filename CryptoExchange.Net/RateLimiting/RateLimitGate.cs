using CryptoExchange.Net.Objects;
using CryptoExchange.Net.RateLimiting.Guards;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.RateLimiting
{
    /// <inheritdoc />
    public class RateLimitGate : IRateLimitGate
    {
        private readonly ConcurrentBag<IRateLimitGuard> _guards;
        private RetryAfterGuard? _retryAfterGuard;

        private readonly SemaphoreSlim _semaphore;
        private RateLimitWindowType _windowType = RateLimitWindowType.Sliding;
        private int _waitingCount;

        /// <inheritdoc />
        public event Action<string, HttpMethod?, TimeSpan>? RateLimitTriggered;

        /// <summary>
        /// ctor
        /// </summary>
        public RateLimitGate()
        {
            _guards = new ConcurrentBag<IRateLimitGuard>();
            _semaphore = new SemaphoreSlim(1);
        }

        /// <inheritdoc />
        public async Task<CallResult> ProcessAsync(ILogger logger, RateLimitItemType type, Uri url, HttpMethod? method, bool signed, SecureString? apiKey, int requestWeight, RateLimitingBehaviour rateLimitingBehaviour, CancellationToken ct)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            _waitingCount++;
            try
            {
                return await CheckGuardsAsync(logger, type, url, method, signed, apiKey, requestWeight, rateLimitingBehaviour, ct).ConfigureAwait(false);
            }
            finally
            {
                _waitingCount--;
                _semaphore.Release();
            }
        }

        private async Task<CallResult> CheckGuardsAsync(ILogger logger, RateLimitItemType type, Uri url, HttpMethod? method, bool signed, SecureString? apiKey, int requestWeight, RateLimitingBehaviour rateLimitingBehaviour, CancellationToken ct)
        {
            foreach (var guard in _guards)
            {
                // Check if a wait is needed for this guard
                var result = guard.Check(logger, type, url, method, signed, apiKey, requestWeight);
                if (result.Delay != TimeSpan.Zero && rateLimitingBehaviour == RateLimitingBehaviour.Fail)
                {
                    logger.LogWarning($"Call to {url} failed because of ratelimit guard {guard.Name}; {guard.Description}");
                    return new CallResult(new ClientRateLimitError($"Rate limit check failed on guard {guard.Name}; {guard.Description}"));
                }

                if (result.Delay != TimeSpan.Zero)
                {
                    _semaphore.Release();

                    if (result.Limit == null)
                        logger.LogWarning($"Delaying call to {url} by {result.Delay} because of ratelimit guard {guard.Name}; {guard.Description}");
                    else
                        logger.LogWarning($"Delaying call to {url} by {result.Delay} because of ratelimit guard {guard.Name}; {guard.Description}, Request weight: {requestWeight}, Current: {result.Current}, Limit: {result.Limit}, requests now being limited: {_waitingCount}");
                    
                    RateLimitTriggered?.Invoke(url.ToString(), method, result.Delay);
                    await Task.Delay(result.Delay).ConfigureAwait(false);
                    await _semaphore.WaitAsync().ConfigureAwait(false);
                    return await CheckGuardsAsync(logger, type, url, method, signed, apiKey, requestWeight, rateLimitingBehaviour, ct).ConfigureAwait(false);
                }
            }

            // Apply the weight on each guard
            foreach (var guard in _guards)
            {
                var result = guard.ApplyWeight(type, url, method, signed, apiKey, requestWeight);
                if (result.IsApplied)
                    logger.LogTrace($"Call to {url} passed ratelimit guard {guard.Name}; {guard.Description}, New count: {result.Current}");
            }

            return new CallResult(null);
        }

        /// <inheritdoc />
        public IRateLimitGate AddGuard(IRateLimitGuard guard)
        {
            _guards.Add(guard);
            if (guard is LimitGuard lg)
            {
                lg.SetWindowType(_windowType);
            }
            return this;
        }

        /// <inheritdoc />
        public async Task SetRetryAfterGuardAsync(DateTime retryAfter)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                if (_retryAfterGuard == null)
                {
                    _retryAfterGuard = new RetryAfterGuard(retryAfter);
                    _guards.Add(_retryAfterGuard);
                }
                else
                {
                    _retryAfterGuard.UpdateAfter(retryAfter);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <inheritdoc />
        public IRateLimitGate WithWindowType(RateLimitWindowType type)
        {
            _windowType = type;

            foreach (var guard in _guards.OfType<LimitGuard>())
                guard.SetWindowType(type);

            return this;
        }
    }
}
