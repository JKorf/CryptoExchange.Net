using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Sockets
{
    /// <summary>
    /// A wrapper around the ClientWebSocket
    /// </summary>
    public class CryptoExchangeWebSocketClient : IWebsocket
    {
        internal static int lastStreamId;
        private static readonly object streamIdLock = new object();

        private ClientWebSocket _socket;
        private Task? _sendTask;
        private Task? _receiveTask;
        private Task? _timeoutTask;
        private readonly AsyncResetEvent _sendEvent;
        private readonly ConcurrentQueue<byte[]> _sendBuffer;
        private readonly IDictionary<string, string> cookies;
        private readonly IDictionary<string, string> headers;
        private CancellationTokenSource _ctsSource;
        private bool _closing;
        private bool _startedSent;
        private bool _startedReceive;

        private readonly List<DateTime> _outgoingMessages;
        private DateTime _lastReceivedMessagesUpdate;

        /// <summary>
        /// Received messages time -> size
        /// </summary>
        protected readonly List<ReceiveItem> _receivedMessages;
        /// <summary>
        /// Received messages lock
        /// </summary>
        protected readonly object _receivedMessagesLock;

        /// <summary>
        /// Log
        /// </summary>
        protected Log log;

        /// <summary>
        /// Handlers for when an error happens on the socket
        /// </summary>
        protected readonly List<Action<Exception>> errorHandlers = new List<Action<Exception>>();
        /// <summary>
        /// Handlers for when the socket connection is opened
        /// </summary>
        protected readonly List<Action> openHandlers = new List<Action>();
        /// <summary>
        /// Handlers for when the connection is closed
        /// </summary>
        protected readonly List<Action> closeHandlers = new List<Action>();
        /// <summary>
        /// Handlers for when a message is received
        /// </summary>
        protected readonly List<Action<string>> messageHandlers = new List<Action<string>>();

        /// <summary>
        /// The id of this socket
        /// </summary>
        public int Id { get; }

        /// <inheritdoc />
        public string? Origin { get; set; }
        /// <summary>
        /// Whether this socket is currently reconnecting
        /// </summary>
        public bool Reconnecting { get; set; }
        /// <summary>
        /// The timestamp this socket has been active for the last time
        /// </summary>
        public DateTime LastActionTime { get; private set; }

        /// <summary>
        /// Delegate used for processing byte data received from socket connections before it is processed by handlers
        /// </summary>
        public Func<byte[], string>? DataInterpreterBytes { get; set; }
        /// <summary>
        /// Delegate used for processing string data received from socket connections before it is processed by handlers
        /// </summary>
        public Func<string, string>? DataInterpreterString { get; set; }
        /// <summary>
        /// Url this socket connects to
        /// </summary>
        public string Url { get; }
        /// <summary>
        /// If the connection is closed
        /// </summary>
        public bool IsClosed => _socket.State == WebSocketState.Closed;

        /// <summary>
        /// If the connection is open
        /// </summary>
        public bool IsOpen => _socket.State == WebSocketState.Open && !_closing;

        /// <summary>
        /// Ssl protocols supported. NOT USED BY THIS IMPLEMENTATION
        /// </summary>
        public SslProtocols SSLProtocols { get; set; }

        private Encoding _encoding = Encoding.UTF8;
        /// <summary>
        /// Encoding used for decoding the received bytes into a string
        /// </summary>
        public Encoding? Encoding
        {
            get => _encoding;
            set
            {
                if(value != null)
                    _encoding = value;
            }
        }
        /// <summary>
        /// The max amount of outgoing messages per second
        /// </summary>
        public int? RatelimitPerSecond { get; set; }

        /// <summary>
        /// The timespan no data is received on the socket. If no data is received within this time an error is generated
        /// </summary>
        public TimeSpan Timeout { get; set; }

        /// <summary>
        /// The current kilobytes per second of data being received, averaged over the last 3 seconds
        /// </summary>
        public double IncomingKbps
        {
            get
            {
                lock (_receivedMessagesLock)
                {
                    UpdateReceivedMessages();

                    if (!_receivedMessages.Any())
                        return 0;

                    return Math.Round(_receivedMessages.Sum(v => v.Bytes) / 1000 / 3d);
                }
            }
        }

        /// <summary>
        /// Socket closed event
        /// </summary>
        public event Action OnClose
        {
            add => closeHandlers.Add(value);
            remove => closeHandlers.Remove(value);
        }
        /// <summary>
        /// Socket message received event
        /// </summary>
        public event Action<string> OnMessage
        {
            add => messageHandlers.Add(value);
            remove => messageHandlers.Remove(value);
        }
        /// <summary>
        /// Socket error event
        /// </summary>
        public event Action<Exception> OnError
        {
            add => errorHandlers.Add(value);
            remove => errorHandlers.Remove(value);
        }
        /// <summary>
        /// Socket opened event
        /// </summary>
        public event Action OnOpen
        {
            add => openHandlers.Add(value);
            remove => openHandlers.Remove(value);
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="log">The log object to use</param>
        /// <param name="url">The url the socket should connect to</param>
        public CryptoExchangeWebSocketClient(Log log, string url) : this(log, url, new Dictionary<string, string>(), new Dictionary<string, string>())
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="log">The log object to use</param>
        /// <param name="url">The url the socket should connect to</param>
        /// <param name="cookies">Cookies to sent in the socket connection request</param>
        /// <param name="headers">Headers to sent in the socket connection request</param>
        public CryptoExchangeWebSocketClient(Log log, string url, IDictionary<string, string> cookies, IDictionary<string, string> headers)
        {
            Id = NextStreamId();
            this.log = log;
            Url = url;
            this.cookies = cookies;
            this.headers = headers;

            _outgoingMessages = new List<DateTime>();
            _receivedMessages = new List<ReceiveItem>();
            _sendEvent = new AsyncResetEvent();
            _sendBuffer = new ConcurrentQueue<byte[]>();
            _ctsSource = new CancellationTokenSource();
            _receivedMessagesLock = new object();

            _socket = CreateSocket();
        }

        /// <summary>
        /// Set a proxy to use. Should be set before connecting
        /// </summary>
        /// <param name="proxy"></param>
        public virtual void SetProxy(ApiProxy proxy)
        {
            _socket.Options.Proxy = new WebProxy(proxy.Host, proxy.Port);
            if (proxy.Login != null)
                _socket.Options.Proxy.Credentials = new NetworkCredential(proxy.Login, proxy.Password);
        }

        /// <summary>
        /// Connect the websocket
        /// </summary>
        /// <returns>True if successfull</returns>
        public virtual async Task<bool> ConnectAsync()
        {
            log.Write(LogLevel.Debug, $"Socket {Id} connecting");
            try
            {
                using CancellationTokenSource tcs = new CancellationTokenSource(TimeSpan.FromSeconds(10));                
                await _socket.ConnectAsync(new Uri(Url), default).ConfigureAwait(false);
                
                Handle(openHandlers);
            }
            catch (Exception e)
            {
                log.Write(LogLevel.Debug, $"Socket {Id} connection failed: " + e.ToLogString());
                return false;
            }

            log.Write(LogLevel.Trace, $"Socket {Id} connection succeeded, starting communication");
            _sendTask = Task.Factory.StartNew(SendLoopAsync, TaskCreationOptions.LongRunning);
            _receiveTask = Task.Factory.StartNew(ReceiveLoopAsync, TaskCreationOptions.LongRunning);
            if (Timeout != default)
                _timeoutTask = Task.Run(CheckTimeoutAsync);

            var sw = Stopwatch.StartNew();
            while (!_startedSent || !_startedReceive)
                // Wait for the tasks to have actually started
                await Task.Delay(10).ConfigureAwait(false);

            log.Write(LogLevel.Debug, $"Socket {Id} connected");
            return true;
        }

        /// <summary>
        /// Send data over the websocket
        /// </summary>
        /// <param name="data">Data to send</param>
        public virtual void Send(string data)
        {
            if (_closing)
                throw new InvalidOperationException($"Socket {Id} Can't send data when socket is not connected");

            var bytes = _encoding.GetBytes(data);
            log.Write(LogLevel.Trace, $"Socket {Id} Adding {bytes.Length} to sent buffer");
            _sendBuffer.Enqueue(bytes);
            _sendEvent.Set();
        }

        /// <summary>
        /// Close the websocket
        /// </summary>
        /// <returns></returns>
        public virtual async Task CloseAsync()
        {
            log.Write(LogLevel.Debug, $"Socket {Id} closing");
            await CloseInternalAsync(true, true).ConfigureAwait(false);
        }
        
        /// <summary>
        /// Internal close method, will wait for each task to complete to gracefully close
        /// </summary>
        /// <param name="waitSend"></param>
        /// <param name="waitReceive"></param>
        /// <returns></returns>
        private async Task CloseInternalAsync(bool waitSend, bool waitReceive)
        {
            if (_closing)
                return;

            _startedSent = false;
            _startedReceive = false;
            _closing = true;
            var tasksToAwait = new List<Task>();
            if (_socket.State == WebSocketState.Open)
                tasksToAwait.Add(_socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", default));

            _ctsSource.Cancel();
            _sendEvent.Set();
            if (waitSend)
                tasksToAwait.Add(_sendTask!);
            if (waitReceive)
                tasksToAwait.Add(_receiveTask!);
            if (_timeoutTask != null)
                tasksToAwait.Add(_timeoutTask);

            log.Write(LogLevel.Trace, $"Socket {Id} waiting for communication loops to finish");
            await Task.WhenAll(tasksToAwait).ConfigureAwait(false);
            log.Write(LogLevel.Debug, $"Socket {Id} closed");
            Handle(closeHandlers);
        }

        /// <summary>
        /// Dispose the socket
        /// </summary>
        public void Dispose()
        {
            log.Write(LogLevel.Debug, $"Socket {Id} disposing");
            _socket.Dispose();
            _ctsSource.Dispose();

            errorHandlers.Clear();
            openHandlers.Clear();
            closeHandlers.Clear();
            messageHandlers.Clear();
            log.Write(LogLevel.Trace, $"Socket {Id} disposed");
        }

        /// <summary>
        /// Reset the socket so a new connection can be attempted after it has been connected before
        /// </summary>
        public void Reset()
        {
            log.Write(LogLevel.Debug, $"Socket {Id} resetting");
            _ctsSource = new CancellationTokenSource();
            _closing = false;
            _socket = CreateSocket();
        }
        
        /// <summary>
        /// Create the socket object
        /// </summary>
        private ClientWebSocket CreateSocket()
        {
            var cookieContainer = new CookieContainer();
            foreach (var cookie in cookies)
                cookieContainer.Add(new Cookie(cookie.Key, cookie.Value));

            var socket = new ClientWebSocket();
            socket.Options.Cookies = cookieContainer;
            foreach (var header in headers)
                socket.Options.SetRequestHeader(header.Key, header.Value);
            socket.Options.KeepAliveInterval = TimeSpan.FromSeconds(10);
            socket.Options.SetBuffer(65536, 65536); // Setting it to anything bigger than 65536 throws an exception in .net framework
            return socket;
        }

        /// <summary>
        /// Loop for sending data
        /// </summary>
        /// <returns></returns>
        private async Task SendLoopAsync()
        {
            _startedSent = true;
            try
            {
                while (true)
                {
                    if (_closing)
                        break;

                    await _sendEvent.WaitAsync().ConfigureAwait(false);

                    if (_closing)
                        break;

                    while (_sendBuffer.TryDequeue(out var data))
                    {
                        if (RatelimitPerSecond != null)
                        {
                            // Wait for rate limit
                            DateTime? start = null;
                            while (MessagesSentLastSecond() >= RatelimitPerSecond)
                            {
                                if (start == null)
                                    start = DateTime.UtcNow;
                                await Task.Delay(10).ConfigureAwait(false);
                            }

                            if (start != null)
                                log.Write(LogLevel.Trace, $"Socket {Id} sent delayed {Math.Round((DateTime.UtcNow - start.Value).TotalMilliseconds)}ms because of rate limit");
                        }

                        try
                        {
                            await _socket.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Text, true, _ctsSource.Token).ConfigureAwait(false);
                            _outgoingMessages.Add(DateTime.UtcNow);
                            log.Write(LogLevel.Trace, $"Socket {Id} sent {data.Length} bytes");
                        }
                        catch (OperationCanceledException)
                        {
                            // cancelled
                            break;
                        }
                        catch (IOException ioe)
                        {
                            // Connection closed unexpectedly, .NET framework                      
                            Handle(errorHandlers, ioe);
                            await CloseInternalAsync(false, true).ConfigureAwait(false);
                            break;
                        }
                        catch (WebSocketException wse)
                        {
                            // Connection closed unexpectedly                        
                            Handle(errorHandlers, wse);
                            await CloseInternalAsync(false, true).ConfigureAwait(false);
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // Because this is running in a separate task and not awaited until the socket gets closed
                // any exception here will crash the send processing, but do so silently unless the socket get's stopped.
                // Make sure we at least let the owner know there was an error
                Handle(errorHandlers, e);
                throw;
            }
        }

        /// <summary>
        /// Loop for receiving and reassembling data
        /// </summary>
        /// <returns></returns>
        private async Task ReceiveLoopAsync()
        {
            _startedReceive = true;

            var buffer = new ArraySegment<byte>(new byte[65536]);
            var received = 0;
            try
            {
                while (true)
                {
                    if (_closing)
                        break;

                    MemoryStream? memoryStream = null;
                    WebSocketReceiveResult? receiveResult = null;
                    bool multiPartMessage = false;
                    while (true)
                    {
                        try
                        {
                            receiveResult = await _socket.ReceiveAsync(buffer, _ctsSource.Token).ConfigureAwait(false);
                            received += receiveResult.Count;
                            lock (_receivedMessagesLock)
                                _receivedMessages.Add(new ReceiveItem(DateTime.UtcNow, receiveResult.Count));
                        }
                        catch (OperationCanceledException)
                        {
                            // cancelled
                            break;
                        }
                        catch (WebSocketException wse)
                        {
                            // Connection closed unexpectedly        
                            Handle(errorHandlers, wse);
                            await CloseInternalAsync(true, false).ConfigureAwait(false);
                            break;
                        }
                        catch (IOException ioe)
                        {
                            // Connection closed unexpectedly, .NET framework
                            Handle(errorHandlers, ioe);
                            await CloseInternalAsync(true, false).ConfigureAwait(false);
                            break;
                        }

                        if (receiveResult.MessageType == WebSocketMessageType.Close)
                        {
                            // Connection closed unexpectedly        
                            log.Write(LogLevel.Debug, $"Socket {Id} received `Close` message");
                            await CloseInternalAsync(true, false).ConfigureAwait(false);
                            break;
                        }

                        if (!receiveResult.EndOfMessage)
                        {
                            // We received data, but it is not complete, write it to a memory stream for reassembling
                            multiPartMessage = true;
                            if (memoryStream == null)
                                memoryStream = new MemoryStream();
                            log.Write(LogLevel.Trace, $"Socket {Id} received {receiveResult.Count} bytes in partial message");
                            await memoryStream.WriteAsync(buffer.Array, buffer.Offset, receiveResult.Count).ConfigureAwait(false);
                        }
                        else
                        {
                            if (!multiPartMessage)
                            {
                                // Received a complete message and it's not multi part
                                log.Write(LogLevel.Trace, $"Socket {Id} received {receiveResult.Count} bytes in single message");
                                HandleMessage(buffer.Array, buffer.Offset, receiveResult.Count, receiveResult.MessageType);
                            }
                            else
                            {
                                // Received the end of a multipart message, write to memory stream for reassembling
                                log.Write(LogLevel.Trace, $"Socket {Id} received {receiveResult.Count} bytes in partial message");
                                await memoryStream!.WriteAsync(buffer.Array, buffer.Offset, receiveResult.Count).ConfigureAwait(false);
                            }
                            break;
                        }
                    }

                    lock (_receivedMessagesLock)
                        UpdateReceivedMessages();

                    if (receiveResult?.MessageType == WebSocketMessageType.Close)
                    {
                        // Received close message
                        break;
                    }

                    if (receiveResult == null || _closing)
                    {
                        // Error during receiving or cancellation requested, stop.
                        break;
                    }

                    if (multiPartMessage)
                    {
                        // Reassemble complete message from memory stream
                        log.Write(LogLevel.Trace, $"Socket {Id} reassembled message of {memoryStream!.Length} bytes");
                        HandleMessage(memoryStream!.ToArray(), 0, (int)memoryStream.Length, receiveResult.MessageType);
                        memoryStream.Dispose();
                    }
                }
            }
            catch(Exception e)
            {
                // Because this is running in a separate task and not awaited until the socket gets closed
                // any exception here will crash the receive processing, but do so silently unless the socket get's stopped.
                // Make sure we at least let the owner know there was an error
                Handle(errorHandlers, e);
                throw;
            }
        }

        /// <summary>
        /// Handles the message
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="messageType"></param>
        private void HandleMessage(byte[] data, int offset, int count, WebSocketMessageType messageType)
        {
            string strData;
            if (messageType == WebSocketMessageType.Binary)
            {
                if (DataInterpreterBytes == null)
                    throw new Exception("Byte interpreter not set while receiving byte data");

                try
                {
                    var relevantData = new byte[count];
                    Array.Copy(data, offset, relevantData, 0, count);
                    strData = DataInterpreterBytes(relevantData);
                }
                catch(Exception e)
                {
                    log.Write(LogLevel.Error, $"Socket {Id} unhandled exception during byte data interpretation: " + e.ToLogString());
                    return;
                }
            }
            else
                strData = _encoding.GetString(data, offset, count);

            if (DataInterpreterString != null)
            {
                try
                {
                    strData = DataInterpreterString(strData);
                }
                catch(Exception e)
                {
                    log.Write(LogLevel.Error, $"Socket {Id} unhandled exception during string data interpretation: " + e.ToLogString());
                    return;
                }
            }

            try
            {
                Handle(messageHandlers, strData);
            }
            catch(Exception e)
            {
                log.Write(LogLevel.Error, $"Socket {Id} unhandled exception during message processing: " + e.ToLogString());
                return;
            }
        }

        /// <summary>
        /// Checks if there is no data received for a period longer than the specified timeout
        /// </summary>
        /// <returns></returns>
        protected async Task CheckTimeoutAsync()
        {
            log.Write(LogLevel.Debug, $"Socket {Id} Starting task checking for no data received for {Timeout}");
            try 
            { 
                while (true)
                {
                    if (_closing)
                        return;

                    if (DateTime.UtcNow - LastActionTime > Timeout)
                    {
                        log.Write(LogLevel.Warning, $"Socket {Id} No data received for {Timeout}, reconnecting socket");
                        _ = CloseAsync().ConfigureAwait(false);
                        return;
                    }
                    try
                    {
                        await Task.Delay(500, _ctsSource.Token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        // cancelled
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                // Because this is running in a separate task and not awaited until the socket gets closed
                // any exception here will stop the timeout checking, but do so silently unless the socket get's stopped.
                // Make sure we at least let the owner know there was an error
                Handle(errorHandlers, e);
                throw;
            }
        }

        /// <summary>
        /// Helper to invoke handlers
        /// </summary>
        /// <param name="handlers"></param>
        protected void Handle(List<Action> handlers)
        {
            LastActionTime = DateTime.UtcNow;
            foreach (var handle in new List<Action>(handlers))
                handle?.Invoke();
        }

        /// <summary>
        /// Helper to invoke handlers
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
        /// Get the next identifier
        /// </summary>
        /// <returns></returns>
        private static int NextStreamId()
        {
            lock (streamIdLock)
            {
                lastStreamId++;
                return lastStreamId;
            }
        }

        private int MessagesSentLastSecond()
        {
            var testTime = DateTime.UtcNow;
            _outgoingMessages.RemoveAll(r => testTime - r > TimeSpan.FromSeconds(1));
            return _outgoingMessages.Count;            
        }

        /// <summary>
        /// Update the received messages list, removing messages received longer than 3s ago
        /// </summary>
        protected void UpdateReceivedMessages()
        {
            var checkTime = DateTime.UtcNow;
            if (checkTime - _lastReceivedMessagesUpdate > TimeSpan.FromSeconds(1))
            {
                foreach (var msg in _receivedMessages.ToList()) // To list here because we're removing from the list
                    if (checkTime - msg.Timestamp > TimeSpan.FromSeconds(3))
                        _receivedMessages.Remove(msg);

                _lastReceivedMessagesUpdate = checkTime;
            }
        }
    }

    /// <summary>
    /// Received message info
    /// </summary>
    public struct ReceiveItem
    {
        /// <summary>
        /// Timestamp of the received data
        /// </summary>
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Number of bytes received
        /// </summary>
        public int Bytes { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="bytes"></param>
        public ReceiveItem(DateTime timestamp, int bytes)
        {
            Timestamp = timestamp;
            Bytes = bytes;
        }
    }
}
