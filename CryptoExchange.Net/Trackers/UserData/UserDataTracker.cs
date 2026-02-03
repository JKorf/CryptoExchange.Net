using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Trackers.UserData.ItemTrackers;
using CryptoExchange.Net.Trackers.UserData.Objects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.UserData
{
    /// <summary>
    /// User data tracker
    /// </summary>
    public abstract class UserDataTracker
    {
#warning max age for data?
        /// <summary>
        /// Logger
        /// </summary>
        protected readonly ILogger _logger;
        /// <summary>
        /// Listen key to use for subscriptions
        /// </summary>
        protected string? _listenKey;
        /// <summary>
        /// List of data trackers
        /// </summary>
        protected abstract UserDataItemTracker[] DataTrackers { get; }

        /// <inheritdoc />
        public string? UserIdentifier { get; }

        /// <summary>
        /// Connected status changed
        /// </summary>
        public event Action<UserDataType, bool>? OnConnectedChange;

        /// <summary>
        /// Whether all trackers are full connected
        /// </summary>
        public bool Connected => DataTrackers.All(x => x.Connected);

        /// <summary>
        /// ctor
        /// </summary>
        public UserDataTracker(
            ILogger logger,
            UserDataTrackerConfig config,
            string? userIdentifier)
        {
            if (config.OnlyTrackProvidedSymbols && !config.TrackedSymbols.Any())
                throw new ArgumentException(nameof(config.TrackedSymbols), "Conflicting options; `OnlyTrackProvidedSymbols` but no symbols specific in `TrackedSymbols`");

            _logger = logger;

            UserIdentifier = userIdentifier;
        }

        /// <summary>
        /// Start the data tracker
        /// </summary>
        public async Task<CallResult> StartAsync()
        {
            foreach(var tracker in DataTrackers)
                tracker.OnConnectedChange += (x) => OnConnectedChange?.Invoke(tracker.DataType, x);            

            var result = await DoStartAsync().ConfigureAwait(false);
            if (!result)
                return result;

            var tasks = new List<Task<CallResult>>();
            foreach (var dataTracker in DataTrackers)
            {
                tasks.Add(dataTracker.StartAsync(_listenKey));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
            if (!tasks.All(x => x.Result.Success))
            {
                await Task.WhenAll(DataTrackers.Select(x => x.StopAsync())).ConfigureAwait(false);
                return tasks.First(x => !x.Result.Success).Result;
            }

            return CallResult.SuccessResult;
        }

        /// <summary>
        /// Implementation specific start logic
        /// </summary>
        protected abstract Task<CallResult> DoStartAsync();

        /// <summary>
        /// Stop the data tracker
        /// </summary>
        public async Task StopAsync()
        {
            _logger.LogDebug("Stopping UserDataTracker");
            var tasks = new List<Task>();
            foreach (var dataTracker in DataTrackers)
                tasks.Add(dataTracker.StopAsync());

            await Task.WhenAll(tasks).ConfigureAwait(false);
            _logger.LogDebug("Stopped UserDataTracker");
        }
    }
}
