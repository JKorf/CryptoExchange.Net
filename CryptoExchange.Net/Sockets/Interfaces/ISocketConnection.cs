using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Objects;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Sockets.Interfaces
{
    /// <summary>
    /// Socket connection
    /// </summary>
    public interface ISocketConnection
    {
        /// <summary>
        /// The API client the connection belongs to
        /// </summary>
        SocketApiClient ApiClient { get; set; }
        /// <summary>
        /// Whether the connection has been authenticated
        /// </summary>
        bool Authenticated { get; set; }
        /// <summary>
        /// Is there a subscription which requires authentication on this connection
        /// </summary>
        bool HasAuthenticatedSubscription { get; }
        /// <summary>
        /// Whether the connection is established
        /// </summary>
        bool Connected { get; }
        /// <summary>
        /// Connection URI
        /// </summary>
        Uri ConnectionUri { get; }
        /// <summary>
        /// Id
        /// </summary>
        int SocketId { get; }
        /// <summary>
        /// Tag
        /// </summary>
        string Tag { get; set; }
        /// <summary>
        /// Closed event
        /// </summary>

        event Action? ConnectionClosed;
        /// <summary>
        /// Connect the websocket
        /// </summary>
        Task<CallResult> ConnectAsync(CancellationToken ct);
        /// <summary>
        /// Close the connection
        /// </summary>
        /// <returns></returns>
        Task CloseAsync();
        /// <summary>
        /// Dispose
        /// </summary>
        void Dispose();
    }
}