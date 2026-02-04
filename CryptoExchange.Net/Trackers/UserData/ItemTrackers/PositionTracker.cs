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
    /// Position tracker
    /// </summary>
    public class PositionTracker : UserDataItemTracker<SharedPosition>
    {
        private readonly IFuturesOrderRestClient _restClient;
        private readonly IPositionSocketClient? _socketClient;
        private readonly ExchangeParameters? _exchangeParameters;

        /// <summary>
        /// Whether websocket position updates are full snapshots and missing positions should be considered 0
        /// </summary>
        protected bool WebsocketPositionUpdatesAreFullSnapshots { get; }

        /// <summary>
        /// ctor
        /// </summary>
        public PositionTracker(
            ILogger logger,
            IFuturesOrderRestClient restClient,
            IPositionSocketClient? socketClient,
            TrackerItemConfig config,
            IEnumerable<SharedSymbol> symbols,
            bool onlyTrackProvidedSymbols,
            bool websocketPositionUpdatesAreFullSnapshots,
            ExchangeParameters? exchangeParameters = null
            ) : base(logger, UserDataType.Positions, config, onlyTrackProvidedSymbols, symbols)
        {
            _restClient = restClient;
            _socketClient = socketClient;
            _exchangeParameters = exchangeParameters;
            WebsocketPositionUpdatesAreFullSnapshots = websocketPositionUpdatesAreFullSnapshots;
        }

        /// <inheritdoc />
        protected override bool Update(SharedPosition existingItem, SharedPosition updateItem)
        {
            // Some other way to way to determine sequence? Maybe timestamp?
            var changed = false;
            if (existingItem.AverageOpenPrice != updateItem.AverageOpenPrice)
            {
                existingItem.AverageOpenPrice = updateItem.AverageOpenPrice;
                changed = true;
            }

            if (existingItem.Leverage != updateItem.Leverage)
            {
                existingItem.Leverage = updateItem.Leverage;
                changed = true;
            }

            if (existingItem.LiquidationPrice != updateItem.LiquidationPrice)
            {
                existingItem.LiquidationPrice = updateItem.LiquidationPrice;
                changed = true;
            }

            if (existingItem.PositionSize != updateItem.PositionSize)
            {
                existingItem.PositionSize = updateItem.PositionSize;
                changed = true;
            }

            if (existingItem.StopLossPrice != updateItem.StopLossPrice)
            {
                existingItem.StopLossPrice = updateItem.StopLossPrice;
                changed = true;
            }

            if (existingItem.TakeProfitPrice != updateItem.TakeProfitPrice)
            {
                existingItem.TakeProfitPrice = updateItem.TakeProfitPrice;
                changed = true;
            }

            if (updateItem.UnrealizedPnl != null && existingItem.UnrealizedPnl != updateItem.UnrealizedPnl)
            {
                existingItem.UnrealizedPnl = updateItem.UnrealizedPnl;
                changed = true;
            }

            if (updateItem.UpdateTime != null && existingItem.UpdateTime != updateItem.UpdateTime)
            {
                existingItem.UpdateTime = updateItem.UpdateTime;
                // If update time is the only changed prop don't mark it as changed
            }

            return changed;
        }

        /// <inheritdoc />
        protected internal override async Task HandleUpdateAsync(UpdateSource source, SharedPosition[] @event)
        {
            LastUpdateTime = DateTime.UtcNow;

            List<SharedPosition>? toRemove = null;
            foreach (var item in @event)
            {
                if (item is SharedSymbolModel symbolModel)
                {
                    if (symbolModel.SharedSymbol == null)
                    {
                        toRemove ??= new List<SharedPosition>();
                        toRemove.Add(item);
                    }
                    else if (_onlyTrackProvidedSymbols
                        && !_symbols.Any(y => y.TradingMode == symbolModel.SharedSymbol!.TradingMode && y.BaseAsset == symbolModel.SharedSymbol.BaseAsset && y.QuoteAsset == symbolModel.SharedSymbol.QuoteAsset))
                    {
                        toRemove ??= new List<SharedPosition>();
                        toRemove.Add(item);
                    }
                }
            }

            if (toRemove != null)
                @event = @event.Except(toRemove).ToArray();

            if (!_onlyTrackProvidedSymbols)
                UpdateSymbolsList(@event.OfType<SharedSymbolModel>().Select(x => x.SharedSymbol!));
            

            // Update local store
            var updatedItems = @event.Select(GetKey).ToList();

            if (WebsocketPositionUpdatesAreFullSnapshots)
            {
                // Reset any tracking position to zero/null values when it's no longer in the snapshot as it means there is no open position any more
                var notInSnapshot = _store.Where(x => !updatedItems.Contains(x.Key) && x.Value.PositionSize != 0).ToList();
                foreach (var position in notInSnapshot)
                {
                    position.Value.UpdateTime = DateTime.UtcNow;
                    position.Value.AverageOpenPrice = null;
                    position.Value.LiquidationPrice = null;
                    position.Value.PositionSize = 0;
                    position.Value.StopLossPrice = null;
                    position.Value.TakeProfitPrice = null;
                    position.Value.UnrealizedPnl = null;
                    updatedItems.Add(position.Key);

                    LastChangeTime = DateTime.UtcNow;
                }
            }

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
                            _logger.LogDebug("Updated {DataType} {Item}", DataType, key);
                            LastChangeTime = DateTime.UtcNow;
                        }
                    }

                    return existing;
                });

                if (!existed)
                {
                    _logger.LogDebug("Added {DataType} {Item}", DataType, GetKey(item));
                    LastChangeTime = DateTime.UtcNow;
                }
            }

            if (updatedItems.Count > 0)
            {
                await InvokeUpdate(
                    new UserDataUpdate<SharedPosition[]>
                    {
                        Source = source,
                        Data = _store.Where(x => updatedItems.Contains(x.Key)).Select(x => x.Value).ToArray()
                    }).ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        protected override string GetKey(SharedPosition item) =>
            item.SharedSymbol!.TradingMode + item.SharedSymbol.BaseAsset + item.SharedSymbol.QuoteAsset + item.PositionMode + (item.PositionMode != SharedPositionMode.OneWay ? item.PositionSide.ToString() : "");

        /// <inheritdoc />
        protected override bool? CheckIfUpdateShouldBeApplied(SharedPosition existingItem, SharedPosition updateItem) => true;

        /// <inheritdoc />
        protected override Task<CallResult<UpdateSubscription?>> DoSubscribeAsync(string? listenKey)
        {
            if (_socketClient == null)
                return Task.FromResult(new CallResult<UpdateSubscription?>(data: null));

            return ExchangeHelpers.ProcessQueuedAsync<SharedPosition[]>(
                async handler => await _socketClient.SubscribeToPositionUpdatesAsync(new SubscribePositionRequest(listenKey, exchangeParameters: _exchangeParameters), handler, ct: _cts!.Token).ConfigureAwait(false),
                x => HandleUpdateAsync(UpdateSource.Push, x.Data))!;
        }

        /// <inheritdoc />
        protected override async Task<bool> DoPollAsync()
        {
            var anyError = false;
            var openOrdersResult = await _restClient.GetPositionsAsync(new GetPositionsRequest(exchangeParameters: _exchangeParameters)).ConfigureAwait(false);
            if (!openOrdersResult.Success)
            {
                // .. ?

                anyError = true;
            }
            else
            {
                await HandleUpdateAsync(UpdateSource.Poll, openOrdersResult.Data).ConfigureAwait(false);
            }

            return anyError;
        }
    }
}
