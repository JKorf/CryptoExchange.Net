using CryptoExchange.Net.Objects;
using System;
using System.Net.Http;

namespace CryptoExchange.Net.RateLimiting
{
    /// <summary>
    /// Rate limit event
    /// </summary>
    public class RateLimitEvent
    {
        /// <summary>
        /// Name of the API limit that is reached
        /// </summary>
        public string ApiLimit { get; set; } = string.Empty;
        /// <summary>
        /// Description of the limit that is reached
        /// </summary>
        public string LimitDescription { get; set; } = string.Empty;
        /// <summary>
        /// The http method of the request if applicable
        /// </summary>
        public HttpMethod? Method { get; set; }
        /// <summary>
        /// The host of the request
        /// </summary>
        public string Host { get; set; } = default!;
        /// <summary>
        /// The path of the request
        /// </summary>
        public string Path { get; set; } = default!;
        /// <summary>
        /// The current limit count
        /// </summary>
        public int Current { get; set; }
        /// <summary>
        /// The weight of the limited request
        /// </summary>
        public int RequestWeight { get; set; }
        /// <summary>
        /// The limit number per time period
        /// </summary>
        public int? Limit { get; set; }
        /// <summary>
        /// The time period of the limiter
        /// </summary>
        public TimeSpan? TimePeriod { get; set; }
        /// <summary>
        /// The time the request will be delayed for if the Behaviour is RateLimitingBehaviour.Wait
        /// </summary>
        public TimeSpan? DelayTime { get; set; }
        /// <summary>
        /// The handling behaviour for the rquest 
        /// </summary>
        public RateLimitingBehaviour Behaviour { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="apiLimit"></param>
        /// <param name="limitDescription"></param>
        /// <param name="method"></param>
        /// <param name="host"></param>
        /// <param name="path"></param>
        /// <param name="current"></param>
        /// <param name="requestWeight"></param>
        /// <param name="limit"></param>
        /// <param name="timePeriod"></param>
        /// <param name="delayTime"></param>
        /// <param name="behaviour"></param>
        public RateLimitEvent(string apiLimit, string limitDescription, HttpMethod? method, string host, string path, int current, int requestWeight, int? limit, TimeSpan? timePeriod, TimeSpan? delayTime, RateLimitingBehaviour behaviour)
        {
            ApiLimit = apiLimit;
            LimitDescription = limitDescription;
            Method = method;
            Host = host;
            Path = path;
            Current = current;
            RequestWeight = requestWeight;
            Limit = limit;
            TimePeriod = timePeriod;
            DelayTime = delayTime;
            Behaviour = behaviour;
        }

    }
}
