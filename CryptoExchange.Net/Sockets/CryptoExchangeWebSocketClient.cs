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
        // TODO keep the same ID's for subscriptions/sockets when reconnecting
        enum ProcessState
        {
            Idle,
            Processing,
            WaitingForClose,
            Reconnecting
        }

        enum CloseState
        {
            Idle,
            Closing,
            Closed
        }

        internal static int lastStreamId;
        private static readonly object streamIdLock = new();

        private ClientWebSocket _socket;
        private readonly AsyncResetEvent _sendEvent;
        private readonly ConcurrentQueue<byte[]> _sendBuffer;
        private readonly IDictionary<string, string> cookies;
        private readonly IDictionary<string, string> headers;
        private CancellationTokenSource _ctsSource;
        private ApiProxy? _proxy;

        private readonly List<DateTime> _outgoingMessages;
        private DateTime _lastReceivedMessagesUpdate;
        private Task? _processTask;
        private Task? _closeTask;
        private bool _stopRequested;
        private bool _disposed;
        private ProcessState _processState;
        //private CloseState _closeState;
        private SemaphoreSlim _closeSem;

        /// <summary>
        /// Received messages, the size and the timstamp
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

        /// <inheritdoc />
        public int Id { get; }

        /// <inheritdoc />
        public string? Origin { get; set; }

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

        /// <inheritdoc />
        public Uri Uri { get; }

        /// <inheritdoc />
        public bool IsClosed => _socket.State == WebSocketState.Closed;

        /// <inheritdoc />
        public bool IsOpen => _socket.State == WebSocketState.Open && !_ctsSource.IsCancellationRequested;

        /// <summary>
        /// Ssl protocols supported. NOT USED BY THIS IMPLEMENTATION
        /// </summary>
        public SslProtocols SSLProtocols { get; set; }

        private Encoding _encoding = Encoding.UTF8;
        /// <inheritdoc />
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

        /// <inheritdoc />
        public TimeSpan Timeout { get; set; }

        /// <inheritdoc />
        public TimeSpan KeepAliveInterval { get; set; }

        /// <inheritdoc />
        public double IncomingKbps
        {
            get
            {
                lock (_receivedMessagesLock)
                {
                    UpdateReceivedMessages();

                    if (!_receivedMessages.Any())
                        return 0;

                    return Math.Round(_receivedMessages.Sum(v => v.Bytes) / 1000d / 3d);
                }
            }
        }

        /// <inheritdoc />
        public event Action? OnClose;
        /// <inheritdoc />
        public event Action<string>? OnMessage;
        /// <inheritdoc />
        public event Action<Exception>? OnError;
        /// <inheritdoc />
        public event Action? OnOpen;
        /// <inheritdoc />
        public event Action? OnReconnecting;
        /// <inheritdoc />
        public event Action? OnReconnected;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="log">The log object to use</param>
        /// <param name="uri">The uri the socket should connect to</param>
        public CryptoExchangeWebSocketClient(Log log, Uri uri) : this(log, uri, new Dictionary<string, string>(), new Dictionary<string, string>())
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="log">The log object to use</param>
        /// <param name="uri">The uri the socket should connect to</param>
        /// <param name="cookies">Cookies to sent in the socket connection request</param>
        /// <param name="headers">Headers to sent in the socket connection request</param>
        public CryptoExchangeWebSocketClient(Log log, Uri uri, IDictionary<string, string> cookies, IDictionary<string, string> headers)
        {
            Id = NextStreamId();
            this.log = log;
            Uri = uri;
            this.cookies = cookies;
            this.headers = headers;

            _outgoingMessages = new List<DateTime>();
            _receivedMessages = new List<ReceiveItem>();
            _sendEvent = new AsyncResetEvent();
            _sendBuffer = new ConcurrentQueue<byte[]>();
            _ctsSource = new CancellationTokenSource();
            _receivedMessagesLock = new object();

            _closeSem = new SemaphoreSlim(1, 1);
            _socket = CreateSocket();
        }

        /// <inheritdoc />
        public virtual void SetProxy(ApiProxy proxy)
        {
            _proxy = proxy;

            if (!Uri.TryCreate($"{proxy.Host}:{proxy.Port}", UriKind.Absolute, out var uri))
                throw new ArgumentException("Proxy settings invalid, {proxy.Host}:{proxy.Port} not a valid URI", nameof(proxy));

            _socket.Options.Proxy = uri?.Scheme == null
                ? _socket.Options.Proxy = new WebProxy(proxy.Host, proxy.Port)
                : _socket.Options.Proxy = new WebProxy
                {
                    Address = uri
                };

            if (proxy.Login != null)
                _socket.Options.Proxy.Credentials = new NetworkCredential(proxy.Login, proxy.Password);
        }

        /// <inheritdoc />
        public virtual async Task<bool> ConnectAsync()
        {
            if (!await ConnectInternalAsync().ConfigureAwait(false))
                return false;
            
            OnOpen?.Invoke();
            _processTask = ProcessAsync();
            return true;            
        }

        private async Task<bool> ConnectInternalAsync()
        {
            log.Write(LogLevel.Debug, $"Socket {Id} connecting");
            try
            {
                using CancellationTokenSource tcs = new(TimeSpan.FromSeconds(10));
                await _socket.ConnectAsync(Uri, tcs.Token).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                log.Write(LogLevel.Debug, $"Socket {Id} connection failed: " + e.ToLogString());
                return false;
            }

            log.Write(LogLevel.Debug, $"Socket {Id} connected to {Uri}");
            return true;
        }

        /// <inheritdoc />
        private async Task ProcessAsync()
        {
            while (!_stopRequested)
            {
                log.Write(LogLevel.Trace, $"Socket {Id} ProcessAsync started");
                _processState = ProcessState.Processing;
                var sendTask = SendLoopAsync();
                var receiveTask = ReceiveLoopAsync();
                var timeoutTask = Timeout != default ? CheckTimeoutAsync() : Task.CompletedTask;
                await Task.WhenAll(sendTask, receiveTask, timeoutTask).ConfigureAwait(false);
                log.Write(LogLevel.Trace, $"Socket {Id} ProcessAsync finished");

                _processState = ProcessState.WaitingForClose;
                while (_closeTask == null)
                    await Task.Delay(50).ConfigureAwait(false);

                await _closeTask.ConfigureAwait(false);
                _closeTask = null;
                //_closeState = CloseState.Idle;

                if (!_stopRequested)
                {
                    _processState = ProcessState.Reconnecting;
                    OnReconnecting?.Invoke();
                }

                while (!_stopRequested)
                {
                    log.Write(LogLevel.Trace, $"Socket {Id} attempting to reconnect");
                    _socket = CreateSocket();
                    _ctsSource.Dispose();
                    _ctsSource = new CancellationTokenSource();
                    while (_sendBuffer.TryDequeue(out _)) { } // Clear send buffer

                    var connected = await ConnectInternalAsync().ConfigureAwait(false);
                    if (!connected)
                    {
                        await Task.Delay(5000).ConfigureAwait(false);
                        continue;
                    }

                    OnReconnected?.Invoke();
                    break;
                }
            }

            _processState = ProcessState.Idle;
        }

        /// <inheritdoc />
        public virtual void Send(string data)
        {
            if (_ctsSource.IsCancellationRequested)
                throw new InvalidOperationException($"Socket {Id} Can't send data when socket is not connected");

            var bytes = _encoding.GetBytes(data);
            log.Write(LogLevel.Trace, $"Socket {Id} Adding {bytes.Length} to sent buffer");
            _sendBuffer.Enqueue(bytes);
            _sendEvent.Set();
        }

        /// <inheritdoc />
        public virtual async Task ReconnectAsync()
        {
            if (_processState != ProcessState.Processing)
                return;

            log.Write(LogLevel.Debug, $"Socket {Id} reconnecting");
            _closeTask = CloseInternalAsync();
            await _closeTask.ConfigureAwait(false);
        }

        /// <inheritdoc />
        public virtual async Task CloseAsync()
        {
            await _closeSem.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_closeTask != null && !_closeTask.IsCompleted)
                {
                    log.Write(LogLevel.Debug, $"Socket {Id} CloseAsync() waiting for existing close task");
                    await _closeTask.ConfigureAwait(false);
                    return;
                }

                if (!IsOpen)
                {
                    log.Write(LogLevel.Debug, $"Socket {Id} CloseAsync() socket not open");
                    return;
                }

                log.Write(LogLevel.Debug, $"Socket {Id} closing");
                _stopRequested = true;

                _closeTask = CloseInternalAsync();
            }
            finally
            {
                _closeSem.Release();
            }

            await _closeTask.ConfigureAwait(false);
            await _processTask!.ConfigureAwait(false);
            OnClose?.Invoke();
            log.Write(LogLevel.Debug, $"Socket {Id} closed");
        }

        /// <summary>
        /// Internal close method
        /// </summary>
        /// <returns></returns>
        private async Task CloseInternalAsync()
        {
            if (_disposed)
                return;

            //_closeState = CloseState.Closing;
            _ctsSource.Cancel();
            _sendEvent.Set();

            if (_socket.State == WebSocketState.Open)
            {
                try
                {
                    await _socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", default).ConfigureAwait(false);
                }
                catch(Exception)
                { } // Can sometimes throw an exception when socket is in aborted state due to timing
            }
            else if(_socket.State == WebSocketState.CloseReceived)
            {
                try
                {
                    await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", default).ConfigureAwait(false);
                }
                catch (Exception)
                { } // Can sometimes throw an exception when socket is in aborted state due to timing
            }
        }

        /// <summary>
        /// Dispose the socket
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            log.Write(LogLevel.Debug, $"Socket {Id} disposing");
            _disposed = true;
            _socket.Dispose();
            _ctsSource.Dispose();
            log.Write(LogLevel.Trace, $"Socket {Id} disposed");
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
            socket.Options.KeepAliveInterval = KeepAliveInterval;
            socket.Options.SetBuffer(65536, 65536); // Setting it to anything bigger than 65536 throws an exception in .net framework
            if (_proxy != null)
                SetProxy(_proxy);
            return socket;
        }

        /// <summary>
        /// Loop for sending data
        /// </summary>
        /// <returns></returns>
        private async Task SendLoopAsync()
        {
            try
            {
                while (true)
                {
                    if (_ctsSource.IsCancellationRequested)
                        break;

                    await _sendEvent.WaitAsync().ConfigureAwait(false);

                    if (_ctsSource.IsCancellationRequested)
                        break;

                    while (_sendBuffer.TryDequeue(out var data))
                    {
                        if (RatelimitPerSecond != null)
                        {
                            // Wait for rate limit
                            DateTime? start = null;
                            while (MessagesSentLastSecond() >= RatelimitPerSecond)
                            {
                                start ??= DateTime.UtcNow;
                                await Task.Delay(50).ConfigureAwait(false);
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
                            // canceled
                            break;
                        }
                        catch (Exception ioe)
                        {
                            // Connection closed unexpectedly, .NET framework
                            OnError?.Invoke(ioe);
                            _closeTask = CloseInternalAsync();
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
                OnError?.Invoke(e);
                throw;
            }
            finally
            {
                log.Write(LogLevel.Trace, $"Socket {Id} Send loop finished");
            }
        }

        /// <summary>
        /// Loop for receiving and reassembling data
        /// </summary>
        /// <returns></returns>
        private async Task ReceiveLoopAsync()
        {
            var buffer = new ArraySegment<byte>(new byte[65536]);
            var received = 0;
            try
            {
                while (true)
                {
                    if (_ctsSource.IsCancellationRequested)
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
                            // canceled
                            break;
                        }
                        catch (Exception wse)
                        {
                            // Connection closed unexpectedly
                            OnError?.Invoke(wse);
                            _closeTask = CloseInternalAsync();
                            break;
                        }

                        if (receiveResult.MessageType == WebSocketMessageType.Close)
                        {
                            // Connection closed unexpectedly        
                            log.Write(LogLevel.Debug, $"Socket {Id} received `Close` message");
                            _closeTask = CloseInternalAsync();
                            break;
                        }

                        if (!receiveResult.EndOfMessage)
                        {
                            // We received data, but it is not complete, write it to a memory stream for reassembling
                            multiPartMessage = true;
                            memoryStream ??= new MemoryStream();
                            log.Write(LogLevel.Trace, $"Socket {Id} received {receiveResult.Count} bytes in partial message");
                            await memoryStream.WriteAsync(buffer.Array, buffer.Offset, receiveResult.Count).ConfigureAwait(false);
                        }
                        else
                        {
                            if (!multiPartMessage)
                            {
                                // Received a complete message and it's not multi part
                                log.Write(LogLevel.Trace, $"Socket {Id} received {receiveResult.Count} bytes in single message");
                                HandleMessage(buffer.Array!, buffer.Offset, receiveResult.Count, receiveResult.MessageType);
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

                    if (receiveResult == null || _ctsSource.IsCancellationRequested)
                    {
                        // Error during receiving or cancellation requested, stop.
                        break;
                    }

                    if (multiPartMessage)
                    {
                        // When the connection gets interupted we might not have received a full message
                        if (receiveResult?.EndOfMessage == true)
                        {
                            // Reassemble complete message from memory stream
                            log.Write(LogLevel.Trace, $"Socket {Id} reassembled message of {memoryStream!.Length} bytes");
                            HandleMessage(memoryStream!.ToArray(), 0, (int)memoryStream.Length, receiveResult.MessageType);
                            memoryStream.Dispose();
                        }
                        else
                            log.Write(LogLevel.Trace, $"Socket {Id} discarding incomplete message of {memoryStream!.Length} bytes");
                    }
                }
            }
            catch(Exception e)
            {
                // Because this is running in a separate task and not awaited until the socket gets closed
                // any exception here will crash the receive processing, but do so silently unless the socket gets stopped.
                // Make sure we at least let the owner know there was an error
                OnError?.Invoke(e);
                throw;
            }
            finally
            {
                log.Write(LogLevel.Trace, $"Socket {Id} Receive loop finished");
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
                OnMessage?.Invoke(strData);
            }
            catch(Exception e)
            {
                log.Write(LogLevel.Error, $"Socket {Id} unhandled exception during message processing: " + e.ToLogString());
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
                    if (_ctsSource.IsCancellationRequested)
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
                        // canceled
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                // Because this is running in a separate task and not awaited until the socket gets closed
                // any exception here will stop the timeout checking, but do so silently unless the socket get's stopped.
                // Make sure we at least let the owner know there was an error
                OnError?.Invoke(e);
                throw;
            }
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
