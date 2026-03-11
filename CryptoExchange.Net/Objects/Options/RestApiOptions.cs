using CryptoExchange.Net.Authentication;
using System;

namespace CryptoExchange.Net.Objects.Options
{
    /// <summary>
    /// Http api options
    /// </summary>
    public class RestApiOptions : ApiOptions
    {
        /// <summary>
        /// How often the timestamp adjustment between client and server is recalculated. If you need a very small TimeSpan here you're probably better of syncing your server time more often
        /// </summary>
        public TimeSpan? TimestampRecalculationInterval { get; set; }

        /// <summary>
        /// Set the values of this options on the target options
        /// </summary>
        public T Set<T>(T item) where T : RestApiOptions, new()
        {
            item.OutputOriginalData = OutputOriginalData;
            item.AutoTimestamp = AutoTimestamp;
            item.TimestampRecalculationInterval = TimestampRecalculationInterval;
            return item;
        }
    }

    public class RestApiOptions<TApiCredentials> : RestApiOptions where TApiCredentials : ApiCredentials
    {
        /// <summary>
        /// The api credentials used for signing requests to this API. Overrides API credentials provided in the client options
        /// </summary>        
        public TApiCredentials? ApiCredentials { get; set; }

        /// <summary>
        /// Set the values of this options on the target options
        /// </summary>
        public T Set<T>(T item) where T : RestApiOptions<TApiCredentials>, new()
        {
            base.Set(item);
            item.ApiCredentials = (TApiCredentials?)ApiCredentials?.Copy();
            return item;
        }
    }
}
