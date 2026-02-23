using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.UserData.ItemTrackers;
using CryptoExchange.Net.Trackers.UserData.Objects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.UserData
{
    /// <summary>
    /// User data tracker
    /// </summary>
    public abstract class UserDataTracker
    {
        /// <summary>
        /// Logger
        /// </summary>
        protected readonly ILogger _logger;
        /// <summary>
        /// Listen key to use for subscriptions
        /// </summary>
        protected string? _listenKey;
        /// <summary>
        /// Cts
        /// </summary>
        protected CancellationTokenSource? _cts;

        /// <summary>
        /// List of data trackers
        /// </summary>
        protected abstract UserDataItemTracker[] DataTrackers { get; }

        /// <summary>
        /// Symbol tracker
        /// </summary>
        protected internal UserDataSymbolTracker SymbolTracker { get; }

        /// <inheritdoc />
        public string? UserIdentifier { get; }

        /// <summary>
        /// Connected status changed
        /// </summary>
        public event Action<UserDataType, bool>? OnConnectedChange;

        /// <summary>
        /// Exchange name
        /// </summary>
        public string Exchange { get; }

        /// <summary>
        /// Whether all trackers are full connected
        /// </summary>
        public bool Connected => DataTrackers.All(x => x.Connected);

        /// <summary>
        /// Currently tracked symbols
        /// </summary>
        public IEnumerable<SharedSymbol> TrackedSymbols => SymbolTracker.GetTrackedSymbols();

        /// <summary>
        /// ctor
        /// </summary>
        public UserDataTracker(
            ILogger logger,
            string exchange,
            UserDataTrackerConfig config,
            string? userIdentifier)
        {
            _logger = logger;

            SymbolTracker = new UserDataSymbolTracker(logger, config);
            Exchange = exchange;
            UserIdentifier = userIdentifier;
        }

        /// <summary>
        /// Start the data tracker
        /// </summary>
        public async Task<CallResult> StartAsync()
        {
            _cts = new CancellationTokenSource();

            foreach(var tracker in DataTrackers)
                tracker.OnConnectedChange += (x) => OnConnectedChange?.Invoke(tracker.DataType, x);            

            var result = await DoStartAsync().ConfigureAwait(false);
            if (!result)
                return result;

            var tasks = new List<Task<CallResult>>();
            foreach (var dataTracker in DataTrackers)
                tasks.Add(dataTracker.StartAsync(_listenKey));

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
            _cts?.Cancel();

            var tasks = new List<Task>();
            foreach (var dataTracker in DataTrackers)
                tasks.Add(dataTracker.StopAsync());

            await DoStopAsync().ConfigureAwait(false);
            await Task.WhenAll(tasks).ConfigureAwait(false);
            _logger.LogDebug("Stopped UserDataTracker");
        }

        /// <summary>
        /// Stop implementation
        /// </summary>
        /// <returns></returns>
        protected virtual Task DoStopAsync() => Task.CompletedTask;
    }
}
