using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Logging.Extensions;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.RateLimiting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Sockets
{
    /// <summary>
    /// A wrapper around the ClientWebSocket
    /// </summary>
    public class CryptoExchangeWebSocketClient : IWebsocket
    {
        enum ProcessState
        {
            Idle,
            Processing,
            WaitingForClose,
            Reconnecting
        }

        internal static int _lastStreamId;
        private static readonly object _streamIdLock = new();

        private readonly AsyncResetEvent _sendEvent;
        private readonly ConcurrentQueue<SendItem> _sendBuffer;
        private readonly SemaphoreSlim _closeSem;

        private ClientWebSocket _socket;
        private CancellationTokenSource _ctsSource;
        private DateTime _lastReceivedMessagesUpdate;
        private Task? _processTask;
        private Task? _closeTask;
        private bool _stopRequested;
        private bool _disposed;
        private ProcessState _processState;
        private DateTime _lastReconnectTime;
        private string _baseAddress;
        private int _reconnectAttempt;

        private const int _receiveBufferSize = 1048576;
        private const int _sendBufferSize = 4096;

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
        protected ILogger _logger;

        /// <inheritdoc />
        public int Id { get; }

        /// <inheritdoc />
        public WebSocketParameters Parameters { get; }

        /// <summary>
        /// The timestamp this socket has been active for the last time
        /// </summary>
        public DateTime LastActionTime { get; private set; }
        
        /// <inheritdoc />
        public Uri Uri => Parameters.Uri;

        /// <inheritdoc />
        public virtual bool IsClosed => _socket.State == WebSocketState.Closed;

        /// <inheritdoc />
        public virtual bool IsOpen => _socket.State == WebSocketState.Open && !_ctsSource.IsCancellationRequested;

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
        public event Func<Task>? OnClose;

        /// <inheritdoc />
        public event Func<WebSocketMessageType, ReadOnlyMemory<byte>, Task>? OnStreamMessage;

        /// <inheritdoc />
        public event Func<int, Task>? OnRequestSent;

        /// <inheritdoc />
        public event Func<int, Task>? OnRequestRateLimited;

        /// <inheritdoc />
        public event Func<Task>? OnConnectRateLimited;

        /// <inheritdoc />
        public event Func<Exception, Task>? OnError;

        /// <inheritdoc />
        public event Func<Task>? OnOpen;

        /// <inheritdoc />
        public event Func<Task>? OnReconnecting;

        /// <inheritdoc />
        public event Func<Task>? OnReconnected;
        /// <inheritdoc />
        public Func<Task<Uri?>>? GetReconnectionUrl { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger">The log object to use</param>
        /// <param name="websocketParameters">The parameters for this socket</param>
        public CryptoExchangeWebSocketClient(ILogger logger, WebSocketParameters websocketParameters)
        {
            Id = NextStreamId();
            _logger = logger;

            Parameters = websocketParameters;
            _receivedMessages = new List<ReceiveItem>();
            _sendEvent = new AsyncResetEvent();
            _sendBuffer = new ConcurrentQueue<SendItem>();
            _ctsSource = new CancellationTokenSource();
            _receivedMessagesLock = new object();

            _closeSem = new SemaphoreSlim(1, 1);
            _socket = CreateSocket();
            _baseAddress = $"{Uri.Scheme}://{Uri.Host}";
        }

        /// <inheritdoc />
        public virtual async Task<CallResult> ConnectAsync()
        {
            var connectResult = await ConnectInternalAsync().ConfigureAwait(false);
            if (!connectResult)
                return connectResult;
            
            await (OnOpen?.Invoke() ?? Task.CompletedTask).ConfigureAwait(false);
            _processTask = ProcessAsync();
            return connectResult;
        }

        /// <summary>
        /// Create the socket object
        /// </summary>
        private ClientWebSocket CreateSocket()
        {
            var cookieContainer = new CookieContainer();
            foreach (var cookie in Parameters.Cookies)
                cookieContainer.Add(new Cookie(cookie.Key, cookie.Value));

            var socket = new ClientWebSocket();
            try
            {
                socket.Options.Cookies = cookieContainer;
                foreach (var header in Parameters.Headers)
                    socket.Options.SetRequestHeader(header.Key, header.Value);
                socket.Options.KeepAliveInterval = Parameters.KeepAliveInterval ?? TimeSpan.Zero;
                if (System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework")) 
                    socket.Options.SetBuffer(65536, 65536); // Setting it to anything bigger than 65536 throws an exception in .net framework
                else
                    socket.Options.SetBuffer(_receiveBufferSize, _sendBufferSize);
                if (Parameters.Proxy != null)
                    SetProxy(socket, Parameters.Proxy);
                #if NET6_0_OR_GREATER
                socket.Options.CollectHttpResponseDetails = true;
                #endif
            }
            catch (PlatformNotSupportedException)
            {
                // Options are not supported on certain platforms (WebAssembly for instance)
                // best we can do it try to connect without setting options.
            }

            return socket;
        }

        private async Task<CallResult> ConnectInternalAsync()
        {
            _logger.SocketConnecting(Id);
            try
            {
                if (Parameters.RateLimiter != null)
                {
                    var definition = new RequestDefinition(Id.ToString(), HttpMethod.Get);
                    var limitResult = await Parameters.RateLimiter.ProcessAsync(_logger, Id, RateLimitItemType.Connection, definition, _baseAddress, null, 1, Parameters.RateLimitingBehaviour, _ctsSource.Token).ConfigureAwait(false);
                    if (!limitResult)
                        return new CallResult(new ClientRateLimitError("Connection limit reached"));
                }

                using CancellationTokenSource tcs = new(TimeSpan.FromSeconds(10));
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(tcs.Token, _ctsSource.Token);
                await _socket.ConnectAsync(Uri, linked.Token).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (!_ctsSource.IsCancellationRequested)
                {
                    // if _ctsSource was canceled this was already logged
                    _logger.SocketConnectionFailed(Id, e.Message, e);
                }

                if (e is WebSocketException we)
                {
                    #if (NET6_0_OR_GREATER)
                    if (_socket.HttpStatusCode == HttpStatusCode.TooManyRequests)
                    {
                        await (OnConnectRateLimited?.Invoke() ?? Task.CompletedTask).ConfigureAwait(false);
                        return new CallResult(new ServerRateLimitError(we.Message));
                    }
                    #else
                    // ClientWebSocket.HttpStatusCode is only available in .NET6+ https://learn.microsoft.com/en-us/dotnet/api/system.net.websockets.clientwebsocket.httpstatuscode?view=net-8.0
                    // Try to read 429 from the message instead
                    if (we.Message.Contains("429"))
                    {
                        await (OnConnectRateLimited?.Invoke() ?? Task.CompletedTask).ConfigureAwait(false);
                        return new CallResult(new ServerRateLimitError(we.Message));
                    }
                    #endif
                }

                return new CallResult(new CantConnectError());
            }

            _logger.SocketConnected(Id, Uri);
            return new CallResult(null);
        }

        /// <inheritdoc />
        private async Task ProcessAsync()
        {
            while (!_stopRequested)
            {
                _logger.SocketStartingProcessing(Id);
                SetProcessState(ProcessState.Processing);
                var sendTask = SendLoopAsync();
                var receiveTask = ReceiveLoopAsync();
                var timeoutTask = Parameters.Timeout != null && Parameters.Timeout > TimeSpan.FromSeconds(0) ? CheckTimeoutAsync() : Task.CompletedTask;
                await Task.WhenAll(sendTask, receiveTask, timeoutTask).ConfigureAwait(false);
                _logger.SocketFinishedProcessing(Id);

                SetProcessState(ProcessState.WaitingForClose);
                while (_closeTask == null)
                    await Task.Delay(50).ConfigureAwait(false);

                await _closeTask.ConfigureAwait(false);
                if (!_stopRequested)
                    _closeTask = null;

                if (Parameters.ReconnectPolicy == ReconnectPolicy.Disabled)
                {
                    SetProcessState(ProcessState.Idle);
                    await (OnClose?.Invoke() ?? Task.CompletedTask).ConfigureAwait(false);
                    return;
                }

                if (!_stopRequested)
                {
                    SetProcessState(ProcessState.Reconnecting);
                    await (OnReconnecting?.Invoke() ?? Task.CompletedTask).ConfigureAwait(false);                    
                }

                // Delay here to prevent very repid looping when a connection to the server is accepted and immediately disconnected
                var initialDelay = GetReconnectDelay();
                await Task.Delay(initialDelay).ConfigureAwait(false);

                while (!_stopRequested)
                {
                    _logger.SocketAttemptReconnect(Id);
                    var task = GetReconnectionUrl?.Invoke();
                    if (task != null)
                    {
                        var reconnectUri = await task.ConfigureAwait(false);
                        if (reconnectUri != null && Parameters.Uri.ToString() != reconnectUri.ToString())
                        {
                            _logger.SocketSetReconnectUri(Id, reconnectUri);
                            Parameters.Uri = reconnectUri;
                        }
                    }

                    _socket?.Dispose();
                    _socket = CreateSocket();
                    _ctsSource.Dispose();
                    _ctsSource = new CancellationTokenSource();
                    while (_sendBuffer.TryDequeue(out _)) { } // Clear send buffer

                    _reconnectAttempt++;
                    var connected = await ConnectInternalAsync().ConfigureAwait(false);
                    if (!connected)
                    {
                        // Delay between reconnect attempts
                        var delay = GetReconnectDelay();
                        await Task.Delay(delay).ConfigureAwait(false);
                        continue;
                    }

                    _reconnectAttempt = 0;
                    _lastReconnectTime = DateTime.UtcNow;

                    // Set to processing before reconnect handling
                    SetProcessState(ProcessState.Processing);
                    await (OnReconnected?.Invoke() ?? Task.CompletedTask).ConfigureAwait(false);
                    break;
                }
            }

            SetProcessState(ProcessState.Idle);
        }

        private TimeSpan GetReconnectDelay()
        {
            if (_reconnectAttempt == 0)
            {
                // Means this is directly after disconnecting. Only delay if the last reconnect time is very recent
                var sinceLastReconnect = DateTime.UtcNow - _lastReconnectTime;
                if (sinceLastReconnect < TimeSpan.FromSeconds(5))
                    return TimeSpan.FromSeconds(5) - sinceLastReconnect;

                return TimeSpan.FromMilliseconds(1);
            }

            var delay = Parameters.ReconnectPolicy == ReconnectPolicy.FixedDelay ? Parameters.ReconnectInterval : TimeSpan.FromSeconds(Math.Pow(2, Math.Min(5, _reconnectAttempt)));
            if (delay > TimeSpan.Zero)
                return delay;
            return TimeSpan.FromMilliseconds(1);
        }

        /// <inheritdoc />
        public virtual bool Send(int id, string data, int weight)
        {
            if (_ctsSource.IsCancellationRequested || _processState != ProcessState.Processing)
                return false;

            var bytes = Parameters.Encoding.GetBytes(data);
            _logger.SocketAddingBytesToSendBuffer(Id, id, bytes);
            _sendBuffer.Enqueue(new SendItem { Id = id, Weight = weight, Bytes = bytes });
            _sendEvent.Set();
            return true;
        }

        /// <inheritdoc />
        public virtual async Task ReconnectAsync()
        {
            if (_processState != ProcessState.Processing && IsOpen)
                return;

            _logger.SocketReconnectRequested(Id);
            _closeTask = CloseInternalAsync();
            await _closeTask.ConfigureAwait(false);
        }

        /// <inheritdoc />
        public virtual async Task CloseAsync()
        {
            await _closeSem.WaitAsync().ConfigureAwait(false);
            _stopRequested = true;

            try
            {
                if (_closeTask?.IsCompleted == false)
                {
                    _logger.SocketCloseAsyncWaitingForExistingCloseTask(Id);
                    await _closeTask.ConfigureAwait(false);
                    return;
                }

                if (!IsOpen)
                {
                    _logger.SocketCloseAsyncSocketNotOpen(Id);
                    return;
                }

                _logger.SocketClosing(Id);
                _closeTask = CloseInternalAsync();
            }
            finally
            {
                _closeSem.Release();
            }

            await _closeTask.ConfigureAwait(false);
            if(_processTask != null)
                await _processTask.ConfigureAwait(false);
            await (OnClose?.Invoke() ?? Task.CompletedTask).ConfigureAwait(false);
            _logger.SocketClosed(Id);
        }

        /// <summary>
        /// Internal close method
        /// </summary>
        /// <returns></returns>
        private async Task CloseInternalAsync()
        {
            if (_disposed)
                return;

            try
            {
                if (_socket.State == WebSocketState.CloseReceived)
                {
                    await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", default).ConfigureAwait(false);
                }
                else if (_socket.State == WebSocketState.Open)
                {
                    await _socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", default).ConfigureAwait(false);
                    var startWait = DateTime.UtcNow;
                    while (_socket.State != WebSocketState.Closed && _socket.State != WebSocketState.Aborted)
                    {
                        // Wait until we receive close confirmation
                        await Task.Delay(10).ConfigureAwait(false);
                        if (DateTime.UtcNow - startWait > TimeSpan.FromSeconds(5))
                            break; // Wait for max 5 seconds, then just abort the connection
                    }
                }
            }
            catch (Exception)
            {
                // Can sometimes throw an exception when socket is in aborted state due to timing
                // Websocket is set to Aborted state when the cancelation token is set during SendAsync/ReceiveAsync
                // So socket might go to aborted state, might still be open
            }

            _ctsSource.Cancel();
        }

        /// <summary>
        /// Dispose the socket
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            if (_ctsSource?.IsCancellationRequested == false)
                _ctsSource.Cancel();

            _logger.SocketDisposing(Id);
            _disposed = true;
            _socket.Dispose();
            _ctsSource?.Dispose();
            _sendEvent.Dispose();
            _logger.SocketDisposed(Id);
        }

        /// <summary>
        /// Loop for sending data
        /// </summary>
        /// <returns></returns>
        private async Task SendLoopAsync()
        {
            var requestDefinition = new RequestDefinition(Id.ToString(), HttpMethod.Get);
            try
            {
                while (true)
                {
                    try
                    {
                        if (!_sendBuffer.Any())
                            await _sendEvent.WaitAsync(ct: _ctsSource.Token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    if (_ctsSource.IsCancellationRequested)
                        break;

                    while (_sendBuffer.TryDequeue(out var data))
                    {
                        if (Parameters.RateLimiter != null)
                        {
                            try
                            {
                                var limitResult = await Parameters.RateLimiter.ProcessAsync(_logger, data.Id, RateLimitItemType.Request, requestDefinition, _baseAddress, null, data.Weight, Parameters.RateLimitingBehaviour, _ctsSource.Token).ConfigureAwait(false);
                                if (!limitResult)
                                {
                                    await (OnRequestRateLimited?.Invoke(data.Id) ?? Task.CompletedTask).ConfigureAwait(false);
                                    continue;
                                }
                            }
                            catch (OperationCanceledException)
                            {
                                // canceled
                                break;
                            }
                        }

                        try
                        {
                            await _socket.SendAsync(new ArraySegment<byte>(data.Bytes, 0, data.Bytes.Length), WebSocketMessageType.Text, true, _ctsSource.Token).ConfigureAwait(false);
                            await (OnRequestSent?.Invoke(data.Id) ?? Task.CompletedTask).ConfigureAwait(false);
                            _logger.SocketSentBytes(Id, data.Id, data.Bytes.Length);
                        }
                        catch (OperationCanceledException)
                        {
                            // canceled
                            break;
                        }
                        catch (Exception ioe)
                        {
                            // Connection closed unexpectedly, .NET framework
                            await (OnError?.Invoke(ioe) ?? Task.CompletedTask).ConfigureAwait(false);
                            if (_closeTask?.IsCompleted != false)
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
                _logger.SocketSendLoopStoppedWithException(Id, e.Message, e);
                await (OnError?.Invoke(e) ?? Task.CompletedTask).ConfigureAwait(false);
                if (_closeTask?.IsCompleted != false)
                    _closeTask = CloseInternalAsync();
            }
            finally
            {
                _logger.SocketSendLoopFinished(Id);
            }
        }

        /// <summary>
        /// Loop for receiving and reassembling data
        /// </summary>
        /// <returns></returns>
        private async Task ReceiveLoopAsync()
        {
            var buffer = new ArraySegment<byte>(new byte[_receiveBufferSize]);
            var received = 0;
            try
            {
                while (true)
                {
                    if (_ctsSource.IsCancellationRequested)
                        break;

                    MemoryStream? multipartStream = null;
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
                            await (OnError?.Invoke(wse) ?? Task.CompletedTask).ConfigureAwait(false);
                            if (_closeTask?.IsCompleted != false)
                                _closeTask = CloseInternalAsync();
                            break;
                        }

                        if (receiveResult.MessageType == WebSocketMessageType.Close)
                        {
                            // Connection closed
                            if (_socket.State == WebSocketState.CloseReceived)
                            {
                                // Close received means it server initiated, we should send a confirmation and close the socket
                                _logger.SocketReceivedCloseMessage(Id, receiveResult.CloseStatus.ToString(), receiveResult.CloseStatusDescription);
                                if (_closeTask?.IsCompleted != false)
                                    _closeTask = CloseInternalAsync();
                            }
                            else
                            {
                                // Means the socket is now closed and we were the one initiating it
                                _logger.SocketReceivedCloseConfirmation(Id, receiveResult.CloseStatus.ToString(), receiveResult.CloseStatusDescription);
                            }

                            break;
                        }

                        if (!receiveResult.EndOfMessage)
                        {
                            // We received data, but it is not complete, write it to a memory stream for reassembling
                            multiPartMessage = true;
                            _logger.SocketReceivedPartialMessage(Id, receiveResult.Count);

                            // Write the data to a memory stream to be reassembled later
                            if (multipartStream == null)
                                multipartStream = new MemoryStream();
                            multipartStream.Write(buffer.Array, buffer.Offset, receiveResult.Count);
                        }
                        else
                        {
                            if (!multiPartMessage)
                            {
                                // Received a complete message and it's not multi part
                                _logger.SocketReceivedSingleMessage(Id, receiveResult.Count);
                                await ProcessData(receiveResult.MessageType, new ReadOnlyMemory<byte>(buffer.Array, buffer.Offset, receiveResult.Count)).ConfigureAwait(false);
                            }
                            else
                            {
                                // Received the end of a multipart message, write to memory stream for reassembling
                                _logger.SocketReceivedPartialMessage(Id, receiveResult.Count);
                                multipartStream!.Write(buffer.Array, buffer.Offset, receiveResult.Count);
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
                            _logger.SocketReassembledMessage(Id, multipartStream!.Length);
                            // Get the underlying buffer of the memorystream holding the written data and delimit it (GetBuffer return the full array, not only the written part)
                            await ProcessData(receiveResult.MessageType, new ReadOnlyMemory<byte>(multipartStream.GetBuffer(), 0, (int)multipartStream.Length)).ConfigureAwait(false);
                        }
                        else
                        {
                            _logger.SocketDiscardIncompleteMessage(Id, multipartStream!.Length);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                // Because this is running in a separate task and not awaited until the socket gets closed
                // any exception here will crash the receive processing, but do so silently unless the socket gets stopped.
                // Make sure we at least let the owner know there was an error
                _logger.SocketReceiveLoopStoppedWithException(Id, e);
                await (OnError?.Invoke(e) ?? Task.CompletedTask).ConfigureAwait(false);
                if (_closeTask?.IsCompleted != false)
                    _closeTask = CloseInternalAsync();
            }
            finally
            {
                _logger.SocketReceiveLoopFinished(Id);
            }
        }

        /// <summary>
        /// Proccess a stream message
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        protected async Task ProcessData(WebSocketMessageType type, ReadOnlyMemory<byte> data)
        {
            LastActionTime = DateTime.UtcNow;
            await (OnStreamMessage?.Invoke(type, data) ?? Task.CompletedTask).ConfigureAwait(false);
        }

        /// <summary>
        /// Checks if there is no data received for a period longer than the specified timeout
        /// </summary>
        /// <returns></returns>
        protected async Task CheckTimeoutAsync()
        {
            _logger.SocketStartingTaskForNoDataReceivedCheck(Id, Parameters.Timeout);
            LastActionTime = DateTime.UtcNow;
            try 
            { 
                while (true)
                {
                    if (_ctsSource.IsCancellationRequested)
                        return;

                    if (DateTime.UtcNow - LastActionTime > Parameters.Timeout)
                    {
                        _logger.SocketNoDataReceiveTimoutReconnect(Id, Parameters.Timeout);
                        _ = ReconnectAsync().ConfigureAwait(false);
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
                await (OnError?.Invoke(e) ?? Task.CompletedTask).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Get the next identifier
        /// </summary>
        /// <returns></returns>
        private static int NextStreamId()
        {
            lock (_streamIdLock)
            {
                _lastStreamId++;
                return _lastStreamId;
            }
        }

        /// <summary>
        /// Update the received messages list, removing messages received longer than 3s ago
        /// </summary>
        protected void UpdateReceivedMessages()
        {
            var checkTime = DateTime.UtcNow;
            if (checkTime - _lastReceivedMessagesUpdate > TimeSpan.FromSeconds(1))
            {
                for (var i = 0; i < _receivedMessages.Count; i++)
                {
                    var msg = _receivedMessages[i];
                    if (checkTime - msg.Timestamp > TimeSpan.FromSeconds(3))
                    {
                        _receivedMessages.Remove(msg);
                        i--;
                    }
                }

                _lastReceivedMessagesUpdate = checkTime;
            }
        }

        /// <summary>
        /// Set proxy on socket
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="proxy"></param>
        /// <exception cref="ArgumentException"></exception>
        protected virtual void SetProxy(ClientWebSocket socket, ApiProxy proxy)
        {
            if (!Uri.TryCreate($"{proxy.Host}:{proxy.Port}", UriKind.Absolute, out var uri))
                throw new ArgumentException("Proxy settings invalid, {proxy.Host}:{proxy.Port} not a valid URI", nameof(proxy));

            socket.Options.Proxy = uri?.Scheme == null
                ? socket.Options.Proxy = new WebProxy(proxy.Host, proxy.Port)
                : socket.Options.Proxy = new WebProxy
                {
                    Address = uri
                };

            if (proxy.Login != null)
                socket.Options.Proxy.Credentials = new NetworkCredential(proxy.Login, proxy.Password);
        }

        private void SetProcessState(ProcessState state)
        {
            if (_processState == state)
                return;

            _logger.SocketProcessingStateChanged(Id, _processState.ToString(), state.ToString());
            _processState = state;
        }
    }

    /// <summary>
    /// Message info
    /// </summary>
    public struct SendItem
    {
        /// <summary>
        /// The request id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The request weight
        /// </summary>
        public int Weight { get; set; }

        /// <summary>
        /// Timestamp the request was sent
        /// </summary>
        public DateTime SendTime { get; set; }

        /// <summary>
        /// The bytes to send
        /// </summary>
        public byte[] Bytes { get; set; }
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
