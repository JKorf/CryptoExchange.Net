using System;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Interfaces;

namespace CryptoExchange.Net.Implementation
{
    public class TestWebsocket: IWebsocket
    {
        public void Dispose()
        {
        }

        public void SetEnabledSslProtocols(SslProtocols protocols)
        {
        }

        public void SetProxy(string host, int port)
        {
        }

        public event Action OnClose;
        public event Action<string> OnMessage;
        public event Action<Exception> OnError;
        public event Action OnOpen;

        public bool IsClosed { get; private set; }
        public bool IsOpen { get; private set; }
        public bool PingConnection { get; set; }
        public TimeSpan PingInterval { get; set; }

        public Task<bool> Connect()
        {
            IsClosed = false;
            IsOpen = true;
            OnOpen?.Invoke();

            return Task.FromResult(true);
        }

        public void Send(string data)
        {
        }
        
        public void EnqueueMessage(string data)
        {
            Thread.Sleep(10);
            OnMessage?.Invoke(data);
        }

        public void InvokeError(Exception ex, bool closeConnection)
        {
            Thread.Sleep(10);
            OnError?.Invoke(ex);
            if (closeConnection)
                Close();
        }

        public Task Close()
        {
            IsClosed = true;
            IsOpen = false;
            OnClose?.Invoke();
            return Task.FromResult(0);
        }
    }
}
