using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using WebSocket4Net;

namespace CryptoExchange.Net.Interfaces
{
    /// <summary>
    /// Interface for websocket interaction
    /// </summary>
    public interface IWebsocket: IDisposable
    {
        /// <summary>
        /// Websocket closed
        /// </summary>
        event Action OnClose;
        /// <summary>
        /// Websocket message received
        /// </summary>
        event Action<string> OnMessage;
        /// <summary>
        /// Websocket error
        /// </summary>
        event Action<Exception> OnError;
        /// <summary>
        /// Websocket opened
        /// </summary>
        event Action OnOpen;

        /// <summary>
        /// Id
        /// </summary>
        int Id { get; }
        /// <summary>
        /// Origin
        /// </summary>
        string? Origin { get; set; }
        /// <summary>
        /// Reconnecting
        /// </summary>
        bool Reconnecting { get; set; }
        /// <summary>
        /// Handler for byte data
        /// </summary>
        Func<byte[], string>? DataInterpreterBytes { get; set; }
        /// <summary>
        /// Handler for string data
        /// </summary>
        Func<string, string>? DataInterpreterString { get; set; }
        /// <summary>
        /// Socket url
        /// </summary>
        string Url { get; }
        /// <summary>
        /// State
        /// </summary>
        WebSocketState SocketState { get; }
        /// <summary>
        /// Is closed
        /// </summary>
        bool IsClosed { get; }
        /// <summary>
        /// Is open
        /// </summary>
        bool IsOpen { get; }
        /// <summary>
        /// Supported ssl protocols
        /// </summary>
        SslProtocols SSLProtocols { get; set; }
        /// <summary>
        /// Timeout
        /// </summary>
        TimeSpan Timeout { get; set; }
        /// <summary>
        /// Connect the socket
        /// </summary>
        /// <returns></returns>
        Task<bool> Connect();
        /// <summary>
        /// Send data
        /// </summary>
        /// <param name="data"></param>
        void Send(string data);
        /// <summary>
        /// Reset socket
        /// </summary>
        void Reset();
        /// <summary>
        /// Close the connecting
        /// </summary>
        /// <returns></returns>
        Task Close();
        /// <summary>
        /// Set proxy
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        void SetProxy(string host, int port);
    }
}
