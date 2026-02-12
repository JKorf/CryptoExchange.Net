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
    /// Futures user trade tracker
    /// </summary>
    public class FuturesUserTradeTracker : UserDataItemTracker<SharedUserTrade>
    {
        private readonly IFuturesOrderRestClient _restClient;
        private readonly IUserTradeSocketClient? _socketClient;
        private readonly ExchangeParameters? _exchangeParameters;

        internal Func<string[]>? GetTrackedOrderIds { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public FuturesUserTradeTracker(
            ILogger logger,
            IFuturesOrderRestClient restClient,
            IUserTradeSocketClient? socketClient,
            TrackerItemConfig config,
            IEnumerable<SharedSymbol> symbols,
            bool onlyTrackProvidedSymbols,
            ExchangeParameters? exchangeParameters = null
            ) : base(logger, UserDataType.Trades, restClient.Exchange, config, onlyTrackProvidedSymbols, symbols)
        {
            if (_socketClient == null)
                config = config with { PollIntervalConnected = config.PollIntervalDisconnected };

            _restClient = restClient;
            _socketClient = socketClient;
            _exchangeParameters = exchangeParameters;
        }

        /// <inheritdoc />
        protected override string GetKey(SharedUserTrade item) => item.Id;
        /// <inheritdoc />
        protected override bool? CheckIfUpdateShouldBeApplied(SharedUserTrade existingItem, SharedUserTrade updateItem) => false;
        /// <inheritdoc />
        protected override bool Update(SharedUserTrade existingItem, SharedUserTrade updateItem) => false; // trades are never updated
        /// <inheritdoc />
        protected override TimeSpan GetAge(DateTime time, SharedUserTrade item) => time - item.Timestamp;

        /// <inheritdoc />
        protected override async Task<bool> DoPollAsync()
        {
            var anyError = false;
            var fromTimeTrades = GetTradesRequestStartTime();
            var updatedPollTime = DateTime.UtcNow;
            foreach (var symbol in _symbols)
            {
                var tradesResult = await _restClient.GetFuturesUserTradesAsync(new GetUserTradesRequest(symbol, startTime: fromTimeTrades, exchangeParameters: _exchangeParameters)).ConfigureAwait(false);
                if (!tradesResult.Success)
                {
                    anyError = true;

                    _initialPollingError ??= tradesResult.Error;
                    if (!_firstPollDone)
                        break;
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

            if (!anyError)
            {
                _lastDataTimeBeforeDisconnect = null;
                _lastPollTime = updatedPollTime;
            }

            return anyError;
        }

        private DateTime? GetTradesRequestStartTime()
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

            if (fromTime == null)
            {
                fromTime = _startTime;
                source = "StartTime";
            }

            _logger.LogTrace("{DataType} UserDataTracker poll startTime filter based on {Source}: {Time:yyyy-MM-dd HH:mm:ss.fff}", DataType, source, fromTime);
            return fromTime!.Value;
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
