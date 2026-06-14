using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Interfaces.Clients;
using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.Clients
{
    /// <summary>
    /// Client provider, which can provide user specific rest and socket clients
    /// </summary>
    /// <typeparam name="TRestClient">The type of the rest client</typeparam>
    /// <typeparam name="TSocketClient">The type of the socket client</typeparam>
    /// <typeparam name="TCredentials">The type of the credentials</typeparam>
    /// <typeparam name="TEnvironment">The type of the environment</typeparam>
    public interface IUserClientProvider<TRestClient, TSocketClient, TCredentials, TEnvironment> : IExchangeService
        where TRestClient : IRestClient
        where TSocketClient : ISocketClient
        where TCredentials : ApiCredentials
        where TEnvironment : TradeEnvironment
    {
        /// <summary>
        /// Initialize a rest client for a specific user
        /// </summary>
        /// <param name="userIdentifier">The identifier of the user</param>
        /// <param name="credentials">The credentials of the user</param>
        /// <param name="environment">The environment</param>
        void InitializeUserClient(string userIdentifier, TCredentials credentials, TEnvironment? environment = null);

        /// <summary>
        /// Clear all cached clients
        /// </summary>
        void Clear();

        /// <summary>
        /// Clear clients for a specific user
        /// </summary>
        /// <param name="userIdentifier"></param>
        void ClearUserClients(string userIdentifier);

        /// <summary>
        /// Get a rest client instance for a specific user. If the client does not exist, it will be created.
        /// </summary>
        /// <param name="userIdentifier">The identifier of the user</param>
        /// <param name="credentials">The credentials of the user</param>
        /// <param name="environment">The environment</param>
        /// <returns>The rest client instance</returns>
        TRestClient GetRestClient(string userIdentifier, TCredentials? credentials = null, TEnvironment? environment = null);

        /// <summary>
        /// Get a socket client instance for a specific user. If the client does not exist, it will be created.
        /// </summary>
        /// <param name="userIdentifier">The identifier of the user</param>
        /// <param name="credentials">The credentials of the user</param>
        /// <param name="environment">The environment</param>
        /// <returns>The socket client instance</returns>
        TSocketClient GetSocketClient(string userIdentifier, TCredentials? credentials = null, TEnvironment? environment = null);
    }
}