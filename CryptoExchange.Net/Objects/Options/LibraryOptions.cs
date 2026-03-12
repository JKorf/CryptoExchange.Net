using CryptoExchange.Net.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoExchange.Net.Objects.Options
{
    /// <summary>
    /// Library options
    /// </summary>
    public class LibraryOptions<TRestOptions, TSocketOptions, TEnvironment>
        where TRestOptions : RestExchangeOptions<TEnvironment>, new()
        where TSocketOptions : SocketExchangeOptions<TEnvironment>, new()
        where TEnvironment : TradeEnvironment
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
        /// The DI service lifetime for the socket client
        /// </summary>
        public ServiceLifetime? SocketClientLifeTime { get; set; }

        /// <summary>
        /// Copy values from these options to the target options
        /// </summary>
        public T Set<T>(T targetOptions) where T : LibraryOptions<TRestOptions, TSocketOptions, TEnvironment>
        {
            targetOptions.Environment = Environment;
            targetOptions.SocketClientLifeTime = SocketClientLifeTime;
            targetOptions.Rest = Rest.Set(targetOptions.Rest);
            targetOptions.Socket = Socket.Set(targetOptions.Socket);

            return targetOptions;
        }
    }

    /// <summary>
    /// Library options
    /// </summary>
    public class LibraryOptions<TRestOptions, TSocketOptions, TApiCredentials, TEnvironment> : LibraryOptions<TRestOptions, TSocketOptions, TEnvironment>
        where TRestOptions: RestExchangeOptions<TEnvironment, TApiCredentials>, new()
        where TSocketOptions: SocketExchangeOptions<TEnvironment, TApiCredentials>, new()
        where TApiCredentials: ApiCredentials
        where TEnvironment: TradeEnvironment
    {
        /// <summary>
        /// The api credentials used for signing requests.
        /// </summary>
        public TApiCredentials? ApiCredentials { get; set; }

        /// <summary>
        /// Copy values from these options to the target options
        /// </summary>
        public new T Set<T>(T targetOptions) where T: LibraryOptions<TRestOptions, TSocketOptions, TApiCredentials, TEnvironment>
        {
            targetOptions = base.Set(targetOptions);
            targetOptions.ApiCredentials = (TApiCredentials?)ApiCredentials?.Copy();
            return targetOptions;
        }
    }
}
