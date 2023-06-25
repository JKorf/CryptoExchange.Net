using CryptoExchange.Net.Authentication;

namespace CryptoExchange.Net.Interfaces
{
    /// <summary>
    /// Base api client
    /// </summary>
    public interface IBaseApiClient
    {
        /// <summary>
        /// Base address
        /// </summary>
        string BaseAddress { get; }

        /// <summary>
        /// Set the API credentials for this API client
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="credentials"></param>
        void SetApiCredentials<T>(T credentials) where T : ApiCredentials;
    }
}