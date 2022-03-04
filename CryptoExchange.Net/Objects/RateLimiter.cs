using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Logging;
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
    /// <summary>
    /// Limits the amount of requests to a certain constraint
    /// </summary>
    public class RateLimiter : IRateLimiter
    {
        private readonly object _limiterLock = new object();
        internal List<Limiter> Limiters = new List<Limiter>();

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
                Limiters.Add(new TotalRateLimiter(limit, perTimePeriod, null));
            return this;
        }

        /// <summary>
        /// Add a rate lmit for the amount of requests per time for an endpoint
        /// </summary>
        /// <param name="endpoint">The endpoint the limit is for</param>
        /// <param name="limit">The limit per period. Note that this is weight, not single request, altough by default requests have a weight of 1</param>
        /// <param name="perTimePeriod">The time period the limit is for</param>
        /// <param name="method">The HttpMethod the limit is for, null for all</param>
        /// <param name="excludeFromOtherRateLimits">If set to true it ignores other rate limits</param>
        public RateLimiter AddEndpointLimit(string endpoint, int limit, TimeSpan perTimePeriod, HttpMethod? method = null, bool excludeFromOtherRateLimits = false)
        {
            lock(_limiterLock)
                Limiters.Add(new EndpointRateLimiter(new[] { endpoint }, limit, perTimePeriod, method, excludeFromOtherRateLimits));
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
                Limiters.Add(new EndpointRateLimiter(endpoints.ToArray(), limit, perTimePeriod, method, excludeFromOtherRateLimits));
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
                Limiters.Add(new PartialEndpointRateLimiter(new[] { endpoint }, limit, perTimePeriod, method, ignoreOtherRateLimits, countPerEndpoint));
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
                Limiters.Add(new ApiKeyRateLimiter(limit, perTimePeriod, null, onlyForSignedRequests, excludeFromTotalRateLimit));
            return this;
        }

        /// <inheritdoc />
        public async Task<CallResult<int>> LimitRequestAsync(Log log, string endpoint, HttpMethod method, bool signed, SecureString? apiKey, RateLimitingBehaviour limitBehaviour, int requestWeight, CancellationToken ct)
        {
            int totalWaitTime = 0;

            EndpointRateLimiter? endpointLimit;
            lock (_limiterLock)
                endpointLimit = Limiters.OfType<EndpointRateLimiter>().SingleOrDefault(h => h.Endpoints.Contains(endpoint) && (h.Method  == null || h.Method == method));
            if(endpointLimit != null)
            {
                var waitResult = await ProcessTopic(log, endpointLimit, endpoint, requestWeight, limitBehaviour, ct).ConfigureAwait(false);
                if (!waitResult)
                    return waitResult;

                totalWaitTime += waitResult.Data;
            }

            if (endpointLimit?.IgnoreOtherRateLimits == true)
                return new CallResult<int>(totalWaitTime);

            List<PartialEndpointRateLimiter> partialEndpointLimits;
            lock (_limiterLock)
                partialEndpointLimits = Limiters.OfType<PartialEndpointRateLimiter>().Where(h => h.PartialEndpoints.Any(h => endpoint.Contains(h)) && (h.Method == null || h.Method == method)).ToList();
            foreach (var partialEndpointLimit in partialEndpointLimits)
            {
                if (partialEndpointLimit.CountPerEndpoint)
                {
                    SingleTopicRateLimiter? thisEndpointLimit;
                    lock (_limiterLock)
                    {
                        thisEndpointLimit = Limiters.OfType<SingleTopicRateLimiter>().SingleOrDefault(h => h.Type == RateLimitType.PartialEndpoint && (string)h.Topic == endpoint);
                        if (thisEndpointLimit == null)
                        {
                            thisEndpointLimit = new SingleTopicRateLimiter(endpoint, partialEndpointLimit);
                            Limiters.Add(thisEndpointLimit);
                        }
                    }

                    var waitResult = await ProcessTopic(log, thisEndpointLimit, endpoint, requestWeight, limitBehaviour, ct).ConfigureAwait(false);
                    if (!waitResult)
                        return waitResult;

                    totalWaitTime += waitResult.Data;
                }
                else
                {
                    var waitResult = await ProcessTopic(log, partialEndpointLimit, endpoint, requestWeight, limitBehaviour, ct).ConfigureAwait(false);
                    if (!waitResult)
                        return waitResult;

                    totalWaitTime += waitResult.Data;
                }
            }

            if(partialEndpointLimits.Any(p => p.IgnoreOtherRateLimits))
                return new CallResult<int>(totalWaitTime);

            ApiKeyRateLimiter? apiLimit;
            lock (_limiterLock)
                apiLimit = Limiters.OfType<ApiKeyRateLimiter>().SingleOrDefault(h => h.Type == RateLimitType.ApiKey);
            if (apiLimit != null)
            {
                if(apiKey == null)
                {
                    if (!apiLimit.OnlyForSignedRequests)
                    {
                        var waitResult = await ProcessTopic(log, apiLimit, endpoint, requestWeight, limitBehaviour, ct).ConfigureAwait(false);
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
                        thisApiLimit = Limiters.OfType<SingleTopicRateLimiter>().SingleOrDefault(h => h.Type == RateLimitType.ApiKey && ((SecureString)h.Topic).IsEqualTo(apiKey));
                        if (thisApiLimit == null)
                        {
                            thisApiLimit = new SingleTopicRateLimiter(apiKey, apiLimit);
                            Limiters.Add(thisApiLimit);
                        }
                    }

                    var waitResult = await ProcessTopic(log, thisApiLimit, endpoint, requestWeight, limitBehaviour, ct).ConfigureAwait(false);
                    if (!waitResult)
                        return waitResult;

                    totalWaitTime += waitResult.Data;
                }
            }

            if ((signed || apiLimit?.OnlyForSignedRequests == false) && apiLimit?.IgnoreTotalRateLimit == true)
                return new CallResult<int>(totalWaitTime);

            TotalRateLimiter? totalLimit;
            lock (_limiterLock)
                totalLimit = Limiters.OfType<TotalRateLimiter>().SingleOrDefault();
            if (totalLimit != null)
            {
                var waitResult = await ProcessTopic(log, totalLimit, endpoint, requestWeight, limitBehaviour, ct).ConfigureAwait(false);
                if (!waitResult)
                    return waitResult;

                totalWaitTime += waitResult.Data;
            }

            return new CallResult<int>(totalWaitTime);
        }

        private static async Task<CallResult<int>> ProcessTopic(Log log, Limiter historyTopic, string endpoint, int requestWeight, RateLimitingBehaviour limitBehaviour, CancellationToken ct)
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

                var currentWeight = !historyTopic.Entries.Any() ? 0: historyTopic.Entries.Sum(h => h.Weight);
                if (currentWeight + requestWeight > historyTopic.Limit)
                {
                    if (currentWeight == 0)
                        throw new Exception("Request limit reached without any prior request. " +
                            $"This request can never execute with the current rate limiter. Request weight: {requestWeight}, Ratelimit: {historyTopic.Limit}");

                    // Wait until the next entry should be removed from the history
                    var thisWaitTime = (int)Math.Round((historyTopic.Entries.First().Timestamp - (checkTime - historyTopic.Period)).TotalMilliseconds);
                    if (thisWaitTime > 0)
                    {
                        if (limitBehaviour == RateLimitingBehaviour.Fail)
                        {
                            historyTopic.Semaphore.Release();
                            var msg = $"Request to {endpoint} failed because of rate limit `{historyTopic.Type}`. Current weight: {currentWeight}/{historyTopic.Limit}, request weight: {requestWeight}";
                            log.Write(LogLevel.Warning, msg);
                            return new CallResult<int>(new RateLimitError(msg));
                        }

                        log.Write(LogLevel.Information, $"Request to {endpoint} waiting {thisWaitTime}ms for rate limit `{historyTopic.Type}`. Current weight: {currentWeight}/{historyTopic.Limit}, request weight: {requestWeight}");
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
            historyTopic.Semaphore.Release();
            return new CallResult<int>(totalWaitTime);
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
            Endpoint,
            PartialEndpoint,
            ApiKey
        }
    }
}
