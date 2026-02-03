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
            ExchangeParameters? exchangeParameters = null) : base(logger, config, userIdentifier)
        {
            // create trackers
            _symbolClient = symbolRestClient;
            _listenKeyClient = listenKeyRestClient;
            _exchangeParameters = exchangeParameters;

            var trackers = new List<UserDataItemTracker>();

            var balanceTracker = new BalanceTracker(logger, balanceRestClient, balanceSocketClient, config.BalancesConfig, exchangeParameters);
            Balances = balanceTracker;
            trackers.Add(balanceTracker);

            var orderTracker = new FuturesOrderTracker(logger, futuresOrderRestClient, futuresOrderSocketClient, config.OrdersConfig, config.TrackedSymbols, config.OnlyTrackProvidedSymbols, exchangeParameters);
            Orders = orderTracker;
            trackers.Add(orderTracker);

            var positionTracker = new PositionTracker(logger, futuresOrderRestClient, positionSocketClient, config.PositionConfig, config.TrackedSymbols, config.OnlyTrackProvidedSymbols, WebsocketPositionUpdatesAreFullSnapshots, exchangeParameters);
            Positions = positionTracker;
            trackers.Add(positionTracker);

            if (config.TrackTrades)
            {
                var tradeTracker = new FuturesUserTradeTracker(logger, futuresOrderRestClient, userTradeSocketClient, config.UserTradesConfig, config.TrackedSymbols, config.OnlyTrackProvidedSymbols, exchangeParameters);
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
            var symbolResult = await _symbolClient.GetFuturesSymbolsAsync(new GetSymbolsRequest(exchangeParameters: _exchangeParameters)).ConfigureAwait(false);
            if (!symbolResult)
            {
                _logger.LogWarning("Failed to start UserFuturesDataTracker; symbols request failed: {Error}", symbolResult.Error!.Message);
                return symbolResult;
            }

            if (_listenKeyClient != null)
            {
                var lkResult = await _listenKeyClient.StartListenKeyAsync(new StartListenKeyRequest(exchangeParameters: _exchangeParameters)).ConfigureAwait(false);
                if (!lkResult)
                {
                    _logger.LogWarning("Failed to start UserFuturesDataTracker; listen key request failed: {Error}", lkResult.Error!.Message);
                    return lkResult;
                }

                _listenKey = lkResult.Data;
            }

            return CallResult.SuccessResult;
        }
    }
}
