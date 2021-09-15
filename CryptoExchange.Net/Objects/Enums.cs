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
        Synced
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
    {
        /// <summary>
        /// Send multiple key=value for each entry
        /// </summary>
        MultipleValues,
        /// <summary>
        /// Create an []=value array
        /// </summary>
        Array
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
        Closest
    }
}
