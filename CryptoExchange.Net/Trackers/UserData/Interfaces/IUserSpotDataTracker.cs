using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.UserData.Objects;
using System;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.UserData.Interfaces
{
    /// <summary>
    /// User data tracker
    /// </summary>
    public interface IUserSpotDataTracker
    {
        /// <summary>
        /// User identifier
        /// </summary>
        string? UserIdentifier { get; }

        /// <summary>
        /// Whether the tracker is currently fully connected
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Balances tracker
        /// </summary>
        IUserDataTracker<SharedBalance> Balances { get; }
        /// <summary>
        /// Orders tracker
        /// </summary>
        IUserDataTracker<SharedSpotOrder> Orders { get; }
        /// <summary>
        /// Trades tracker
        /// </summary>
        IUserDataTracker<SharedUserTrade>? Trades { get; }

        /// <summary>
        /// On connection status change
        /// </summary>
        event Action<UserDataType, bool>? OnConnectedChange;

        /// <summary>
        /// Start tracking user data
        /// </summary>
        Task<CallResult> StartAsync();
        /// <summary>
        /// Stop tracking data
        /// </summary>
        /// <returns></returns>
        Task StopAsync();
    }
}