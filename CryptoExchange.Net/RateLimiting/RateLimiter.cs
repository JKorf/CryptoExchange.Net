using CryptoExchange.Net.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Objects
{
    // Request order is not guarenteed if rate limited

    // Add event when rate limiting is applied
    // Add dynamic updating of limits (binance orders limits)
    // Rate limiter should be shared between clients
    // Retry-After = 0 for some Binance limits
    // Binance has IP and UID limits, endpoints don't always apply both, so should be able per call to distinquish which limit is applied. Allow some sort of topic per request which filters the ratelimit guards

    /// <summary>
    /// Limits the amount of requests to a certain constraint
    /// </summary>
    public class RateLimiter : IRateLimiter
    {
        private readonly object _limiterLock = new object();
        internal List<Limiter> _limiters = new List<Limiter>();

        /// <summary>
        /// Create a new RateLimiter. Configure the rate limiter by calling <see cref="AddTotalRateLimit"/>, 
        /// <see cref="AddEndpointLimit(string, int, TimeSpan, HttpMethod?, bool)"/>, <see cref="AddPartialEndpointLimit(string, int, TimeSpan, HttpMethod?, bool, bool)"/> or <see cref="AddApiKeyLimit"/>.
        /// </summary>
        public RateLimiter()
        {
        }

        /// <summary>
        /// Add a rate limit for the total amount of requests per time period
        /// </summary>
        /// <param name="limit">The limit per period. Note that this is weight, not single request, altough by default requests have a weight of 1</param>
        /// <param name="perTimePeriod">The time period the limit is for</param>
        public RateLimiter AddTotalRateLimit(int limit, TimeSpan perTimePeriod)
        {
            lock(_limiterLock)
                _limiters.Add(new TotalRateLimiter(limit, perTimePeriod, null));
            return this;
        }

        /// <summary>
        /// Add a rate limit for the amount of requests per time for an endpoint
        /// </summary>
        /// <param name="endpoint">The endpoint the limit is for</param>
        /// <param name="limit">The limit per period. Note that this is weight, not single request, altough by default requests have a weight of 1</param>
        /// <param name="perTimePeriod">The time period the limit is for</param>
        /// <param name="method">The HttpMethod the limit is for, null for all</param>
        /// <param name="excludeFromOtherRateLimits">If set to true it ignores other rate limits</param>
        public RateLimiter AddEndpointLimit(string endpoint, int limit, TimeSpan perTimePeriod, HttpMethod? method = null, bool excludeFromOtherRateLimits = false)
        {
            lock(_limiterLock)
                _limiters.Add(new EndpointRateLimiter(new[] { endpoint }, limit, perTimePeriod, method, excludeFromOtherRateLimits));
            return this;
        }

        /// <summary>
        /// Add a rate lmit for the amount of requests per time for an endpoint
        /// </summary>
        /// <param name="endpoints">The endpoints the limit is for</param>
        /// <param name="limit">The limit per period. Note that this is weight, not single request, altough by default requests have a weight of 1</param>
        /// <param name="perTimePeriod">The time period the limit is for</param>
        /// <param name="method">The HttpMethod the limit is for, null for all</param>
        /// <param name="excludeFromOtherRateLimits">If set to true it ignores other rate limits</param>
        public RateLimiter AddEndpointLimit(IEnumerable<string> endpoints, int limit, TimeSpan perTimePeriod, HttpMethod? method = null, bool excludeFromOtherRateLimits = false)
        {
            lock(_limiterLock)
                _limiters.Add(new EndpointRateLimiter(endpoints.ToArray(), limit, perTimePeriod, method, excludeFromOtherRateLimits));
            return this;
        }

        /// <summary>
        /// Add a rate lmit for the amount of requests per time for an endpoint
        /// </summary>
        /// <param name="endpoint">The endpoint the limit is for</param>
        /// <param name="limit">The limit per period. Note that this is weight, not single request, altough by default requests have a weight of 1</param>
        /// <param name="perTimePeriod">The time period the limit is for</param>
        /// <param name="method">The HttpMethod the limit is for, null for all</param>
        /// <param name="ignoreOtherRateLimits">If set to true it ignores other rate limits</param>
        /// <param name="countPerEndpoint">Whether all requests for this partial endpoint are bound to the same limit or each individual endpoint has its own limit</param>
        public RateLimiter AddPartialEndpointLimit(string endpoint, int limit, TimeSpan perTimePeriod, HttpMethod? method = null, bool countPerEndpoint = false, bool ignoreOtherRateLimits = false)
        {
            lock(_limiterLock)
                _limiters.Add(new PartialEndpointRateLimiter(new[] { endpoint }, limit, perTimePeriod, method, ignoreOtherRateLimits, countPerEndpoint));
            return this;
        }

        /// <summary>
        /// Add a rate limit for the amount of requests per Api key
        /// </summary>
        /// <param name="limit">The limit per period. Note that this is weight, not single request, altough by default requests have a weight of 1</param>
        /// <param name="perTimePeriod">The time period the limit is for</param>
        /// <param name="onlyForSignedRequests">Only include calls that are signed in this limiter</param>
        /// <param name="excludeFromTotalRateLimit">Exclude requests with API key from the total rate limiter</param>
        public RateLimiter AddApiKeyLimit(int limit, TimeSpan perTimePeriod, bool onlyForSignedRequests, bool excludeFromTotalRateLimit)
        {
            lock(_limiterLock)
                _limiters.Add(new ApiKeyRateLimiter(limit, perTimePeriod, null, onlyForSignedRequests, excludeFromTotalRateLimit));
            return this;
        }

        /// <summary>
        /// Add a rate limit for the amount of messages that can be send per connection
        /// </summary>
        /// <param name="endpoint">The endpoint that the limit is for</param>
        /// <param name="limit">The limit per period. Note that this is weight, not single request, altough by default requests have a weight of 1</param>
        /// <param name="perTimePeriod">The time period the limit is for</param>
        public RateLimiter AddConnectionRateLimit(string endpoint, int limit, TimeSpan perTimePeriod)
        {
            lock (_limiterLock)
                _limiters.Add(new ConnectionRateLimiter(new[] { endpoint }, limit, perTimePeriod));
            return this;
        }

        /// <inheritdoc />
        public async Task<CallResult<int>> LimitRequestAsync(ILogger logger, string endpoint, HttpMethod method, bool signed, SecureString? apiKey, RateLimitingBehaviour limitBehaviour, int requestWeight, CancellationToken ct)
        {
            int totalWaitTime = 0;

            List<EndpointRateLimiter> endpointLimits;
            lock (_limiterLock)
                endpointLimits = _limiters.OfType<EndpointRateLimiter>().Where(h => h.Endpoints.Contains(endpoint) && (h.Method  == null || h.Method == method)).ToList();
            foreach (var endpointLimit in endpointLimits)
            {
                var waitResult = await ProcessTopic(logger, endpointLimit, endpoint, requestWeight, limitBehaviour, ct).ConfigureAwait(false);
                if (!waitResult)
                    return waitResult;

                totalWaitTime += waitResult.Data;
            }

            if (endpointLimits.Any(l => l.IgnoreOtherRateLimits))
                return new CallResult<int>(totalWaitTime);

            List<PartialEndpointRateLimiter> partialEndpointLimits;
            lock (_limiterLock)
                partialEndpointLimits = _limiters.OfType<PartialEndpointRateLimiter>().Where(h => h.PartialEndpoints.Any(h => endpoint.Contains(h)) && (h.Method == null || h.Method == method)).ToList();
            foreach (var partialEndpointLimit in partialEndpointLimits)
            {
                if (partialEndpointLimit.CountPerEndpoint)
                {
                    SingleTopicRateLimiter? thisEndpointLimit;
                    lock (_limiterLock)
                    {
                        thisEndpointLimit = _limiters.OfType<SingleTopicRateLimiter>().SingleOrDefault(h => h.Type == RateLimitType.PartialEndpoint && (string)h.Topic == endpoint);
                        if (thisEndpointLimit == null)
                        {
                            thisEndpointLimit = new SingleTopicRateLimiter(endpoint, partialEndpointLimit);
                            _limiters.Add(thisEndpointLimit);
                        }
                    }

                    var waitResult = await ProcessTopic(logger, thisEndpointLimit, endpoint, requestWeight, limitBehaviour, ct).ConfigureAwait(false);
                    if (!waitResult)
                        return waitResult;

                    totalWaitTime += waitResult.Data;
                }
                else
                {
                    var waitResult = await ProcessTopic(logger, partialEndpointLimit, endpoint, requestWeight, limitBehaviour, ct).ConfigureAwait(false);
                    if (!waitResult)
                        return waitResult;

                    totalWaitTime += waitResult.Data;
                }
            }

            if(partialEndpointLimits.Any(p => p.IgnoreOtherRateLimits))
                return new CallResult<int>(totalWaitTime);

            List<ApiKeyRateLimiter> apiLimits;
            lock (_limiterLock)
                apiLimits = _limiters.OfType<ApiKeyRateLimiter>().Where(h => h.Type == RateLimitType.ApiKey).ToList();
            foreach (var apiLimit in apiLimits)
            {
                if(apiKey == null)
                {
                    if (!apiLimit.OnlyForSignedRequests)
                    {
                        var waitResult = await ProcessTopic(logger, apiLimit, endpoint, requestWeight, limitBehaviour, ct).ConfigureAwait(false);
                        if (!waitResult)
                            return waitResult;

                        totalWaitTime += waitResult.Data;
                    }
                }
                else if (signed || !apiLimit.OnlyForSignedRequests)
                {
                    SingleTopicRateLimiter? thisApiLimit;
                    lock (_limiterLock)
                    {
                        thisApiLimit = _limiters.OfType<SingleTopicRateLimiter>().SingleOrDefault(h => h.Type == RateLimitType.ApiKey && ((SecureString)h.Topic).IsEqualTo(apiKey));
                        if (thisApiLimit == null)
                        {
                            thisApiLimit = new SingleTopicRateLimiter(apiKey, apiLimit);
                            _limiters.Add(thisApiLimit);
                        }
                    }

                    var waitResult = await ProcessTopic(logger, thisApiLimit, endpoint, requestWeight, limitBehaviour, ct).ConfigureAwait(false);
                    if (!waitResult)
                        return waitResult;

                    totalWaitTime += waitResult.Data;
                }
            }

            if ((signed || apiLimits.All(l => !l.OnlyForSignedRequests)) && apiLimits.Any(l => l.IgnoreTotalRateLimit))
                return new CallResult<int>(totalWaitTime);

            List<TotalRateLimiter> totalLimits;
            lock (_limiterLock)
                totalLimits = _limiters.OfType<TotalRateLimiter>().ToList();
            foreach(var totalLimit in  totalLimits) 
            {
                var waitResult = await ProcessTopic(logger, totalLimit, endpoint, requestWeight, limitBehaviour, ct).ConfigureAwait(false);
                if (!waitResult)
                    return waitResult;

                totalWaitTime += waitResult.Data;
            }

            return new CallResult<int>(totalWaitTime);
        }

        private static async Task<CallResult<int>> ProcessTopic(ILogger logger, Limiter historyTopic, string endpoint, int requestWeight, RateLimitingBehaviour limitBehaviour, CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                await historyTopic.Semaphore.WaitAsync(ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return new CallResult<int>(new CancellationRequestedError());
            }
            sw.Stop();

            try
            {
                int totalWaitTime = 0;
                while (true)
                {
                    // Remove requests no longer in time period from the history
                    var checkTime = DateTime.UtcNow;
                    for (var i = 0; i < historyTopic.Entries.Count; i++)
                    {
                        if (historyTopic.Entries[i].Timestamp < checkTime - historyTopic.Period)
                        {
                            historyTopic.Entries.Remove(historyTopic.Entries[i]);
                            i--;
                        }
                        else
                            break;
                    }

                    var currentWeight = !historyTopic.Entries.Any() ? 0 : historyTopic.Entries.Sum(h => h.Weight);
                    if (currentWeight + requestWeight > historyTopic.Limit)
                    {
                        if (currentWeight == 0)
                            throw new Exception("Request limit reached without any prior request. " +
                                $"This request can never execute with the current rate limiter. Request weight: {requestWeight}, Ratelimit: {historyTopic.Limit}");

                        // Wait until the next entry should be removed from the history
                        var thisWaitTime = (int)Math.Round(((historyTopic.Entries.First().Timestamp + historyTopic.Period) - checkTime).TotalMilliseconds);
                        if (thisWaitTime > 0)
                        {
                            if (limitBehaviour == RateLimitingBehaviour.Fail)
                            {
                                var msg = $"Request to {endpoint} failed because of rate limit `{historyTopic.Type}`. Current weight: {currentWeight}/{historyTopic.Limit}, request weight: {requestWeight}";
                                logger.Log(LogLevel.Warning, msg);
                                return new CallResult<int>(new ClientRateLimitError(msg) { RetryAfter = DateTime.UtcNow.AddSeconds(thisWaitTime) });
                            }

                            logger.Log(LogLevel.Information, $"Message to {endpoint} waiting {thisWaitTime}ms for rate limit `{historyTopic.Type}`. Current weight: {currentWeight}/{historyTopic.Limit}, request weight: {requestWeight}");
                            try
                            {
                                await Task.Delay(thisWaitTime, ct).ConfigureAwait(false);
                            }
                            catch (OperationCanceledException)
                            {
                                return new CallResult<int>(new CancellationRequestedError());
                            }
                            totalWaitTime += thisWaitTime;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                var newTime = DateTime.UtcNow;
                historyTopic.Entries.Add(new LimitEntry(newTime, requestWeight));
                return new CallResult<int>(totalWaitTime);
            }
            finally
            {
                historyTopic.Semaphore.Release();
            }
        }

        internal struct LimitEntry
        {
            public DateTime Timestamp { get; set; }
            public int Weight { get; set; }

            public LimitEntry(DateTime timestamp, int weight)
            {
                Timestamp = timestamp;
                Weight = weight;
            }
        }

        internal class Limiter
        {
            public RateLimitType Type { get; set; }
            public HttpMethod? Method { get; set; }

            public SemaphoreSlim Semaphore { get; set; }
            public int Limit { get; set; }

            public TimeSpan Period { get; set; }
            public List<LimitEntry> Entries { get; set; } = new List<LimitEntry>();

            public Limiter(RateLimitType type, int limit, TimeSpan perPeriod, HttpMethod? method)
            {
                Semaphore = new SemaphoreSlim(1, 1);
                Type = type;
                Limit = limit;
                Period = perPeriod;
                Method = method;
            }
        }

        internal class TotalRateLimiter : Limiter
        {
            public TotalRateLimiter(int limit, TimeSpan perPeriod, HttpMethod? method)
                : base(RateLimitType.Total, limit, perPeriod, method)
            {
            }

            public override string ToString()
            {
                return nameof(TotalRateLimiter);
            }
        }

        internal class AfterDateTimeLimiter
        {
            private readonly DateTime _after;

            public AfterDateTimeLimiter(DateTime after)
            {
                _after = after;
            }

            public override string ToString()
            {
                return nameof(TotalRateLimiter);
            }
        }

        internal class ConnectionRateLimiter : PartialEndpointRateLimiter
        {
            public ConnectionRateLimiter(int limit, TimeSpan perPeriod)
                : base(new[] { "/" }, limit, perPeriod, null, true, true)
            {
            }

            public ConnectionRateLimiter(string[] endpoints, int limit, TimeSpan perPeriod)
                : base(endpoints, limit, perPeriod, null, true, true)
            {
            }

            public override string ToString()
            {
                return nameof(ConnectionRateLimiter);
            }
        }

        internal class EndpointRateLimiter: Limiter
        {
            public string[] Endpoints { get; set; }
            public bool IgnoreOtherRateLimits { get; set; }

            public EndpointRateLimiter(string[] endpoints, int limit, TimeSpan perPeriod, HttpMethod? method, bool ignoreOtherRateLimits)
                :base(RateLimitType.Endpoint, limit, perPeriod, method)
            {
                Endpoints = endpoints;
                IgnoreOtherRateLimits = ignoreOtherRateLimits;
            }

            public override string ToString()
            {
                return nameof(EndpointRateLimiter) + $": {string.Join(", ", Endpoints)}";
            }
        }

        internal class PartialEndpointRateLimiter : Limiter
        {
            public string[] PartialEndpoints { get; set; }
            public bool IgnoreOtherRateLimits { get; set; }
            public bool CountPerEndpoint { get; set; }

            public PartialEndpointRateLimiter(string[] partialEndpoints, int limit, TimeSpan perPeriod, HttpMethod? method, bool ignoreOtherRateLimits, bool countPerEndpoint)
                : base(RateLimitType.PartialEndpoint, limit, perPeriod, method)
            {
                PartialEndpoints = partialEndpoints;
                IgnoreOtherRateLimits = ignoreOtherRateLimits;
                CountPerEndpoint = countPerEndpoint;
            }

            public override string ToString()
            {
                return nameof(PartialEndpointRateLimiter) + $": {string.Join(", ", PartialEndpoints)}";
            }
        }

        internal class ApiKeyRateLimiter : Limiter
        {
            public bool OnlyForSignedRequests { get; set; }
            public bool IgnoreTotalRateLimit { get; set; }

            public ApiKeyRateLimiter(int limit, TimeSpan perPeriod, HttpMethod? method, bool onlyForSignedRequests, bool ignoreTotalRateLimit)
                :base(RateLimitType.ApiKey, limit, perPeriod, method)
            {
                OnlyForSignedRequests = onlyForSignedRequests;
                IgnoreTotalRateLimit = ignoreTotalRateLimit;
            }
        }

        internal class SingleTopicRateLimiter: Limiter
        {
            public object Topic { get; set; }

            public SingleTopicRateLimiter(object topic, Limiter limiter)
                :base(limiter.Type, limiter.Limit, limiter.Period, limiter.Method)
            {
                Topic = topic;
            }

            public override string ToString()
            {
                return (Type == RateLimitType.ApiKey ? nameof(ApiKeyRateLimiter): nameof(EndpointRateLimiter)) + $": {Topic}";
            }
        }

        internal enum RateLimitType 
        { 
            Total,
            After,
            Endpoint,
            PartialEndpoint,
            ApiKey
        }
    }
}
