using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.UserData
{
    public abstract class UserDataTracker
    {
        protected readonly ILogger _logger;

        // State management
        protected DateTime? _startTime = null;
        protected DateTime? _lastPollAttempt = null;
        protected bool _lastPollSuccessful = false;
        protected DateTime? _lastPollTimeOrders = null;
        protected DateTime? _lastPollTimeTrades = null;
        protected DateTime? _lastDataTimeOrdersBeforeDisconnect = null;
        protected DateTime? _lastDataTimeTradesBeforeDisconnect = null;
        protected bool _firstPollDone = false;
        protected bool _wasDisconnected = false;

        // Config
        protected List<SharedSymbol> _symbols = new List<SharedSymbol>();
        protected TimeSpan _pollIntervalConnected;
        protected TimeSpan _pollIntervalDisconnected;
        protected bool _pollAtStart;
        protected bool _onlyTrackProvidedSymbols;
        protected bool _trackTrades = true;


        protected AsyncResetEvent _pollWaitEvent = new AsyncResetEvent(false, true);
        protected Task? _pollTask;
        protected CancellationTokenSource? _cts;
        protected object _symbolLock = new object();


        /// <inheritdoc />
        public string? UserIdentifier { get; }
        /// <inheritdoc />
        public IEnumerable<SharedSymbol> TrackedSymbols => _symbols.AsEnumerable();

        private bool _connected;
        /// <inheritdoc />
        public bool Connected
        {
            get => _connected;
            protected set
            {
                if (_connected == value)
                    return;

                _connected = value;
                if (!_connected)
                    _wasDisconnected = true;
                else
                    _pollWaitEvent.Set();

                InvokeConnectedStatusChanged();
            }
        }

        /// <inheritdoc />
        public event Action<bool>? OnConnectedStatusChange;

        public UserDataTracker(ILogger logger, UserDataTrackerConfig config, string? userIdentifier)
        {
            if (config.OnlyTrackProvidedSymbols && !config.TrackedSymbols.Any())
                throw new ArgumentException(nameof(config.TrackedSymbols), "Conflicting options; `OnlyTrackProvidedSymbols` but no symbols specific in `TrackedSymbols`");

            _logger = logger;

            _pollIntervalConnected = config.PollIntervalConnected;
            _pollIntervalDisconnected = config.PollIntervalDisconnected;
            _symbols = config.TrackedSymbols?.ToList() ?? [];
            _onlyTrackProvidedSymbols = config.OnlyTrackProvidedSymbols;
            _pollAtStart = config.PollAtStart;
            _trackTrades = config.TrackTrades;

            UserIdentifier = userIdentifier;
        }

        protected void InvokeConnectedStatusChanged()
        {
            OnConnectedStatusChange?.Invoke(Connected);            
        }
    
        public async Task<CallResult> StartAsync()
        {
            _startTime = DateTime.UtcNow;
            _cts = new CancellationTokenSource();

            var start = await DoStartAsync().ConfigureAwait(false);
            if (!start)
                return start;

            Connected = true;

            _pollTask = PollAsync();
            return CallResult.SuccessResult;
        }

        protected abstract Task<CallResult> DoStartAsync();

        /// <inheritdoc />
        public async Task StopAsync()
        {
            _logger.LogDebug("Stopping UserDataTracker");
            _cts?.Cancel();

            if (_pollTask != null)
                await _pollTask.ConfigureAwait(false);

            _logger.LogDebug("Stopped UserDataTracker");
        }

        private TimeSpan? GetNextPollDelay()
        {
            if (!_firstPollDone && _pollAtStart)
                // First polling should be done immediately
                return TimeSpan.Zero;

            if (!Connected)
            {
                if (_pollIntervalDisconnected == TimeSpan.Zero)
                    // No polling interval
                    return null;

                return _pollIntervalDisconnected;
            }

            if (_pollIntervalConnected == TimeSpan.Zero)
                // No polling interval
                return null;

            // Wait for next poll
            return _pollIntervalConnected;
        }

        public async Task PollAsync()
        {
            while (!_cts!.IsCancellationRequested)
            {
                var delayForNextPoll = GetNextPollDelay();
                if (delayForNextPoll != TimeSpan.Zero)
                {
                    try
                    {
                        if (delayForNextPoll != null)
                            _logger.LogTrace("Delay for next polling: {Delay}", delayForNextPoll);

                        await _pollWaitEvent.WaitAsync(delayForNextPoll, _cts.Token).ConfigureAwait(false);
                    }
                    catch { }
                }

                _firstPollDone = true;
                if (_cts.IsCancellationRequested)
                    break;

                if (_lastPollAttempt != null
                    && (DateTime.UtcNow - _lastPollAttempt.Value) < TimeSpan.FromSeconds(2)
                    && !(Connected && _wasDisconnected))
                {
                    if (_lastPollSuccessful)
                        // If last poll was less than 2 seconds ago and it was successful don't bother immediately polling again
                        continue;
                }

                if (Connected)
                    _wasDisconnected = false;

                _lastPollSuccessful = false;

                try
                {
                    var anyError = await DoPollAsync().ConfigureAwait(false);

                    _lastPollAttempt = DateTime.UtcNow;
                    _lastPollSuccessful = !anyError;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "UserDataTracker polling exception");
                }
            }
        }

        protected abstract Task<bool> DoPollAsync();

        protected void UpdateSymbolsList(IEnumerable<SharedSymbol> symbols)
        {
            lock (_symbolLock)
            {
                foreach (var symbol in symbols.Distinct())
                {
                    if (!_symbols.Any(x => x.TradingMode == symbol.TradingMode && x.BaseAsset == symbol.BaseAsset && x.QuoteAsset == symbol.QuoteAsset))
                    {
                        _symbols.Add(symbol);
                        _logger.LogDebug("Adding {BaseAsset}/{QuoteAsset} to symbol tracking list", symbol.BaseAsset, symbol.QuoteAsset);
                    }
                }
            }
        }
    }
}
