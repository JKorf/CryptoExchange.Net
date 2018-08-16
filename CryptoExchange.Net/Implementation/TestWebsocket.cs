using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;
using CryptoExchange.Net.Interfaces;

namespace CryptoExchange.Net.Implementation
{
    public class TestWebsocket: IWebsocket
    {
        public List<string> MessagesSend = new List<string>();

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

        public bool IsClosed { get; private set; } = true;
        public bool IsOpen { get; private set; }
        public bool PingConnection { get; set; }
        public TimeSpan PingInterval { get; set; }

        public bool HasConnection = true;

        public Task<bool> Connect()
        {
            if (!HasConnection)
            {
                OnError?.Invoke(new Exception("No connection"));
                return Task.FromResult(false);
            }

            IsClosed = false;
            IsOpen = true;
            OnOpen?.Invoke();

            return Task.FromResult(true);
        }

        public void Send(string data)
        {
            if (!HasConnection)
            {
                OnError?.Invoke(new Exception("No connection"));
                Close();
                return;
            }

            MessagesSend.Add(data);
        }
        
        public async Task EnqueueMessage(string data, int wait)
        {
            await Task.Delay(wait);
            OnMessage?.Invoke(data);
        }

        public async Task InvokeError(Exception ex, bool closeConnection)
        {
            await Task.Delay(10);
            OnError?.Invoke(ex);
            if (closeConnection)
                await Close();
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
