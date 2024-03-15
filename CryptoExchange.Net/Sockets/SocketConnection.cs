﻿using CryptoExchange.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CryptoExchange.Net.Objects;
using System.Net.WebSockets;
using System.IO;
using CryptoExchange.Net.Objects.Sockets;
using System.Diagnostics;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.JsonNet;
using System.Threading;

namespace CryptoExchange.Net.Sockets
{
    /// <summary>
    /// A single socket connection to the server
    /// </summary>
    public class SocketConnection
    {
        /// <summary>
        /// Connection lost event
        /// </summary>
        public event Action? ConnectionLost;

        /// <summary>
        /// Connection closed and no reconnect is happening
        /// </summary>
        public event Action? ConnectionClosed;

        /// <summary>
        /// Connecting restored event
        /// </summary>
        public event Action<TimeSpan>? ConnectionRestored;

        /// <summary>
        /// The connection is paused event
        /// </summary>
        public event Action? ActivityPaused;

        /// <summary>
        /// The connection is unpaused event
        /// </summary>
        public event Action? ActivityUnpaused;

        /// <summary>
        /// Unhandled message event
        /// </summary>
        public event Action<IMessageAccessor>? UnhandledMessage;

        /// <summary>
        /// The amount of subscriptions on this connection
        /// </summary>
        public int UserSubscriptionCount
        {
            get
            {
                lock(_listenersLock)
                    return _listeners.OfType<Subscription>().Count(h => h.UserSubscription);
            }
        }

        /// <summary>
        /// Get a copy of the current message subscriptions
        /// </summary>
        public Subscription[] Subscriptions
        {
            get
            {
                lock(_listenersLock)
                    return _listeners.OfType<Subscription>().Where(h => h.UserSubscription).ToArray();
            }
        }

        /// <summary>
        /// If the connection has been authenticated
        /// </summary>
        public bool Authenticated { get; set; }

        /// <summary>
        /// If connection is made
        /// </summary>
        public bool Connected => _socket.IsOpen;

        /// <summary>
        /// The unique ID of the socket
        /// </summary>
        public int SocketId => _socket.Id;

        /// <summary>
        /// The current kilobytes per second of data being received, averaged over the last 3 seconds
        /// </summary>
        public double IncomingKbps => _socket.IncomingKbps;

        /// <summary>
        /// The connection uri
        /// </summary>
        public Uri ConnectionUri => _socket.Uri;

        /// <summary>
        /// The API client the connection is for
        /// </summary>
        public SocketApiClient ApiClient { get; set; }

        /// <summary>
        /// Time of disconnecting
        /// </summary>
        public DateTime? DisconnectTime { get; set; }

        /// <summary>
        /// Tag for identificaion
        /// </summary>
        public string Tag { get; set; }
        
        /// <summary>
        /// Additional properties for this connection
        /// </summary>
        public Dictionary<string, object> Properties { get; set; }

        /// <summary>
        /// If activity is paused
        /// </summary>
        public bool PausedActivity
        {
            get => _pausedActivity;
            set
            {
                if (_pausedActivity != value)
                {
                    _pausedActivity = value;
                    _logger.Log(LogLevel.Information, $"[Sckt {SocketId}] paused activity: " + value);
                    if(_pausedActivity) _ = Task.Run(() => ActivityPaused?.Invoke());
                    else _ = Task.Run(() => ActivityUnpaused?.Invoke());
                }
            }
        }

        /// <summary>
        /// Status of the socket connection
        /// </summary>
        public SocketStatus Status
        {
            get => _status;
            private set
            {
                if (_status == value)
                    return;

                var oldStatus = _status;
                _status = value;
                _logger.Log(LogLevel.Debug, $"[Sckt {SocketId}] status changed from {oldStatus} to {_status}");
            }
        }

        private bool _pausedActivity;
        private readonly object _listenersLock;
        private readonly List<IMessageProcessor> _listeners;
        private readonly ILogger _logger;
        private SocketStatus _status;

        private readonly IMessageSerializer _serializer;
        private readonly IByteMessageAccessor _accessor;

        /// <summary>
        /// The task that is sending periodic data on the websocket. Can be used for sending Ping messages every x seconds or similair. Not necesarry.
        /// </summary>
        protected Task? periodicTask;

        /// <summary>
        /// Wait event for the periodicTask
        /// </summary>
        protected AsyncResetEvent? periodicEvent;

        /// <summary>
        /// The underlying websocket
        /// </summary>
        private readonly IWebsocket _socket;

        /// <summary>
        /// New socket connection
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="apiClient">The api client</param>
        /// <param name="socket">The socket</param>
        /// <param name="tag"></param>
        public SocketConnection(ILogger logger, SocketApiClient apiClient, IWebsocket socket, string tag)
        {
            _logger = logger;
            ApiClient = apiClient;
            Tag = tag;
            Properties = new Dictionary<string, object>();

            _socket = socket;
            _socket.OnStreamMessage += HandleStreamMessage;
            _socket.OnRequestSent += HandleRequestSentAsync;
            _socket.OnOpen += HandleOpenAsync;
            _socket.OnClose += HandleCloseAsync;
            _socket.OnReconnecting += HandleReconnectingAsync;
            _socket.OnReconnected += HandleReconnectedAsync;
            _socket.OnError += HandleErrorAsync;
            _socket.GetReconnectionUrl = GetReconnectionUrlAsync;

            _listenersLock = new object();
            _listeners = new List<IMessageProcessor>();

            _serializer = apiClient.CreateSerializer();
            _accessor = apiClient.CreateAccessor();
        }

        /// <summary>
        /// Handler for a socket opening
        /// </summary>
        protected virtual Task HandleOpenAsync()
        {
            Status = SocketStatus.Connected;
            PausedActivity = false;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handler for a socket closing without reconnect
        /// </summary>
        protected virtual Task HandleCloseAsync()
        {
            Status = SocketStatus.Closed;
            Authenticated = false;

            lock (_listenersLock)
            {
                foreach (var subscription in _listeners.OfType<Subscription>())
                    subscription.Confirmed = false;

                foreach (var query in _listeners.OfType<Query>().ToList())
                {
                    query.Fail(new WebError("Connection interupted"));
                    _listeners.Remove(query);
                }
            }

            _ = Task.Run(() => ConnectionClosed?.Invoke());
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handler for a socket losing connection and starting reconnect
        /// </summary>
        protected virtual Task HandleReconnectingAsync()
        {
            Status = SocketStatus.Reconnecting;
            DisconnectTime = DateTime.UtcNow;
            Authenticated = false;

            lock (_listenersLock)
            {
                foreach (var subscription in _listeners.OfType<Subscription>())
                    subscription.Confirmed = false;

                foreach (var query in _listeners.OfType<Query>().ToList())
                {
                    query.Fail(new WebError("Connection interupted"));
                    _listeners.Remove(query);
                }
            }

            _ = Task.Run(() => ConnectionLost?.Invoke());
            return Task.CompletedTask;
        }

        /// <summary>
        /// Get the url to connect to when reconnecting
        /// </summary>
        /// <returns></returns>
        protected virtual async Task<Uri?> GetReconnectionUrlAsync()
        {
            return await ApiClient.GetReconnectUriAsync(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Handler for a socket which has reconnected
        /// </summary>
        protected virtual Task HandleReconnectedAsync()
        {
            Status = SocketStatus.Resubscribing;

            lock (_listenersLock)
            {
                foreach (var query in _listeners.OfType<Query>().ToList())
                {
                    query.Fail(new WebError("Connection interupted"));
                    _listeners.Remove(query);
                }
            }

            // Can't wait for this as it would cause a deadlock
            _ = Task.Run(async () =>
            {
                try
                {
                    var reconnectSuccessful = await ProcessReconnectAsync().ConfigureAwait(false);
                    if (!reconnectSuccessful)
                    {
                        _logger.Log(LogLevel.Warning, $"[Sckt {SocketId}] failed reconnect processing: {reconnectSuccessful.Error}, reconnecting again");
                        _ = _socket.ReconnectAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        Status = SocketStatus.Connected;
                        _ = Task.Run(() =>
                        {
                            ConnectionRestored?.Invoke(DateTime.UtcNow - DisconnectTime!.Value);
                            DisconnectTime = null;
                        });
                    }
                }
                catch(Exception ex)
                {
                    _logger.Log(LogLevel.Warning, ex, $"[Sckt {SocketId}] Unknown exception while processing reconnection, reconnecting again");
                    _ = _socket.ReconnectAsync().ConfigureAwait(false);
                }
            });

            return Task.CompletedTask;
        }

        /// <summary>
        /// Handler for an error on a websocket
        /// </summary>
        /// <param name="e">The exception</param>
        protected virtual Task HandleErrorAsync(Exception e)
        {
            if (e is WebSocketException wse)
                _logger.Log(LogLevel.Warning, $"[Sckt {SocketId}] error: Websocket error code {wse.WebSocketErrorCode}, details: " + e.ToLogString());
            else
                _logger.Log(LogLevel.Warning, $"[Sckt {SocketId}] error: " + e.ToLogString());

            return Task.CompletedTask;
        }

        /// <summary>
        /// Handler for whenever a request is sent over the websocket
        /// </summary>
        /// <param name="requestId">Id of the request sent</param>
        protected virtual Task HandleRequestSentAsync(int requestId)
        {
            Query query;
            lock (_listenersLock)
            {
                query = _listeners.OfType<Query>().FirstOrDefault(x => x.Id == requestId);
            }

            if (query == null)
            {
                _logger.Log(LogLevel.Debug, $"[Sckt {SocketId}] msg {requestId} - message sent, but not pending");
                return Task.CompletedTask;
            }

            query.IsSend(ApiClient.ClientOptions.RequestTimeout);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handle a message
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected virtual void HandleStreamMessage(WebSocketMessageType type, ReadOnlyMemory<byte> data)
        {
            var sw = Stopwatch.StartNew();
            var receiveTime = DateTime.UtcNow;
            string? originalData = null;

            // 1. Decrypt/Preprocess if necessary
            data = ApiClient.PreprocessStreamMessage(type, data);

            // 2. Read data into accessor
            _accessor.Read(data);
            try {
                if (ApiClient.ApiOptions.OutputOriginalData ?? ApiClient.ClientOptions.OutputOriginalData)
                {
                    originalData = _accessor.GetOriginalString();
                    _logger.LogTrace("[Sckt {SocketId}] received {Data}", SocketId, originalData);
                }

                // 3. Determine the identifying properties of this message
                var listenId = ApiClient.GetListenerIdentifier(_accessor);
                if (listenId == null)
                {
                    if (!ApiClient.UnhandledMessageExpected)
                        _logger.LogWarning("[Sckt {SocketId}] failed to evaluate message", SocketId);

                    UnhandledMessage?.Invoke(_accessor);
                    return;
                }

                // 4. Get the listeners interested in this message
                List<IMessageProcessor> processors;
                lock (_listenersLock)
                    processors = _listeners.Where(s => s.ListenerIdentifiers.Contains(listenId) && s.CanHandleData).ToList();

                if (!processors.Any())
                {
                    if (!ApiClient.UnhandledMessageExpected)
                    {
                        List<string> listenerIds;
                        lock (_listenersLock)
                            listenerIds = _listeners.SelectMany(l => l.ListenerIdentifiers).ToList();
                        _logger.LogWarning("[Sckt {SocketId}] received message not matched to any listener. ListenId: {ListenId}, current listeners: {ListenIds}", SocketId, listenId, listenerIds);
                        UnhandledMessage?.Invoke(_accessor);
                    }

                    return;
                }

                _logger.LogTrace("[Sckt {SocketId}] {Count} processor(s) matched to message with listener identifier {ListenerId}", SocketId, processors.Count, listenId);
                var totalUserTime = 0;
                Dictionary<Type, object>? desCache = null;
                if (processors.Count > 1)
                {
                    // Only instantiate a cache if there are multiple processors
                    desCache = new Dictionary<Type, object>();
                }

                foreach (var processor in processors)
                {
                    // 5. Determine the type to deserialize to for this processor
                    var messageType = processor.GetMessageType(_accessor);
                    if (messageType == null)
                    {
                        _logger.LogWarning("[Sckt {SocketId}] received message not recognized by handler {Id}", SocketId, processor.Id);
                        continue;
                    }

                    // 6. Deserialize the message
                    object? deserialized = null;
                    desCache?.TryGetValue(messageType, out deserialized);

                    if (deserialized == null)
                    {
                        var desResult = processor.Deserialize(_accessor, messageType);
                        if (!desResult)
                        {
                            _logger.LogWarning("[Sckt {SocketId}] deserialization failed: {Error}", SocketId, desResult.Error);
                            continue;
                        }
                        deserialized = desResult.Data;
                        desCache?.Add(messageType, deserialized);
                    }

                    // 7. Hand of the message to the subscription
                    try
                    {
                        var innerSw = Stopwatch.StartNew();
                        processor.Handle(this, new DataEvent<object>(deserialized, null, originalData, receiveTime, null));
                        totalUserTime += (int)innerSw.ElapsedMilliseconds;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("[Sckt {SocketId}] user message processing failed: {Exception}", SocketId, ex.ToLogString());
                        if (processor is Subscription subscription)
                            subscription.InvokeExceptionHandler(ex);
                    }
                }

                _logger.LogTrace($"[Sckt {SocketId}] message processed in {(int)sw.ElapsedMilliseconds}ms ({sw.ElapsedMilliseconds - totalUserTime}ms parsing)");
            }
            finally
            {
                _accessor.Clear();
            }
        }

        /// <summary>
        /// Connect the websocket
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ConnectAsync() => await _socket.ConnectAsync().ConfigureAwait(false);

        /// <summary>
        /// Retrieve the underlying socket
        /// </summary>
        /// <returns></returns>
        public IWebsocket GetSocket() => _socket;

        /// <summary>
        /// Trigger a reconnect of the socket connection
        /// </summary>
        /// <returns></returns>
        public async Task TriggerReconnectAsync() => await _socket.ReconnectAsync().ConfigureAwait(false);

        /// <summary>
        /// Close the connection
        /// </summary>
        /// <returns></returns>
        public async Task CloseAsync()
        {
            if (Status == SocketStatus.Closed || Status == SocketStatus.Disposed)
                return;

            if (ApiClient.socketConnections.ContainsKey(SocketId))
                ApiClient.socketConnections.TryRemove(SocketId, out _);

            lock (_listenersLock)
            {
                foreach (var subscription in _listeners.OfType<Subscription>())
                {
                    if (subscription.CancellationTokenRegistration.HasValue)
                        subscription.CancellationTokenRegistration.Value.Dispose();
                }
            }

            await _socket.CloseAsync().ConfigureAwait(false);
            _socket.Dispose();
        }

        /// <summary>
        /// Close a subscription on this connection. If all subscriptions on this connection are closed the connection gets closed as well
        /// </summary>
        /// <param name="subscription">Subscription to close</param>
        /// <param name="unsubEvenIfNotConfirmed">Whether to send an unsub request even if the subscription wasn't confirmed</param>
        /// <returns></returns>
        public async Task CloseAsync(Subscription subscription, bool unsubEvenIfNotConfirmed = false)
        {
            subscription.Closed = true;

            if (Status == SocketStatus.Closing || Status == SocketStatus.Closed || Status == SocketStatus.Disposed)
                return;

            _logger.Log(LogLevel.Debug, $"[Sckt {SocketId}] closing subscription {subscription.Id}");
            if (subscription.CancellationTokenRegistration.HasValue)
                subscription.CancellationTokenRegistration.Value.Dispose();

            bool anyDuplicateSubscription;
            lock (_listenersLock)
                anyDuplicateSubscription = _listeners.OfType<Subscription>().Any(x => x != subscription && x.ListenerIdentifiers.All(l => subscription.ListenerIdentifiers.Contains(l)));
            
            if (!anyDuplicateSubscription)
            {
                bool needUnsub;
                lock (_listenersLock)
                    needUnsub = _listeners.Contains(subscription);

                if (needUnsub && (unsubEvenIfNotConfirmed || subscription.Confirmed) && _socket.IsOpen)
                    await UnsubscribeAsync(subscription).ConfigureAwait(false);
            }
            else
            {
                _logger.Log(LogLevel.Debug, $"[Sckt {SocketId}] not unsubscribing subscription as there is still a duplicate subscription running");
            }

            if (Status == SocketStatus.Closing)
            {
                _logger.Log(LogLevel.Debug, $"[Sckt {SocketId}] already closing");
                return;
            }

            bool shouldCloseConnection;
            lock (_listenersLock)
            {
                shouldCloseConnection = _listeners.OfType<Subscription>().All(r => !r.UserSubscription || r.Closed);
                if (shouldCloseConnection)
                    Status = SocketStatus.Closing;
            }

            if (shouldCloseConnection)
            {
                _logger.Log(LogLevel.Debug, $"[Sckt {SocketId}] closing as there are no more subscriptions");
                await CloseAsync().ConfigureAwait(false);
            }

            lock (_listenersLock)
                _listeners.Remove(subscription);
        }

        /// <summary>
        /// Dispose the connection
        /// </summary>
        public void Dispose()
        {
            Status = SocketStatus.Disposed;
            periodicEvent?.Set();
            periodicEvent?.Dispose();
            _socket.Dispose();
        }

        /// <summary>
        /// Whether or not a new subscription can be added to this connection
        /// </summary>
        /// <returns></returns>
        public bool CanAddSubscription() => Status == SocketStatus.None || Status == SocketStatus.Connected;

        /// <summary>
        /// Add a subscription to this connection
        /// </summary>
        /// <param name="subscription"></param>
        public bool AddSubscription(Subscription subscription)
        {
            if (Status != SocketStatus.None && Status != SocketStatus.Connected)
                return false;

            lock (_listenersLock)
                _listeners.Add(subscription);

            if (subscription.UserSubscription)
                _logger.Log(LogLevel.Debug, $"[Sckt {SocketId}] adding new subscription with id {subscription.Id}, total subscriptions on connection: {UserSubscriptionCount}");
            return true;
        }

        /// <summary>
        /// Get a subscription on this connection by id
        /// </summary>
        /// <param name="id"></param>
        public Subscription? GetSubscription(int id)
        {
            lock (_listenersLock)
                return _listeners.OfType<Subscription>().SingleOrDefault(s => s.Id == id);
        }

        /// <summary>
        /// Send a query request and wait for an answer
        /// </summary>
        /// <param name="query">Query to send</param>
        /// <param name="continueEvent">Wait event for when the socket message handler can continue</param>
        /// <returns></returns>
        public virtual async Task<CallResult> SendAndWaitQueryAsync(Query query, ManualResetEvent? continueEvent = null)
        {
            await SendAndWaitIntAsync(query, continueEvent).ConfigureAwait(false);
            return query.Result ?? new CallResult(new ServerError("Timeout"));
        }

        /// <summary>
        /// Send a query request and wait for an answer
        /// </summary>
        /// <typeparam name="T">Query response type</typeparam>
        /// <param name="query">Query to send</param>
        /// <param name="continueEvent">Wait event for when the socket message handler can continue</param>
        /// <returns></returns>
        public virtual async Task<CallResult<T>> SendAndWaitQueryAsync<T>(Query<T> query, ManualResetEvent? continueEvent = null)
        {
            await SendAndWaitIntAsync(query, continueEvent).ConfigureAwait(false);
            return query.TypedResult ?? new CallResult<T>(new ServerError("Timeout"));
        }

        private async Task SendAndWaitIntAsync(Query query, ManualResetEvent? continueEvent)
        {
            lock(_listenersLock)
                _listeners.Add(query);

            query.ContinueAwaiter = continueEvent;
            var sendResult = Send(query.Id, query.Request, query.Weight);
            if (!sendResult)
            {
                query.Fail(sendResult.Error!);
                lock (_listenersLock)
                    _listeners.Remove(query);
                return;
            }

            try
            {
                while (true)
                {
                    if (!_socket.IsOpen)
                    {
                        query.Fail(new WebError("Socket not open"));
                        return;
                    }

                    if (query.Completed)
                        return;

                    await query.WaitAsync(TimeSpan.FromMilliseconds(500)).ConfigureAwait(false);

                    if (query.Completed)
                        return;
                }
            }
            finally
            {
                lock (_listenersLock)
                    _listeners.Remove(query);
            }
        }

        /// <summary>
        /// Send data over the websocket connection
        /// </summary>
        /// <typeparam name="T">The type of the object to send</typeparam>
        /// <param name="requestId">The request id</param>
        /// <param name="obj">The object to send</param>
        /// <param name="weight">The weight of the message</param>
        public virtual CallResult Send<T>(int requestId, T obj, int weight)
        {
            var data = obj is string str ? str : _serializer.Serialize(obj!);
            return Send(requestId, data, weight);
        }

        /// <summary>
        /// Send string data over the websocket connection
        /// </summary>
        /// <param name="data">The data to send</param>
        /// <param name="weight">The weight of the message</param>
        /// <param name="requestId">The id of the request</param>
        public virtual CallResult Send(int requestId, string data, int weight)
        {
            if (ApiClient.MessageSendSizeLimit != null && data.Length > ApiClient.MessageSendSizeLimit.Value)
            {
                var info = $"Message to send exceeds the max server message size ({ApiClient.MessageSendSizeLimit.Value} bytes). Split the request into batches to keep below this limit";
                _logger.LogWarning("[Sckt {SocketId}] msg {RequestId} - {Info}", SocketId, requestId, info);
                return new CallResult(new InvalidOperationError(info));
            }

            _logger.Log(LogLevel.Trace, $"[Sckt {SocketId}] msg {requestId} - sending messsage: {data}");
            try
            {
                _socket.Send(requestId, data, weight);
                return new CallResult(null);
            }
            catch(Exception ex)
            {
                return new CallResult(new WebError("Failed to send message: " + ex.Message));
            }
        }

        private async Task<CallResult> ProcessReconnectAsync()
        {
            if (!_socket.IsOpen)
                return new CallResult<bool>(new WebError("Socket not connected"));

            bool anySubscriptions;
            lock (_listenersLock)
                anySubscriptions = _listeners.OfType<Subscription>().Any(s => s.UserSubscription);
            if (!anySubscriptions)
            {
                // No need to resubscribe anything
                _logger.Log(LogLevel.Debug, $"[Sckt {SocketId}] nothing to resubscribe, closing connection");
                _ = _socket.CloseAsync();
                return new CallResult<bool>(true);
            }

            bool anyAuthenticated;
            lock (_listenersLock)
                anyAuthenticated = _listeners.OfType<Subscription>().Any(s => s.Authenticated);
            if (anyAuthenticated)
            {
                // If we reconnected a authenticated connection we need to re-authenticate
                var authResult = await ApiClient.AuthenticateSocketAsync(this).ConfigureAwait(false);
                if (!authResult)
                {
                    _logger.Log(LogLevel.Warning, $"[Sckt {SocketId}] authentication failed on reconnected socket. Disconnecting and reconnecting.");
                    return authResult;
                }

                Authenticated = true;
                _logger.Log(LogLevel.Debug, $"[Sckt {SocketId}] authentication succeeded on reconnected socket.");
            }

            // Get a list of all subscriptions on the socket
            List<Subscription> subList;
            lock (_listenersLock)
                subList = _listeners.OfType<Subscription>().ToList();

            foreach(var subscription in subList)
            {
                subscription.ConnectionInvocations = 0;
                var result = await ApiClient.RevitalizeRequestAsync(subscription).ConfigureAwait(false);
                if (!result)
                {
                    _logger.Log(LogLevel.Warning, $"[Sckt {SocketId}] failed request revitalization: " + result.Error);
                    return result;
                }
            }

            // Foreach subscription which is subscribed by a subscription request we will need to resend that request to resubscribe
            for (var i = 0; i < subList.Count; i += ApiClient.ClientOptions.MaxConcurrentResubscriptionsPerSocket)
            {
                if (!_socket.IsOpen)
                    return new CallResult<bool>(new WebError("Socket not connected"));

                var taskList = new List<Task<CallResult>>();
                foreach (var subscription in subList.Skip(i).Take(ApiClient.ClientOptions.MaxConcurrentResubscriptionsPerSocket))
                {
                    var subQuery = subscription.GetSubQuery(this);
                    if (subQuery == null)
                        continue;

                    var waitEvent = new ManualResetEvent(false);
                    taskList.Add(SendAndWaitQueryAsync(subQuery, waitEvent).ContinueWith((r) => 
                    { 
                        subscription.HandleSubQueryResponse(subQuery.Response!);
                        waitEvent.Set();
                        return r.Result;
                    }));
                }

                await Task.WhenAll(taskList).ConfigureAwait(false);
                if (taskList.Any(t => !t.Result.Success))
                    return taskList.First(t => !t.Result.Success).Result;
            }

            foreach (var subscription in subList)
                subscription.Confirmed = true;

            if (!_socket.IsOpen)
                return new CallResult<bool>(new WebError("Socket not connected"));

            _logger.Log(LogLevel.Debug, $"[Sckt {SocketId}] all subscription successfully resubscribed on reconnected socket.");
            return new CallResult<bool>(true);
        }

        internal async Task UnsubscribeAsync(Subscription subscription)
        {
            var unsubscribeRequest = subscription.GetUnsubQuery();
            if (unsubscribeRequest == null)
                return;

            await SendAndWaitQueryAsync(unsubscribeRequest).ConfigureAwait(false);
            _logger.Log(LogLevel.Information, $"[Sckt {SocketId}] subscription {subscription!.Id} unsubscribed");
        }

        internal async Task<CallResult> ResubscribeAsync(Subscription subscription)
        {
            if (!_socket.IsOpen)
                return new CallResult(new UnknownError("Socket is not connected"));

            var subQuery = subscription.GetSubQuery(this);
            if (subQuery == null)
                return new CallResult(null);

            var result = await SendAndWaitQueryAsync(subQuery).ConfigureAwait(false);
            subscription.HandleSubQueryResponse(subQuery.Response!);
            return result;
        }

        /// <summary>
        /// Periodically sends data over a socket connection
        /// </summary>
        /// <param name="identifier">Identifier for the periodic send</param>
        /// <param name="interval">How often</param>
        /// <param name="queryDelegate">Method returning the query to send</param>
        /// <param name="callback">The callback for processing the response</param>
        public virtual void QueryPeriodic(string identifier, TimeSpan interval, Func<SocketConnection, Query> queryDelegate, Action<CallResult>? callback)
        {
            if (queryDelegate == null)
                throw new ArgumentNullException(nameof(queryDelegate));

            periodicEvent = new AsyncResetEvent();
            periodicTask = Task.Run(async () =>
            {
                while (Status != SocketStatus.Disposed
                    && Status != SocketStatus.Closed
                    && Status != SocketStatus.Closing)
                {
                    await periodicEvent.WaitAsync(interval).ConfigureAwait(false);
                    if (Status == SocketStatus.Disposed
                    || Status == SocketStatus.Closed
                    || Status == SocketStatus.Closing)
                    {
                        break;
                    }

                    if (!Connected)
                        continue;

                    var query = queryDelegate(this);
                    if (query == null)
                        continue;

                    _logger.Log(LogLevel.Trace, $"[Sckt {SocketId}] sending periodic {identifier}");

                    try
                    {
                        var result = await SendAndWaitQueryAsync(query).ConfigureAwait(false);
                        callback?.Invoke(result);
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(LogLevel.Warning, $"[Sckt {SocketId}] Periodic send {identifier} failed: " + ex.ToLogString());
                    }
                }
            });
        }

        /// <summary>
        /// Status of the socket connection
        /// </summary>
        public enum SocketStatus
        {
            /// <summary>
            /// None/Initial
            /// </summary>
            None,
            /// <summary>
            /// Connected
            /// </summary>
            Connected,
            /// <summary>
            /// Reconnecting
            /// </summary>
            Reconnecting,
            /// <summary>
            /// Resubscribing on reconnected socket
            /// </summary>
            Resubscribing,
            /// <summary>
            /// Closing
            /// </summary>
            Closing,
            /// <summary>
            /// Closed
            /// </summary>
            Closed,
            /// <summary>
            /// Disposed
            /// </summary>
            Disposed
        }
    }
}

