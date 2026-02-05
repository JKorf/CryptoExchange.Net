using CryptoExchange.Net.Objects;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Sockets.Default.Interfaces
{
    /// <summary>
    /// Websocket connection interface
    /// </summary>
    public interface IWebsocket: IDisposable
    {
        /// <summary>
        /// Websocket closed event
        /// </summary>
        event Func<Task> OnClose;
        /// <summary>
        /// Websocket sent event, RequestId as parameter
        /// </summary>
        event Func<int, Task> OnRequestSent;
        /// <summary>
        /// Websocket query was ratelimited and couldn't be send
        /// </summary>
        event Func<int, Task>? OnRequestRateLimited;
        /// <summary>
        /// Connection was ratelimited and couldn't be established
        /// </summary>
        event Func<Task>? OnConnectRateLimited;
        /// <summary>
        /// Websocket error event
        /// </summary>
        event Func<Exception, Task> OnError;
        /// <summary>
        /// Websocket opened event
        /// </summary>
        event Func<Task> OnOpen;
        /// <summary>
        /// Websocket has lost connection to the server and is attempting to reconnect
        /// </summary>
        event Func<Task> OnReconnecting;
        /// <summary>
        /// Websocket has reconnected to the server
        /// </summary>
        event Func<Task> OnReconnected;
        /// <summary>
        /// Get reconnection url
        /// </summary>
        Func<Task<Uri?>>? GetReconnectionUrl { get; set; }

        /// <summary>
        /// Unique id for this socket
        /// </summary>
        int Id { get; }
        /// <summary>
        /// The current kilobytes per second of data being received, averaged over the last 3 seconds
        /// </summary>
        double IncomingKbps { get; }
        /// <summary>
        /// The uri the socket connects to
        /// </summary>
        Uri Uri { get; }
        /// <summary>
        /// Whether the socket connection is closed
        /// </summary>
        bool IsClosed { get; }
        /// <summary>
        /// Whether the socket connection is open
        /// </summary>
        bool IsOpen { get; }
        /// <summary>
        /// Last timestamp something was received from the server
        /// </summary>
        DateTime? LastReceiveTime { get; }
        /// <summary>
        /// Connect the socket
        /// </summary>
        /// <returns></returns>
        Task<CallResult> ConnectAsync(CancellationToken ct);
        /// <summary>
        /// Send string data
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <param name="weight"></param>
        bool Send(int id, string data, int weight);
        /// <summary>
        /// Send byte data
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <param name="weight"></param>
        bool Send(int id, byte[] data, int weight);
        /// <summary>
        /// Reconnect the socket
        /// </summary>
        /// <returns></returns>
        Task ReconnectAsync();
        /// <summary>
        /// Close the connection
        /// </summary>
        /// <returns></returns>
        Task CloseAsync();

        /// <summary>
        /// Update proxy setting
        /// </summary>
        void UpdateProxy(ApiProxy? proxy);
    }
}
