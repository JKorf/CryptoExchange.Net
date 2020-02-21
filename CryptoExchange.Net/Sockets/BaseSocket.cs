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
    /// <summary>
    /// Socket implementation
    /// </summary>
    public class BaseSocket: IWebsocket
    {
        internal static int lastStreamId;
        private static readonly object streamIdLock = new object();

        /// <summary>
        /// Socket
        /// </summary>
        protected WebSocket? socket;
        /// <summary>
        /// Log
        /// </summary>
        protected Log log;
        private readonly object socketLock = new object();

        /// <summary>
        /// Error handlers
        /// </summary>
        protected readonly List<Action<Exception>> errorHandlers = new List<Action<Exception>>();
        /// <summary>
        /// Open handlers
        /// </summary>
        protected readonly List<Action> openHandlers = new List<Action>();
        /// <summary>
        /// Close handlers
        /// </summary>
        protected readonly List<Action> closeHandlers = new List<Action>();
        /// <summary>
        /// Message handlers
        /// </summary>
        protected readonly List<Action<string>> messageHandlers = new List<Action<string>>();

        private readonly IDictionary<string, string> cookies;
        private readonly IDictionary<string, string> headers;
        private HttpConnectProxy? proxy;

        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; }
        /// <summary>
        /// If is reconnecting
        /// </summary>
        public bool Reconnecting { get; set; }
        /// <summary>
        /// Origin
        /// </summary>
        public string? Origin { get; set; }

        /// <summary>
        /// Url
        /// </summary>
        public string Url { get; }
        /// <summary>
        /// Is closed
        /// </summary>
        public bool IsClosed => socket?.State == null || socket.State == WebSocketState.Closed;
        /// <summary>
        /// Is open
        /// </summary>
        public bool IsOpen => socket?.State == WebSocketState.Open;
        /// <summary>
        /// Protocols
        /// </summary>
        public SslProtocols SSLProtocols { get; set; } = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls;
        /// <summary>
        /// Interpreter for bytes
        /// </summary>
        public Func<byte[], string>? DataInterpreterBytes { get; set; }
        /// <summary>
        /// Interpreter for strings
        /// </summary>
        public Func<string, string>? DataInterpreterString { get; set; }

        /// <summary>
        /// Last action time
        /// </summary>
        public DateTime LastActionTime { get; private set; }
        /// <summary>
        /// Timeout
        /// </summary>
        public TimeSpan Timeout { get; set; }
        private Task? timeoutTask;

        /// <summary>
        /// Socket state
        /// </summary>
        public WebSocketState SocketState => socket?.State ?? WebSocketState.None;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="log"></param>
        /// <param name="url"></param>
        public BaseSocket(Log log, string url):this(log, url, new Dictionary<string, string>(), new Dictionary<string, string>())
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="log"></param>
        /// <param name="url"></param>
        /// <param name="cookies"></param>
        /// <param name="headers"></param>
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
            if (DataInterpreterBytes == null)
                throw new Exception("Byte interpreter not set while receiving byte data");

            try
            {
                var message = DataInterpreterBytes(data);
                Handle(messageHandlers, message);
            }
            catch (Exception ex)
            {
                log.Write(LogVerbosity.Error, $"{Id} Something went wrong while processing a byte message from the socket: {ex}");
            }
        }

        private void HandleStringData(string data)
        {
            try
            {
                if (DataInterpreterString != null)
                    data = DataInterpreterString(data);
                Handle(messageHandlers, data);
            }
            catch (Exception ex)
            {
                log.Write(LogVerbosity.Error, $"{Id} Something went wrong while processing a string message from the socket: {ex}");
            }
        }

        /// <summary>
        /// On close
        /// </summary>
        public event Action OnClose
        {
            add => closeHandlers.Add(value);
            remove => closeHandlers.Remove(value);
        }
        /// <summary>
        /// On message
        /// </summary>
        public event Action<string> OnMessage
        {
            add => messageHandlers.Add(value);
            remove => messageHandlers.Remove(value);
        }
        /// <summary>
        /// On error
        /// </summary>
        public event Action<Exception> OnError
        {
            add => errorHandlers.Add(value);
            remove => errorHandlers.Remove(value);
        }
        /// <summary>
        /// On open
        /// </summary>
        public event Action OnOpen
        {
            add => openHandlers.Add(value);
            remove => openHandlers.Remove(value);
        }

        /// <summary>
        /// Handle
        /// </summary>
        /// <param name="handlers"></param>
        protected void Handle(List<Action> handlers)
        {
            LastActionTime = DateTime.UtcNow;
            foreach (var handle in new List<Action>(handlers))
                handle?.Invoke();
        }

        /// <summary>
        /// Handle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handlers"></param>
        /// <param name="data"></param>
        protected void Handle<T>(List<Action<T>> handlers, T data)
        {
            LastActionTime = DateTime.UtcNow;
            foreach (var handle in new List<Action<T>>(handlers))
                handle?.Invoke(data);
        }

        /// <summary>
        /// Checks if timed out
        /// </summary>
        /// <returns></returns>
        protected async Task CheckTimeout()
        {
            while (true)
            {
                lock (socketLock)
                {
                    if (socket == null || socket.State != WebSocketState.Open)
                        return;

                    if (DateTime.UtcNow - LastActionTime > Timeout)
                    {
                        log.Write(LogVerbosity.Warning, $"No data received for {Timeout}, reconnecting socket");
                        _ = Close().ConfigureAwait(false);
                        return;
                    }
                }

                await Task.Delay(500).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Close socket
        /// </summary>
        /// <returns></returns>
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
                    ManualResetEvent? evnt = new ManualResetEvent(false);
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

        /// <summary>
        /// Reset socket
        /// </summary>
        public virtual void Reset()
        {
            lock (socketLock)
            {
                log.Write(LogVerbosity.Debug, $"Socket {Id} resetting");
                socket?.Dispose();
                socket = null;
            }
        }

        /// <summary>
        /// Send data
        /// </summary>
        /// <param name="data"></param>
        public virtual void Send(string data)
        {
            socket?.Send(data);
        }

        /// <summary>
        /// Connect socket
        /// </summary>
        /// <returns></returns>
        public virtual Task<bool> Connect()
        {
            if (socket == null)
            {
                socket = new WebSocket(Url, cookies: cookies.ToList(), customHeaderItems: headers.ToList(), origin: Origin ?? "")
                {
                    EnableAutoSendPing = true,
                    AutoSendPingInterval = 10
                };

                if (proxy != null)
                    socket.Proxy = proxy;

                socket.Security.EnabledSslProtocols = SSLProtocols;
                socket.Opened += (o, s) => Handle(openHandlers);
                socket.Closed += (o, s) => Handle(closeHandlers);
                socket.Error += (o, s) => Handle(errorHandlers, s.Exception);
                socket.MessageReceived += (o, s) => HandleStringData(s.Message);
                socket.DataReceived += (o, s) => HandleByteData(s.Data);
            }

            return Task.Run(() =>
            {
                bool connected;
                lock (socketLock)
                {
                    log?.Write(LogVerbosity.Debug, $"Socket {Id} connecting");
                    var waitLock = new object();
                    ManualResetEvent? evnt = new ManualResetEvent(false);
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
                        if ((timeoutTask == null || timeoutTask.IsCompleted) && Timeout != default)
                            timeoutTask = Task.Run(CheckTimeout);
                    }
                    else
                    {
                        log?.Write(LogVerbosity.Debug, $"Socket {Id} connection failed, state: " + socket.State);
                    }
                }

                if (socket.State == WebSocketState.Connecting)
                    socket.Close();

                return connected;
            });
        }
        
        /// <summary>
        /// Set a proxy
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public virtual void SetProxy(string host, int port)
        {
            proxy = IPAddress.TryParse(host, out var address)
                ? new HttpConnectProxy(new IPEndPoint(address, port))
                : new HttpConnectProxy(new DnsEndPoint(host, port));
        }

        /// <summary>
        /// Dispose
        /// </summary>
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

        private static int NextStreamId()
        {
            lock (streamIdLock)
            {
                lastStreamId++;
                return lastStreamId;
            }
        }
    }
}
