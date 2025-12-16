using CryptoExchange.Net.Authentication;

namespace CryptoExchange.Net.Objects.Options
{
    /// <summary>
    /// Options for API usage
    /// </summary>
    public class ApiOptions
    {
        /// <summary>
        /// If true, the CallResult and DataEvent objects will also include the originally received string data in the OriginalData property.
        /// Note that this comes at a performance cost
        /// </summary>
        public bool? OutputOriginalData { get; set; }

        /// <summary>
        /// The api credentials used for signing requests to this API. Overrides API credentials provided in the client options
        /// </summary>        
        public ApiCredentials? ApiCredentials { get; set; }
    }
}
