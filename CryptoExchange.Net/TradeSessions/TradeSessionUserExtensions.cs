using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.TradeSessions.Features;
using CryptoExchange.Net.Trackers.UserData.Objects;
using CryptoExchange.Net.TradeSessions.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.TradeSessions
{
    /// <summary>
    /// Trade session user extensions
    /// </summary>
    public static class TradeSessionUserExtensions
    {
        /// <summary>
        /// Add spot user data tracking for this user. This tracks the live spot trading data of a user, including
        /// balances and orders.
        /// </summary>
        /// <param name="builder">Builder</param>
        /// <param name="config">The spot data tracking configuration</param>
        public static TradeSessionUserBuilder<TTradeSession, TEnvironment> AddSpotUserDataTracking<TTradeSession, TEnvironment>(
            this TradeSessionUserBuilder<TTradeSession, TEnvironment> builder, SpotUserDataTrackerConfig? config = null)
            where TTradeSession : TradeSession
            where TEnvironment : TradeEnvironment
        {
            if (builder.TrackerFactory is not ISpotUserDataTrackerFactory<TEnvironment> factory)
                throw new NotSupportedException("This exchange does not support spot user data tracking.");

            var userIdentifier = builder.UserIdentifier;
            var environment = builder.Environment;

            return builder.RegisterFeature("spot", session =>
                new SpotUserDataFeature(
                    userIdentifier, 
                    ct => factory.TryCreateUserSpotDataTrackerAsync(userIdentifier, config, environment, ct), 
                    session));
        }

        /// <summary>
        /// Add futures user data tracking for this user. This tracks the live futures trading data of a user, including
        /// balances, orders and positions.
        /// </summary>
        /// <typeparam name="TTradeSession"></typeparam>
        /// <typeparam name="TEnvironment"></typeparam>
        /// <param name="builder">The builder</param>
        /// <param name="tradingMode">The trading mode, depending on the exchange multiple different futures API's might
        /// be available</param>
        /// <param name="config">The futures data tracking configuration</param>
        public static TradeSessionUserBuilder<TTradeSession, TEnvironment> AddFuturesUserDataTracking<TTradeSession, TEnvironment>(
                this TradeSessionUserBuilder<TTradeSession, TEnvironment> builder,
                TradingMode tradingMode,
                FuturesUserDataTrackerConfig? config = null)
            where TTradeSession : TradeSession
            where TEnvironment : TradeEnvironment
        {
            if (builder.TrackerFactory is not IFuturesUserDataTrackerFactory<TEnvironment> factory)
                throw new NotSupportedException("This exchange does not support futures user data tracking.");

            var userIdentifier = builder.UserIdentifier;
            var environment = builder.Environment;

            return builder.RegisterFeature($"futures:{tradingMode}", session =>
                new FuturesUserDataFeature(
                    userIdentifier,
                    tradingMode,
                    ct => factory.TryCreateUserFuturesDataTrackerAsync(userIdentifier, tradingMode, config, environment, ct),
                    session));
        }
    }
}
