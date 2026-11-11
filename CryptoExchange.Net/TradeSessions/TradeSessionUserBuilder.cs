using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.TradeSessions.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.TradeSessions
{
    /// <summary>
    /// Trading session user configuration builder
    /// </summary>
    public sealed class TradeSessionUserBuilder<TStrategy, TEnvironment>
        where TStrategy : TradeSession
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

        internal Dictionary<string, Func<TStrategy, ITradingSessionFeature>> Features { get; } = new();

        internal TradeSessionUserBuilder(
            string userIdentifier,
            TEnvironment? environment,
            ITrackerFactory trackerFactory)
        {
            UserIdentifier = userIdentifier;
            Environment = environment;
            TrackerFactory = trackerFactory;
        }

        internal TradeSessionUserBuilder<TStrategy, TEnvironment> RegisterFeature(
            string name,
            Func<TStrategy, ITradingSessionFeature> create)
        {
            if (Features.ContainsKey(name))
                throw new InvalidOperationException($"Feature '{name}' is already configured for this user.");

            Features.Add(name, create);
            return this;
        }
    }
}
