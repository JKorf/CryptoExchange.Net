using CryptoExchange.Net.Objects;
using System;

namespace CryptoExchange.Net.RateLimiting;

/// <summary>
/// Rate limit triggered event
/// </summary>
public record RateLimitEvent
{
    /// <summary>
    /// Id of the item the limit was checked for
    /// </summary>
    public int ItemId { get; set; }
    /// <summary>
    /// Name of the API limit that is reached
    /// </summary>
    public string ApiLimit { get; set; } = string.Empty;
    /// <summary>
    /// Description of the limit that is reached
    /// </summary>
    public string LimitDescription { get; set; } = string.Empty;
    /// <summary>
    /// The request definition
    /// </summary>
    public RequestDefinition RequestDefinition { get; set; }
    /// <summary>
    /// The host the request is for
    /// </summary>
    public string Host { get; set; } = default!;
    /// <summary>
    /// The current counter value
    /// </summary>
    public int Current { get; set; }
    /// <summary>
    /// The weight of the limited request
    /// </summary>
    public int RequestWeight { get; set; }
    /// <summary>
    /// The limit per time period
    /// </summary>
    public int? Limit { get; set; }
    /// <summary>
    /// The time period the limit is for
    /// </summary>
    public TimeSpan? TimePeriod { get; set; }
    /// <summary>
    /// The time the request will be delayed for if the Behaviour is RateLimitingBehaviour.Wait
    /// </summary>
    public TimeSpan? DelayTime { get; set; }
    /// <summary>
    /// The handling behaviour for the request 
    /// </summary>
    public RateLimitingBehaviour Behaviour { get; set; }

    /// <summary>
    /// ctor
    /// </summary>
    public RateLimitEvent(int itemId, string apiLimit, string limitDescription, RequestDefinition definition, string host, int current, int requestWeight, int? limit, TimeSpan? timePeriod, TimeSpan? delayTime, RateLimitingBehaviour behaviour)
    {
        ItemId = itemId;
        ApiLimit = apiLimit;
        LimitDescription = limitDescription;
        RequestDefinition = definition;
        Host = host;
        Current = current;
        RequestWeight = requestWeight;
        Limit = limit;
        TimePeriod = timePeriod;
        DelayTime = delayTime;
        Behaviour = behaviour;
    }

}
