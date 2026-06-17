using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.UserData.Objects;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.UserData.Interfaces
{
    /// <summary>
    /// Data tracker interface
    /// </summary>
    public interface IUserDataTracker<T>
    {
        /// <summary>
        /// Whether the tracker is currently fully connected
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// On connection status change. Might trigger multiple times with the same status depending on the underlying subscriptions.
        /// </summary>
        event Action<bool>? OnConnectedChange;

        /// <summary>
        /// Currently tracker values
        /// </summary>
        T[] Values { get; }

        /// <summary>
        /// On data update
        /// </summary>
        event Func<UserDataUpdate<T[]>, Task>? OnUpdate;

        /// <summary>
        /// Stream updates as they are received
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        IAsyncEnumerable<T> StreamUpdatesAsync(CancellationToken ct = default);
    }
}
