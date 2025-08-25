using CryptoExchange.Net.RateLimiting.Interfaces;
using System.Collections.Concurrent;
using System.Net.Http;

namespace CryptoExchange.Net.Objects;

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
    /// <param name="limitGuard">The rate limit guard for this specific endpoint</param>
    /// <param name="weight">Request weight</param>
    /// <param name="authenticated">Endpoint is authenticated</param>
    /// <param name="requestBodyFormat">Request body format</param>
    /// <param name="parameterPosition">Parameter position</param>
    /// <param name="arraySerialization">Array serialization type</param>
    /// <param name="preventCaching">Prevent request caching</param>
    /// <param name="tryParseOnNonSuccess">Try parse the response even when status is not success</param>
    /// <returns></returns>
    public RequestDefinition GetOrCreate(
        HttpMethod method,
        string path,
        IRateLimitGate? rateLimitGate,
        int weight,
        bool authenticated,
        IRateLimitGuard? limitGuard = null,
        RequestBodyFormat? requestBodyFormat = null,
        HttpMethodParameterPosition? parameterPosition = null,
        ArrayParametersSerialization? arraySerialization = null,
        bool? preventCaching = null,
        bool? tryParseOnNonSuccess = null)
        => GetOrCreate(method + path, method, path, rateLimitGate, weight, authenticated, limitGuard, requestBodyFormat, parameterPosition, arraySerialization, preventCaching, tryParseOnNonSuccess);

    /// <summary>
    /// Get a definition if it is already in the cache or create a new definition and add it to the cache
    /// </summary>
    /// <param name="identifier">Request identifier</param>
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
    /// <returns></returns>
    public RequestDefinition GetOrCreate(
        string identifier,
        HttpMethod method,
        string path,
        IRateLimitGate? rateLimitGate,
        int weight,
        bool authenticated,
        IRateLimitGuard? limitGuard = null,
        RequestBodyFormat? requestBodyFormat = null,
        HttpMethodParameterPosition? parameterPosition = null,
        ArrayParametersSerialization? arraySerialization = null,
        bool? preventCaching = null,
        bool? tryParseOnNonSuccess = null)
    {

        if (!_definitions.TryGetValue(identifier, out var def))
        {
            def = new RequestDefinition(path, method)
            {
                Authenticated = authenticated,
                LimitGuard = limitGuard,
                RateLimitGate = rateLimitGate,
                Weight = weight,
                ArraySerialization = arraySerialization,
                RequestBodyFormat = requestBodyFormat,
                ParameterPosition = parameterPosition,
                PreventCaching = preventCaching ?? false,
                TryParseOnNonSuccess = tryParseOnNonSuccess ?? false
            };
            _definitions.TryAdd(identifier, def);
        }

        return def;
    }
}
