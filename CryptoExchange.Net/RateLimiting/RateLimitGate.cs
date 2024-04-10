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
using System.Xml.Linq;

namespace CryptoExchange.Net.RateLimiting
{
    /// <inheritdoc />
    public class RateLimitGate : IRateLimitGate
    {
        private EndpointIndividualLimitGuard? _singleEndpointGuard;
        private readonly ConcurrentBag<IRateLimitGuard> _guards;
        private readonly SemaphoreSlim _semaphore;
        private readonly string _name;

        private RetryAfterGuard? _retryAfterGuard;
        private RateLimitWindowType _windowType = RateLimitWindowType.Sliding;
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
        public async Task<CallResult> ProcessAsync(ILogger logger, RateLimitItemType type, Uri url, HttpMethod? method, bool signed, SecureString? apiKey, int requestWeight, RateLimitingBehaviour rateLimitingBehaviour, CancellationToken ct)
        {
            await _semaphore.WaitAsync(ct).ConfigureAwait(false);
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

        /// <inheritdoc />
        public async Task<CallResult> ProcessSingleAsync(ILogger logger, string key, int limit, TimeSpan period, RateLimitItemType type, Uri url, HttpMethod? method, int requestWeight, RateLimitingBehaviour rateLimitingBehaviour, CancellationToken ct)
        {
            await _semaphore.WaitAsync(ct).ConfigureAwait(false);
            _waitingCount++;
            try
            {
               return await CheckGuardAsync(logger, key, limit, period, type, url, method, requestWeight, rateLimitingBehaviour, ct).ConfigureAwait(false);
            }
            finally
            {
                _waitingCount--;
                _semaphore.Release();
            }
        }

        private async Task<CallResult> CheckGuardAsync(ILogger logger, string key, int limit, TimeSpan period, RateLimitItemType type, Uri url, HttpMethod? method, int requestWeight, RateLimitingBehaviour rateLimitingBehaviour, CancellationToken ct)
        {
            _singleEndpointGuard ??= new EndpointIndividualLimitGuard(_windowType);
            var desc = $"Limit of {limit}[Requests] per {period}";
            var result = _singleEndpointGuard.Check(key, limit, period, requestWeight);
            if (result.Delay != TimeSpan.Zero && rateLimitingBehaviour == RateLimitingBehaviour.Fail)
            {
                logger.LogWarning($"[{_name}] Call {key} failed because of ratelimit");
                RateLimitTriggered?.Invoke(new RateLimitEvent(_name, desc, method, url, result.Current, requestWeight, result.Limit, result.Period, result.Delay, rateLimitingBehaviour));
                return new CallResult(new ClientRateLimitError($"Rate limit check failed; {desc}"));
            }

            if (result.Delay != TimeSpan.Zero)
            {
                _semaphore.Release();

                logger.LogWarning($"[{_name}] Delaying call to {url} by {result.Delay} because of endpoint specific guard; {desc}, Request weight: {requestWeight}, Current: {result.Current}, Limit: {result.Limit}, requests now being limited: {_waitingCount}");

                RateLimitTriggered?.Invoke(new RateLimitEvent(_name, desc, method, url, result.Current, requestWeight, result.Limit, result.Period, result.Delay, rateLimitingBehaviour));
                await Task.Delay(result.Delay).ConfigureAwait(false);
                await _semaphore.WaitAsync().ConfigureAwait(false);
                return await CheckGuardAsync(logger, key, limit, period, type, url, method, requestWeight, rateLimitingBehaviour, ct).ConfigureAwait(false);
            }

            var applyResult = _singleEndpointGuard.ApplyWeight(key, requestWeight);
            logger.LogTrace($"[{_name}] Call to {url} passed endpoint specific guard; {desc}, New count: {applyResult.Current}");
            return new CallResult(null);
        }

        private async Task<CallResult> CheckGuardsAsync(ILogger logger, RateLimitItemType type, Uri url, HttpMethod? method, bool signed, SecureString? apiKey, int requestWeight, RateLimitingBehaviour rateLimitingBehaviour, CancellationToken ct)
        {
            foreach (var guard in _guards)
            {
                // Check if a wait is needed for this guard
                var result = guard.Check(type, url, method, signed, apiKey, requestWeight);
                if (result.Delay != TimeSpan.Zero && rateLimitingBehaviour == RateLimitingBehaviour.Fail)
                {
                    logger.LogWarning($"[{_name}] Call to {url} failed because of ratelimit guard {guard.Name}; {guard.Description}");
                    RateLimitTriggered?.Invoke(new RateLimitEvent(_name, guard.Description, method, url, result.Current, requestWeight, result.Limit, result.Period, result.Delay, rateLimitingBehaviour));
                    return new CallResult(new ClientRateLimitError($"Rate limit check failed on guard {guard.Name}; {guard.Description}"));
                }

                if (result.Delay != TimeSpan.Zero)
                {
                    _semaphore.Release();

                    if (result.Limit == null)
                        logger.LogWarning($"[{_name}] Delaying call to {url} by {result.Delay} because of ratelimit guard {guard.Name}; {guard.Description}");
                    else
                        logger.LogWarning($"[{_name}] Delaying call to {url} by {result.Delay} because of ratelimit guard {guard.Name}; {guard.Description}, Request weight: {requestWeight}, Current: {result.Current}, Limit: {result.Limit}, requests now being limited: {_waitingCount}");

                    RateLimitTriggered?.Invoke(new RateLimitEvent(_name, guard.Description, method, url, result.Current, requestWeight, result.Limit, result.Period, result.Delay, rateLimitingBehaviour));
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
                    logger.LogTrace($"[{_name}] Call to {url} passed ratelimit guard {guard.Name}; {guard.Description}, New count: {result.Current}");
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
