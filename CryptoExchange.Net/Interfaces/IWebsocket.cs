using CryptoExchange.Net.Objects;
using System;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Interfaces
{
    /// <summary>
    /// Webscoket connection interface
    /// </summary>
    public interface IWebsocket: IDisposable
    {
        /// <summary>
        /// Websocket closed event
        /// </summary>
        event Action OnClose;
        /// <summary>
        /// Websocket message received event
        /// </summary>
        event Action<string> OnMessage;
        /// <summary>
        /// Websocket error event
        /// </summary>
        event Action<Exception> OnError;
        /// <summary>
        /// Websocket opened event
        /// </summary>
        event Action OnOpen;
        /// <summary>
        /// Websocket has lost connection to the server and is attempting to reconnect
        /// </summary>
        event Action OnReconnecting;
        /// <summary>
        /// Websocket has reconnected to the server
        /// </summary>
        event Action OnReconnected;

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
        /// Connect the socket
        /// </summary>
        /// <returns></returns>
        Task<bool> ConnectAsync();
        /// <summary>
        /// Send data
        /// </summary>
        /// <param name="data"></param>
        void Send(string data);
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
    }
}
