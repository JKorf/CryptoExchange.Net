using System;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects.Options;

namespace CryptoExchange.Net.Interfaces.Clients
{
    /// <summary>
    /// Base class for rest API implementations
    /// </summary>
    public interface IRestClient : IDisposable
    {
        /// <summary>
        /// The options provided for this client
        /// </summary>
        ExchangeOptions ClientOptions { get; }

        /// <summary>
        /// The total amount of requests made with this client
        /// </summary>
        int TotalRequestsMade { get; }

        /// <summary>
        /// The exchange name
        /// </summary>
        string Exchange { get; }

        /// <summary>
        /// Whether client is disposed
        /// </summary>
        bool Disposed { get; }
    }

    /// <inheritdoc />
    public interface IRestClient<TApiCredentials> : IRestClient where TApiCredentials : ApiCredentials
    {
        /// <summary>
        /// Set the API credentials for this client. All Api clients in this client will use the new credentials, regardless of earlier set options.
        /// </summary>
        /// <param name="credentials">The credentials to set</param>
        void SetApiCredentials(TApiCredentials credentials);

        /// <summary>
        /// Update specific options
        /// </summary>
        /// <param name="options">Options to update. Only specific options are changeable after the client has been created</param>
        void SetOptions(UpdateOptions<TApiCredentials> options);
    }
}