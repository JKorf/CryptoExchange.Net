using CryptoExchange.Net.Authentication;
using System;

namespace CryptoExchange.Net.Objects.Options
{
    /// <summary>
    /// Socket api options
    /// </summary>
    public class SocketApiOptions : ApiOptions
    {
        /// <summary>
        /// The max time of not receiving any data after which the connection is assumed to be dropped. This can only be used for socket connections where a steady flow of data is expected,
        /// for example when the server sends intermittent ping requests
        /// </summary>
        public TimeSpan? SocketNoDataTimeout { get; set; }

        /// <summary>
        /// The max amount of connections to make to the server. Can be used for API's which only allow a certain number of connections. Changing this to a high value might cause issues.
        /// </summary>
        public int? MaxSocketConnections { get; set; }

        /// <summary>
        /// Create a copy of this options
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Copy<T>() where T : SocketApiOptions, new()
        {
            return new T
            {
                ApiCredentials = ApiCredentials?.Copy(),
                OutputOriginalData = OutputOriginalData,
                SocketNoDataTimeout = SocketNoDataTimeout,
                MaxSocketConnections = MaxSocketConnections,
            };
        }
    }

    /// <summary>
    /// Socket API options
    /// </summary>
    /// <typeparam name="TApiCredentials"></typeparam>
    public class SocketApiOptions<TApiCredentials> : SocketApiOptions where TApiCredentials : ApiCredentials
    {
        /// <summary>
        /// The api credentials used for signing requests to this API.
        /// </summary>        
        public new TApiCredentials? ApiCredentials
        {
            get => (TApiCredentials?)base.ApiCredentials;
            set => base.ApiCredentials = value;
        }
    }
}
