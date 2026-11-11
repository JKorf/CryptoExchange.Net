using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.Sessions.Interfaces;
using CryptoExchange.Net.Trackers.UserData.Interfaces;
using CryptoExchange.Net.Trackers.UserData.Objects;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.Sessions.Features
{
    /// <summary>
    /// Futures user data tracking feature
    /// </summary>
    public sealed class FuturesUserDataFeature : ITradingSessionFeature
    {
        private IUserFuturesDataTracker? _tracker;
        private readonly IFuturesUserDataUpdateHandler _handler;
        private bool _attached;
        private bool _disposed;
        private readonly Func<CancellationToken, Task<CallResult<IUserFuturesDataTracker>>> _trackerFactory;

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
            IFuturesUserDataUpdateHandler handler)
        {
            UserIdentifier = userIdentifier;
            TradingMode = tradingMode;
            _trackerFactory = trackerFactory ?? throw new ArgumentNullException(nameof(trackerFactory));
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        ///Initializes the user data tracker if needed, attaches update handlers, and starts the tracker.
        /// <remarks>Event handlers are attached only once, before the first successful start.</remarks>Token used to cancel tracker creation or startup.A <see cref="CallResult"/> indicating the outcome of creating and starting the tracker, including any
        /// failure returned by the tracker factory.The instance has been disposed.
        public async Task<CallResult> StartAsync(CancellationToken ct)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(FuturesUserDataFeature));

            if (!_attached)
            {
                var trackerResult = await _trackerFactory(ct).ConfigureAwait(false);
                if (!trackerResult.Success)
                    return trackerResult;

                _tracker = trackerResult.Data;

                _tracker.Balances.OnUpdate += HandleBalances;
                _tracker.Orders.OnUpdate += HandleOrders;
                _tracker.Trades?.OnUpdate += HandleTrades;
                _tracker.Positions.OnUpdate += HandlePositions;
                _attached = true;
            }

            return await _tracker!.StartAsync(ct).ConfigureAwait(false);
        }

#warning only push if session is started
        private Task HandleBalances(UserDataUpdate<SharedBalance[]> update)
            => _handler.HandleFuturesBalancesUpdateAsync(
                UserIdentifier, TradingMode, update);

        private Task HandleOrders(UserDataUpdate<SharedFuturesOrder[]> update)
            => _handler.HandleFuturesOrdersUpdateAsync(
                UserIdentifier, TradingMode, update);

        private Task HandleTrades(UserDataUpdate<SharedUserTrade[]> update)
            => _handler.HandleFuturesTradesUpdateAsync(
                UserIdentifier, TradingMode, update);

        private Task HandlePositions(UserDataUpdate<SharedPosition[]> update)
            => _handler.HandleFuturesPositionsUpdateAsync(
                UserIdentifier, TradingMode, update);

        /// <inheritdoc />
        public async Task StopAsync()
        {
            if (_disposed)
                return;
            try
            {
                if (_tracker != null)
                    await _tracker.StopAsync().ConfigureAwait(false);
            }
            finally
            {
                if (_attached)
                {
                    _tracker!.Balances.OnUpdate -= HandleBalances;
                    _tracker.Orders.OnUpdate -= HandleOrders;
                    _tracker.Trades?.OnUpdate -= HandleTrades;
                    _tracker.Positions.OnUpdate -= HandlePositions;
                    _tracker = null;
                    _attached = false;
                }
            }
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
