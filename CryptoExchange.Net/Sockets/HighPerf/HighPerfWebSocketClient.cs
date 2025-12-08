using CryptoExchange.Net.Logging.Extensions;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Errors;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets.HighPerf.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Sockets.HighPerf
{
    /// <summary>
    /// A high performance websocket client implementation
    /// </summary>
    public class HighPerfWebSocketClient : IHighPerfWebsocket
    {
        private ClientWebSocket? _socket;

#if NETSTANDARD2_0
        private static readonly ArrayPool<byte> _receiveBufferPool = ArrayPool<byte>.Shared;
#endif

        private readonly SemaphoreSlim _closeSem;

        private CancellationTokenSource _ctsSource;
        private Task? _processTask;
        private Task? _closeTask;
        private bool _stopRequested;
        private bool _disposed;
        private bool _processing;
        private readonly int _receiveBufferSize;
        private readonly PipeWriter _pipeWriter;

        private const int _defaultReceiveBufferSize = 4096;
        private const int _sendBufferSize = 4096;

        /// <summary>
        /// Log
        /// </summary>
        protected ILogger _logger;

        /// <inheritdoc />
        public int Id { get; }

        /// <inheritdoc />
        public WebSocketParameters Parameters { get; }
                
        /// <inheritdoc />
        public Uri Uri => Parameters.Uri;

        /// <inheritdoc />
        public virtual bool IsClosed => _socket == null || _socket?.State == WebSocketState.Closed;

        /// <inheritdoc />
        public virtual bool IsOpen => _socket?.State == WebSocketState.Open && !_ctsSource.IsCancellationRequested;

        /// <inheritdoc />
        public event Func<Task>? OnClose;

        /// <inheritdoc />
        public event Func<Exception, Task>? OnError;

        /// <inheritdoc />
        public event Func<Task>? OnOpen;

        /// <summary>
        /// ctor
        /// </summary>
        public HighPerfWebSocketClient(ILogger logger, WebSocketParameters websocketParameters, PipeWriter pipeWriter)
        {
            Id = ExchangeHelpers.NextId();
            _logger = logger;

            Parameters = websocketParameters;
            _ctsSource = new CancellationTokenSource();
            _receiveBufferSize = websocketParameters.ReceiveBufferSize ?? _defaultReceiveBufferSize;

            _pipeWriter = pipeWriter;
            _closeSem = new SemaphoreSlim(1, 1);
        }

        /// <inheritdoc />
        public virtual async Task<CallResult> ConnectAsync(CancellationToken ct)
        {
            var connectResult = await ConnectInternalAsync(ct).ConfigureAwait(false);
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
                socket.Options.SetBuffer(_receiveBufferSize, _sendBufferSize);
                if (Parameters.Proxy != null)
                    SetProxy(socket, Parameters.Proxy);

#if NET6_0_OR_GREATER
                socket.Options.CollectHttpResponseDetails = true;
#endif
#if NET9_0_OR_GREATER
                socket.Options.KeepAliveTimeout = Parameters.KeepAliveTimeout ?? TimeSpan.FromSeconds(10);
#endif
            }
            catch (PlatformNotSupportedException)
            {
                // Options are not supported on certain platforms (WebAssembly for instance)
                // best we can do it try to connect without setting options.
            }

            return socket;
        }

        private async Task<CallResult> ConnectInternalAsync(CancellationToken ct)
        {
            _logger.SocketConnecting(Id);
            try
            {
                using CancellationTokenSource tcs = new(TimeSpan.FromSeconds(10));
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(tcs.Token, _ctsSource.Token, ct);
                _socket = CreateSocket();
                await _socket.ConnectAsync(Uri, linked.Token).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (ct.IsCancellationRequested)
                    _logger.SocketConnectingCanceled(Id);
                else if (!_ctsSource.IsCancellationRequested)
                {
                    // if _ctsSource was canceled this was already logged
                    _logger.SocketConnectionFailed(Id, e.Message, e);
                }

                if (e is WebSocketException we)
                {
#if (NET6_0_OR_GREATER)
                    if (_socket!.HttpStatusCode == HttpStatusCode.TooManyRequests)
                        return new CallResult(new ServerRateLimitError(we.Message, we));

                    if (_socket.HttpStatusCode == HttpStatusCode.Unauthorized)
                        return new CallResult(new ServerError(new ErrorInfo(ErrorType.Unauthorized, "Server returned status code `401` when `101` was expected")));
#else
                    // ClientWebSocket.HttpStatusCode is only available in .NET6+ https://learn.microsoft.com/en-us/dotnet/api/system.net.websockets.clientwebsocket.httpstatuscode?view=net-8.0
                    // Try to read 429 from the message instead
                    if (we.Message.Contains("429"))
                        return new CallResult(new ServerRateLimitError(we.Message, we));
#endif
                }

                return new CallResult(new CantConnectError(e));
            }

            _logger.SocketConnected(Id, Uri);
            return CallResult.SuccessResult;
        }

        /// <inheritdoc />
        private async Task ProcessAsync()
        {
            _logger.SocketStartingProcessing(Id);
            _processing = true;
            await ReceiveLoopAsync().ConfigureAwait(false);
            _processing = false;
            _logger.SocketFinishedProcessing(Id);

            while (_closeTask == null)
                await Task.Delay(50).ConfigureAwait(false);

            await _closeTask.ConfigureAwait(false);
            await (OnClose?.Invoke() ?? Task.CompletedTask).ConfigureAwait(false);
            _logger.SocketClosed(Id);
        }

        /// <inheritdoc />
        public virtual ValueTask<bool> SendAsync(string data)
        {
            var bytes = Parameters.Encoding.GetBytes(data);
            return SendAsync(bytes, WebSocketMessageType.Text);
        }

        /// <inheritdoc />
        public virtual async ValueTask<bool> SendAsync(byte[] data, WebSocketMessageType type = WebSocketMessageType.Binary)
        {
            if (_ctsSource.IsCancellationRequested || !_processing)
                return false;

            try
            {
                await _socket!.SendAsync(new ArraySegment<byte>(data, 0, data.Length), type, true, _ctsSource.Token).ConfigureAwait(false);
                return true;
            }
            catch (OperationCanceledException)
            {
                // canceled
                return false;
            }
            catch (Exception ioe)
            {
                // Connection closed unexpectedly, .NET framework
                await (OnError?.Invoke(ioe) ?? Task.CompletedTask).ConfigureAwait(false);
                if (_closeTask?.IsCompleted != false)
                    _closeTask = CloseInternalAsync();
                return false;
            }
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
                if (_socket!.State == WebSocketState.CloseReceived)
                    await _socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", default).ConfigureAwait(false);
                else if (_socket.State == WebSocketState.Open)
                {
                    await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", default).ConfigureAwait(false);

                    var startWait = DateTime.UtcNow;
                    while (_processing && _socket.State != WebSocketState.Closed && _socket.State != WebSocketState.Aborted)
                    {
                        // Wait until we receive close confirmation
                        await Task.Delay(10).ConfigureAwait(false);
                        if (DateTime.UtcNow - startWait > TimeSpan.FromSeconds(1))
                            break; // Wait for max 1 second, then just abort the connection
                    }                    
                }
            }
            catch (Exception)
            {
                // Can sometimes throw an exception when socket is in aborted state due to timing
                // Websocket is set to Aborted state when the cancelation token is set during SendAsync/ReceiveAsync
                // So socket might go to aborted state, might still be open
            }

            if (!_disposed)
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
            _socket?.Dispose();
            _ctsSource?.Dispose();
            _logger.SocketDisposed(Id);
        }

#if NETSTANDARD2_1 || NET8_0_OR_GREATER
        private async Task ReceiveLoopAsync()
        {
            Exception? exitException = null;

            try
            {
                while (true)
                {
                    if (_ctsSource.IsCancellationRequested)
                        break;

                    ValueWebSocketReceiveResult receiveResult;

                    try
                    {
                        receiveResult = await _socket!.ReceiveAsync(_pipeWriter.GetMemory(_receiveBufferSize), _ctsSource.Token).ConfigureAwait(false);

                        // Advance the writer to communicate which part of the memory was written
                        _pipeWriter.Advance(receiveResult.Count);                        
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (ex.InnerException?.InnerException?.Message.Contains("KeepAliveTimeout") == true)
                            // Specific case that the websocket connection got closed because of a ping frame timeout
                            // Unfortunately doesn't seem to be a nicer way to catch
                            _logger.SocketPingTimeout(Id);

                        if (_closeTask?.IsCompleted != false)
                            _closeTask = CloseInternalAsync();

                        exitException = ex;
                        break;
                    }
                    catch (Exception wse)
                    {
                        if (!_ctsSource.Token.IsCancellationRequested && !_stopRequested)
                            // Connection closed unexpectedly
                            await (OnError?.Invoke(wse) ?? Task.CompletedTask).ConfigureAwait(false);

                        if (_closeTask?.IsCompleted != false)
                            _closeTask = CloseInternalAsync();

                        exitException = wse;
                        break;
                    }

                    if (receiveResult.EndOfMessage)
                    {
                        // Flush the full message
                        var flushResult = await _pipeWriter.FlushAsync().ConfigureAwait(false);
                        if (flushResult.IsCompleted)
                        {
                            // Flush indicated that the reader is no longer listening, so we should stop writing
                            if (_closeTask?.IsCompleted != false)
                                _closeTask = CloseInternalAsync();

                            break;
                        }
                    }

                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        // Connection closed
                        if (_socket.State == WebSocketState.CloseReceived)
                        {
                            // Close received means it's server initiated, we should send a confirmation and close the socket
                            _logger.SocketReceivedCloseMessage(Id, _socket.CloseStatus?.ToString() ?? string.Empty, _socket.CloseStatusDescription ?? string.Empty);
                            if (_closeTask?.IsCompleted != false)
                                _closeTask = CloseInternalAsync();
                        }
                        else
                        {
                            // Means the socket is now closed and we were the one initiating it
                            _logger.SocketReceivedCloseConfirmation(Id, _socket.CloseStatus?.ToString() ?? string.Empty, _socket.CloseStatusDescription ?? string.Empty);
                        }

                        break;
                    }
                }
            }
            catch (Exception e)
            {
                // Because this is running in a separate task and not awaited until the socket gets closed
                // any exception here will crash the receive processing, but do so silently unless the socket gets stopped.
                // Make sure we at least let the owner know there was an error
                _logger.SocketReceiveLoopStoppedWithException(Id, e);

                exitException = e;
                await (OnError?.Invoke(e) ?? Task.CompletedTask).ConfigureAwait(false);
                if (_closeTask?.IsCompleted != false)
                    _closeTask = CloseInternalAsync();
            }
            finally
            {
                await _pipeWriter.CompleteAsync(exitException).ConfigureAwait(false);
                _logger.SocketReceiveLoopFinished(Id);
            }
        }
#else

        private async Task ReceiveLoopAsync()
        {
            byte[] rentedBuffer = _receiveBufferPool.Rent(_receiveBufferSize);
            var buffer = new ArraySegment<byte>(rentedBuffer);
            Exception? exitException = null;

            try
            {
                while (true)
                {
                    if (_ctsSource.IsCancellationRequested)
                        break;

                    WebSocketReceiveResult? receiveResult = null;
                    try
                    {
                        receiveResult = await _socket!.ReceiveAsync(buffer, _ctsSource.Token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (ex.InnerException?.InnerException?.Message.Contains("KeepAliveTimeout") == true)
                            // Specific case that the websocket connection got closed because of a ping frame timeout
                            // Unfortunately doesn't seem to be a nicer way to catch
                            _logger.SocketPingTimeout(Id);

                        if (_closeTask?.IsCompleted != false)
                            _closeTask = CloseInternalAsync();

                        exitException = ex;
                        break;
                    }
                    catch (Exception wse)
                    {
                        if (!_ctsSource.Token.IsCancellationRequested && !_stopRequested)
                            // Connection closed unexpectedly
                            await (OnError?.Invoke(wse) ?? Task.CompletedTask).ConfigureAwait(false);

                        if (_closeTask?.IsCompleted != false)
                            _closeTask = CloseInternalAsync();

                        exitException = wse;
                        break;
                    }

                    if (receiveResult.Count > 0)
                        await _pipeWriter.WriteAsync(buffer.AsMemory(0, receiveResult.Count)).ConfigureAwait(false);

                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        // Connection closed
                        if (_socket.State == WebSocketState.CloseReceived)
                        {
                            // Close received means it server initiated, we should send a confirmation and close the socket
                            _logger.SocketReceivedCloseMessage(Id, receiveResult.CloseStatus.ToString()!, receiveResult.CloseStatusDescription ?? string.Empty);
                            if (_closeTask?.IsCompleted != false)
                                _closeTask = CloseInternalAsync();
                        }
                        else
                        {
                            // Means the socket is now closed and we were the one initiating it
                            _logger.SocketReceivedCloseConfirmation(Id, receiveResult.CloseStatus.ToString()!, receiveResult.CloseStatusDescription ?? string.Empty);
                        }

                        break;
                    }
                }
                
            }
            catch (Exception e)
            {
                // Because this is running in a separate task and not awaited until the socket gets closed
                // any exception here will crash the receive processing, but do so silently unless the socket gets stopped.
                // Make sure we at least let the owner know there was an error
                _logger.SocketReceiveLoopStoppedWithException(Id, e);

                exitException = e;
                await (OnError?.Invoke(e) ?? Task.CompletedTask).ConfigureAwait(false);
                if (_closeTask?.IsCompleted != false)
                    _closeTask = CloseInternalAsync();
            }
            finally
            {
                await _pipeWriter.CompleteAsync(exitException).ConfigureAwait(false);

                _receiveBufferPool.Return(rentedBuffer, true);
                _logger.SocketReceiveLoopFinished(Id);
            }
        }
#endif

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
    }
}