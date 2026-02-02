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
    public abstract class UserFuturesDataTracker : UserDataTracker, IUserFuturesDataTracker
    {
        // Cached data
        private ConcurrentDictionary<string, SharedBalance> _balanceStore = new ConcurrentDictionary<string, SharedBalance>();
        private ConcurrentDictionary<string, SharedPosition> _positionStore = new ConcurrentDictionary<string, SharedPosition>();
        private ConcurrentDictionary<string, SharedFuturesOrder> _orderStore = new ConcurrentDictionary<string, SharedFuturesOrder>();
        private ConcurrentDictionary<string, SharedUserTrade> _tradeStore = new ConcurrentDictionary<string, SharedUserTrade>();

        // Typed clients
        private readonly IListenKeyRestClient? _listenKeyRestClient;
        private readonly IFuturesSymbolRestClient _futuresSymbolRestClient;
        private readonly IBalanceRestClient _balanceRestClient;
        private readonly IBalanceSocketClient _balanceSocketClient;
        private readonly IFuturesOrderRestClient _futuresOrderRestClient;
        private readonly IFuturesOrderSocketClient _futuresOrderSocketClient;
        private readonly IPositionSocketClient _positionSocketClient;
        private readonly IUserTradeSocketClient? _userTradeSocketClient;

        // Subscriptions
        private UpdateSubscription? _balanceSubscription;
        private UpdateSubscription? _orderSubscription;
        private UpdateSubscription? _positionSubscription;
        private UpdateSubscription? _tradeSubscription;

        private ExchangeParameters? _exchangeParameters;

        /// <summary>
        /// Whether updates received from the websocket for positions are full snapshots, or individual updates for positions.
        /// This is used to determine if positions should be removed if no longer present in the update data.
        /// </summary>
        protected abstract bool WebsocketPositionUpdatesAreFullSnapshots { get; }

        /// <inheritdoc />
        public event Func<UserDataUpdate<SharedBalance[]>, Task>? OnBalanceUpdate;
        /// <inheritdoc />
        public event Func<UserDataUpdate<SharedFuturesOrder[]>, Task>? OnOrderUpdate;
        /// <inheritdoc />
        public event Func<UserDataUpdate<SharedPosition[]>, Task>? OnPositionUpdate;
        /// <inheritdoc />
        public event Func<UserDataUpdate<SharedUserTrade[]>, Task>? OnTradeUpdate;

        /// <inheritdoc />
        public SharedBalance[] Balances => _balanceStore.Values.ToArray();
        /// <inheritdoc />
        public SharedFuturesOrder[] Orders => _orderStore.Values.ToArray();
        /// <inheritdoc />
        public SharedPosition[] Positions => _positionStore.Values.ToArray();
        /// <inheritdoc />
        public SharedUserTrade[] Trades => _tradeStore.Values.ToArray();

        /// <summary>
        /// ctor
        /// </summary>
        protected UserFuturesDataTracker(
            ILogger logger,
            ISharedClient restClient,
            ISharedClient socketClient,
            string? userIdentifier,
            UserDataTrackerConfig config,
            ExchangeParameters? exchangeParameters = null) : base(logger, config, userIdentifier)
        {
            _exchangeParameters = exchangeParameters;
            _futuresSymbolRestClient = (IFuturesSymbolRestClient)restClient;
            _balanceRestClient = (IBalanceRestClient)restClient;
            _balanceSocketClient = (IBalanceSocketClient)socketClient;
            _futuresOrderRestClient = (IFuturesOrderRestClient)restClient;
            _futuresOrderSocketClient = (IFuturesOrderSocketClient)socketClient;
            _positionSocketClient = (IPositionSocketClient)socketClient;
            _listenKeyRestClient = restClient as IListenKeyRestClient;
            _userTradeSocketClient = socketClient as IUserTradeSocketClient;
        }

        /// <inheritdoc />
        protected override async Task<CallResult> DoStartAsync()
        {
            _logger.LogDebug("Starting UserDataTracker");

            // Request symbols so SharedSymbol property can be filled on updates
            var symbolResult = await _futuresSymbolRestClient.GetFuturesSymbolsAsync(new GetSymbolsRequest(exchangeParameters: _exchangeParameters)).ConfigureAwait(false);
            if (!symbolResult)
            {
                _logger.LogWarning("Failed to start UserFuturesDataTracker; symbols request failed: {Error}", symbolResult.Error!.Message);
                return symbolResult;
            }

            string? listenKey = null;
            if (_listenKeyRestClient != null)
            {
                var lkResult = await _listenKeyRestClient.StartListenKeyAsync(new StartListenKeyRequest(exchangeParameters: _exchangeParameters)).ConfigureAwait(false);
                if (!lkResult)
                {
                    _logger.LogWarning("Failed to start UserFuturesDataTracker; listen key request failed: {Error}", lkResult.Error!.Message);
                    return lkResult;
                }

                listenKey = lkResult.Data;
            }

            var subBalanceResult = await ExchangeHelpers.ProcessQueuedAsync<SharedBalance[]>(
                async handler => await _balanceSocketClient.SubscribeToBalanceUpdatesAsync(new SubscribeBalancesRequest(listenKey, exchangeParameters: _exchangeParameters), handler, ct: _cts!.Token).ConfigureAwait(false),
                x => HandleBalanceUpdateAsync(UpdateSource.Push, x.Data)).ConfigureAwait(false);
            if (!subBalanceResult)
            {
                _logger.LogWarning("Failed to start UserFuturesDataTracker; failed to subscribe to balance stream: {Error}", subBalanceResult.Error!.Message);
                return subBalanceResult;
            }

            _balanceSubscription = subBalanceResult.Data;
            subBalanceResult.Data.SubscriptionStatusChanged += BalanceSubscriptionStatusChanged;

            var subOrderResult = await ExchangeHelpers.ProcessQueuedAsync<SharedFuturesOrder[]>(
                async handler => await _futuresOrderSocketClient.SubscribeToFuturesOrderUpdatesAsync(new SubscribeFuturesOrderRequest(listenKey, exchangeParameters: _exchangeParameters), handler, ct: _cts!.Token).ConfigureAwait(false),
                x => HandleOrderUpdateAsync(UpdateSource.Push, x.Data)).ConfigureAwait(false);
            if (!subOrderResult)
            {
                _cts!.Cancel();
                _logger.LogWarning("Failed to start UserFuturesDataTracker; failed to subscribe to order stream: {Error}", subOrderResult.Error!.Message);
                return subOrderResult;
            }

            _orderSubscription = subOrderResult.Data;
            subOrderResult.Data.SubscriptionStatusChanged += OrderSubscriptionStatusChanged;

            if (_positionSocketClient != null)
            {
                var subPositionResult = await ExchangeHelpers.ProcessQueuedAsync<SharedPosition[]>(
                    async handler => await _positionSocketClient.SubscribeToPositionUpdatesAsync(new SubscribePositionRequest(listenKey, exchangeParameters: _exchangeParameters), handler, ct: _cts!.Token).ConfigureAwait(false),
                    x => HandlePositionUpdateAsync(UpdateSource.Push, x.Data)).ConfigureAwait(false);
                if (!subPositionResult)
                {
                    _cts!.Cancel();
                    _logger.LogWarning("Failed to start UserFuturesDataTracker; failed to subscribe to trade stream: {Error}", subPositionResult.Error!.Message);
                    return subPositionResult;
                }

                _positionSubscription = subPositionResult.Data;
                subPositionResult.Data.SubscriptionStatusChanged += PositionSubscriptionStatusChanged;
            }

            if (_userTradeSocketClient != null && _trackTrades)
            {
                var subTradeResult = await ExchangeHelpers.ProcessQueuedAsync<SharedUserTrade[]>(
                    async handler => await _userTradeSocketClient.SubscribeToUserTradeUpdatesAsync(new SubscribeUserTradeRequest(listenKey, exchangeParameters: _exchangeParameters), handler, ct: _cts!.Token).ConfigureAwait(false),
                    x => HandleTradeUpdateAsync(UpdateSource.Push, x.Data)).ConfigureAwait(false);
                if (!subOrderResult)
                {
                    _cts.Cancel();
                    _logger.LogWarning("Failed to start UserFuturesDataTracker; failed to subscribe to trade stream: {Error}", subTradeResult.Error!.Message);
                    return subOrderResult;
                }

                _tradeSubscription = subTradeResult.Data;
                subTradeResult.Data.SubscriptionStatusChanged += TradeSubscriptionStatusChanged;
            }

            _logger.LogDebug("Started UserFuturesDataTracker");
            return CallResult.SuccessResult;
        }

        private async Task HandleTradeUpdateAsync(UpdateSource source, SharedUserTrade[] @event)
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

            if (updatedIds.Count > 0 && OnTradeUpdate != null)
            {
                await OnTradeUpdate.Invoke(
                    new UserDataUpdate<SharedUserTrade[]>
                    {
                        Source = source,
                        Data = _tradeStore.Where(x => updatedIds.Contains(x.Key)).Select(x => x.Value).ToArray()
                    }).ConfigureAwait(false);
            }
        }

        private async Task HandleOrderUpdateAsync(UpdateSource source, SharedFuturesOrder[] @event)
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
                    var updated = UpdateFuturesOrder(existing, item);
                    if (!updated)
                        updatedIds.Remove(id);
                    else
                        _logger.LogDebug("Updated futures order {Symbol}.{Id}", item.Symbol, item.OrderId);

                    return existing;
                });

                if (!orderExisted)
                    _logger.LogDebug("Added futures order {Symbol}.{Id}", item.Symbol, item.OrderId);
            }

            if (updatedIds.Count > 0 && OnOrderUpdate != null)
            {
                await OnOrderUpdate.Invoke(
                    new UserDataUpdate<SharedFuturesOrder[]>
                    {
                        Source = source,
                        Data = _orderStore.Where(x => updatedIds.Contains(x.Key)).Select(x => x.Value).ToArray()
                    }).ConfigureAwait(false);
            }

            var trades = @event.Where(x => x.LastTrade != null).Select(x => x.LastTrade!).ToArray();
            if (trades.Length != 0)
                await HandleTradeUpdateAsync(source, trades).ConfigureAwait(false);
        }

        private string PositionIdentifier(SharedPosition position) =>
            position.Id ?? position.Symbol + position.PositionMode + (position.PositionMode != SharedPositionMode.OneWay ? position.PositionSide.ToString() : "");

        private async Task HandlePositionUpdateAsync(UpdateSource source, SharedPosition[] @event)
        {
            // Update local store
            var updatedPositions = @event.Select(PositionIdentifier).ToList();

            if (WebsocketPositionUpdatesAreFullSnapshots)
            {
                // Reset any tracking position to zero/null values when it's no longer in the snapshot as it means there is no open position any more
                var notInSnapshot = _positionStore.Where(x => !updatedPositions.Contains(x.Key) && x.Value.PositionSize != 0).ToList();
                foreach (var position in notInSnapshot)
                {
                    position.Value.UpdateTime = DateTime.UtcNow;
                    position.Value.AverageOpenPrice = null;
                    position.Value.LiquidationPrice = null;
                    position.Value.PositionSize = 0;
                    position.Value.StopLossPrice = null;
                    position.Value.TakeProfitPrice = null;
                    position.Value.UnrealizedPnl = null;
                    updatedPositions.Add(position.Key);
                }
            }

            foreach (var item in @event)
            {
                bool positionExisted = false;
                _positionStore.AddOrUpdate(PositionIdentifier(item), item, (key, existing) =>
                {
                    positionExisted = true;
                    var updated = UpdatePosition(existing, item);
                    if (!updated)
                        updatedPositions.Remove(key);
                    else
                        _logger.LogDebug("Updated position for {Symbol}", item.Symbol);

                    return existing;
                });

                if (!positionExisted)
                    _logger.LogDebug("Added position for {Symbol}", item.Symbol);
            }

            if (updatedPositions.Count > 0 && OnPositionUpdate != null)
            {
                await OnPositionUpdate.Invoke(
                    new UserDataUpdate<SharedPosition[]>
                    {
                        Source = source,
                        Data = _positionStore.Where(x => updatedPositions.Contains(x.Key)).Select(x => x.Value).ToArray()
                    }).ConfigureAwait(false);
            }
        }

        private async Task HandleBalanceUpdateAsync(UpdateSource source, SharedBalance[] @event)
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

            if (updatedAssets.Count > 0 && OnBalanceUpdate != null)
            {
                await OnBalanceUpdate.Invoke(
                    new UserDataUpdate<SharedBalance[]>
                    {
                        Source = source,
                        Data = _balanceStore.Where(x => updatedAssets.Contains(x.Key)).Select(x => x.Value).ToArray()
                    }).ConfigureAwait(false);
            }
        }

        private void CheckConnectedChanged()
        {
            Connected = _balanceSubscription?.SubscriptionStatus == SubscriptionStatus.Subscribed
                && _orderSubscription?.SubscriptionStatus == SubscriptionStatus.Subscribed
                && (_tradeSubscription == null || _tradeSubscription.SubscriptionStatus == SubscriptionStatus.Subscribed)
                && (_positionSubscription == null || _positionSubscription.SubscriptionStatus == SubscriptionStatus.Subscribed);
        }

        private void BalanceSubscriptionStatusChanged(SubscriptionStatus newState)
        {
            _logger.LogDebug("Balance stream status changed: {NewState}", newState);

            CheckConnectedChanged();
        }

        private void PositionSubscriptionStatusChanged(SubscriptionStatus newState)
        {
            _logger.LogDebug("Position stream status changed: {NewState}", newState);

            CheckConnectedChanged();
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

            CheckConnectedChanged();
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

            CheckConnectedChanged();
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

        private bool UpdatePosition(SharedPosition existingPosition, SharedPosition newPosition)
        {
            // Some other way to way to determine sequence? Maybe timestamp?
            var changed = false;
            if (existingPosition.AverageOpenPrice != newPosition.AverageOpenPrice)
            {
                existingPosition.AverageOpenPrice = newPosition.AverageOpenPrice;
                changed = true;
            }

            if (existingPosition.Leverage != newPosition.Leverage)
            {
                existingPosition.Leverage = newPosition.Leverage;
                changed = true;
            }

            if (existingPosition.LiquidationPrice != newPosition.LiquidationPrice)
            {
                existingPosition.LiquidationPrice = newPosition.LiquidationPrice;
                changed = true;
            }

            if (existingPosition.PositionSize != newPosition.PositionSize)
            {
                existingPosition.PositionSize = newPosition.PositionSize;
                changed = true;
            }

            if (existingPosition.StopLossPrice != newPosition.StopLossPrice)
            {
                existingPosition.StopLossPrice = newPosition.StopLossPrice;
                changed = true;
            }

            if (existingPosition.TakeProfitPrice != newPosition.TakeProfitPrice)
            {
                existingPosition.TakeProfitPrice = newPosition.TakeProfitPrice;
                changed = true;
            }

            if (newPosition.UnrealizedPnl != null && existingPosition.UnrealizedPnl != newPosition.UnrealizedPnl)
            {
                existingPosition.UnrealizedPnl = newPosition.UnrealizedPnl;
                changed = true;
            }

            if (newPosition.UpdateTime != null && existingPosition.UpdateTime != newPosition.UpdateTime)
            {
                existingPosition.UpdateTime = newPosition.UpdateTime;
                // If update time is the only changed prop don't mark it as changed
            }

            return changed;
        }

        private bool UpdateFuturesOrder(SharedFuturesOrder existingOrder, SharedFuturesOrder newOrder)
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

            if (newOrder.StopLossPrice != existingOrder.StopLossPrice)
            {
                existingOrder.StopLossPrice = newOrder.StopLossPrice;
                changed = true;
            }

            if (newOrder.TakeProfitPrice != existingOrder.TakeProfitPrice)
            {
                existingOrder.TakeProfitPrice = newOrder.TakeProfitPrice;
                changed = true;
            }

            if (newOrder.TriggerPrice != existingOrder.TriggerPrice)
            {
                existingOrder.TriggerPrice = newOrder.TriggerPrice;
                changed = true;
            }

            if (newOrder.UpdateTime != null && newOrder.UpdateTime != existingOrder.UpdateTime)
            {
                existingOrder.UpdateTime = newOrder.UpdateTime;
                changed = true;
            }

            return changed;
        }

        private bool? CheckIfOrderUpdateIsNewer(SharedFuturesOrder existingOrder, SharedFuturesOrder newOrder)
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

        /// <inheritdoc />
        protected override async Task<bool> DoPollAsync()
        {
            _logger.LogDebug("Starting user data requesting");
            var anyError = false;
            var balancesResult = await _balanceRestClient.GetBalancesAsync(new GetBalancesRequest(exchangeParameters: _exchangeParameters)).ConfigureAwait(false);
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
                await HandleBalanceUpdateAsync(UpdateSource.Poll, balancesResult.Data).ConfigureAwait(false);
            }

            var openOrdersResult = await _futuresOrderRestClient.GetOpenFuturesOrdersAsync(new GetOpenOrdersRequest(exchangeParameters: _exchangeParameters)).ConfigureAwait(false);
            if (!openOrdersResult.Success)
            {
                // .. ?

                anyError = true;
            }
            else
            {
                await HandleOrderUpdateAsync(UpdateSource.Poll, openOrdersResult.Data).ConfigureAwait(false);
            }

            var positionResult = await _futuresOrderRestClient.GetPositionsAsync(new GetPositionsRequest(exchangeParameters: _exchangeParameters)).ConfigureAwait(false);
            if (!positionResult.Success)
            {
                // .. ?

                anyError = true;
            }
            else
            {
                await HandlePositionUpdateAsync(UpdateSource.Poll, positionResult.Data).ConfigureAwait(false);
            }

            foreach (var symbol in _symbols)
            {
                var fromTimeOrders = _lastDataTimeOrdersBeforeDisconnect ?? _lastPollTimeOrders ?? _startTime;
                var updatedPollTime = DateTime.UtcNow;
                var closedOrdersResult = await _futuresOrderRestClient.GetClosedFuturesOrdersAsync(new GetClosedOrdersRequest(symbol, startTime: fromTimeOrders, exchangeParameters: _exchangeParameters)).ConfigureAwait(false);
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
                        await HandleOrderUpdateAsync(UpdateSource.Poll, relevantOrders).ConfigureAwait(false);
                }

                if (_trackTrades)
                {
                    var fromTimeTrades = _lastDataTimeTradesBeforeDisconnect ?? _lastPollTimeTrades ?? _startTime;
                    var tradesResult = await _futuresOrderRestClient.GetFuturesUserTradesAsync(new GetUserTradesRequest(symbol, startTime: fromTimeTrades, exchangeParameters: _exchangeParameters)).ConfigureAwait(false);
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
                            await HandleTradeUpdateAsync(UpdateSource.Poll, tradesResult.Data).ConfigureAwait(false);
                    }
                }
            }

            _logger.LogDebug("User data requesting completed");
            return anyError;
        }
    }
}
