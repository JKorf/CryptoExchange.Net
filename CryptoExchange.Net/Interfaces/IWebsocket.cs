using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using WebSocket4Net;

namespace CryptoExchange.Net.Interfaces
{
    public interface IWebsocket: IDisposable
    {
        void SetEnabledSslProtocols(SslProtocols protocols);

        event Action OnClose;
        event Action<string> OnMessage;
        event Action<Exception> OnError;
        event Action OnOpen;

        WebSocketState SocketState { get; }
        bool IsClosed { get; }
        bool IsOpen { get; }
        bool PingConnection { get; set; }
        TimeSpan PingInterval { get; set; }

        Task<bool> Connect();
        void Send(string data);
        Task Close();
        void SetProxy(string host, int port);
    }
}
