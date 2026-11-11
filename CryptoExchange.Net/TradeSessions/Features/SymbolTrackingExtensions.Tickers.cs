using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.TradeSessions;
using CryptoExchange.Net.TradeSessions.Interfaces;

namespace CryptoExchange.Net.TradeSessions.Features
{
    public static partial class SymbolTrackingExtensions
    {
        /// <summary>
        /// Add live price ticker updates to <typeparamref name="TStrategy"/>. Requires <typeparamref name="TStrategy"/>
        /// to implement <see cref="ITickerUpdateHandler"/>.
        /// </summary>
        /// <typeparam name="TStrategy">The implementation type.</typeparam>
        /// <param name="builder">The builder</param>
        public static TradeSessionSymbolBuilder<TStrategy> AddTickerUpdates<TStrategy>(
            this TradeSessionSymbolBuilder<TStrategy> builder)
            where TStrategy : TradeSession, ITickerUpdateHandler
            => AddTickerUpdates<TStrategy, TradeSessionSymbolBuilder<TStrategy>>(
                builder);

        /// <summary>
        /// Add live price ticker updates to <typeparamref name="TStrategy"/>. Requires <typeparamref name="TStrategy"/>
        /// to implement <see cref="ITickerUpdateHandler"/>.
        /// </summary>
        /// <typeparam name="TBuilder">The builder.</typeparam>
        /// <typeparam name="TStrategy">The implementation type.</typeparam>
        /// <param name="builder">The builder</param>
        public static TBuilder AddTickerUpdates<TStrategy, TBuilder>(
            this TradingSessionSymbolBuilder<TStrategy, TBuilder> builder)
            where TStrategy : TradeSession, ITickerUpdateHandler
            where TBuilder : TradingSessionSymbolBuilder<TStrategy, TBuilder>, ITickerTrackingBuilder
        {
            return builder.AddFeature("tickers", provider =>
                TickerTrackingFeature.Bind<TStrategy>(provider, builder.Symbol, builder.Environment));
        }
    }
}
