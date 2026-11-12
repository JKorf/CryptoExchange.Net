using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.UserData.Interfaces;
using CryptoExchange.Net.Trackers.UserData.Objects;
using CryptoExchange.Net.TradeSessions;
using CryptoExchange.Net.TradeSessions.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.TradeSessions.Features
{
    /// <summary>
    /// Futures user data tracking feature
    /// </summary>
    public sealed class FuturesUserDataFeature : ITradingSessionFeature
    {
        private IUserFuturesDataTracker? _tracker;
        private readonly TradeSession _strategy;
        private readonly Func<CancellationToken, Task<CallResult<IUserFuturesDataTracker>>> _trackerFactory;
        private bool _attached;
        private bool _disposed;

        /// <summary>
        /// The underlying tracker
        /// </summary>
        public IUserFuturesDataTracker? Tracker => _tracker;
        /// <summary>
        /// The user identifier
        /// </summary>
        public string UserIdentifier { get; }
        /// <summary>
        /// The trading mode
        /// </summary>
        public TradingMode TradingMode { get; }

        /// <summary>
        /// ctor
        /// </summary>
        public FuturesUserDataFeature(
            string userIdentifier,
            TradingMode tradingMode,
            Func<CancellationToken, Task<CallResult<IUserFuturesDataTracker>>> trackerFactory,
            TradeSession session)
        {
            UserIdentifier = userIdentifier;
            TradingMode = tradingMode;
            _trackerFactory = trackerFactory ?? throw new ArgumentNullException(nameof(trackerFactory));
            _strategy = session ?? throw new ArgumentNullException(nameof(session));
        }

        /// Initializes the user data tracker if needed, attaches update handlers, and starts the tracker.
        /// <remarks>Event handlers are attached only once, before the first successful start.</remarks>Token used to
        /// cancel tracker creation or startup.A <see cref="CallResult"/> indicating the outcome of creating and
        /// starting the tracker, including any
        /// failure returned by the tracker factory.The instance has been disposed.
        public async Task<CallResult> StartAsync(CancellationToken ct)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(FuturesUserDataFeature));

            if (!_attached)
            {
                var result = await _trackerFactory(ct).ConfigureAwait(false);
                if (!result.Success)
                    return result;

                _tracker = result.Data;
                _tracker.Balances.OnUpdate += HandleBalances;
                _tracker.Orders.OnUpdate += HandleOrders;
                if (_tracker.Trades != null)
                    _tracker.Trades.OnUpdate += HandleTrades;
                _tracker.Positions.OnUpdate += HandlePositions;
                _attached = true;
            }

            return await _tracker!.StartAsync(ct).ConfigureAwait(false);
        }

        private Task HandleBalances(UserDataUpdate<SharedBalance[]> update)
            => _strategy.DispatchAsync(() => _strategy.InvokeFuturesBalancesUpdateAsync(
                UserIdentifier, TradingMode, update));

        private Task HandleOrders(UserDataUpdate<SharedFuturesOrder[]> update)
            => _strategy.DispatchAsync(() => _strategy.InvokeFuturesOrdersUpdateAsync(
                UserIdentifier, TradingMode, update));

        private Task HandleTrades(UserDataUpdate<SharedUserTrade[]> update)
            => _strategy.DispatchAsync(() => _strategy.InvokeFuturesTradesUpdateAsync(
                UserIdentifier, TradingMode, update));

        private Task HandlePositions(UserDataUpdate<SharedPosition[]> update)
            => _strategy.DispatchAsync(() => _strategy.InvokeFuturesPositionsUpdateAsync(
                UserIdentifier, TradingMode, update));

        /// <inheritdoc />
        public async Task StopAsync()
        {
            if (_disposed || _tracker == null)
                return;

            var tracker = _tracker;
            try
            {
                await tracker.StopAsync().ConfigureAwait(false);
            }
            finally
            {
                if (_attached)
                {
                    tracker.Balances.OnUpdate -= HandleBalances;
                    tracker.Orders.OnUpdate -= HandleOrders;
                    if (tracker.Trades != null)
                        tracker.Trades.OnUpdate -= HandleTrades;
                    tracker.Positions.OnUpdate -= HandlePositions;
                    _attached = false;
                }
            }
            _tracker = null;
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;
            await StopAsync().ConfigureAwait(false);
            _disposed = true;
        }
    }
}
