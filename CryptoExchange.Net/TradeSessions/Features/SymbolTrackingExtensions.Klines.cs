using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.TradeSessions;
using CryptoExchange.Net.TradeSessions.Interfaces;
using CryptoExchange.Net.TradeSessions.Interfaces.Handlers;

namespace CryptoExchange.Net.TradeSessions.Features
{
    public static partial class SymbolTrackingExtensions
    {
        /// <summary>
        /// Add live kline updates to <typeparamref name="TTradeSession"/>. Requires <typeparamref name="TTradeSession"/> to
        /// implement <see cref="IKlineUpdateHandler"/>.
        /// </summary>
        /// <typeparam name="TTradeSession">The implementation type.</typeparam>
        /// <param name="builder">The builder</param>
        /// <param name="interval">The kline interval</param>
        public static TradeSessionSymbolBuilder<TTradeSession> AddKlineUpdates<TTradeSession>(
            this TradeSessionSymbolBuilder<TTradeSession> builder, SharedKlineInterval interval)
            where TTradeSession : TradeSession, IKlineUpdateHandler
            => AddKlineUpdates<TTradeSession, TradeSessionSymbolBuilder<TTradeSession>>(
                builder, interval);

        /// <summary>
        /// Add live kline updates to <typeparamref name="TTradeSession"/>. Requires <typeparamref name="TTradeSession"/> to
        /// implement <see cref="IKlineUpdateHandler"/>.
        /// </summary>
        /// <typeparam name="TTradeSession">The implementation type.</typeparam>
        /// <typeparam name="TBuilder">The builder</typeparam>
        /// <param name="builder">The builder</param>
        /// <param name="interval">The kline interval</param>
        public static TBuilder AddKlineUpdates<TTradeSession, TBuilder>(
            this TradeSessionSymbolBuilder<TTradeSession, TBuilder> builder, SharedKlineInterval interval)
            where TTradeSession : TradeSession, IKlineUpdateHandler
            where TBuilder : TradeSessionSymbolBuilder<TTradeSession, TBuilder>, IKlineTrackingBuilder
        {
            return builder.AddFeature("klines", provider =>
                KlineTrackingFeature.Bind<TTradeSession>(provider, builder.Symbol, builder.Environment, interval));
        }

        /// <summary>
        /// Queue kline updates and process them sequentially with an asynchronous handler.
        /// The queue is unbounded unless a maximum size is specified.
        /// </summary>
        /// <typeparam name="TTradeSession">
        /// The implementation type, implementing <see cref="IAsyncKlineUpdateHandler"/>
        /// </typeparam>
        /// <param name="builder">The builder</param>
        /// <param name="interval">The kline interval</param>
        /// <param name="maxQueuedItems">Maximum pending updates, or null for an unbounded queue</param>
        /// <param name="fullBehavior">Bounded queue overflow behaviour; defaults to DropWrite</param>
        public static TradeSessionSymbolBuilder<TTradeSession> AddAsyncKlineUpdates<TTradeSession>(
            this TradeSessionSymbolBuilder<TTradeSession> builder,
            SharedKlineInterval interval,
            int? maxQueuedItems = null,
            QueueFullBehavior? fullBehavior = null)
            where TTradeSession : TradeSession, IAsyncKlineUpdateHandler
            => AddAsyncKlineUpdates<TTradeSession, TradeSessionSymbolBuilder<TTradeSession>>(
                builder, interval, maxQueuedItems, fullBehavior);

        /// <summary>
        /// Queue kline updates and process them sequentially with an asynchronous handler.
        /// The queue is unbounded unless a maximum size is specified.
        /// </summary>
        /// <typeparam name="TTradeSession">
        /// The implementation type, implementing <see cref="IAsyncKlineUpdateHandler"/>
        /// </typeparam>
        /// <typeparam name="TBuilder">The builder type</typeparam>
        /// <param name="builder">The builder</param>
        /// <param name="interval">The kline interval</param>
        /// <param name="maxQueuedItems">Maximum pending updates, or null for an unbounded queue</param>
        /// <param name="fullBehavior">Bounded queue overflow behaviour; defaults to DropWrite</param>
        public static TBuilder AddAsyncKlineUpdates<TTradeSession, TBuilder>(
            this TradeSessionSymbolBuilder<TTradeSession, TBuilder> builder,
            SharedKlineInterval interval,
            int? maxQueuedItems = null,
            QueueFullBehavior? fullBehavior = null)
            where TTradeSession : TradeSession, IAsyncKlineUpdateHandler
            where TBuilder : TradeSessionSymbolBuilder<TTradeSession, TBuilder>, IKlineTrackingBuilder
        {
            ValidateQueueCapacity(maxQueuedItems);
            return builder.AddFeature("klines", provider =>
                KlineTrackingFeature.BindAsync<TTradeSession>(
                    provider, builder.Symbol, builder.Environment, interval, maxQueuedItems, fullBehavior));
        }
    }
}
