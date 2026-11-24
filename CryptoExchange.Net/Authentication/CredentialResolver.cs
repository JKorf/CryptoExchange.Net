using CryptoExchange.Net.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Authentication
{
    /// <summary>
    /// Credential resolver delegate
    /// </summary>
    /// <typeparam name="TCredentials">The type of credential</typeparam>
    /// <typeparam name="TEnvironment">The type of trading environment</typeparam>
    /// <param name="userIdentifier">The user identifier</param>
    /// <param name="environment">The trading environment</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Resolved credentials, or null if not found</returns>
    public delegate Task<TCredentials?> CredentialResolver<TCredentials, TEnvironment>(string userIdentifier, TEnvironment? environment, CancellationToken ct)
        where TCredentials : ApiCredentials
        where TEnvironment : TradeEnvironment;
}
