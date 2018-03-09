using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Interfaces;
using SuperSocket.ClientEngine.Proxy;
using WebSocket4Net;

namespace CryptoExchange.Net.Implementation
{
    public class BaseSocket: IWebsocket
    {
        protected WebSocket socket;

        protected readonly List<Action<Exception>> errorhandlers = new List<Action<Exception>>();
        protected readonly List<Action> openhandlers = new List<Action>();
        protected readonly List<Action> closehandlers = new List<Action>();
        protected readonly List<Action<string>> messagehandlers = new List<Action<string>>();

        public bool IsClosed => socket.State == WebSocketState.Closed;
        public bool IsOpen => socket.State == WebSocketState.Open;

        public BaseSocket(string url):this(url, new Dictionary<string, string>(), new Dictionary<string, string>())
        {
        }

        public BaseSocket(string url, IDictionary<string, string> cookies, IDictionary<string, string> headers)
        {
            socket = new WebSocket(url, cookies: cookies.ToList(), customHeaderItems: headers.ToList());
            socket.Opened += (o, s) => Handle(openhandlers);
            socket.Closed += (o, s) => Handle(closehandlers);
            socket.Error += (o, s) => Handle(errorhandlers, s.Exception);
            socket.MessageReceived += (o, s) => Handle(messagehandlers, s.Message);
        }

        public event Action OnClose
        {
            add => closehandlers.Add(value);
            remove => closehandlers.Remove(value);
        }
        public event Action<string> OnMessage
        {
            add => messagehandlers.Add(value);
            remove => messagehandlers.Remove(value);
        }
        public event Action<Exception> OnError
        {
            add => errorhandlers.Add(value);
            remove => errorhandlers.Remove(value);
        }
        public event Action OnOpen
        {
            add => openhandlers.Add(value);
            remove => closehandlers.Remove(value);
        }

        protected static void Handle(List<Action> handlers)
        {
            foreach (var handle in new List<Action>(handlers))
                handle();
        }

        protected void Handle<T>(List<Action<T>> handlers, T data)
        {
            foreach (var handle in new List<Action<T>>(handlers))
                handle(data);
        }

        public async Task Close()
        {
            await Task.Run(() =>
            {
                ManualResetEvent evnt = new ManualResetEvent(false);
                var handler = new EventHandler((o, a) => evnt.Set());
                socket.Closed += handler;
                socket.Close();
                evnt.WaitOne();
                socket.Closed -= handler;
            });
        }

        public void Send(string data)
        {
            socket.Send(data);
        }

        public async Task<bool> Connect()
        {
            return await Task.Run(() =>
            {
                ManualResetEvent evnt = new ManualResetEvent(false);
                var handler = new EventHandler((o, a) => evnt.Set());
                socket.Opened += handler;
                socket.Closed += handler;
                socket.Open();
                evnt.WaitOne();
                socket.Opened -= handler;
                socket.Closed -= handler;
                return socket.State == WebSocketState.Open;
            });
        }

        public void SetEnabledSslProtocols(SslProtocols protocols)
        {
            socket.Security.EnabledSslProtocols = protocols;
        }

        public void SetProxy(string host, int port)
        {
            IPAddress address;
            socket.Proxy = IPAddress.TryParse(host, out address)
                ? new HttpConnectProxy(new IPEndPoint(address, port))
                : new HttpConnectProxy(new DnsEndPoint(host, port));
        }
    }
}
