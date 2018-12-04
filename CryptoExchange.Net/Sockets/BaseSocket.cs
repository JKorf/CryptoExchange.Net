using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Logging;
using SuperSocket.ClientEngine;
using SuperSocket.ClientEngine.Proxy;
using WebSocket4Net;

namespace CryptoExchange.Net.Sockets
{
    public class BaseSocket: IWebsocket
    {
        internal static int lastStreamId;
        private static readonly object streamIdLock = new object();

        protected WebSocket socket;
        protected Log log;
        protected object socketLock = new object();

        protected readonly List<Action<Exception>> errorHandlers = new List<Action<Exception>>();
        protected readonly List<Action> openHandlers = new List<Action>();
        protected readonly List<Action> closeHandlers = new List<Action>();
        protected readonly List<Action<string>> messageHandlers = new List<Action<string>>();

        protected IDictionary<string, string> cookies;
        protected IDictionary<string, string> headers;

        public int Id { get; }
        public DateTime? DisconnectTime { get; set; }
        public bool ShouldReconnect { get; set; }
        public string Url { get; }
        public bool IsClosed => socket.State == WebSocketState.Closed;
        public bool IsOpen => socket.State == WebSocketState.Open;
        public SslProtocols SSLProtocols { get; set; } = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls;
        public Func<byte[], string> DataInterpreter { get; set; }

        public DateTime LastActionTime { get; private set; }
        public TimeSpan Timeout { get; set; }
        private Task timeoutTask;

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

        public WebSocketState SocketState
        {
            get
            {
                if (socket == null)
                    return WebSocketState.None;
                return socket.State;
            }
        }

        public BaseSocket(Log log, string url):this(log, url, new Dictionary<string, string>(), new Dictionary<string, string>())
        {
        }

        public BaseSocket(Log log, string url, IDictionary<string, string> cookies, IDictionary<string, string> headers)
        {
            Id = NextStreamId();
            this.log = log;
            Url = url;
            this.cookies = cookies;
            this.headers = headers;
        }

        private void HandleByteData(byte[] data)
        {
            var message = DataInterpreter(data);
            Handle(messageHandlers, message);
        }

        public event Action OnClose
        {
            add => closeHandlers.Add(value);
            remove => closeHandlers.Remove(value);
        }
        public event Action<string> OnMessage
        {
            add => messageHandlers.Add(value);
            remove => messageHandlers.Remove(value);
        }
        public event Action<Exception> OnError
        {
            add => errorHandlers.Add(value);
            remove => errorHandlers.Remove(value);
        }
        public event Action OnOpen
        {
            add => openHandlers.Add(value);
            remove => openHandlers.Remove(value);
        }

        protected void Handle(List<Action> handlers)
        {
            LastActionTime = DateTime.UtcNow;
            foreach (var handle in new List<Action>(handlers))
                handle?.Invoke();
        }

        protected void Handle<T>(List<Action<T>> handlers, T data)
        {
            LastActionTime = DateTime.UtcNow;
            foreach (var handle in new List<Action<T>>(handlers))
                handle?.Invoke(data);
        }

        protected void CheckTimeout()
        {
            while (true)
            {
                if (socket == null || socket.State != WebSocketState.Open)
                    return;

                if (DateTime.UtcNow - LastActionTime > Timeout)
                {
                    log.Write(LogVerbosity.Warning, $"No data received for {Timeout}, reconnecting socket");
                    Close().Wait();
                    return;
                }

                Thread.Sleep(500);
            }
        }

        public virtual async Task Close()
        {
            await Task.Run(() =>
            {
                lock (socketLock)
                {
                    if (socket == null || IsClosed)
                    {
                        log?.Write(LogVerbosity.Debug, $"Socket {Id} was already closed/disposed");
                        return;
                    }

                    var waitLock = new object();
                    log?.Write(LogVerbosity.Debug, $"Socket {Id} closing");
                    ManualResetEvent evnt = new ManualResetEvent(false);
                    var handler = new EventHandler((o, a) =>
                    {
                        lock(waitLock)
                            evnt?.Set();
                    });
                    socket.Closed += handler;
                    socket.Close();
                    evnt.WaitOne(2000);
                    lock (waitLock)
                    {
                        socket.Closed -= handler;
                        evnt.Dispose();
                        evnt = null;
                    }
                    log?.Write(LogVerbosity.Debug, $"Socket {Id} closed");
                }
            }).ConfigureAwait(false);
        }

        public virtual void Send(string data)
        {
            socket.Send(data);
        }

        public virtual async Task<bool> Connect()
        {
            if (socket == null)
            {
                socket = new WebSocket(Url, cookies: cookies.ToList(), customHeaderItems: headers.ToList())
                {
                    EnableAutoSendPing = true,
                    AutoSendPingInterval = 10
                };
                socket.Security.EnabledSslProtocols = SSLProtocols;
                socket.Opened += (o, s) => Handle(openHandlers);
                socket.Closed += (o, s) => Handle(closeHandlers);
                socket.Error += (o, s) => Handle(errorHandlers, s.Exception);
                socket.MessageReceived += (o, s) => Handle(messageHandlers, s.Message);
                socket.DataReceived += (o, s) => HandleByteData(s.Data);
            }

            return await Task.Run(() =>
            {
                bool connected;
                lock (socketLock)
                {
                    log?.Write(LogVerbosity.Debug, $"Socket {Id} connecting");
                    var waitLock = new object();
                    ManualResetEvent evnt = new ManualResetEvent(false);
                    var handler = new EventHandler((o, a) =>
                    {
                        lock (waitLock)
                            evnt?.Set();
                    });
                    var errorHandler = new EventHandler<ErrorEventArgs>((o, a) =>
                    {
                        lock(waitLock)
                            evnt?.Set();
                    });
                    socket.Opened += handler;
                    socket.Closed += handler;
                    socket.Error += errorHandler;
                    socket.Open();
                    evnt.WaitOne(TimeSpan.FromSeconds(15));
                    lock (waitLock)
                    {
                        socket.Opened -= handler;
                        socket.Closed -= handler;
                        socket.Error -= errorHandler;
                        evnt.Dispose();
                        evnt = null;
                    }
                    connected = socket.State == WebSocketState.Open;
                    if (connected)
                    {
                        log?.Write(LogVerbosity.Debug, $"Socket {Id} connected");
                        if ((timeoutTask == null || timeoutTask.IsCompleted) && Timeout != default(TimeSpan))
                            timeoutTask = Task.Run(() => CheckTimeout());
                    }
                    else
                        log?.Write(LogVerbosity.Debug, $"Socket {Id} connection failed, state: " + socket.State);
                }

                if (socket.State == WebSocketState.Connecting)
                    Close().Wait();

                return connected;
            }).ConfigureAwait(false);
        }
        
        public virtual void SetProxy(string host, int port)
        {
            socket.Proxy = IPAddress.TryParse(host, out var address)
                ? new HttpConnectProxy(new IPEndPoint(address, port))
                : new HttpConnectProxy(new DnsEndPoint(host, port));
        }

        public void Dispose()
        {
            lock (socketLock)
            {
                if (socket != null)
                    log?.Write(LogVerbosity.Debug, $"Socket {Id} disposing websocket");

                socket?.Dispose();
                socket = null;

                errorHandlers.Clear();
                openHandlers.Clear();
                closeHandlers.Clear();
                messageHandlers.Clear();
            }
        }

        private int NextStreamId()
        {
            lock (streamIdLock)
            {
                lastStreamId++;
                return lastStreamId;
            }
        }
    }
}
