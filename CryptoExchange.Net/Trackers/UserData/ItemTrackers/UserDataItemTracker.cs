using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.UserData.Interfaces;
using CryptoExchange.Net.Trackers.UserData.Objects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.UserData.ItemTrackers
{
    /// <summary>
    /// User data tracker
    /// </summary>
    public abstract class UserDataItemTracker
    {
        private bool _connected;

        /// <summary>
        /// Logger
        /// </summary>
        protected ILogger _logger;
        /// <summary>
        /// Polling wait event
        /// </summary>
        protected AsyncResetEvent _pollWaitEvent = new AsyncResetEvent(false, true);
        /// <summary>
        /// Initial polling done event
        /// </summary>
        protected AsyncResetEvent _initialPollDoneEvent = new AsyncResetEvent(false, false);
        /// <summary>
        /// The error from the initial polling;
        /// </summary>
        protected Error? _initialPollingError;
        /// <summary>
        /// Polling task
        /// </summary>
        protected Task? _pollTask;
        /// <summary>
        /// Cancellation token
        /// </summary>
        protected CancellationTokenSource? _cts;
        /// <summary>
        /// Websocket subscription
        /// </summary>
        protected UpdateSubscription? _subscription;
        /// <summary>
        /// Start time
        /// </summary>
        protected DateTime? _startTime = null;
        /// <summary>
        /// Last polling attempt
        /// </summary>
        protected DateTime? _lastPollAttempt;
        /// <summary>
        /// Last polling timestamp
        /// </summary>
        protected DateTime? _lastPollTime;
        /// <summary>
        /// Timestamp of last message received before websocket disconnecting
        /// </summary>
        protected DateTime? _lastDataTimeBeforeDisconnect;
        /// <summary>
        /// Whether last polling was successful
        /// </summary>
        protected bool _lastPollSuccess;
        /// <summary>
        /// Whether first polling was done
        /// </summary>
        protected bool _firstPollDone;
        /// <summary>
        /// Whether websocket was disconnected before a polling
        /// </summary>
        protected bool _wasDisconnected;
        /// <summary>
        /// Poll at the start
        /// </summary>
        protected bool _pollAtStart;
        /// <summary>
        /// Poll interval when connected
        /// </summary>
        protected TimeSpan _pollIntervalConnected;
        /// <summary>
        /// Poll interval when disconnected
        /// </summary>
        protected TimeSpan _pollIntervalDisconnected;
        /// <summary>
        /// Exchange name
        /// </summary>
        protected string _exchange;
        /// <summary>
        /// Time completed data is retained
        /// </summary>
        public TimeSpan _retentionTime;

        /// <summary>
        /// Data type
        /// </summary>
        public UserDataType DataType { get; }

        /// <summary>
        /// Timestamp an update was handled. Does not necessarily mean the data was changed
        /// </summary>
        public DateTime? LastUpdateTime { get; protected set; }
        /// <summary>
        /// Timestamp any change was applied to the data
        /// </summary>
        public DateTime? LastChangeTime { get; protected set; }

        /// <summary>
        /// Connection status changed
        /// </summary>
        public event Action<bool>? OnConnectedChange;

        /// <summary>
        /// ctor
        /// </summary>
        public UserDataItemTracker(ILogger logger, UserDataType dataType, string exchange)
        {
            _logger = logger;
            _exchange = exchange;

            DataType = dataType;
        }

        /// <summary>
        /// Start the tracker
        /// </summary>
        /// <param name="listenKey">Optional listen key</param>
        public abstract Task<CallResult> StartAsync(string? listenKey);

        /// <summary>
        /// Stop the tracker
        /// </summary>
        /// <returns></returns>
        public async Task StopAsync()
        {
            _cts?.Cancel();

            if (_pollTask != null)
                await _pollTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Get the delay until next poll
        /// </summary>
        /// <returns></returns>
        protected TimeSpan? GetNextPollDelay()
        {
            if (!_firstPollDone && _pollAtStart)
                // First polling should be done immediately
                return TimeSpan.Zero;

            if (!Connected)
            {
                if (_pollIntervalDisconnected == TimeSpan.Zero)
                    // No polling interval
                    return null;

                return _pollIntervalDisconnected;
            }

            if (_pollIntervalConnected == TimeSpan.Zero)
                // No polling interval
                return null;

            // Wait for next poll
            return _pollIntervalConnected;
        }


        /// <inheritdoc />
        public bool Connected
        {
            get => _connected;
            protected set
            {
                if (_connected == value)
                    return;

                _connected = value;
                if (!_connected)
                    _wasDisconnected = true;
                else
                    _pollWaitEvent.Set();

                OnConnectedChange?.Invoke(_connected);
            }
        }
    }

    /// <summary>
    /// User data tracker
    /// </summary>
    public abstract class UserDataItemTracker<T> : UserDataItemTracker, IUserDataTracker<T>
    {
        /// <summary>
        /// Data store
        /// </summary>
        protected ConcurrentDictionary<string, T> _store = new ConcurrentDictionary<string, T>(StringComparer.InvariantCultureIgnoreCase);
        /// <summary>
        /// Tracked symbols list
        /// </summary>
        protected readonly List<SharedSymbol> _symbols;
        /// <summary>
        /// Symbol lock
        /// </summary>
        protected object _symbolLock = new object();
        /// <summary>
        /// Only track provided symbols setting
        /// </summary>
        protected bool _onlyTrackProvidedSymbols;
        /// <summary>
        /// Is SharedSymbol model
        /// </summary>
        protected bool _isSymbolModel;

        /// <inheritdoc />
        public T[] Values
        {
            get
            {
                if (_retentionTime != TimeSpan.MaxValue)
                {
                    var timestamp = DateTime.UtcNow;
                    foreach (var value in _store.Values)
                    {
                        if (GetAge(timestamp, value) > _retentionTime)
                            _store.TryRemove(GetKey(value), out _);
                    }
                }

                return _store.Values.ToArray();
            }
        }

        /// <inheritdoc />
        public event Func<UserDataUpdate<T[]>, Task>? OnUpdate;
        /// <inheritdoc />
        public IEnumerable<SharedSymbol> TrackedSymbols => _symbols;

        /// <summary>
        /// ctor
        /// </summary>
        public UserDataItemTracker(ILogger logger, UserDataType dataType, string exchange, TrackerItemConfig config, bool onlyTrackProvidedSymbols, IEnumerable<SharedSymbol>? symbols) : base(logger, dataType, exchange)
        {
            _onlyTrackProvidedSymbols = onlyTrackProvidedSymbols;
            _symbols = symbols?.ToList() ?? [];

            _pollIntervalDisconnected = config.PollIntervalDisconnected;
            _pollIntervalConnected = config.PollIntervalConnected;
            _pollAtStart = config.PollAtStart;
            _retentionTime = config is TrackerTimedItemConfig timeConfig ? timeConfig.RetentionTime : TimeSpan.MaxValue;
            _isSymbolModel = typeof(T).IsSubclassOf(typeof(SharedSymbolModel));
        }

        /// <summary>
        /// Invoke OnUpdate event
        /// </summary>
        protected async Task InvokeUpdate(UserDataUpdate<T[]> data)
        {
            if (OnUpdate == null)
                return;

            await OnUpdate(data).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async override Task<CallResult> StartAsync(string? listenKey)
        {
            _startTime = DateTime.UtcNow;
            _cts = new CancellationTokenSource();

            var start = await SubscribeAsync(listenKey).ConfigureAwait(false);
            if (!start)
                return start;

            Connected = true;

            _pollTask = PollAsync();

            await _initialPollDoneEvent.WaitAsync().ConfigureAwait(false);
            if (_initialPollingError != null)
            {
                await StopAsync().ConfigureAwait(false);
                return new CallResult(_initialPollingError);
            }

            return CallResult.SuccessResult;
        }

        /// <summary>
        /// Subscribe the websocket
        /// </summary>
        public async Task<CallResult> SubscribeAsync(string? listenKey)
        {
            var subscriptionResult = await DoSubscribeAsync(listenKey).ConfigureAwait(false);
            if (!subscriptionResult)
            {
                // Failed
                // ..
                return subscriptionResult;
            }

            if (subscriptionResult.Data == null)
            {
                // No subscription available
                // ..
                return CallResult.SuccessResult;
            }

            _subscription = subscriptionResult.Data;
            _subscription.SubscriptionStatusChanged += SubscriptionStatusChanged;
            return CallResult.SuccessResult;
        }

        /// <summary>
        /// Get the unique identifier for the item
        /// </summary>
        protected abstract string GetKey(T item);
        /// <summary>
        /// Check whether an update should be applied
        /// </summary>
        protected abstract bool? CheckIfUpdateShouldBeApplied(T existingItem, T updateItem);
        /// <summary>
        /// Update an existing item with an update
        /// </summary>
        protected abstract bool Update(T existingItem, T updateItem);
        /// <summary>
        /// Get the age of an item
        /// </summary>
        protected virtual TimeSpan GetAge(DateTime time, T item) => TimeSpan.Zero;

        /// <summary>
        /// Update the tracked symbol list with potential new symbols
        /// </summary>
        /// <param name="symbols"></param>
        protected void UpdateSymbolsList(IEnumerable<SharedSymbol> symbols)
        {
            lock (_symbolLock)
            {
                foreach (var symbol in symbols.Distinct())
                {
                    if (!_symbols.Any(x => x.TradingMode == symbol.TradingMode && x.BaseAsset == symbol.BaseAsset && x.QuoteAsset == symbol.QuoteAsset))
                    {
                        _symbols.Add(symbol);
                        _logger.LogDebug("Adding {BaseAsset}/{QuoteAsset} to symbol tracking list", symbol.BaseAsset, symbol.QuoteAsset);
                    }
                }
            }
        }

        /// <summary>
        /// Handle an update
        /// </summary>
        protected internal virtual async Task HandleUpdateAsync(UpdateSource source, T[] @event)
        {
            LastUpdateTime = DateTime.UtcNow;

            if (_isSymbolModel)
            {
                List<T>? toRemove = null;
                foreach (var item in @event)
                {
                    if (item is SharedSymbolModel symbolModel)
                    {
                        if (symbolModel.SharedSymbol == null)
                        {
                            toRemove ??= new List<T>();
                            toRemove.Add(item);
                        }
                        else if (_onlyTrackProvidedSymbols
                            && !_symbols.Any(y => y.TradingMode == symbolModel.SharedSymbol!.TradingMode && y.BaseAsset == symbolModel.SharedSymbol.BaseAsset && y.QuoteAsset == symbolModel.SharedSymbol.QuoteAsset))
                        {
                            toRemove ??= new List<T>();
                            toRemove.Add(item);
                        }
                    }
                }

                if (toRemove != null)
                    @event = @event.Except(toRemove).ToArray();

                if (!_onlyTrackProvidedSymbols)
                    UpdateSymbolsList(@event.OfType<SharedSymbolModel>().Select(x => x.SharedSymbol!));
            }

            // Update local store
            var updatedItems = @event.Select(GetKey).ToList();

            foreach (var item in @event)
            {
                bool existed = false;
                _store.AddOrUpdate(GetKey(item), item, (key, existing) =>
                {
                    existed = true;
                    if (CheckIfUpdateShouldBeApplied(existing, item) == false)
                    {
                        updatedItems.Remove(key);
                    }
                    else
                    {
                        var updated = Update(existing, item);
                        if (!updated)
                        {
                            updatedItems.Remove(key);
                        }
                        else
                        {
                            _logger.LogTrace("Updated {DataType} {Item}", DataType, key);
                            LastChangeTime = DateTime.UtcNow;
                        }
                    }

                    return existing;
                });

                if (!existed)
                {
                    _logger.LogTrace("Added {DataType} {Item}", DataType, GetKey(item));
                    LastChangeTime = DateTime.UtcNow;
                }
            }

            if (updatedItems.Count > 0 && OnUpdate != null)
            {
                await OnUpdate.Invoke(
                    new UserDataUpdate<T[]>(source, _exchange, _store.Where(x => updatedItems.Contains(x.Key)).Select(x => x.Value).ToArray())).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Websocket subscription implementation
        /// </summary>
        protected abstract Task<CallResult<UpdateSubscription?>> DoSubscribeAsync(string? listenKey);

        /// <summary>
        /// Polling task
        /// </summary>
        protected async Task PollAsync()
        {
            while (!_cts!.IsCancellationRequested)
            {
                var delayForNextPoll = GetNextPollDelay();
                if (delayForNextPoll != TimeSpan.Zero)
                {
                    try
                    {
                        if (delayForNextPoll != null)
                            _logger.LogTrace("{DataType} delay for next polling: {Delay}", DataType, delayForNextPoll);

                        await _pollWaitEvent.WaitAsync(delayForNextPoll, _cts.Token).ConfigureAwait(false);
                    }
                    catch { }
                }

                var currentlyFirstPoll = !_firstPollDone;
                _firstPollDone = true;
                if (_cts.IsCancellationRequested)
                    break;

                if (_lastPollAttempt != null
                    && (DateTime.UtcNow - _lastPollAttempt.Value) < TimeSpan.FromSeconds(2)
                    && !(Connected && _wasDisconnected))
                {
                    if (_lastPollSuccess)
                        // If last poll was less than 2 seconds ago and it was successful don't bother immediately polling again
                        continue;
                }

                if (Connected)
                    _wasDisconnected = false;

                _lastPollSuccess = false;

                try
                {
                    var anyError = await DoPollAsync().ConfigureAwait(false);

                    _initialPollDoneEvent.Set();
                    _lastPollAttempt = DateTime.UtcNow;
                    _lastPollSuccess = !anyError;

                    if (anyError && currentlyFirstPoll && _pollAtStart)
                    {
                        if (_initialPollingError == null)
                            throw new Exception("Error in initial polling but error not set");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{DataType} UserDataTracker polling exception", DataType);
                }
            }
        }

        /// <summary>
        /// Polling implementation
        /// </summary>
        /// <returns></returns>
        protected abstract Task<bool> DoPollAsync();

        /// <summary>
        /// Handle subscription status change
        /// </summary>
        /// <param name="newState"></param>
        private void SubscriptionStatusChanged(SubscriptionStatus newState)
        {
            _logger.LogDebug("{DataType} stream status changed: {NewState}", DataType, newState);

            if (newState == SubscriptionStatus.Pending)
            {
                // Record last data receive time since we need to request data from that timestamp on when polling
                // Only set to new value if it isn't already set since if we disconnect/reconnect a couple of times without
                // managing to do a poll we don't want to override the time since we still need to request that earlier data

                if (_lastDataTimeBeforeDisconnect == null)
                {
                    _lastDataTimeBeforeDisconnect = _subscription!.LastReceiveTime;

                    // When changing to pending (disconnected) trigger polling to start checking
                    _pollWaitEvent.Set();
                }
            }

            Connected = newState == SubscriptionStatus.Subscribed;
        }
    }
}
