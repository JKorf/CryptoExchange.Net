using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Logging;
using SuperSocket.ClientEngine.Proxy;
using WebSocket4Net;

namespace CryptoExchange.Net.Implementation
{
    public class BaseSocket: IWebsocket
    {
        protected WebSocket socket;
        protected Log log;
        protected object socketLock = new object();

        protected readonly List<Action<Exception>> errorhandlers = new List<Action<Exception>>();
        protected readonly List<Action> openhandlers = new List<Action>();
        protected readonly List<Action> closehandlers = new List<Action>();
        protected readonly List<Action<string>> messagehandlers = new List<Action<string>>();

        public bool IsClosed => socket.State == WebSocketState.Closed;
        public bool IsOpen => socket.State == WebSocketState.Open;

        public bool PingConnection
        {
            get => socket.EnableAutoSendPing;
            set => socket.EnableAutoSendPing = value;
        }

        public TimeSpan PingInterval
        {
            get => TimeSpan.FromSeconds(socket.AutoSendPingInterval);
            set => socket.AutoSendPingInterval = (int) Math.Round(value.TotalSeconds);
        }

        public BaseSocket(Log log, string url):this(log, url, new Dictionary<string, string>(), new Dictionary<string, string>())
        {
        }

        public BaseSocket(Log log, string url, IDictionary<string, string> cookies, IDictionary<string, string> headers)
        {
            this.log = log;
            socket = new WebSocket(url, cookies: cookies.ToList(), customHeaderItems: headers.ToList());
            socket.EnableAutoSendPing = true;
            socket.AutoSendPingInterval = 10;
            socket.Opened += (o, s) => Handle(openhandlers);
            socket.Closed += (o, s) => Handle(closehandlers);
            socket.Error += (o, s) => Handle(errorhandlers, s.Exception);
            socket.MessageReceived += (o, s) => Handle(messagehandlers, s.Message);
            socket.EnableAutoSendPing = true;
            socket.AutoSendPingInterval = 10;
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
                handle?.Invoke();
        }

        protected void Handle<T>(List<Action<T>> handlers, T data)
        {
            foreach (var handle in new List<Action<T>>(handlers))
                handle?.Invoke(data);
        }

        public async Task Close()
        {
            await Task.Run(() =>
            {
                lock (socketLock)
                {
                    if (socket == null || IsClosed)
                        return;

                    log.Write(LogVerbosity.Debug, "Closing websocket");
                    ManualResetEvent evnt = new ManualResetEvent(false);
                    var handler = new EventHandler((o, a) => evnt.Set());
                    socket.Closed += handler;
                    socket.Close();
                    bool triggered = evnt.WaitOne(3000);
                    socket.Closed -= handler;

                    if (!triggered)
                        log.Write(LogVerbosity.Debug, "Websocket closed event did not trigger");
                    else
                        log.Write(LogVerbosity.Debug, "Websocket closed");
                }
            }).ConfigureAwait(false);
        }

        public void Send(string data)
        {
            socket.Send(data);
        }

        public async Task<bool> Connect()
        {
            return await Task.Run(() =>
            {
                lock (socketLock)
                {
                    log.Write(LogVerbosity.Debug, "Connecting websocket");
                    ManualResetEvent evnt = new ManualResetEvent(false);
                    var handler = new EventHandler((o, a) => evnt.Set());
                    socket.Opened += handler;
                    socket.Closed += handler;
                    socket.Open();
                    evnt.WaitOne();
                    socket.Opened -= handler;
                    socket.Closed -= handler;
                    var connected = socket.State == WebSocketState.Open;
                    if (connected)
                        log.Write(LogVerbosity.Debug, "Websocket connected");
                    else
                        log.Write(LogVerbosity.Debug, "Websocket connection failed, state: " + socket.State);
                    return connected;
                }
            }).ConfigureAwait(false);
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

        public void Dispose()
        {
            lock (socketLock)
            {
                if (socket != null)
                    log.Write(LogVerbosity.Debug, "Disposing websocket");

                socket?.Dispose();
                socket = null;

                errorhandlers.Clear();
                openhandlers.Clear();
                closehandlers.Clear();
                messagehandlers.Clear();
            }
        }
    }
}
