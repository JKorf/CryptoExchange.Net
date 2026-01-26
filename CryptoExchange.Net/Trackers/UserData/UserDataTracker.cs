using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.UserData
{
    /// <summary>
    /// User data tracker
    /// </summary>
    public abstract class UserDataTracker : IUserDataTracker
    {
        // Cached data
        private ConcurrentDictionary<string, SharedBalance> _balanceStore = new ConcurrentDictionary<string, SharedBalance>();
        private ConcurrentDictionary<string, SharedSpotOrder> _orderStore = new ConcurrentDictionary<string, SharedSpotOrder>();
        private ConcurrentDictionary<string, SharedUserTrade> _tradeStore = new ConcurrentDictionary<string, SharedUserTrade>();

        // Typed clients
        private readonly IListenKeyRestClient? _listenKeyRestClient;
        private readonly ISpotSymbolRestClient _spotSymbolRestClient;
        private readonly IBalanceRestClient _balanceRestClient;
        private readonly IBalanceSocketClient _balanceSocketClient;
        private readonly ISpotOrderRestClient _spotOrderRestClient;
        private readonly ISpotOrderSocketClient _spotOrderSocketClient;
        private readonly IUserTradeSocketClient? _userTradeSocketClient;
        private readonly ILogger _logger;

        // State management
        private DateTime? _startTime = null;
        private DateTime? _lastPollAttempt = null;
        private bool _lastPollSuccessful = false;
        private DateTime? _lastPollTimeOrders = null;
        private DateTime? _lastPollTimeTrades = null;
        private DateTime? _lastDataTimeOrdersBeforeDisconnect = null;
        private DateTime? _lastDataTimeTradesBeforeDisconnect = null;
        private bool _firstPollDone = false;

        // Config
        private List<SharedSymbol> _symbols = new List<SharedSymbol>();
        private TimeSpan _pollIntervalConnected;
        private TimeSpan _pollIntervalDisconnected;
        private bool _pollAtStart;
        private bool _onlyTrackProvidedSymbols;
        private bool _trackTrades = true;

        // Subscriptions
        private UpdateSubscription? _balanceSubscription;
        private UpdateSubscription? _orderSubscription;
        private UpdateSubscription? _tradeSubscription;


        private AsyncResetEvent _pollWaitEvent = new AsyncResetEvent(false, true);
        private Task? _pollTask;
        private CancellationTokenSource? _cts;
        private object _symbolLock = new object();

        private bool Connected =>
            _balanceSubscription?.Status == Sockets.Default.SocketStatus.Connected
            && _orderSubscription?.Status == Sockets.Default.SocketStatus.Connected
            && _tradeSubscription == null || _tradeSubscription?.Status == Sockets.Default.SocketStatus.Connected;

        /// <inheritdoc />
        public event Action<UserDataUpdate<SharedBalance[]>>? OnBalanceUpdate;
        /// <inheritdoc />
        public event Action<UserDataUpdate<SharedSpotOrder[]>>? OnOrderUpdate;
        /// <inheritdoc />
        public event Action<UserDataUpdate<SharedUserTrade[]>>? OnTradeUpdate;

        /// <inheritdoc />
        public string? UserIdentifier { get; }
        /// <inheritdoc />
        public SharedBalance[] Balances => _balanceStore.Values.ToArray();
        /// <inheritdoc />
        public SharedSpotOrder[] Orders => _orderStore.Values.ToArray();
        /// <inheritdoc />
        public SharedUserTrade[] Trades => _tradeStore.Values.ToArray();

        /// <summary>
        /// ctor
        /// </summary>
        protected UserDataTracker(
            ILogger logger,
            ISharedClient restClient,
            ISharedClient socketClient,
            string? userIdentifier,
            UserDataTrackerConfig config
            )
        {
            _logger = logger;
            _spotSymbolRestClient = (ISpotSymbolRestClient)restClient;
            _balanceRestClient = (IBalanceRestClient)restClient;
            _balanceSocketClient = (IBalanceSocketClient)socketClient;
            _spotOrderRestClient = (ISpotOrderRestClient)restClient;
            _spotOrderSocketClient = (ISpotOrderSocketClient)socketClient;
            _listenKeyRestClient = restClient as IListenKeyRestClient;
            _userTradeSocketClient = socketClient as IUserTradeSocketClient;

            _pollIntervalConnected = config.PollIntervalConnected;
            _pollIntervalDisconnected = config.PollIntervalDisconnected;
            _symbols = config.Symbols?.ToList() ?? [];
            _onlyTrackProvidedSymbols = config.OnlyTrackProvidedSymbols;
            _pollAtStart = config.PollAtStart;
            _trackTrades = config.TrackTrades;

            UserIdentifier = userIdentifier;
        }

        /// <inheritdoc />
        public async Task<CallResult> StartAsync()
        {
            _startTime = DateTime.UtcNow;

            _logger.LogDebug("Starting UserDataTracker");
            _cts = new CancellationTokenSource();

            // Request symbols so SharedSymbol property can be filled on updates
            var symbolResult = await _spotSymbolRestClient.GetSpotSymbolsAsync(new GetSymbolsRequest()).ConfigureAwait(false);
            if (!symbolResult)
            {
                _logger.LogWarning("Failed to start UserDataTracker; symbols request failed: {Error}", symbolResult.Error!.Message);
                return symbolResult;
            }

            string? listenKey = null;
            if (_listenKeyRestClient != null)
            {
                var lkResult = await _listenKeyRestClient.StartListenKeyAsync(new StartListenKeyRequest()).ConfigureAwait(false);
                if (!lkResult)
                {
                    _logger.LogWarning("Failed to start UserDataTracker; listen key request failed: {Error}", lkResult.Error!.Message);
                    return lkResult;
                }

                listenKey = lkResult.Data;
            }

            var subBalanceResult = await _balanceSocketClient.SubscribeToBalanceUpdatesAsync(new SubscribeBalancesRequest(listenKey), x => HandleBalanceUpdate(UpdateSource.Push, x.Data), ct: _cts.Token).ConfigureAwait(false);
            if (!subBalanceResult)
            {
                _logger.LogWarning("Failed to start UserDataTracker; failed to subscribe to balance stream: {Error}", subBalanceResult.Error!.Message);
                return subBalanceResult;
            }

            _balanceSubscription = subBalanceResult.Data;
            subBalanceResult.Data.SubscriptionStatusChanged += BalanceSubscriptionStatusChanged;

            var subOrderResult = await _spotOrderSocketClient.SubscribeToSpotOrderUpdatesAsync(new SubscribeSpotOrderRequest(listenKey), x => HandleOrderUpdate(UpdateSource.Push, x.Data), ct: _cts.Token).ConfigureAwait(false);
            if (!subOrderResult)
            {
                _cts.Cancel();
                _logger.LogWarning("Failed to start UserDataTracker; failed to subscribe to order stream: {Error}", subOrderResult.Error!.Message);
                return subOrderResult;
            }

            _orderSubscription = subOrderResult.Data;
            subOrderResult.Data.SubscriptionStatusChanged += OrderSubscriptionStatusChanged;

            if (_userTradeSocketClient != null && _trackTrades)
            {
                var subTradeResult = await _userTradeSocketClient.SubscribeToUserTradeUpdatesAsync(new SubscribeUserTradeRequest(listenKey), x => HandleTradeUpdate(UpdateSource.Push, x.Data), ct: _cts.Token).ConfigureAwait(false);
                if (!subOrderResult)
                {
                    _cts.Cancel();
                    _logger.LogWarning("Failed to start UserDataTracker; failed to subscribe to trade stream: {Error}", subTradeResult.Error!.Message);
                    return subOrderResult;
                }

                _tradeSubscription = subTradeResult.Data;
                subTradeResult.Data.SubscriptionStatusChanged += TradeSubscriptionStatusChanged;
            }

            _pollTask = PollAsync();
            _logger.LogDebug("Started UserDataTracker");
            return CallResult.SuccessResult;
        }

        private void UpdateSymbolsList(IEnumerable<SharedSymbol> symbols)
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

        private void HandleTradeUpdate(UpdateSource source, SharedUserTrade[] @event)
        {
            var unknownSymbols = @event.Where(x => x.SharedSymbol == null);
            if (unknownSymbols.Any())
            {
                _logger.LogWarning("Received order without SharedSymbol set, ignoring");
                @event = @event.Except(unknownSymbols).ToArray();
            }

            if (!_onlyTrackProvidedSymbols)
                UpdateSymbolsList(@event.Select(x => x.SharedSymbol!));
            else
                @event = @event.Where(x => _symbols.Any(y => y.TradingMode == x.SharedSymbol!.TradingMode && y.BaseAsset == x.SharedSymbol.BaseAsset && y.QuoteAsset == x.SharedSymbol.QuoteAsset)).ToArray();

            // Update local store
            var updatedIds = @event.Select(x => x.Id).ToList();
            foreach (var item in @event)
            {
                if (_tradeStore.TryAdd(item.Id, item))
                    _logger.LogDebug("Added user trade {Symbol}.{Id}", item.Symbol, item.Id);
                else
                    updatedIds.Remove(item.Id);
            }

            if (updatedIds.Count > 0)
            {
                OnTradeUpdate?.Invoke(
                    new UserDataUpdate<SharedUserTrade[]>
                    {
                        Source = source,
                        Data = _tradeStore.Where(x => updatedIds.Contains(x.Key)).Select(x => x.Value).ToArray()
                    });
            }
        }

        private void HandleOrderUpdate(UpdateSource source, SharedSpotOrder[] @event)
        {
            var unknownSymbols = @event.Where(x => x.SharedSymbol == null);
            if (unknownSymbols.Any())
            {
                _logger.LogWarning("Received order without SharedSymbol set, ignoring");
                @event = @event.Except(unknownSymbols).ToArray();
            }

            if (!_onlyTrackProvidedSymbols)
                UpdateSymbolsList(@event.Select(x => x.SharedSymbol!));
            else
                @event = @event.Where(x => _symbols.Any(y => y.TradingMode == x.SharedSymbol!.TradingMode && y.BaseAsset == x.SharedSymbol.BaseAsset && y.QuoteAsset == x.SharedSymbol.QuoteAsset)).ToArray();

            // Update local store
            var updatedIds = @event.Select(x => x.OrderId).ToList();

            foreach (var item in @event)
            {
                bool orderExisted = false;
                _orderStore.AddOrUpdate(item.OrderId, item, (id, existing) =>
                {
                    orderExisted = true;
                    var updated = UpdateSpotOrder(existing, item);
                    if (!updated)
                        updatedIds.Remove(id);
                    else
                        _logger.LogDebug("Updated spot order {Symbol}.{Id}", item.Symbol, item.OrderId);

                    return existing;
                });

                if (!orderExisted)
                    _logger.LogDebug("Added spot order {Symbol}.{Id}", item.Symbol, item.OrderId);
            }

            if (updatedIds.Count > 0)
            {
                OnOrderUpdate?.Invoke(
                    new UserDataUpdate<SharedSpotOrder[]>
                    {
                        Source = source,
                        Data = _orderStore.Where(x => updatedIds.Contains(x.Key)).Select(x => x.Value).ToArray()
                    });
            }

            var trades = @event.Where(x => x.LastTrade != null).Select(x => x.LastTrade!).ToArray();
            if (trades.Length != 0)
                HandleTradeUpdate(source, trades);
        }

        private void HandleBalanceUpdate(UpdateSource source, SharedBalance[] @event)
        {
            // Update local store
            var updatedAssets = @event.Select(x => x.Asset).ToList();

            foreach (var item in @event)
            {
                bool balanceExisted = false;
                _balanceStore.AddOrUpdate(item.Asset, item, (asset, existing) =>
                {
                    balanceExisted = true;
                    var updated = UpdateBalance(existing, item);
                    if (!updated)
                        updatedAssets.Remove(asset);
                    else
                        _logger.LogDebug("Updated balance for {Asset}", item.Asset);

                    return existing;
                });

                if (!balanceExisted)
                    _logger.LogDebug("Added balance for {Asset}", item.Asset);
            }

            if (updatedAssets.Count > 0)
            {
                OnBalanceUpdate?.Invoke(
                new UserDataUpdate<SharedBalance[]>
                {
                    Source = source,
                    Data = _balanceStore.Where(x => updatedAssets.Contains(x.Key)).Select(x => x.Value).ToArray()
                });
            }
        }

        private void BalanceSubscriptionStatusChanged(SubscriptionStatus newState)
        {
            _logger.LogDebug("Balance stream status changed: {NewState}", newState);
            if (newState == SubscriptionStatus.Subscribed)
                // Trigger REST polling since we weren't connected
                _pollWaitEvent.Set();
        }

        private void OrderSubscriptionStatusChanged(SubscriptionStatus newState)
        {
            _logger.LogDebug("Order stream status changed: {NewState}", newState);

            if (newState == SubscriptionStatus.Pending)
            {
                // Record last data receive time since we need to request data from that timestamp on when polling
                // Only set to new value if it isn't already set since if we disconnect/reconnect a couple of times without
                // managing to do a poll we don't want to override the time since we still need to request that earlier data

                if (_lastDataTimeOrdersBeforeDisconnect == null)
                    _lastDataTimeOrdersBeforeDisconnect = _orderSubscription!.LastReceiveTime;
            }

            if (newState == SubscriptionStatus.Subscribed)
                // Trigger REST polling since we weren't connected
                _pollWaitEvent.Set();
        }

        private void TradeSubscriptionStatusChanged(SubscriptionStatus newState)
        {
            _logger.LogDebug("Trade stream status changed: {NewState}", newState);

            if (newState == SubscriptionStatus.Pending)
            {
                // Record last data receive time since we need to request data from that timestamp on when polling
                // Only set to new value if it isn't already set since if we disconnect/reconnect a couple of times without
                // managing to do a poll we don't want to override the time since we still need to request that earlier data

                if (_lastDataTimeTradesBeforeDisconnect == null)
                    _lastDataTimeTradesBeforeDisconnect = _tradeSubscription?.LastReceiveTime;
            }

            if (newState == SubscriptionStatus.Subscribed)
                // Trigger REST polling since we weren't connected
                _pollWaitEvent.Set();
        }

        private bool UpdateBalance(SharedBalance existingBalance, SharedBalance newBalance)
        {
            // Some other way to way to determine sequence? Maybe timestamp?
            var changed = false;
            if (existingBalance.Total != newBalance.Total)
            {
                existingBalance.Total = newBalance.Total;
                changed = true;
            }

            if (existingBalance.Available != newBalance.Available)
            {
                existingBalance.Available = newBalance.Available;
                changed = true;
            }

            return changed;
        }

        private bool UpdateSpotOrder(SharedSpotOrder existingOrder, SharedSpotOrder newOrder)
        {
            if (CheckIfOrderUpdateIsNewer(existingOrder, newOrder) == false)
                // Update is older than the existing data, ignore
                return false;

            var changed = false;
            if (newOrder.AveragePrice != null && newOrder.AveragePrice != existingOrder.AveragePrice)
            {
                existingOrder.AveragePrice = newOrder.AveragePrice;
                changed = true;
            }

            if (newOrder.OrderPrice != null && newOrder.OrderPrice != existingOrder.OrderPrice)
            {
                existingOrder.OrderPrice = newOrder.OrderPrice;
                changed = true;
            }

            if (newOrder.Fee != null && newOrder.Fee != existingOrder.Fee)
            {
                existingOrder.Fee = newOrder.Fee;
                changed = true;
            }

            if (newOrder.FeeAsset != null && newOrder.FeeAsset != existingOrder.FeeAsset)
            {
                existingOrder.FeeAsset = newOrder.FeeAsset;
                changed = true;
            }

            if (newOrder.OrderQuantity != null && newOrder.OrderQuantity != existingOrder.OrderQuantity)
            {
                existingOrder.OrderQuantity = newOrder.OrderQuantity;
                changed = true;
            }

            if (newOrder.QuantityFilled != null && newOrder.QuantityFilled != existingOrder.QuantityFilled)
            {
                existingOrder.QuantityFilled = newOrder.QuantityFilled;
                changed = true;
            }

            if (newOrder.Status != existingOrder.Status)
            {
                existingOrder.Status = newOrder.Status;
                changed = true;
            }

            if (newOrder.UpdateTime != null && newOrder.UpdateTime != existingOrder.UpdateTime)
            {
                existingOrder.UpdateTime = newOrder.UpdateTime;
                changed = true;
            }

            return changed;
        }

        private bool? CheckIfOrderUpdateIsNewer(SharedSpotOrder existingOrder, SharedSpotOrder newOrder)
        {
            if (existingOrder.Status == SharedOrderStatus.Open && newOrder.Status != SharedOrderStatus.Open)
                // status changed from open to not open
                return true;

            if (existingOrder.Status != SharedOrderStatus.Open && newOrder.Status == SharedOrderStatus.Open)
                // status changed from not open to open; stale
                return false;

            if (existingOrder.UpdateTime != null && newOrder.UpdateTime != null)
            {
                // If both have an update time base of that
                if (existingOrder.UpdateTime < newOrder.UpdateTime)
                    return true;

                if (existingOrder.UpdateTime > newOrder.UpdateTime)
                    return false;
            }

            if (existingOrder.QuantityFilled != null && newOrder.QuantityFilled != null)
            {
                if (existingOrder.QuantityFilled.QuantityInBaseAsset != null && newOrder.QuantityFilled.QuantityInBaseAsset != null)
                {
                    // If base quantity is not null we can base it on that
                    if (existingOrder.QuantityFilled.QuantityInBaseAsset < newOrder.QuantityFilled.QuantityInBaseAsset)
                        return true;

                    else if (existingOrder.QuantityFilled.QuantityInBaseAsset > newOrder.QuantityFilled.QuantityInBaseAsset)
                        return false;
                }

                if (existingOrder.QuantityFilled.QuantityInQuoteAsset != null && newOrder.QuantityFilled.QuantityInQuoteAsset != null)
                {
                    // If quote quantity is not null we can base it on that
                    if (existingOrder.QuantityFilled.QuantityInQuoteAsset < newOrder.QuantityFilled.QuantityInQuoteAsset)
                        return true;

                    else if (existingOrder.QuantityFilled.QuantityInQuoteAsset > newOrder.QuantityFilled.QuantityInQuoteAsset)
                        return false;
                }
            }

            if (existingOrder.Fee != null && newOrder.Fee != null)
            {
                // Higher fee means later processing
                if (existingOrder.Fee < newOrder.Fee)
                    return true;

                if (existingOrder.Fee > newOrder.Fee)
                    return false;
            }

            return null;
        }

        private TimeSpan? GetNextPollDelay()
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

        private async Task PollAsync()
        {
            while (!_cts!.IsCancellationRequested)
            {
                var delayForNextPoll = GetNextPollDelay();                
                if (delayForNextPoll != TimeSpan.Zero)
                {
                    try
                    {
                        if (delayForNextPoll != null)
                            _logger.LogTrace("Delay for next polling: {Delay}", delayForNextPoll);

                        await _pollWaitEvent.WaitAsync(delayForNextPoll, _cts.Token).ConfigureAwait(false);
                    }
                    catch { }
                }

                _firstPollDone = true;
                if (_cts.IsCancellationRequested)
                    break;

                if (_lastPollAttempt != null && (DateTime.UtcNow - _lastPollAttempt.Value) < TimeSpan.FromSeconds(2))
                {
                    if (_lastPollSuccessful)
                        // If last poll was less than 2 seconds ago and it was successful don't bother immediately polling again
                        continue;
                }

                _logger.LogDebug("Starting user data requesting");
                _lastPollAttempt = DateTime.UtcNow;
                _lastPollSuccessful = false;

                var anyError = false;
                var balancesResult = await _balanceRestClient.GetBalancesAsync(new GetBalancesRequest()).ConfigureAwait(false);
                if (!balancesResult.Success)
                {
                    // .. ?
                    var transientError = balancesResult.Error!.IsTransient;
                    // If transient we can retry
                    // Should communicate errors, also for websocket disconnecting

                    anyError = true;
                }
                else
                {
                    HandleBalanceUpdate(UpdateSource.Poll, balancesResult.Data);
                }

                var openOrdersResult = await _spotOrderRestClient.GetOpenSpotOrdersAsync(new GetOpenOrdersRequest()).ConfigureAwait(false);
                if (!openOrdersResult.Success)
                {
                    // .. ?

                    anyError = true;
                }
                else
                {
                    HandleOrderUpdate(UpdateSource.Poll, openOrdersResult.Data);
                }

                foreach (var symbol in _symbols)
                {
                    var fromTimeOrders = _lastDataTimeOrdersBeforeDisconnect ?? _lastPollTimeOrders ?? _startTime;
                    var updatedPollTime = DateTime.UtcNow;
                    var closedOrdersResult = await _spotOrderRestClient.GetClosedSpotOrdersAsync(new GetClosedOrdersRequest(symbol, startTime: fromTimeOrders)).ConfigureAwait(false);
                    if (!closedOrdersResult.Success)
                    {
                        // .. ?

                        anyError = true;
                    }
                    else
                    {
                        _lastDataTimeOrdersBeforeDisconnect = null;
                        _lastPollTimeOrders = updatedPollTime;

                        // Filter orders to only include where close time is after the start time
                        var relevantOrders = closedOrdersResult.Data.Where(x =>
                            x.UpdateTime != null && x.UpdateTime >= _startTime // Updated after the tracker start time
                            || x.CreateTime != null && x.CreateTime >= _startTime // Created after the tracker start time
                            || x.CreateTime == null && x.UpdateTime == null // Unknown time
                        ).ToArray();

                        if (relevantOrders.Length > 0)
                            HandleOrderUpdate(UpdateSource.Poll, relevantOrders);
                    }

                    if (_trackTrades)
                    {
                        var fromTimeTrades = _lastDataTimeTradesBeforeDisconnect ?? _lastPollTimeTrades ?? _startTime;
                        var tradesResult = await _spotOrderRestClient.GetSpotUserTradesAsync(new GetUserTradesRequest(symbol, startTime: fromTimeTrades)).ConfigureAwait(false);
                        if (!tradesResult.Success)
                        {
                            // .. ?
                            anyError = true;
                        }
                        else
                        {
                            _lastDataTimeTradesBeforeDisconnect = null;
                            _lastPollTimeTrades = updatedPollTime;

                            // Filter trades to only include where timestamp is after the start time OR it's part of an order we're tracking
                            var relevantTrades = tradesResult.Data.Where(x => x.Timestamp >= _startTime || _orderStore.ContainsKey(x.OrderId)).ToArray();
                            if (relevantTrades.Length > 0)
                                HandleTradeUpdate(UpdateSource.Poll, tradesResult.Data);
                        }
                    }
                }

                _lastPollSuccessful = !anyError;
                _logger.LogDebug("User data requesting completed");
            }
        }

        /// <inheritdoc />
        public async Task StopAsync()
        {
            _logger.LogDebug("Stopping UserDataTracker");
            _cts?.Cancel();

            if (_pollTask != null)
                await _pollTask.ConfigureAwait(false);

            _logger.LogDebug("Stopped UserDataTracker");
        }
    }
}
