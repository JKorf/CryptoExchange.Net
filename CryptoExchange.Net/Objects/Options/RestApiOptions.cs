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
        /// Whether or not to automatically sync the local time with the server time
        /// </summary>
        public bool? AutoTimestamp { get; set; }

        /// <summary>
        /// How often the timestamp adjustment between client and server is recalculated. If you need a very small TimeSpan here you're probably better of syncing your server time more often
        /// </summary>
        public TimeSpan? TimestampRecalculationInterval { get; set; }

        /// <summary>
        /// Set the values of this options on the target options
        /// </summary>
        public T Set<T>(T item) where T : RestApiOptions, new()
        {
            item.ApiCredentials = ApiCredentials?.Copy();
            item.OutputOriginalData = OutputOriginalData;
            item.AutoTimestamp = AutoTimestamp;
            item.TimestampRecalculationInterval = TimestampRecalculationInterval;
            return item;
        }
    }

    /// <summary>
    /// Http API options
    /// </summary>
    /// <typeparam name="TApiCredentials"></typeparam>
    public class RestApiOptions<TApiCredentials>: RestApiOptions where TApiCredentials: ApiCredentials
    {
        /// <summary>
        /// The api credentials used for signing requests to this API.
        /// </summary>
        public new TApiCredentials? ApiCredentials
        {
            get => (TApiCredentials?)base.ApiCredentials;
            set => base.ApiCredentials = value;
        }
    }
}
