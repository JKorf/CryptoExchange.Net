using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.RateLimiting
{
    public class RateLimitGate : IRateLimitGate
    {
        private readonly ConcurrentBag<IRateLimitGuard> _guards;
        private readonly SemaphoreSlim _semaphore;
        private RateLimitingBehaviour _limitBehaviour = RateLimitingBehaviour.Wait;

        public RateLimitGate()
        {
            _guards = new ConcurrentBag<IRateLimitGuard>();
            _semaphore = new SemaphoreSlim(1);
        }

        public async Task<CallResult> ProcessAsync(ILogger logger, Uri url, HttpMethod method, bool signed, SecureString? apiKey, int requestWeight, CancellationToken ct)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                foreach (var guard in _guards)
                {
                    // Check if a wait is needed for this guard
                    var result = guard.Check(logger, url, method, signed, apiKey, requestWeight);
                    if (result != TimeSpan.Zero && _limitBehaviour == RateLimitingBehaviour.Fail)
                        return new CallResult(new ClientRateLimitError($"Rate limit check failed on guard {guard.Name}"));

                    if (result != TimeSpan.Zero)
                    {
                        logger.LogWarning($"Delaying call to {url} by {result} because of ratelimit guard {guard.Name}; Request weight: {requestWeight}, {guard.GetState(url, method, signed, apiKey, requestWeight)}");
                        await Task.Delay(result).ConfigureAwait(false);
                    }
                }

                // Apply the weight on each guard
                foreach (var guard in _guards)
                    guard.Enter(url, method, signed, apiKey, requestWeight);

                return new CallResult(null);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public IRateLimitGate AddGuard(IRateLimitGuard guard)
        {
            _guards.Add(guard);
            return this;
        }

        public IRateLimitGate WithLimitBehaviour(RateLimitingBehaviour behaviour) 
        {
            _limitBehaviour = behaviour;
            return this;
        }
    }
}
