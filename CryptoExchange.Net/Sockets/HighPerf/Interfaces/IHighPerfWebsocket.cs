using CryptoExchange.Net.Objects;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Sockets.HighPerf.Interfaces
{
    /// <summary>
    /// Websocket connection interface
    /// </summary>
    public interface IHighPerfWebsocket : IDisposable
    {
        /// <summary>
        /// Websocket closed event
        /// </summary>
        event Func<Task> OnClose;
        /// <summary>
        /// Websocket error event
        /// </summary>
        event Func<Exception, Task> OnError;
        /// <summary>
        /// Websocket opened event
        /// </summary>
        event Func<Task> OnOpen;

        /// <summary>
        /// Unique id for this socket
        /// </summary>
        int Id { get; }
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
        /// Connect the socket
        /// </summary>
        /// <returns></returns>
        Task<CallResult> ConnectAsync(CancellationToken ct);
        /// <summary>
        /// Send string data
        /// </summary>
        ValueTask<bool> SendAsync(string data);
        /// <summary>
        /// Send byte data
        /// </summary>
        ValueTask<bool> SendAsync(byte[] data, WebSocketMessageType type = WebSocketMessageType.Binary);
        /// <summary>
        /// Close the connection
        /// </summary>
        /// <returns></returns>
        Task CloseAsync();
    }
}
