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
    /// Spot user data tracking feature
    /// </summary>
    public sealed class SpotUserDataFeature : ITradingSessionFeature
    {
        private IUserSpotDataTracker? _tracker;
        private readonly ISpotUserDataUpdateHandler _handler;
        private readonly TradeSession _strategy;
        private readonly Func<CancellationToken, Task<CallResult<IUserSpotDataTracker>>> _trackerFactory;
        private bool _attached;
        private bool _disposed;

        /// <summary>
        /// The underlying tracker
        /// </summary>
        public IUserSpotDataTracker? Tracker => _tracker;
        /// <summary>
        /// The user identifier
        /// </summary>
        public string UserIdentifier { get; }

        /// <summary>
        /// ctor
        /// </summary>
        public SpotUserDataFeature(
            string userIdentifier,
            Func<CancellationToken, Task<CallResult<IUserSpotDataTracker>>> trackerFactory,
            ISpotUserDataUpdateHandler handler)
        {
            UserIdentifier = userIdentifier;
            _trackerFactory = trackerFactory ?? throw new ArgumentNullException(nameof(trackerFactory));
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _strategy = handler as TradeSession
                ?? throw new ArgumentException("The handler must be a trading strategy.", nameof(handler));
        }

        /// <inheritdoc />
        public async Task<CallResult> StartAsync(CancellationToken ct)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SpotUserDataFeature));

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
                _attached = true;
            }

            return await _tracker!.StartAsync(ct).ConfigureAwait(false);
        }

        private Task HandleBalances(UserDataUpdate<SharedBalance[]> update)
            => _strategy.DispatchAsync(() => _handler.HandleSpotBalancesUpdateAsync(
                UserIdentifier, update));

        private Task HandleOrders(UserDataUpdate<SharedSpotOrder[]> update)
            => _strategy.DispatchAsync(() => _handler.HandleSpotOrdersUpdateAsync(
                UserIdentifier, update));

        private Task HandleTrades(UserDataUpdate<SharedUserTrade[]> update)
            => _strategy.DispatchAsync(() => _handler.HandleSpotTradesUpdateAsync(
                UserIdentifier, update));

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
