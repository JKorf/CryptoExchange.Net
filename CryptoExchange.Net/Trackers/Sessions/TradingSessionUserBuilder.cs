using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sessions;
using CryptoExchange.Net.Trackers.Sessions.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Trackers.Sessions
{
    /// <summary>
    /// Trading session user configuration builder
    /// </summary>
    public sealed class TradingSessionUserBuilder<TSession, TEnvironment>
        where TSession : TradingSession
        where TEnvironment : TradeEnvironment
    {
        /// <summary>
        /// The user identifier
        /// </summary>
        public string UserIdentifier { get; }
        /// <summary>
        /// The trading environment
        /// </summary>
        public TEnvironment? Environment { get; }

        internal ITrackerFactory TrackerFactory { get; }

        internal Dictionary<string, Func<TSession, ITradingSessionFeature>> Features { get; } = new();

        internal TradingSessionUserBuilder(
            string userIdentifier,
            TEnvironment? environment,
            ITrackerFactory trackerFactory)
        {
            UserIdentifier = userIdentifier;
            Environment = environment;
            TrackerFactory = trackerFactory;
        }

        internal TradingSessionUserBuilder<TSession, TEnvironment> RegisterFeature(
            string name,
            Func<TSession, ITradingSessionFeature> create)
        {
            if (Features.ContainsKey(name))
                throw new InvalidOperationException($"Feature '{name}' is already configured for this user.");

            Features.Add(name, create);
            return this;
        }
    }
}
