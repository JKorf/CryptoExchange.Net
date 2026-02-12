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
    /// Spot order tracker
    /// </summary>
    public class SpotOrderTracker : UserDataItemTracker<SharedSpotOrder>
    {
        private readonly ISpotOrderRestClient _restClient;
        private readonly ISpotOrderSocketClient? _socketClient;
        private readonly ExchangeParameters? _exchangeParameters;
        private readonly bool _requiresSymbolParameterOpenOrders;
        private readonly Dictionary<string, int> _openOrderNotReturnedTimes = new();

        internal event Func<UpdateSource, SharedUserTrade[], Task>? OnTradeUpdate;

        /// <summary>
        /// ctor
        /// </summary>
        public SpotOrderTracker(
            ILogger logger,
            ISpotOrderRestClient restClient,
            ISpotOrderSocketClient? socketClient,
            TrackerItemConfig config,
            IEnumerable<SharedSymbol> symbols,
            bool onlyTrackProvidedSymbols,
            ExchangeParameters? exchangeParameters = null
            ) : base(logger, UserDataType.Orders, restClient.Exchange, config, onlyTrackProvidedSymbols, symbols)
        {
            if (_socketClient == null)
                config = config with { PollIntervalConnected = config.PollIntervalDisconnected };

            _restClient = restClient;
            _socketClient = socketClient;
            _exchangeParameters = exchangeParameters;

            _requiresSymbolParameterOpenOrders = restClient.GetOpenSpotOrdersOptions.RequiredOptionalParameters.Any(x => x.Name == "Symbol");
        }

        /// <inheritdoc />
        protected override bool Update(SharedSpotOrder existingItem, SharedSpotOrder updateItem)
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
                existingItem.OrderQuantity ??= new SharedOrderQuantity();
                if (updateItem.OrderQuantity.QuantityInBaseAsset != null)
                {
                    existingItem.OrderQuantity.QuantityInBaseAsset = updateItem.OrderQuantity.QuantityInBaseAsset;
                    changed = true;
                }
                if (updateItem.OrderQuantity.QuantityInQuoteAsset != null)
                {
                    existingItem.OrderQuantity.QuantityInQuoteAsset = updateItem.OrderQuantity.QuantityInQuoteAsset;
                    changed = true;
                }
                if (updateItem.OrderQuantity.QuantityInContracts != null)
                {
                    existingItem.OrderQuantity.QuantityInContracts = updateItem.OrderQuantity.QuantityInContracts;
                    changed = true;
                }
            }

            if (updateItem.QuantityFilled != null && updateItem.QuantityFilled != existingItem.QuantityFilled)
            {
                existingItem.QuantityFilled ??= new SharedOrderQuantity();
                if (updateItem.QuantityFilled.QuantityInBaseAsset != null)
                {
                    existingItem.QuantityFilled.QuantityInBaseAsset = updateItem.QuantityFilled.QuantityInBaseAsset;
                    changed = true;
                }
                if (updateItem.QuantityFilled.QuantityInQuoteAsset != null)
                {
                    existingItem.QuantityFilled.QuantityInQuoteAsset = updateItem.QuantityFilled.QuantityInQuoteAsset;
                    changed = true;
                }
                if (updateItem.QuantityFilled.QuantityInContracts != null)
                {
                    existingItem.QuantityFilled.QuantityInContracts = updateItem.QuantityFilled.QuantityInContracts;
                    changed = true;
                }
            }

            if (updateItem.Status != existingItem.Status)
            {
                existingItem.Status = updateItem.Status;
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
        protected override string GetKey(SharedSpotOrder item) => item.OrderId;
        /// <inheritdoc />
        protected override TimeSpan GetAge(DateTime time, SharedSpotOrder item) => item.Status == SharedOrderStatus.Open ? TimeSpan.Zero : time - (item.UpdateTime ?? item.CreateTime ?? time);

        /// <inheritdoc />
        protected override bool? CheckIfUpdateShouldBeApplied(SharedSpotOrder existingItem, SharedSpotOrder updateItem)
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
        protected internal override async Task HandleUpdateAsync(UpdateSource source, SharedSpotOrder[] @event)
        {
            await base.HandleUpdateAsync(source, @event).ConfigureAwait(false);

            var trades = @event.Where(x => x.LastTrade != null).Select(x => x.LastTrade!).ToArray();
            if (trades.Length != 0 && OnTradeUpdate != null)
                await OnTradeUpdate(source, trades).ConfigureAwait(false);
        }

        /// <inheritdoc />
        protected override Task<CallResult<UpdateSubscription?>> DoSubscribeAsync(string? listenKey)
        {
            if (_socketClient == null)
                return Task.FromResult(new CallResult<UpdateSubscription?>(data: null));

            return ExchangeHelpers.ProcessQueuedAsync<SharedSpotOrder[]>(
                async handler => await _socketClient.SubscribeToSpotOrderUpdatesAsync(new SubscribeSpotOrderRequest(listenKey, exchangeParameters: _exchangeParameters), handler, ct: _cts!.Token).ConfigureAwait(false),
                x => HandleUpdateAsync(UpdateSource.Push, x.Data))!;
        }

        /// <inheritdoc />
        protected override async Task<bool> DoPollAsync()
        {
            var anyError = false;
            List<SharedSpotOrder> openOrders = new List<SharedSpotOrder>();

            if (!_requiresSymbolParameterOpenOrders)
            {
                var openOrdersResult = await _restClient.GetOpenSpotOrdersAsync(new GetOpenOrdersRequest(exchangeParameters: _exchangeParameters)).ConfigureAwait(false);
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
                foreach (var symbol in _symbols.ToList())
                {
                    var openOrdersResult = await _restClient.GetOpenSpotOrdersAsync(new GetOpenOrdersRequest(symbol, exchangeParameters: _exchangeParameters)).ConfigureAwait(false);
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
            foreach (var symbol in _symbols.ToList())
            {
                DateTime? fromTimeOrders = GetClosedOrdersRequestStartTime(symbol);

               
                var closedOrdersResult = await _restClient.GetClosedSpotOrdersAsync(new GetClosedOrdersRequest(symbol, startTime: fromTimeOrders, exchangeParameters: _exchangeParameters)).ConfigureAwait(false);
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

                    var additionalUpdates = new List<SharedSpotOrder>();
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
                _lastDataTimeBeforeDisconnect = null;
                _lastPollTime = updatedPollTime;
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

            _logger.LogTrace("{DataType} UserDataTracker poll startTime filter based on {Source}: {Time:yyyy-MM-dd HH:mm:ss.fff}", DataType, source, fromTime);
            return fromTime!.Value;
        }
    }
}
