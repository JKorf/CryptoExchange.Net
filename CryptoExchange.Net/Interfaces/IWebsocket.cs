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
        /// Unique id for this socket
        /// </summary>
        int Id { get; }
        /// <summary>
        /// Origin header
        /// </summary>
        string? Origin { get; set; }
        /// <summary>
        /// Encoding to use for sending/receiving string data
        /// </summary>
        Encoding? Encoding { get; set; }
        /// <summary>
        /// Whether socket is in the process of reconnecting
        /// </summary>
        bool Reconnecting { get; set; }
        /// <summary>
        /// The max amount of outgoing messages per second
        /// </summary>
        int? RatelimitPerSecond { get; set; }
        /// <summary>
        /// The current kilobytes per second of data being received, averaged over the last 3 seconds
        /// </summary>
        double IncomingKbps { get; }
        /// <summary>
        /// Handler for byte data
        /// </summary>
        Func<byte[], string>? DataInterpreterBytes { get; set; }
        /// <summary>
        /// Handler for string data
        /// </summary>
        Func<string, string>? DataInterpreterString { get; set; }
        /// <summary>
        /// The url the socket connects to
        /// </summary>
        string Url { get; }
        /// <summary>
        /// Whether the socket connection is closed
        /// </summary>
        bool IsClosed { get; }
        /// <summary>
        /// Whether the socket connection is open
        /// </summary>
        bool IsOpen { get; }
        /// <summary>
        /// Supported ssl protocols
        /// </summary>
        SslProtocols SSLProtocols { get; set; }
        /// <summary>
        /// The max time for no data being received before the connection is considered lost
        /// </summary>
        TimeSpan Timeout { get; set; }
        /// <summary>
        /// Set a proxy to use when connecting
        /// </summary>
        /// <param name="proxy"></param>
        void SetProxy(ApiProxy proxy);
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
        /// Reset socket when a connection is lost to prepare for a new connection
        /// </summary>
        void Reset();
        /// <summary>
        /// Close the connection
        /// </summary>
        /// <returns></returns>
        Task CloseAsync();
    }
}
