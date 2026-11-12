using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.UserData.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using CryptoExchange.Net.TradeSessions.Interfaces;

namespace CryptoExchange.Net.TradeSessions
{
    /// <summary>
    /// Trade session user reference
    /// </summary>
    /// <param name="UserIdentifier">The user identifier</param>
    /// <param name="Environment">The trading environment</param>
    public sealed record TradeSessionUserDefinition(
        string UserIdentifier,
        TradeEnvironment? Environment);

    /// <summary>
    /// Trade session user
    /// </summary>
    public abstract class TradeSessionUser : ITradeSessionUser
    {
        private readonly Func<IUserSpotDataTracker> _getSpotTracker;
        private readonly Func<TradingMode, IUserFuturesDataTracker> _getFuturesTracker;
        /// <summary>
        /// User identifier
        /// </summary>
        public string UserIdentifier { get; }
        /// <summary>
        /// Trading environment
        /// </summary>
        public TradeEnvironment? Environment { get; }

        /// <summary>
        /// The registered spot user data tracker
        /// </summary>
        public IUserSpotDataTracker GetSpotUserDataTracker()
            => _getSpotTracker();

        /// <summary>
        /// Get the registered futured user data tracker
        /// </summary>
        public IUserFuturesDataTracker GetFuturesUserDataTracker(TradingMode tradingMode)
            => _getFuturesTracker(tradingMode);

        /// <summary>
        /// ctor
        /// </summary>
        protected TradeSessionUser(
            TradeSessionUserContext context)
        {
            UserIdentifier = context.Definition.UserIdentifier;
            Environment = context.Definition.Environment;
            _getSpotTracker = context.GetSpotUserDataTracker;
            _getFuturesTracker = context.GetFuturesUserDataTracker;
        }
    }

    /// <summary>
    /// Trade session user collection
    /// </summary>
    public sealed class TradeSessionUsers<TUser>
        where TUser : class, ITradeSessionUser
    {
        private readonly Dictionary<string, TUser> _users;

        /// <summary>
        /// ctor
        /// </summary>
        public TradeSessionUsers(IEnumerable<TUser> users)
        {
            _users = users.ToDictionary(
                user => user.UserIdentifier,
                StringComparer.Ordinal);
        }

        /// <summary>
        /// Get a user by identifier
        /// </summary>
        /// <exception cref="KeyNotFoundException"></exception>
        public TUser this[string userIdentifier] =>
            _users.TryGetValue(userIdentifier, out var user)
                ? user
                : throw new KeyNotFoundException(
                    $"User '{userIdentifier}' is not configured.");
    }
}
