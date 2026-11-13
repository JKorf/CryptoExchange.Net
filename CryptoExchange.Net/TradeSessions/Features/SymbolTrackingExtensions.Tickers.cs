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
        /// Add live price ticker updates to <typeparamref name="TTradeSession"/>. Requires <typeparamref name="TTradeSession"/>
        /// to implement <see cref="ITickerUpdateHandler"/>.
        /// </summary>
        /// <typeparam name="TTradeSession">The implementation type.</typeparam>
        /// <param name="builder">The builder</param>
        public static TradeSessionSymbolBuilder<TTradeSession> AddTickerUpdates<TTradeSession>(
            this TradeSessionSymbolBuilder<TTradeSession> builder)
            where TTradeSession : TradeSession, ITickerUpdateHandler
            => AddTickerUpdates<TTradeSession, TradeSessionSymbolBuilder<TTradeSession>>(
                builder);

        /// <summary>
        /// Add live price ticker updates to <typeparamref name="TTradeSession"/>. Requires <typeparamref name="TTradeSession"/>
        /// to implement <see cref="ITickerUpdateHandler"/>.
        /// </summary>
        /// <typeparam name="TBuilder">The builder.</typeparam>
        /// <typeparam name="TTradeSession">The implementation type.</typeparam>
        /// <param name="builder">The builder</param>
        public static TBuilder AddTickerUpdates<TTradeSession, TBuilder>(
            this TradeSessionSymbolBuilder<TTradeSession, TBuilder> builder)
            where TTradeSession : TradeSession, ITickerUpdateHandler
            where TBuilder : TradeSessionSymbolBuilder<TTradeSession, TBuilder>, ITickerTrackingBuilder
        {
            return builder.AddFeature("tickers", provider =>
                TickerTrackingFeature.Bind<TTradeSession>(provider, builder.Symbol, builder.Environment));
        }

        /// <summary>
        /// Queue ticker updates and process them sequentially with an asynchronous handler.
        /// The queue is unbounded unless a maximum size is specified.
        /// </summary>
        /// <typeparam name="TTradeSession">
        /// The implementation type, implementing <see cref="IAsyncTickerUpdateHandler"/>
        /// </typeparam>
        /// <param name="builder">The builder</param>
        /// <param name="maxQueuedItems">Maximum pending updates, or null for an unbounded queue</param>
        /// <param name="fullBehavior">Bounded queue overflow behaviour; defaults to DropWrite</param>
        public static TradeSessionSymbolBuilder<TTradeSession> AddAsyncTickerUpdates<TTradeSession>(
            this TradeSessionSymbolBuilder<TTradeSession> builder,
            int? maxQueuedItems = null,
            QueueFullBehavior? fullBehavior = null)
            where TTradeSession : TradeSession, IAsyncTickerUpdateHandler
            => AddAsyncTickerUpdates<TTradeSession, TradeSessionSymbolBuilder<TTradeSession>>(
                builder, maxQueuedItems, fullBehavior);

        /// <summary>
        /// Queue ticker updates and process them sequentially with an asynchronous handler.
        /// The queue is unbounded unless a maximum size is specified.
        /// </summary>
        /// <typeparam name="TTradeSession">
        /// The implementation type, implementing <see cref="IAsyncTickerUpdateHandler"/>
        /// </typeparam>
        /// <typeparam name="TBuilder">The builder type</typeparam>
        /// <param name="builder">The builder</param>
        /// <param name="maxQueuedItems">Maximum pending updates, or null for an unbounded queue</param>
        /// <param name="fullBehavior">Bounded queue overflow behaviour; defaults to DropWrite</param>
        public static TBuilder AddAsyncTickerUpdates<TTradeSession, TBuilder>(
            this TradeSessionSymbolBuilder<TTradeSession, TBuilder> builder,
            int? maxQueuedItems = null,
            QueueFullBehavior? fullBehavior = null)
            where TTradeSession : TradeSession, IAsyncTickerUpdateHandler
            where TBuilder : TradeSessionSymbolBuilder<TTradeSession, TBuilder>, ITickerTrackingBuilder
        {
            ValidateQueueCapacity(maxQueuedItems);
            return builder.AddFeature("tickers", provider =>
                TickerTrackingFeature.BindAsync<TTradeSession>(
                    provider, builder.Symbol, builder.Environment, maxQueuedItems, fullBehavior));
        }
    }
}
