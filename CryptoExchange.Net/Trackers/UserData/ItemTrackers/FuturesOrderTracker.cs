using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.UserData.Objects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.UserData.ItemTrackers
{
    /// <summary>
    /// Futures order tracker
    /// </summary>
    public class FuturesOrderTracker : UserDataItemTracker<SharedFuturesOrder>
    {
        private readonly IFuturesOrderRestClient _restClient;
        private readonly IFuturesOrderSocketClient? _socketClient;
        private readonly ExchangeParameters? _exchangeParameters;
        private readonly bool _requiresSymbolParameterOpenOrders;
        private readonly Dictionary<string, int> _openOrderNotReturnedTimes = new();

        internal event Func<UpdateSource, SharedUserTrade[], Task>? OnTradeUpdate;

        /// <summary>
        /// ctor
        /// </summary>
        public FuturesOrderTracker(
            ILogger logger,
            UserDataSymbolTracker symbolTracker,
            IFuturesOrderRestClient restClient,
            IFuturesOrderSocketClient? socketClient,
            TrackerItemConfig config,
            IEnumerable<SharedSymbol> symbols,
            bool onlyTrackProvidedSymbols,
            ExchangeParameters? exchangeParameters = null
            ) : base(logger, symbolTracker, UserDataType.Orders, restClient.Exchange, config)
        {
            if (_socketClient == null)
                config = config with { PollIntervalConnected = config.PollIntervalDisconnected };

            _restClient = restClient;
            _socketClient = socketClient;
            _exchangeParameters = exchangeParameters;

            _requiresSymbolParameterOpenOrders = restClient.GetOpenFuturesOrdersOptions.RequiredOptionalParameters.Any(x => x.Name == "Symbol");
        }

        internal void ClearDataForSymbol(SharedSymbol symbol)
        {
            foreach (var order in _store)
            {
                if (order.Value.SharedSymbol!.TradingMode == symbol.TradingMode
                    && order.Value.SharedSymbol.BaseAsset == symbol.BaseAsset
                    && order.Value.SharedSymbol.QuoteAsset == symbol.QuoteAsset
                    && order.Value.SharedSymbol.DeliverTime == symbol.DeliverTime)
                {
                    _store.TryRemove(order.Key, out _);
                }
            }
        }

        /// <inheritdoc />
        protected override bool Update(SharedFuturesOrder existingItem, SharedFuturesOrder updateItem)
        {
            var changed = false;
            if (updateItem.AveragePrice != null && updateItem.AveragePrice != existingItem.AveragePrice)
            {
                existingItem.AveragePrice = updateItem.AveragePrice;
                changed = true;
            }

            if (updateItem.OrderPrice != null && updateItem.OrderPrice != existingItem.OrderPrice)
            {
                existingItem.OrderPrice = updateItem.OrderPrice;
                changed = true;
            }

            if (updateItem.Fee != null && updateItem.Fee != existingItem.Fee)
            {
                existingItem.Fee = updateItem.Fee;
                changed = true;
            }

            if (updateItem.FeeAsset != null && updateItem.FeeAsset != existingItem.FeeAsset)
            {
                existingItem.FeeAsset = updateItem.FeeAsset;
                changed = true;
            }

            if (updateItem.OrderQuantity != null && updateItem.OrderQuantity != existingItem.OrderQuantity)
            {
                existingItem.OrderQuantity = updateItem.OrderQuantity;
                changed = true;
            }

            if (updateItem.QuantityFilled != null && updateItem.QuantityFilled != existingItem.QuantityFilled)
            {
                existingItem.QuantityFilled = updateItem.QuantityFilled;
                changed = true;
            }

            if (updateItem.Status != existingItem.Status)
            {
                existingItem.Status = updateItem.Status;
                changed = true;
            }

            if (updateItem.StopLossPrice != existingItem.StopLossPrice)
            {
                existingItem.StopLossPrice = updateItem.StopLossPrice;
                changed = true;
            }

            if (updateItem.TakeProfitPrice != existingItem.TakeProfitPrice)
            {
                existingItem.TakeProfitPrice = updateItem.TakeProfitPrice;
                changed = true;
            }

            if (updateItem.TriggerPrice != existingItem.TriggerPrice)
            {
                existingItem.TriggerPrice = updateItem.TriggerPrice;
                changed = true;
            }

            if (updateItem.UpdateTime != null && updateItem.UpdateTime != existingItem.UpdateTime)
            {
                existingItem.UpdateTime = updateItem.UpdateTime;
                changed = true;
            }

            return changed;
        }

        /// <inheritdoc />
        protected override string GetKey(SharedFuturesOrder item) => item.OrderId;
        /// <inheritdoc />
        protected override TimeSpan GetAge(DateTime time, SharedFuturesOrder item) => item.Status == SharedOrderStatus.Open ? TimeSpan.Zero : time - (item.UpdateTime ?? item.CreateTime ?? time);
        /// <inheritdoc />
        protected override bool? CheckIfUpdateShouldBeApplied(SharedFuturesOrder existingItem, SharedFuturesOrder updateItem)
        {
            if (existingItem.Status == SharedOrderStatus.Open && updateItem.Status != SharedOrderStatus.Open)
                // status changed from open to not open
                return true;

            if (existingItem.Status != SharedOrderStatus.Open
                && updateItem.Status != SharedOrderStatus.Open
                && existingItem.Status != updateItem.Status)
            {
                _logger.LogWarning("Invalid order update detected for order {OrderId}; current status: {OldStatus}, new status: {NewStatus}", existingItem.OrderId, existingItem.Status, updateItem.Status);
                return false;
            }

            if (existingItem.Status != SharedOrderStatus.Open && updateItem.Status == SharedOrderStatus.Open)
                // status changed from not open to open; stale
                return false;

            if (existingItem.UpdateTime != null && updateItem.UpdateTime != null)
            {
                // If both have an update time base of that
                if (existingItem.UpdateTime < updateItem.UpdateTime)
                    return true;

                if (existingItem.UpdateTime > updateItem.UpdateTime)
                    return false;
            }

            if (existingItem.QuantityFilled != null && updateItem.QuantityFilled != null)
            {
                if (existingItem.QuantityFilled.QuantityInBaseAsset != null && updateItem.QuantityFilled.QuantityInBaseAsset != null)
                {
                    // If base quantity is not null we can base it on that
                    if (existingItem.QuantityFilled.QuantityInBaseAsset < updateItem.QuantityFilled.QuantityInBaseAsset)
                        return true;

                    else if (existingItem.QuantityFilled.QuantityInBaseAsset > updateItem.QuantityFilled.QuantityInBaseAsset)
                        return false;
                }

                if (existingItem.QuantityFilled.QuantityInQuoteAsset != null && updateItem.QuantityFilled.QuantityInQuoteAsset != null)
                {
                    // If quote quantity is not null we can base it on that
                    if (existingItem.QuantityFilled.QuantityInQuoteAsset < updateItem.QuantityFilled.QuantityInQuoteAsset)
                        return true;

                    else if (existingItem.QuantityFilled.QuantityInQuoteAsset > updateItem.QuantityFilled.QuantityInQuoteAsset)
                        return false;
                }
            }

            if (existingItem.Fee != null && updateItem.Fee != null)
            {
                // Higher fee means later processing
                if (existingItem.Fee < updateItem.Fee)
                    return true;

                if (existingItem.Fee > updateItem.Fee)
                    return false;
            }

            return null;
        }

        /// <inheritdoc />
        protected internal override async Task HandleUpdateAsync(UpdateSource source, SharedFuturesOrder[] @event)
        {
            await base.HandleUpdateAsync(source, @event).ConfigureAwait(false);

            var trades = @event.Where(x => x.LastTrade != null).Select(x => x.LastTrade!).ToArray();
            if (trades.Length != 0 && OnTradeUpdate != null)
                await OnTradeUpdate.Invoke(source, trades).ConfigureAwait(false);
        }

        /// <inheritdoc />
        protected override Task<CallResult<UpdateSubscription?>> DoSubscribeAsync(string? listenKey)
        {
            if (_socketClient == null)
                return Task.FromResult(new CallResult<UpdateSubscription?>(data: null));

            return ExchangeHelpers.ProcessQueuedAsync<SharedFuturesOrder[]>(
                async handler => await _socketClient.SubscribeToFuturesOrderUpdatesAsync(new SubscribeFuturesOrderRequest(listenKey, exchangeParameters: _exchangeParameters), handler, ct: _cts!.Token).ConfigureAwait(false),
                x => HandleUpdateAsync(UpdateSource.Push, x.Data))!;
        }

        /// <inheritdoc />
        protected override async Task<bool> DoPollAsync()
        {
            var anyError = false;
            List<SharedFuturesOrder> openOrders = new List<SharedFuturesOrder>();

            if (!_requiresSymbolParameterOpenOrders)
            {
                var openOrdersResult = await _restClient.GetOpenFuturesOrdersAsync(new GetOpenOrdersRequest(exchangeParameters: _exchangeParameters)).ConfigureAwait(false);
                if (!openOrdersResult.Success)
                {
                    anyError = true;

                    _initialPollingError ??= openOrdersResult.Error;
                    if (!_firstPollDone)
                        return anyError;
                }
                else
                {
                    openOrders.AddRange(openOrdersResult.Data);
                    await HandleUpdateAsync(UpdateSource.Poll, openOrdersResult.Data).ConfigureAwait(false);
                }
            }
            else
            {
                foreach (var symbol in _symbolTracker.GetTrackedSymbols())
                {
                    var openOrdersResult = await _restClient.GetOpenFuturesOrdersAsync(new GetOpenOrdersRequest(symbol, exchangeParameters: _exchangeParameters)).ConfigureAwait(false);
                    if (!openOrdersResult.Success)
                    {
                        anyError = true;

                        _initialPollingError ??= openOrdersResult.Error;
                        if (!_firstPollDone)
                            break;
                    }
                    else
                    {
                        openOrders.AddRange(openOrdersResult.Data);
                        await HandleUpdateAsync(UpdateSource.Poll, openOrdersResult.Data).ConfigureAwait(false);
                    }
                }
            }

            if (!_firstPollDone && anyError)
                return anyError;

            // Check all current open orders
            // Keep track of the orders no longer returned in the open list
            // Order should be set to canceled state when it's no longer returned in the open list
            // but also is not returned in the closed list
            foreach (var order in Values.Where(x => x.Status == SharedOrderStatus.Open))
            {
                if (openOrders.Any(x => x.OrderId == order.OrderId))
                    continue;

                if (!_openOrderNotReturnedTimes.ContainsKey(order.OrderId))
                    _openOrderNotReturnedTimes[order.OrderId] = 0;

                _openOrderNotReturnedTimes[order.OrderId] += 1;
            }

            var updatedPollTime = DateTime.UtcNow;
            foreach (var symbol in _symbolTracker.GetTrackedSymbols())
            {
                DateTime? fromTimeOrders = GetClosedOrdersRequestStartTime(symbol);

                var closedOrdersResult = await _restClient.GetClosedFuturesOrdersAsync(new GetClosedOrdersRequest(symbol, startTime: fromTimeOrders, exchangeParameters: _exchangeParameters)).ConfigureAwait(false);
                if (!closedOrdersResult.Success)
                {
                    anyError = true;

                    _initialPollingError ??= closedOrdersResult.Error;
                    if (!_firstPollDone)
                        break;
                }
                else
                {
                    // Filter orders to only include where close time is after the start time
                    var relevantOrders = closedOrdersResult.Data.Where(x =>
                        (x.UpdateTime != null && x.UpdateTime >= _startTime) // Updated after the tracker start time
                        || (x.CreateTime != null && x.CreateTime >= _startTime) // Created after the tracker start time
                        || (x.CreateTime == null && x.UpdateTime == null) // Unknown time
                        || (Values.Any(e => e.OrderId == x.OrderId && x.Status == SharedOrderStatus.Open)) // Or we're currently tracking this open order
                    ).ToArray();

                    // Check for orders which are no longer returned in either open/closed and assume they're canceled without fill
                    var openOrdersNotReturned = Values.Where(x =>
                        // Orders for the same symbol
                        x.SharedSymbol!.BaseAsset == symbol.BaseAsset && x.SharedSymbol.QuoteAsset == symbol.QuoteAsset
                        // With no filled value
                        && x.QuantityFilled?.IsZero == true
                        // Not returned in open orders
                        && !openOrders.Any(r => r.OrderId == x.OrderId)
                        // Not returned in closed orders
                        && !relevantOrders.Any(r => r.OrderId == x.OrderId)
                        // Open order has not been returned in the open list at least 2 times
                        && (_openOrderNotReturnedTimes.TryGetValue(x.OrderId, out var notReturnedTimes) ? notReturnedTimes >= 2 : false)
                        ).ToList();

                    var additionalUpdates = new List<SharedFuturesOrder>();
                    foreach (var order in openOrdersNotReturned)
                    {
                        additionalUpdates.Add(order with
                        {
                            Status = SharedOrderStatus.Canceled
                        });
                    }

                    relevantOrders = relevantOrders.Concat(additionalUpdates).ToArray();
                    if (relevantOrders.Length > 0)
                        await HandleUpdateAsync(UpdateSource.Poll, relevantOrders).ConfigureAwait(false);
                }
            }

            if (!anyError)
            {
                _lastPollTime = updatedPollTime;
                _lastDataTimeBeforeDisconnect = null;
            }

            return anyError;
        }

        private DateTime? GetClosedOrdersRequestStartTime(SharedSymbol symbol)
        {
            // Determine the timestamp from which we need to check order status
            // Use the timestamp we last know the correct state of the data
            DateTime? fromTime = null;
            string? source = null;

            // Use the last timestamp we we received data from the websocket as state should be correct at that time. 1 seconds buffer
            if (_lastDataTimeBeforeDisconnect.HasValue && (fromTime == null || fromTime > _lastDataTimeBeforeDisconnect.Value))
            {
                fromTime = _lastDataTimeBeforeDisconnect.Value.AddSeconds(-1);
                source = "LastDataTimeBeforeDisconnect";
            }

            // If we've previously polled use that timestamp to request data from
            if (_lastPollTime.HasValue && (fromTime == null || _lastPollTime.Value > fromTime))
            {
                fromTime = _lastPollTime;
                source = "LastPollTime";
            }

                // If we known open orders with a create time before this time we need to use that timestamp to make sure that order is included in the response
            var trackedOrdersMinOpenTime = Values
                .Where(x => x.Status == SharedOrderStatus.Open && x.SharedSymbol!.BaseAsset == symbol.BaseAsset && x.SharedSymbol.QuoteAsset == symbol.QuoteAsset)
                .OrderBy(x => x.CreateTime)
                .FirstOrDefault()?.CreateTime;
            if (trackedOrdersMinOpenTime.HasValue && (fromTime == null || trackedOrdersMinOpenTime.Value < fromTime))
            {
                // Could be improved by only requesting the specific open orders if there are only a few that would be better than trying to request a long
                // history if the open order is far back
                fromTime = trackedOrdersMinOpenTime.Value.AddMilliseconds(-1);
                source = "OpenOrder";
            }

            if (fromTime == null)
            {
                fromTime = _startTime;
                source = "StartTime";
            }

            if (DateTime.UtcNow - fromTime < TimeSpan.FromSeconds(1))
            {
                // Set it to at least 5 seconds in the past to prevent issues when local time isn't in sync
                fromTime = DateTime.UtcNow.AddSeconds(-5);
            }

            _logger.LogTrace("{DataType}.{Symbol} UserDataTracker poll startTime filter based on {Source}: {Time:yyyy-MM-dd HH:mm:ss.fff}",
                DataType, $"{symbol.BaseAsset}/{symbol.QuoteAsset}", source, fromTime);
            return fromTime!.Value;
        }
    }
}
