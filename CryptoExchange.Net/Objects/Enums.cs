namespace CryptoExchange.Net.Objects
{
    /// <summary>
    /// What to do when a request would exceed the rate limit
    /// </summary>
    public enum RateLimitingBehaviour
    {
        /// <summary>
        /// Fail the request
        /// </summary>
        Fail,
        /// <summary>
        /// Wait till the request can be send
        /// </summary>
        Wait
    }

    /// <summary>
    /// What to do when a request would exceed the rate limit
    /// </summary>
    public enum RateLimitWindowType
    {
        /// <summary>
        /// A sliding window
        /// </summary>
        Sliding,
        /// <summary>
        /// A fixed interval window
        /// </summary>
        Fixed,
        /// <summary>
        /// A fixed interval starting after the first request
        /// </summary>
        FixedAfterFirst,
        /// <summary>
        /// Decaying window
        /// </summary>
        Decay
    }

    /// <summary>
    /// Where the parameters for a HttpMethod should be added in a request
    /// </summary>
    public enum HttpMethodParameterPosition
    {
        /// <summary>
        /// Parameters in body
        /// </summary>
        InBody,
        /// <summary>
        /// Parameters in url
        /// </summary>
        InUri
    }

    /// <summary>
    /// The format of the request body
    /// </summary>
    public enum RequestBodyFormat
    {
        /// <summary>
        /// Form data
        /// </summary>
        FormData,
        /// <summary>
        /// Json
        /// </summary>
        Json
    }

    /// <summary>
    /// Status of the order book
    /// </summary>
    public enum OrderBookStatus
    {
        /// <summary>
        /// Not connected
        /// </summary>
        Disconnected,
        /// <summary>
        /// Connecting
        /// </summary>
        Connecting,
        /// <summary>
        /// Reconnecting
        /// </summary>
        Reconnecting,
        /// <summary>
        /// Syncing data
        /// </summary>
        Syncing,
        /// <summary>
        /// Data synced, order book is up to date
        /// </summary>
        Synced,
        /// <summary>
        /// Disposing
        /// </summary>
        Disposing,
        /// <summary>
        /// Disposed
        /// </summary>
        Disposed
    }

    /// <summary>
    /// Order book entry type
    /// </summary>
    public enum OrderBookEntryType
    {
        /// <summary>
        /// Ask
        /// </summary>
        Ask,
        /// <summary>
        /// Bid
        /// </summary>
        Bid
    }

    /// <summary>
    /// Define how array parameters should be send
    /// </summary>
    public enum ArrayParametersSerialization
#pragma warning disable CS1570 // XML comment has badly formed XML
    {
        /// <summary>
        /// Send as key=value1&key=value2
        /// </summary>
        MultipleValues,

        /// <summary>
        /// Send as key[]=value1&key[]=value2
        /// </summary>
        Array,
        /// <summary>
        /// Send as key=[value1, value2]
        /// </summary>
        JsonArray
#pragma warning restore CS1570 // XML comment has badly formed XML
    }

    /// <summary>
    /// How to round
    /// </summary>
    public enum RoundingType
    {
        /// <summary>
        /// Round down (flooring)
        /// </summary>
        Down,
        /// <summary>
        /// Round to closest value
        /// </summary>
        Closest,
        /// <summary>
        /// Round up (ceil)
        /// </summary>
        Up
    }

    /// <summary>
    /// Type of the update
    /// </summary>
    public enum SocketUpdateType
    {
        /// <summary>
        /// A update
        /// </summary>
        Update,
        /// <summary>
        /// A snapshot, generally send at the start of the connection
        /// </summary>
        Snapshot
    }

    /// <summary>
    /// Reconnect policy
    /// </summary>
    public enum ReconnectPolicy
    {
        /// <summary>
        /// Reconnect is disabled
        /// </summary>
        Disabled,
        /// <summary>
        /// Fixed delay of `ReconnectInterval` between retries
        /// </summary>
        FixedDelay,
        /// <summary>
        /// Backof policy of 2^`reconnectAttempt`, where `reconnectAttempt` has a max value of 5
        /// </summary>
        ExponentialBackoff
    }

    /// <summary>
    /// The data source of the result
    /// </summary>
    public enum ResultDataSource
    {
        /// <summary>
        /// From server
        /// </summary>
        Server,
        /// <summary>
        /// From cache
        /// </summary>
        Cache
    }

}
