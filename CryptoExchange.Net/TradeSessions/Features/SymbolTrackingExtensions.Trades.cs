using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.TradeSessions;
using CryptoExchange.Net.TradeSessions.Interfaces;

namespace CryptoExchange.Net.TradeSessions.Features
{
    public static partial class SymbolTrackingExtensions
    {
        /// <summary>
        /// Add live trade updates to <typeparamref name="TStrategy"/>. Requires <typeparamref name="TStrategy"/> to
        /// implement <see cref="ITradeUpdateHandler"/>.
        /// </summary>
        /// <typeparam name="TStrategy">The implementation type.</typeparam>
        /// <param name="builder">The builder</param>
        public static TradeSessionSymbolBuilder<TStrategy> AddTradeUpdates<TStrategy>(
            this TradeSessionSymbolBuilder<TStrategy> builder)
            where TStrategy : TradeSession, ITradeUpdateHandler
            => AddTradeUpdates<TStrategy, TradeSessionSymbolBuilder<TStrategy>>(
                builder);

        /// <summary>
        /// Add live trade updates to <typeparamref name="TStrategy"/>. Requires <typeparamref name="TStrategy"/> to
        /// implement <see cref="ITradeUpdateHandler"/>.
        /// </summary>
        /// <typeparam name="TBuilder">The builder type.</typeparam>
        /// <typeparam name="TStrategy">The implementation type.</typeparam>
        /// <param name="builder">The builder</param>
        public static TBuilder AddTradeUpdates<TStrategy, TBuilder>(
            this TradingSessionSymbolBuilder<TStrategy, TBuilder> builder)
            where TStrategy : TradeSession, ITradeUpdateHandler
            where TBuilder : TradingSessionSymbolBuilder<TStrategy, TBuilder>, ITradeTrackingBuilder
        {
            return builder.AddFeature("trades", provider =>
                TradeTrackingFeature.Bind<TStrategy>(provider, builder.Symbol, builder.Environment));
        }
    }
}
