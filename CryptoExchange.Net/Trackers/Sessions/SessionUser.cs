using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sessions;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.UserData.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.Trackers.Sessions
{
    public abstract class SessionUser
    {
        private readonly Func<IUserSpotDataTracker> _getSpotTracker;
        private readonly Func<TradingMode, IUserFuturesDataTracker> _getFuturesTracker;

        public string UserIdentifier { get; }
        public TradeEnvironment? Environment { get; }

        public IUserSpotDataTracker SpotUserDataTracker
            => _getSpotTracker();

        public IUserFuturesDataTracker GetFuturesUserDataTracker(
            TradingMode tradingMode)
            => _getFuturesTracker(tradingMode);

        protected SessionUser(
            SessionUserDefinition definition,
            Func<IUserSpotDataTracker> getSpotTracker,
            Func<TradingMode, IUserFuturesDataTracker> getFuturesTracker)
        {
            UserIdentifier = definition.UserIdentifier;
            Environment = definition.Environment;
            _getSpotTracker = getSpotTracker;
            _getFuturesTracker = getFuturesTracker;
        }
    }

    public sealed class SessionUsers<TUser>
        where TUser : SessionUser
    {
        private readonly Dictionary<string, TUser> _users;

        public SessionUsers(IEnumerable<TUser> users)
        {
            _users = users.ToDictionary(
                user => user.UserIdentifier,
                StringComparer.Ordinal);
        }

        public TUser this[string userIdentifier] =>
            _users.TryGetValue(userIdentifier, out var user)
                ? user
                : throw new KeyNotFoundException(
                    $"User '{userIdentifier}' is not configured.");
    }
}
