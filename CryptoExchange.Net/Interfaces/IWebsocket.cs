using CryptoExchange.Net.Objects;
using System;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

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
        /// Encoding to use
        /// </summary>
        Encoding? Encoding { get; set; }
        /// <summary>
        /// Reconnecting
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
        /// Socket url
        /// </summary>
        string Url { get; }
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
        Task<bool> ConnectAsync();
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
        Task CloseAsync();
        /// <summary>
        /// Set proxy
        /// </summary>
        /// <param name="proxy"></param>
        void SetProxy(ApiProxy proxy);
    }
}
