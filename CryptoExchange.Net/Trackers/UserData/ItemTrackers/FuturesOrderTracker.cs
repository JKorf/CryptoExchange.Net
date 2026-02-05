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

        internal event Func<UpdateSource, SharedUserTrade[], Task>? OnTradeUpdate;

        /// <summary>
        /// ctor
        /// </summary>
        public FuturesOrderTracker(
            ILogger logger,
            IFuturesOrderRestClient restClient,
            IFuturesOrderSocketClient? socketClient,
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

            _requiresSymbolParameterOpenOrders = restClient.GetOpenFuturesOrdersOptions.RequiredOptionalParameters.Any(x => x.Name == "Symbol");
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
                foreach (var symbol in _symbols.ToList())
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
                
            foreach (var symbol in _symbols.ToList())
            {
                var fromTimeOrders = _lastDataTimeBeforeDisconnect ?? _lastPollTime ?? _startTime;
                var updatedPollTime = DateTime.UtcNow;
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
                    _lastDataTimeBeforeDisconnect = null;
                    _lastPollTime = updatedPollTime;

                    // Filter orders to only include where close time is after the start time
                    var relevantOrders = closedOrdersResult.Data.Where(x =>
                        x.UpdateTime != null && x.UpdateTime >= _startTime // Updated after the tracker start time
                        || x.CreateTime != null && x.CreateTime >= _startTime // Created after the tracker start time
                        || x.CreateTime == null && x.UpdateTime == null // Unknown time
                    ).ToArray();

                    // Check for orders which are no longer returned in either open/closed and assume they're canceled without fill
                    var openOrdersNotReturned = Values.Where(x =>
                        x.SharedSymbol!.BaseAsset == symbol.BaseAsset && x.SharedSymbol.QuoteAsset == symbol.QuoteAsset // Orders for the same symbol
                        && x.QuantityFilled?.IsZero == true // With no filled value
                        && !openOrders.Any(r => r.OrderId == x.OrderId) // Not returned in open orders
                        && !relevantOrders.Any(r => r.OrderId == x.OrderId) // Not return in closed orders
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

            return anyError;
        }
    }
}
