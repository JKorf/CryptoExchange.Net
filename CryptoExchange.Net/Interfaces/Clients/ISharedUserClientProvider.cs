using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Interfaces.Clients
{
    /// <summary>
    /// Shared API client provider
    /// </summary>
    public interface ISharedUserClientProvider: IExchangeService
    {
        /// <summary>
        /// Get a public Shared API client
        /// </summary>
        /// <param name="environment">Trade environment</param>
        ISharedApiClientBase GetPublicSharedApiClient(TradeEnvironment? environment = null);

        /// <summary>
        /// Try getting an authenticated Shared API client for the user. 
        /// Will call the CredentialResolver configured in the UserClientProviderCredentialResolver client options to get the credentials for the user.
        /// </summary>
        /// <param name="userIdentifier">Unique user identifier</param>
        /// <param name="environment">Trade environment</param>
        /// <param name="ct">Cancellation token</param>
        Task<CallResult<ISharedApiClientBase>> TryGetSharedApiClientAsync(
            string userIdentifier, 
            TradeEnvironment? environment = null, 
            CancellationToken ct = default);
    }

    /// <inheritdoc />
    public interface ISharedUserClientProvider<TSharedClient, TEnvironment> : ISharedUserClientProvider
        where TSharedClient : class, ISharedApiClientBase
        where TEnvironment : TradeEnvironment
    {
        /// <inheritdoc />
        TSharedClient GetPublicSharedApiClient(TEnvironment? environment = null);

        /// <inheritdoc />
        Task<CallResult<TSharedClient>> TryGetSharedApiClientAsync(
            string userIdentifier,
            TEnvironment? environment = null,
            CancellationToken ct = default);
    }
}
