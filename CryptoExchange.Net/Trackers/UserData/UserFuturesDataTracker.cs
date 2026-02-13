using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using CryptoExchange.Net.Trackers.UserData.ItemTrackers;
using CryptoExchange.Net.Trackers.UserData.Interfaces;
using CryptoExchange.Net.Trackers.UserData.Objects;

namespace CryptoExchange.Net.Trackers.UserData
{
    /// <summary>
    /// User futures data tracker
    /// </summary>
    public abstract class UserFuturesDataTracker : UserDataTracker, IUserFuturesDataTracker
    {
        private readonly IFuturesSymbolRestClient _symbolClient;
        private readonly IListenKeyRestClient? _listenKeyClient;
        private readonly ExchangeParameters? _exchangeParameters;
        private readonly TradingMode _tradingMode;
        private Task? _lkKeepAliveTask;

        /// <inheritdoc />
        protected override UserDataItemTracker[] DataTrackers { get; }
        /// <summary>
        /// Balances tracker
        /// </summary>
        public IUserDataTracker<SharedBalance> Balances { get; }
        /// <summary>
        /// Orders tracker
        /// </summary>
        public IUserDataTracker<SharedFuturesOrder> Orders { get; }
        /// <summary>
        /// Positions tracker
        /// </summary>
        public IUserDataTracker<SharedPosition> Positions { get; }
        /// <summary>
        /// Trades tracker
        /// </summary>
        public IUserDataTracker<SharedUserTrade>? Trades { get; }

        /// <summary>
        /// Whether websocket position updates are full snapshots and missing positions should be considered 0
        /// </summary>
        protected abstract bool WebsocketPositionUpdatesAreFullSnapshots { get; }

        /// <summary>
        /// ctor
        /// </summary>
        public UserFuturesDataTracker(
            ILogger logger,
            IFuturesSymbolRestClient symbolRestClient,
            IListenKeyRestClient? listenKeyRestClient,
            IBalanceRestClient balanceRestClient,
            IBalanceSocketClient? balanceSocketClient,
            IFuturesOrderRestClient futuresOrderRestClient,
            IFuturesOrderSocketClient? futuresOrderSocketClient,
            IUserTradeSocketClient? userTradeSocketClient,
            IPositionSocketClient? positionSocketClient,
            string? userIdentifier,
            FuturesUserDataTrackerConfig config,
            SharedAccountType? accountType = null,
            ExchangeParameters? exchangeParameters = null) : base(logger, symbolRestClient.Exchange, config, userIdentifier)
        {
            // create trackers
            _symbolClient = symbolRestClient;
            _listenKeyClient = listenKeyRestClient;
            _exchangeParameters = exchangeParameters;

            _tradingMode = accountType == SharedAccountType.PerpetualInverseFutures ? TradingMode.PerpetualInverse :
                              accountType == SharedAccountType.DeliveryLinearFutures ? TradingMode.DeliveryLinear :
                              accountType == SharedAccountType.DeliveryInverseFutures ? TradingMode.DeliveryInverse :
                              TradingMode.PerpetualLinear;

            var trackers = new List<UserDataItemTracker>();

            var balanceTracker = new BalanceTracker(logger, SymbolTracker, balanceRestClient, balanceSocketClient, accountType ?? SharedAccountType.PerpetualLinearFutures, config.BalancesConfig, exchangeParameters);
            Balances = balanceTracker;
            trackers.Add(balanceTracker);

            var orderTracker = new FuturesOrderTracker(logger, SymbolTracker, futuresOrderRestClient, futuresOrderSocketClient, config.OrdersConfig, config.TrackedSymbols, config.OnlyTrackProvidedSymbols, exchangeParameters);
            Orders = orderTracker;
            trackers.Add(orderTracker);

            var positionTracker = new PositionTracker(logger, SymbolTracker, futuresOrderRestClient, positionSocketClient, config.PositionConfig, config.TrackedSymbols, config.OnlyTrackProvidedSymbols, WebsocketPositionUpdatesAreFullSnapshots, exchangeParameters);
            Positions = positionTracker;
            trackers.Add(positionTracker);

            if (config.TrackTrades)
            {
                var tradeTracker = new FuturesUserTradeTracker(logger, SymbolTracker, futuresOrderRestClient, userTradeSocketClient, config.UserTradesConfig, config.TrackedSymbols, config.OnlyTrackProvidedSymbols, exchangeParameters);
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
            var symbolResult = await _symbolClient.GetFuturesSymbolsAsync(new GetSymbolsRequest(_tradingMode, exchangeParameters: _exchangeParameters)).ConfigureAwait(false);
            if (!symbolResult)
            {
                _logger.LogWarning("Failed to start UserFuturesDataTracker; symbols request failed: {Error}", symbolResult.Error);
                return symbolResult;
            }

            if (_listenKeyClient != null)
            {
                var lkResult = await _listenKeyClient.StartListenKeyAsync(new StartListenKeyRequest(_tradingMode, exchangeParameters: _exchangeParameters)).ConfigureAwait(false);
                if (!lkResult)
                {
                    _logger.LogWarning("Failed to start UserFuturesDataTracker; listen key request failed: {Error}", lkResult.Error);
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
                try { await Task.Delay(interval, _cts.Token).ConfigureAwait(false); } catch (Exception) 
                {
                    break;
                }

                var result = await _listenKeyClient!.KeepAliveListenKeyAsync(new KeepAliveListenKeyRequest(_listenKey!, _tradingMode)).ConfigureAwait(false);
                if (!result)
                    _logger.LogWarning("Listen key keep alive failed: " + result.Error);

                // If failed shorten the delay to allow a couple more retries
                interval = result ? TimeSpan.FromMinutes(30) : TimeSpan.FromMinutes(5);
            }
        }
    }
}
