using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using CryptoExchange.Net.Trackers.UserData.Interfaces;
using CryptoExchange.Net.Trackers.UserData.Objects;
using CryptoExchange.Net.Trackers.UserData.ItemTrackers;
using System;

namespace CryptoExchange.Net.Trackers.UserData
{
    /// <summary>
    /// Spot user data tracker
    /// </summary>
    public class UserSpotDataTracker : UserDataTracker, IUserSpotDataTracker
    {
        private readonly ISpotSymbolRestClient _symbolClient;
        private readonly IListenKeyRestClient? _listenKeyClient;
        private readonly ExchangeParameters? _exchangeParameters;
        private Task? _lkKeepAliveTask;

        /// <inheritdoc />
        protected override UserDataItemTracker[] DataTrackers { get; } 
        /// <inheritdoc />
        public IUserDataTracker<SharedBalance> Balances { get; }
        /// <inheritdoc />
        public IUserDataTracker<SharedSpotOrder> Orders { get; }
        /// <inheritdoc />
        public IUserDataTracker<SharedUserTrade>? Trades { get; }

        /// <summary>
        /// ctor
        /// </summary>
        public UserSpotDataTracker(
            ILogger logger,
            ISpotSymbolRestClient symbolRestClient,
            IListenKeyRestClient? listenKeyRestClient,
            IBalanceRestClient balanceRestClient,
            IBalanceSocketClient? balanceSocketClient,
            ISpotOrderRestClient spotOrderRestClient,
            ISpotOrderSocketClient? spotOrderSocketClient,
            IUserTradeSocketClient? userTradeSocketClient,
            string? userIdentifier,
            SpotUserDataTrackerConfig config,
            ExchangeParameters? exchangeParameters = null) : base(logger, symbolRestClient.Exchange, config, userIdentifier)
        {
            // create trackers
            _symbolClient = symbolRestClient;
            _listenKeyClient = listenKeyRestClient;
            _exchangeParameters = exchangeParameters;

            var trackers = new List<UserDataItemTracker>();

            var balanceTracker = new BalanceTracker(logger, SymbolTracker, balanceRestClient, balanceSocketClient, SharedAccountType.Spot, config.BalancesConfig, exchangeParameters);
            Balances = balanceTracker;
            trackers.Add(balanceTracker);

            var orderTracker = new SpotOrderTracker(logger, SymbolTracker, spotOrderRestClient, spotOrderSocketClient, config.OrdersConfig, config.TrackedSymbols, config.OnlyTrackProvidedSymbols, exchangeParameters);
            Orders = orderTracker;
            trackers.Add(orderTracker);

            if (config.TrackTrades)
            {
                var tradeTracker = new SpotUserTradeTracker(logger, SymbolTracker, spotOrderRestClient, userTradeSocketClient, config.UserTradesConfig, config.TrackedSymbols, config.OnlyTrackProvidedSymbols, exchangeParameters);
                Trades = tradeTracker;
                trackers.Add(tradeTracker);

                orderTracker.OnTradeUpdate += tradeTracker.HandleUpdateAsync;
                tradeTracker.GetTrackedOrderIds = () => orderTracker.Values.Select(x => x.OrderId).ToArray();
            }

            DataTrackers = trackers.ToArray();
        }

        /// <inheritdoc />
        protected override async Task<CallResult> DoStartAsync()
        {
            var symbolResult = await _symbolClient.GetSpotSymbolsAsync(new GetSymbolsRequest(exchangeParameters: _exchangeParameters)).ConfigureAwait(false);
            if (!symbolResult)
            {
                _logger.LogWarning("Failed to start UserSpotDataTracker; symbols request failed: {Error}", symbolResult.Error);
                return symbolResult;
            }

            if (_listenKeyClient != null)
            {
                var lkResult = await _listenKeyClient.StartListenKeyAsync(new StartListenKeyRequest(exchangeParameters: _exchangeParameters)).ConfigureAwait(false);
                if (!lkResult)
                {
                    _logger.LogWarning("Failed to start UserSpotDataTracker; listen key request failed: {Error}", lkResult.Error);
                    return lkResult;
                }

                _lkKeepAliveTask = KeepAliveListenKeyAsync();

                _listenKey = lkResult.Data;
            }

            return CallResult.SuccessResult;
        }

        /// <inheritdoc />
        protected override async Task DoStopAsync()
        {
            if (_lkKeepAliveTask != null)
                await _lkKeepAliveTask.ConfigureAwait(false);
        }

        private async Task KeepAliveListenKeyAsync()
        {
            var interval = TimeSpan.FromMinutes(30);
            while (!_cts!.IsCancellationRequested)
            {
                try { await Task.Delay(interval, _cts.Token).ConfigureAwait(false); } 
                catch (Exception)
                {
                    break;
                }

                var result = await _listenKeyClient!.KeepAliveListenKeyAsync(new KeepAliveListenKeyRequest(_listenKey!, TradingMode.Spot)).ConfigureAwait(false);
                if (!result)
                    _logger.LogWarning("Listen key keep alive failed: " + result.Error);

                // If failed shorten the delay to allow a couple more retries
                interval = result ? TimeSpan.FromMinutes(30) : TimeSpan.FromMinutes(5);
            }
        }

        /// <summary>
        /// Add symbols to the list of symbols for which data is being tracked
        /// </summary>
        /// <param name="symbols">Symbols to add</param>
        public void AddTrackedSymbolsAsync(IEnumerable<SharedSymbol> symbols)
        {
            if (symbols.Any(x => x.TradingMode != TradingMode.Spot))
                throw new ArgumentException("Futures symbol not allowed in spot tracker", nameof(symbols));

            SymbolTracker.UpdateTrackedSymbols(symbols, true);
        }

        /// <summary>
        /// Remove a symbol from the list of symbols for which data is being tracked. Also removes stored data for that symbol.
        /// Note that the symbol will be added again if new data for that symbol is received, unless the OnlyTrackProvidedSymbols option has been set to true.
        /// </summary>
        /// <param name="symbol">Symbol to remove</param>
        public void RemoveTrackedSymbolAsync(SharedSymbol symbol)
        {
            SymbolTracker.RemoveTrackedSymbol(symbol);

            ((SpotOrderTracker)Orders).ClearDataForSymbol(symbol);
            ((SpotUserTradeTracker)Trades).ClearDataForSymbol(symbol);
        }
    }
}
