using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sessions;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.Sessions.Features;
using CryptoExchange.Net.Trackers.Sessions.Interfaces;
using CryptoExchange.Net.Trackers.UserData.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Trackers.Sessions
{
    public static class UserTrackingExtensions
    {
        public static UserTrackingBuilder<TSession, TEnvironment>
            AddSpotUserDataTracking<TSession, TEnvironment>(
                this UserTrackingBuilder<TSession, TEnvironment> builder,
                SpotUserDataTrackerConfig? config = null)
            where TSession : Session, ISpotUserDataUpdateHandler
            where TEnvironment : TradeEnvironment
        {
            if (builder.TrackerFactory
                is not ISpotUserDataTrackerFactory<TEnvironment> factory)
            {
                throw new NotSupportedException(
                    "This exchange does not support spot user data tracking.");
            }

            var userIdentifier = builder.UserIdentifier;
            var environment = builder.Environment;

            return builder.RegisterFeature("spot", session =>
                new SpotUserDataFeature(
                    userIdentifier,
                    ct => factory.TryCreateUserSpotDataTrackerAsync(
                        userIdentifier, config, environment, ct),
                    session));
        }

        public static UserTrackingBuilder<TSession, TEnvironment>
            AddFuturesUserDataTracking<TSession, TEnvironment>(
                this UserTrackingBuilder<TSession, TEnvironment> builder,
                TradingMode tradingMode,
                FuturesUserDataTrackerConfig? config = null)
            where TSession : Session, IFuturesUserDataUpdateHandler
            where TEnvironment : TradeEnvironment
        {
            if (builder.TrackerFactory
                is not IFuturesUserDataTrackerFactory<TEnvironment> factory)
            {
                throw new NotSupportedException(
                    "This exchange does not support futures user data tracking.");
            }

            var userIdentifier = builder.UserIdentifier;
            var environment = builder.Environment;

            return builder.RegisterFeature($"futures:{tradingMode}", session =>
                new FuturesUserDataFeature(
                    userIdentifier,
                    tradingMode,
                    ct => factory.TryCreateUserFuturesDataTrackerAsync(
                        userIdentifier, tradingMode, config, environment, ct),
                    session));
        }
    }
}
