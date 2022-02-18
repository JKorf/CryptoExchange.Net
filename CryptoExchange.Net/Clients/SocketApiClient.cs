using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net
{
    /// <summary>
    /// Base socket API client for interaction with a websocket API
    /// </summary>
    public abstract class SocketApiClient : BaseApiClient
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="options">The base client options</param>
        /// <param name="apiOptions">The Api client options</param>
        public SocketApiClient(BaseClientOptions options, ApiClientOptions apiOptions): base(options, apiOptions)
        {
        }
    }
}
