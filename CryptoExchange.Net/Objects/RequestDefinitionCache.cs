using CryptoExchange.Net.RateLimiting;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace CryptoExchange.Net.Objects
{
    /// <summary>
    /// Request definitions cache
    /// </summary>
    public class RequestDefinitionCache
    {
        private readonly Dictionary<string, RequestDefinition> _definitions = new();

        /// <summary>
        /// Get a definition if it is already in the cache or create a new definition and add it to the cache
        /// </summary>
        /// <param name="method">The HttpMethod</param>
        /// <param name="path">Endpoint path</param>
        /// <param name="authenticated">Endpoint is authenticated</param>
        /// <returns></returns>
        public RequestDefinition GetOrCreate(HttpMethod method, string path, bool authenticated = false)
            => GetOrCreate(method, path, null, 0, authenticated, null, null, null, null, null);

        /// <summary>
        /// Get a definition if it is already in the cache or create a new definition and add it to the cache
        /// </summary>
        /// <param name="method">The HttpMethod</param>
        /// <param name="path">Endpoint path</param>
        /// <param name="rateLimitGate">The rate limit gate</param>
        /// <param name="weight">Request weight</param>
        /// <param name="authenticated">Endpoint is authenticated</param>
        /// <returns></returns>
        public RequestDefinition GetOrCreate(HttpMethod method, string path, IRateLimitGate rateLimitGate, int weight = 1, bool authenticated = false)
            => GetOrCreate(method, path, rateLimitGate,  weight, authenticated, null, null, null, null, null);

        /// <summary>
        /// Get a definition if it is already in the cache or create a new definition and add it to the cache
        /// </summary>
        /// <param name="method">The HttpMethod</param>
        /// <param name="path">Endpoint path</param>
        /// <param name="rateLimitGate">The rate limit gate</param>
        /// <param name="endpointLimitCount">The limit count for this specific endpoint</param>
        /// <param name="endpointLimitPeriod">The period for the limit for this specific endpoint</param>
        /// <param name="weight">Request weight</param>
        /// <param name="authenticated">Endpoint is authenticated</param>
        /// <param name="requestBodyFormat">Request body format</param>
        /// <param name="parameterPosition">Parameter position</param>
        /// <param name="arraySerialization">Array serialization type</param>
        /// <returns></returns>
        public RequestDefinition GetOrCreate(
            HttpMethod method,
            string path,
            IRateLimitGate? rateLimitGate,
            int weight,
            bool authenticated,
            int? endpointLimitCount = null,
            TimeSpan? endpointLimitPeriod = null,
            RequestBodyFormat? requestBodyFormat = null,
            HttpMethodParameterPosition? parameterPosition = null,
            ArrayParametersSerialization? arraySerialization = null)
        {

            if (!_definitions.TryGetValue(method + path, out var def))
            {
                def = new RequestDefinition(path, method)
                {
                    Authenticated = authenticated,
                    EndpointLimitCount = endpointLimitCount,
                    EndpointLimitPeriod = endpointLimitPeriod,
                    RateLimitGate = rateLimitGate,
                    Weight = weight,
                    ArraySerialization = arraySerialization,
                    RequestBodyFormat = requestBodyFormat,
                    ParameterPosition = parameterPosition,
                };
                _definitions.Add(method + path, def);
            }

            return def;
        }
    }
}
