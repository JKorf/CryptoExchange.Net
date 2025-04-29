using CryptoExchange.Net.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoExchange.Net.Objects.Options
{
    /// <summary>
    /// Library options
    /// </summary>
    /// <typeparam name="TRestOptions"></typeparam>
    /// <typeparam name="TSocketOptions"></typeparam>
    /// <typeparam name="TApiCredentials"></typeparam>
    /// <typeparam name="TEnvironment"></typeparam>
    public class LibraryOptions<TRestOptions, TSocketOptions, TApiCredentials, TEnvironment>
        where TRestOptions: RestExchangeOptions, new()
        where TSocketOptions: SocketExchangeOptions, new()
        where TApiCredentials: ApiCredentials
        where TEnvironment: TradeEnvironment
    {
        /// <summary>
        /// Rest client options
        /// </summary>
        public TRestOptions Rest { get; set; } = new TRestOptions();

        /// <summary>
        /// Socket client options
        /// </summary>
        public TSocketOptions Socket { get; set; } = new TSocketOptions();

        /// <summary>
        /// Trade environment. Contains info about URL's to use to connect to the API.
        /// </summary>
        public TEnvironment? Environment { get; set; }

        /// <summary>
        /// The api credentials used for signing requests.
        /// </summary>
        public TApiCredentials? ApiCredentials { get; set; }

        /// <summary>
        /// The DI service lifetime for the socket client
        /// </summary>
        public ServiceLifetime? SocketClientLifeTime { get; set; }

        /// <summary>
        /// Copy values from these options to the target options
        /// </summary>
        public T Set<T>(T targetOptions) where T: LibraryOptions<TRestOptions, TSocketOptions, TApiCredentials, TEnvironment>
        {
            targetOptions.ApiCredentials = (TApiCredentials?)ApiCredentials?.Copy();
            targetOptions.Environment = Environment;
            targetOptions.SocketClientLifeTime = SocketClientLifeTime;
            targetOptions.Rest = Rest.Set(targetOptions.Rest);
            targetOptions.Socket = Socket.Set(targetOptions.Socket);

            return targetOptions;
        }
    }
}
