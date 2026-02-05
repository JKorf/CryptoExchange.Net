using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using CryptoExchange.Net.Trackers.UserData.Interfaces;
using CryptoExchange.Net.Trackers.UserData.Objects;
using CryptoExchange.Net.Trackers.UserData.ItemTrackers;

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

            var balanceTracker = new BalanceTracker(logger, balanceRestClient, balanceSocketClient, SharedAccountType.Spot, config.BalancesConfig, exchangeParameters);
            Balances = balanceTracker;
            trackers.Add(balanceTracker);

            var orderTracker = new SpotOrderTracker(logger, spotOrderRestClient, spotOrderSocketClient, config.OrdersConfig, config.TrackedSymbols, config.OnlyTrackProvidedSymbols, exchangeParameters);
            Orders = orderTracker;
            trackers.Add(orderTracker);

            if (config.TrackTrades)
            {
                var tradeTracker = new SpotUserTradeTracker(logger, spotOrderRestClient, userTradeSocketClient, config.UserTradesConfig, config.TrackedSymbols, config.OnlyTrackProvidedSymbols, exchangeParameters);
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

                _listenKey = lkResult.Data;
            }

            return CallResult.SuccessResult;
        }
    }
}
