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
        /// Add live trade updates to <typeparamref name="TTradeSession"/>. Requires <typeparamref name="TTradeSession"/> to
        /// implement <see cref="ITradeUpdateHandler"/>.
        /// </summary>
        /// <typeparam name="TTradeSession">The implementation type.</typeparam>
        /// <param name="builder">The builder</param>
        public static TradeSessionSymbolBuilder<TTradeSession> AddTradeUpdates<TTradeSession>(
            this TradeSessionSymbolBuilder<TTradeSession> builder)
            where TTradeSession : TradeSession, ITradeUpdateHandler
            => AddTradeUpdates<TTradeSession, TradeSessionSymbolBuilder<TTradeSession>>(
                builder);

        /// <summary>
        /// Add live trade updates to <typeparamref name="TTradeSession"/>. Requires <typeparamref name="TTradeSession"/> to
        /// implement <see cref="ITradeUpdateHandler"/>.
        /// </summary>
        /// <typeparam name="TBuilder">The builder type.</typeparam>
        /// <typeparam name="TTradeSession">The implementation type.</typeparam>
        /// <param name="builder">The builder</param>
        public static TBuilder AddTradeUpdates<TTradeSession, TBuilder>(
            this TradeSessionSymbolBuilder<TTradeSession, TBuilder> builder)
            where TTradeSession : TradeSession, ITradeUpdateHandler
            where TBuilder : TradeSessionSymbolBuilder<TTradeSession, TBuilder>, ITradeTrackingBuilder
        {
            return builder.AddFeature("trades", provider =>
                TradeTrackingFeature.Bind<TTradeSession>(provider, builder.Symbol, builder.Environment));
        }

        /// <summary>
        /// Queue trade updates and process them sequentially with an asynchronous handler.
        /// The queue is unbounded unless a maximum size is specified.
        /// </summary>
        /// <typeparam name="TTradeSession">
        /// The implementation type, implementing <see cref="IAsyncTradeUpdateHandler"/>
        /// </typeparam>
        /// <param name="builder">The builder</param>
        /// <param name="maxQueuedItems">Maximum pending updates, or null for an unbounded queue</param>
        /// <param name="fullBehavior">Bounded queue overflow behaviour; defaults to DropWrite</param>
        public static TradeSessionSymbolBuilder<TTradeSession> AddAsyncTradeUpdates<TTradeSession>(
            this TradeSessionSymbolBuilder<TTradeSession> builder,
            int? maxQueuedItems = null,
            QueueFullBehavior? fullBehavior = null)
            where TTradeSession : TradeSession, IAsyncTradeUpdateHandler
            => AddAsyncTradeUpdates<TTradeSession, TradeSessionSymbolBuilder<TTradeSession>>(
                builder, maxQueuedItems, fullBehavior);

        /// <summary>
        /// Queue trade updates and process them sequentially with an asynchronous handler.
        /// The queue is unbounded unless a maximum size is specified.
        /// </summary>
        /// <typeparam name="TTradeSession">
        /// The implementation type, implementing <see cref="IAsyncTradeUpdateHandler"/>
        /// </typeparam>
        /// <typeparam name="TBuilder">The builder type</typeparam>
        /// <param name="builder">The builder</param>
        /// <param name="maxQueuedItems">Maximum pending updates, or null for an unbounded queue</param>
        /// <param name="fullBehavior">Bounded queue overflow behaviour; defaults to DropWrite</param>
        public static TBuilder AddAsyncTradeUpdates<TTradeSession, TBuilder>(
            this TradeSessionSymbolBuilder<TTradeSession, TBuilder> builder,
            int? maxQueuedItems = null,
            QueueFullBehavior? fullBehavior = null)
            where TTradeSession : TradeSession, IAsyncTradeUpdateHandler
            where TBuilder : TradeSessionSymbolBuilder<TTradeSession, TBuilder>, ITradeTrackingBuilder
        {
            ValidateQueueCapacity(maxQueuedItems);
            return builder.AddFeature("trades", provider =>
                TradeTrackingFeature.BindAsync<TTradeSession>(
                    provider, builder.Symbol, builder.Environment, maxQueuedItems, fullBehavior));
        }
    }
}
