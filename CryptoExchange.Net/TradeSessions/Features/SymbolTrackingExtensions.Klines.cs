using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.TradeSessions;
using CryptoExchange.Net.TradeSessions.Interfaces;

namespace CryptoExchange.Net.TradeSessions.Features
{
    public static partial class SymbolTrackingExtensions
    {
        /// <summary>
        /// Add live kline updates to <typeparamref name="TStrategy"/>. Requires <typeparamref name="TStrategy"/> to
        /// implement <see cref="IKlineUpdateHandler"/>.
        /// </summary>
        /// <typeparam name="TStrategy">The implementation type.</typeparam>
        /// <param name="builder">The builder</param>
        /// <param name="interval">The kline interval</param>
        public static TradeSessionSymbolBuilder<TStrategy> AddKlineUpdates<TStrategy>(
            this TradeSessionSymbolBuilder<TStrategy> builder, SharedKlineInterval interval)
            where TStrategy : TradeSession, IKlineUpdateHandler
            => AddKlineUpdates<TStrategy, TradeSessionSymbolBuilder<TStrategy>>(
                builder, interval);

        /// <summary>
        /// Add live kline updates to <typeparamref name="TStrategy"/>. Requires <typeparamref name="TStrategy"/> to
        /// implement <see cref="IKlineUpdateHandler"/>.
        /// </summary>
        /// <typeparam name="TStrategy">The implementation type.</typeparam>
        /// <typeparam name="TBuilder">The builder</typeparam>
        /// <param name="builder">The builder</param>
        /// <param name="interval">The kline interval</param>
        public static TBuilder AddKlineUpdates<TStrategy, TBuilder>(
            this TradingSessionSymbolBuilder<TStrategy, TBuilder> builder, SharedKlineInterval interval)
            where TStrategy : TradeSession, IKlineUpdateHandler
            where TBuilder : TradingSessionSymbolBuilder<TStrategy, TBuilder>, IKlineTrackingBuilder
        {
            return builder.AddFeature("klines", provider =>
                KlineTrackingFeature.Bind<TStrategy>(provider, builder.Symbol, builder.Environment, interval));
        }
    }
}
