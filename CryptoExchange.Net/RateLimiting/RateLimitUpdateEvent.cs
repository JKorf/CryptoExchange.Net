using CryptoExchange.Net.Objects;
using System;

namespace CryptoExchange.Net.RateLimiting
{
    /// <summary>
    /// Rate limit update event
    /// </summary>
    public record RateLimitUpdateEvent
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
        /// The current counter value
        /// </summary>
        public int Current { get; set; }
        /// <summary>
        /// The limit per time period
        /// </summary>
        public int? Limit { get; set; }
        /// <summary>
        /// The time period the limit is for
        /// </summary>
        public TimeSpan? TimePeriod { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public RateLimitUpdateEvent(int itemId, string apiLimit, string limitDescription, int current, int? limit, TimeSpan? timePeriod)
        {
            ItemId = itemId;
            ApiLimit = apiLimit;
            LimitDescription = limitDescription;
            Current = current;
            Limit = limit;
            TimePeriod = timePeriod;
        }

    }
}
