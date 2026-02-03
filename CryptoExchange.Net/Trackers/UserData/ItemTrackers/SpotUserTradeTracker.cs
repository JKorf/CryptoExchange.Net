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
    /// Spot user trade tracker
    /// </summary>
    public class SpotUserTradeTracker : UserDataItemTracker<SharedUserTrade>
    {
        private readonly ISpotOrderRestClient _restClient;
        private readonly IUserTradeSocketClient? _socketClient;
        private readonly ExchangeParameters? _exchangeParameters;

        internal Func<string[]>? GetTrackedOrderIds { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SpotUserTradeTracker(
            ILogger logger,
            ISpotOrderRestClient restClient,
            IUserTradeSocketClient? socketClient,
            TrackerItemConfig config,
            IEnumerable<SharedSymbol> symbols,
            bool onlyTrackProvidedSymbols,
            ExchangeParameters? exchangeParameters = null
            ) : base(logger, UserDataType.Trades, config, onlyTrackProvidedSymbols, symbols)
        {
            _restClient = restClient;
            _socketClient = socketClient;
            _exchangeParameters = exchangeParameters;
        }

        /// <inheritdoc />
        protected override string GetKey(SharedUserTrade item) => item.Id;
        /// <inheritdoc />
        protected override bool? CheckIfUpdateShouldBeApplied(SharedUserTrade existingItem, SharedUserTrade updateItem) => false;
        /// <inheritdoc />
        protected override bool Update(SharedUserTrade existingItem, SharedUserTrade updateItem) => false; // Trades are never updated

        /// <inheritdoc />
        protected override async Task<bool> DoPollAsync()
        {
            var anyError = false;
            foreach (var symbol in _symbols)
            {
                var fromTimeTrades = _lastDataTimeBeforeDisconnect ?? _lastPollTime ?? _startTime;
                var updatedPollTime = DateTime.UtcNow;
                var tradesResult = await _restClient.GetSpotUserTradesAsync(new GetUserTradesRequest(symbol, startTime: fromTimeTrades, exchangeParameters: _exchangeParameters)).ConfigureAwait(false);
                if (!tradesResult.Success)
                {
                    // .. ?
                    anyError = true;
                }
                else
                {
                    _lastDataTimeBeforeDisconnect = null;
                    _lastPollTime = updatedPollTime;

                    // Filter trades to only include where timestamp is after the start time OR it's part of an order we're tracking
                    var relevantTrades = tradesResult.Data.Where(x => x.Timestamp >= _startTime || (GetTrackedOrderIds?.Invoke() ?? []).Any(o => o == x.OrderId)).ToArray();
                    if (relevantTrades.Length > 0)
                        await HandleUpdateAsync(UpdateSource.Poll, tradesResult.Data).ConfigureAwait(false);
                }
            }

            return anyError;
        }

        /// <inheritdoc />
        protected override Task<CallResult<UpdateSubscription?>> DoSubscribeAsync(string? listenKey)
        {
            if (_socketClient == null)
                return Task.FromResult(new CallResult<UpdateSubscription?>(data: null));

            return ExchangeHelpers.ProcessQueuedAsync<SharedUserTrade[]>(
                async handler => await _socketClient.SubscribeToUserTradeUpdatesAsync(new SubscribeUserTradeRequest(listenKey, exchangeParameters: _exchangeParameters), handler, ct: _cts!.Token).ConfigureAwait(false),
                x => HandleUpdateAsync(UpdateSource.Push, x.Data))!;
        }

    }
}
