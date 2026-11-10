using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.UserData.Interfaces;
using CryptoExchange.Net.Trackers.UserData.ItemTrackers;
using CryptoExchange.Net.Trackers.UserData.Objects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.UserData
{
    /// <summary>
    /// User futures data tracker
    /// </summary>
    public abstract class UserFuturesDataTracker : UserDataTracker, IUserFuturesDataTracker
    {
        private readonly IGetFuturesSymbolsRest _symbolClient;
        private readonly ExchangeParameters? _exchangeParameters;
        private readonly TradingMode _tradingMode;

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
            ISharedApiClientBase client,
            string? userIdentifier,
            FuturesUserDataTrackerConfig config,
            SharedAccountType? accountType = null,
            ExchangeParameters? exchangeParameters = null) : base(logger, client.Exchange, config, userIdentifier)
        {
            // create trackers
            _exchangeParameters = exchangeParameters;

            _tradingMode = accountType == SharedAccountType.PerpetualInverseFutures ? TradingMode.PerpetualInverse :
                              accountType == SharedAccountType.DeliveryLinearFutures ? TradingMode.DeliveryLinear :
                              accountType == SharedAccountType.DeliveryInverseFutures ? TradingMode.DeliveryInverse :
                              TradingMode.PerpetualLinear;

            _symbolClient = client.GetCapability<IGetFuturesSymbolsRest>(_tradingMode)?.Capability ?? throw new ArgumentException("No Futures symbols capability available");

            var trackers = new List<UserDataItemTracker>();

            var balanceTracker = new BalanceTracker(
                logger,
                SymbolTracker, 
                client.GetCapability<IGetBalancesRest>(_tradingMode)?.Capability ?? throw new ArgumentException("No Futures balances capability available"),
                client.GetCapability<ISubscribeBalancesSocket>(_tradingMode)?.Capability,
                accountType ?? SharedAccountType.PerpetualLinearFutures, 
                config.BalancesConfig, 
                exchangeParameters);
            Balances = balanceTracker;
            trackers.Add(balanceTracker);

            var orderTracker = new FuturesOrderTracker(
                logger, 
                SymbolTracker,
                client.GetCapability<IGetOpenFuturesOrdersRest>(_tradingMode)?.Capability ?? throw new ArgumentException("No Futures open orders capability available"),
                client.GetCapability<IGetClosedFuturesOrdersRest>(_tradingMode)?.Capability ?? throw new ArgumentException("No Futures closed orders capability available"),
                client.GetCapability<ISubscribeFuturesOrdersSocket>(_tradingMode)?.Capability,
                config.OrdersConfig, 
                config.TrackedSymbols, 
                config.OnlyTrackProvidedSymbols, 
                exchangeParameters);
            Orders = orderTracker;
            trackers.Add(orderTracker);

            var positionTracker = new PositionTracker(
                logger,
                SymbolTracker,
                client.GetCapability<IGetPositionsRest>(_tradingMode)?.Capability ?? throw new ArgumentException("No positions capability available"),
                client.GetCapability<ISubscribePositionsSocket>(_tradingMode)?.Capability,
                config.PositionConfig,
                config.TrackedSymbols,
                config.OnlyTrackProvidedSymbols,
                WebsocketPositionUpdatesAreFullSnapshots,
                exchangeParameters);
            Positions = positionTracker;
            trackers.Add(positionTracker);

            if (config.TrackTrades)
            {
                var tradeTracker = new FuturesUserTradeTracker(
                    logger, 
                    SymbolTracker,
                    client.GetCapability<IGetFuturesUserTradeHistoryRest>(_tradingMode)?.Capability ?? throw new ArgumentException("No futures user trade history capability available"),
                    client.GetCapability<ISubscribeUserTradesSocket>(_tradingMode)?.Capability,
                    config.UserTradesConfig, 
                    config.TrackedSymbols, 
                    config.OnlyTrackProvidedSymbols, 
                    exchangeParameters);
                Trades = tradeTracker;
                trackers.Add(tradeTracker);

                orderTracker.OnTradeUpdate += tradeTracker.HandleUpdateAsync;
                tradeTracker.GetTrackedOrderIds = () => orderTracker.Values.Select(x => x.OrderId).ToArray();
            }

            DataTrackers = trackers.ToArray();
        }

        /// <inheritdoc />
        protected override async Task<CallResult> DoStartAsync(CancellationToken ct = default)
        {
            var symbolResult = await _symbolClient.GetFuturesSymbolsAsync(new GetSymbolsRequest(_tradingMode, exchangeParameters: _exchangeParameters), ct).ConfigureAwait(false);
            if (!symbolResult.Success)
            {
                _logger.LogWarning("Failed to start UserFuturesDataTracker; symbols request failed: {Error}", symbolResult.Error);
                return CallResult.Fail(symbolResult.Error);
            }

            return CallResult.Ok();
        }

        /// <summary>
        /// Add symbols to the list of symbols for which data is being tracked
        /// </summary>
        /// <param name="symbols">Symbols to add</param>
        public void AddTrackedSymbolsAsync(IEnumerable<SharedSymbol> symbols)
        {
            if (symbols.Any(x => x.TradingMode == TradingMode.Spot))
                throw new ArgumentException("Spot symbol not allowed in futures tracker", nameof(symbols));

            SymbolTracker.UpdateTrackedSymbols(symbols, true);
        }

        /// <summary>
        /// Remove a symbol from the list of symbols for which data is being tracked. 
        /// Note that the symbol will be added again if new data for that symbol is received, unless the OnlyTrackProvidedSymbols option has been set to true.
        /// </summary>
        /// <param name="symbol">Symbol to remove</param>
        public void RemoveTrackedSymbolAsync(SharedSymbol symbol)
        {
            SymbolTracker.RemoveTrackedSymbol(symbol);

            ((FuturesOrderTracker)Orders).ClearDataForSymbol(symbol);
            ((FuturesUserTradeTracker?)Trades)?.ClearDataForSymbol(symbol);
        }
    }
}
