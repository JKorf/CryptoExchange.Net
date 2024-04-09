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
    [Flags]
    public enum RateLimitType
    {
        Connection = 1,
        Request = 2
    }

    public class RateLimitGate : IRateLimitGate
    {
        private readonly ConcurrentBag<IRateLimitGuard> _guards;
        private RetryAfterGuard _retryAfterGuard;

        private readonly SemaphoreSlim _semaphore;
        private RateLimitWindowType _windowType = RateLimitWindowType.Sliding;
        private int _waitingCount;

        public event Action<string, HttpMethod?, TimeSpan> RateLimitTriggered;

        public RateLimitGate()
        {
            _guards = new ConcurrentBag<IRateLimitGuard>();
            _semaphore = new SemaphoreSlim(1);
        }

        public async Task<CallResult> ProcessAsync(ILogger logger, RateLimitType type, Uri url, HttpMethod method, bool signed, SecureString? apiKey, int requestWeight, RateLimitingBehaviour rateLimitingBehaviour, CancellationToken ct)
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

        private async Task<CallResult> CheckGuardsAsync(ILogger logger, RateLimitType type, Uri url, HttpMethod? method, bool signed, SecureString? apiKey, int requestWeight, RateLimitingBehaviour rateLimitingBehaviour, CancellationToken ct)
        {
            foreach (var guard in _guards)
            {
                // Check if a wait is needed for this guard
                var result = guard.Check(logger, type, url, method, signed, apiKey, requestWeight);
                var tracker = guard.GetTracker(type, url, method, signed, apiKey);

                if (result != TimeSpan.Zero && rateLimitingBehaviour == RateLimitingBehaviour.Fail)
                {
                    if (tracker == null)
                        logger.LogWarning($"Call to {url} failed because of ratelimit guard {guard.Name}");
                    else
                        logger.LogWarning($"Call to {url} failed because of ratelimit guard {guard.Name}; Request weight: {requestWeight}, Count {tracker?.Current}, Limit: {tracker?.Limit}, requests now being limited: {_waitingCount}");

                    return new CallResult(new ClientRateLimitError($"Rate limit check failed on guard {guard.Name}"));
                }

                if (result != TimeSpan.Zero)
                {
                    _semaphore.Release();

                    if (tracker == null)
                        logger.LogWarning($"Delaying call to {url} by {result} because of ratelimit guard {guard.Name}");
                    else
                        logger.LogWarning($"Delaying call to {url} by {result} because of ratelimit guard {guard.Name}; Request weight: {requestWeight}, Count {tracker?.Current}, Limit: {tracker?.Limit}, requests now being limited: {_waitingCount}");
                    
                    RateLimitTriggered?.Invoke(url.ToString(), method, result);
                    await Task.Delay(result).ConfigureAwait(false);
                    await _semaphore.WaitAsync().ConfigureAwait(false);
                    return await CheckGuardsAsync(logger, type, url, method, signed, apiKey, requestWeight, rateLimitingBehaviour, ct).ConfigureAwait(false);
                }
            }

            // Apply the weight on each guard
            foreach (var guard in _guards)
            {
                guard.Enter(type, url, method, signed, apiKey, requestWeight);

                var tracker = guard.GetTracker(type, url, method, signed, apiKey);
                if (tracker != null)
                    logger.LogTrace($"Call to {url} passed ratelimit guard {guard.Name}; Request weight: {requestWeight}, New count: {tracker.Current}, Limit: {tracker?.Limit}");
            }

            return new CallResult(null);
        }

        public IRateLimitGate AddGuard(IRateLimitGuard guard, int initialCount = 0)
        {
            _guards.Add(guard);
            if (guard is LimitGuard lg)
            {
                lg.SetInitialCount(initialCount);
                lg.SetWindowType(_windowType);
            }
            return this;
        }

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
                    _retryAfterGuard.UpdateAfter(retryAfter);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public IRateLimitGate WithWindowType(RateLimitWindowType type)
        {
            _windowType = type;

            foreach (var guard in _guards.OfType<LimitGuard>())
                guard.SetWindowType(type);

            return this;
        }
    }
}
