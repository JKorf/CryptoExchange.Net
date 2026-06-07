using CryptoExchange.Net.RateLimiting.Interfaces;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;

namespace CryptoExchange.Net.Objects
{
    /// <summary>
    /// Request definitions cache
    /// </summary>
    public class RequestDefinitionCache
    {
        private readonly ConcurrentDictionary<string, RequestDefinition> _definitions = new();

        /// <summary>
        /// Get a definition if it is already in the cache or create a new definition and add it to the cache
        /// </summary>
        /// <param name="method">The HttpMethod</param>
        /// <param name="baseAddress">The base address/host</param>
        /// <param name="path">Endpoint path</param>
        /// <param name="authenticated">Endpoint is authenticated</param>
        /// <returns></returns>
        public RequestDefinition GetOrCreate(HttpMethod method, string baseAddress, string path, bool authenticated = false)
            => GetOrCreate(method, baseAddress, path, null, 0, authenticated, null, null, null, null, null, null, null);

        /// <summary>
        /// Get a definition if it is already in the cache or create a new definition and add it to the cache
        /// </summary>
        /// <param name="method">The HttpMethod</param>
        /// <param name="baseAddress">The base address/host</param>
        /// <param name="path">Endpoint path</param>
        /// <param name="rateLimitGate">The rate limit gate</param>
        /// <param name="weight">Request weight</param>
        /// <param name="authenticated">Endpoint is authenticated</param>
        /// <returns></returns>
        public RequestDefinition GetOrCreate(HttpMethod method, string baseAddress, string path, IRateLimitGate rateLimitGate, int weight = 1, bool authenticated = false)
            => GetOrCreate(method, baseAddress, path, rateLimitGate,  weight, authenticated, null, null, null, null, null, null, null);

        /// <summary>
        /// Get a definition if it is already in the cache or create a new definition and add it to the cache
        /// </summary>
        /// <param name="baseAddress">The base address/host</param>
        /// <param name="method">The HttpMethod</param>
        /// <param name="path">Endpoint path</param>
        /// <param name="rateLimitGate">The rate limit gate</param>
        /// <param name="limitGuard">The rate limit guard for this specific endpoint</param>
        /// <param name="weight">Request weight</param>
        /// <param name="authenticated">Endpoint is authenticated</param>
        /// <param name="requestBodyFormat">Request body format</param>
        /// <param name="parameterPosition">Parameter position</param>
        /// <param name="arraySerialization">Array serialization type</param>
        /// <param name="preventCaching">Prevent request caching</param>
        /// <param name="tryParseOnNonSuccess">Try parse the response even when status is not success</param>
        /// <param name="forcePathEndWithSlash">Force trailing `/`</param>
        /// <param name="identifier">Optional request identifier override</param>
        /// <returns></returns>
        public RequestDefinition GetOrCreate(
            HttpMethod method,
            string baseAddress,
            string path,
            IRateLimitGate? rateLimitGate,
            int weight,
            bool authenticated,
            IRateLimitGuard? limitGuard = null,
            RequestBodyFormat? requestBodyFormat = null,
            HttpMethodParameterPosition? parameterPosition = null,
            ArrayParametersSerialization? arraySerialization = null,
            bool? preventCaching = null,
            bool? tryParseOnNonSuccess = null,
            bool? forcePathEndWithSlash = null,
            string? identifier = null)
        {
            var identifierToUse = identifier ?? $"{path}{method.Method}{baseAddress}";
            if (!_definitions.TryGetValue(identifierToUse, out var def))
            {
                def = new RequestDefinition(baseAddress, path, method)
                {
                    Authenticated = authenticated,
                    LimitGuard = limitGuard,
                    RateLimitGate = rateLimitGate,
                    Weight = weight,
                    ArraySerialization = arraySerialization,
                    RequestBodyFormat = requestBodyFormat,
                    ParameterPosition = parameterPosition,
                    PreventCaching = preventCaching ?? false,
                    TryParseOnNonSuccess = tryParseOnNonSuccess ?? false,
                    ForcePathEndWithSlash = forcePathEndWithSlash ?? false,
                };
                _definitions.TryAdd(identifierToUse, def);
            }

            return def;
        }
    }
}
