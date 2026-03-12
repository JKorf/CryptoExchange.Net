using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects.Options;

namespace CryptoExchange.Net.Interfaces.Clients
{
    /// <summary>
    /// Base rest API client
    /// </summary>
    public interface IRestApiClient : IBaseApiClient
    {
        /// <summary>
        /// The factory for creating requests. Used for unit testing
        /// </summary>
        IRequestFactory RequestFactory { get; set; }

        /// <summary>
        /// Total amount of requests made with this API client
        /// </summary>
        int TotalRequestsMade { get; set; }
    }

    public interface IRestApiClient<TApiCredentials> : IRestApiClient
        where TApiCredentials : ApiCredentials
    {
        /// <summary>
        /// Whether or not API credentials have been configured for this client. Does not check the credentials are actually valid.
        /// </summary>
        bool Authenticated { get; }

        /// <summary>
        /// Set the API credentials for this API client
        /// </summary>
        void SetApiCredentials(TApiCredentials credentials);

        /// <summary>
        /// Set new options. Note that when using a proxy this should be provided in the options even when already set before or it will be reset.
        /// </summary>
        /// <param name="options">Options to set</param>
        void SetOptions(UpdateOptions<TApiCredentials> options);
    }
}