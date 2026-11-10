using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sessions;
using CryptoExchange.Net.Trackers.Sessions.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Trackers.Sessions
{
    public sealed class UserTrackingBuilder<TSession, TEnvironment>
        where TSession : Session
        where TEnvironment : TradeEnvironment
    {
        public string UserIdentifier { get; }
        public TEnvironment? Environment { get; }

        internal ITrackerFactory TrackerFactory { get; }

        internal Dictionary<string, Func<TSession, ISessionFeature>>
            Features
        { get; } = new();

        internal UserTrackingBuilder(
            string userIdentifier,
            TEnvironment? environment,
            ITrackerFactory trackerFactory)
        {
            UserIdentifier = userIdentifier;
            Environment = environment;
            TrackerFactory = trackerFactory;
        }

        internal UserTrackingBuilder<TSession, TEnvironment> RegisterFeature(
            string name,
            Func<TSession, ISessionFeature> create)
        {
            if (Features.ContainsKey(name))
            {
                throw new InvalidOperationException(
                    $"Feature '{name}' is already configured for this user.");
            }

            Features.Add(name, create);
            return this;
        }
    }
}
