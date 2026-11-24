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
    public sealed class SpotUserDataFeature : ITradeSessionFeature
    {
        private IUserSpotDataTracker? _tracker;
        private readonly TradeSession _session;
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
            TradeSession session)
        {
            UserIdentifier = userIdentifier;
            _trackerFactory = trackerFactory ?? throw new ArgumentNullException(nameof(trackerFactory));
            _session = session ?? throw new ArgumentNullException(nameof(session));
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
            => _session.DispatchAsync(() => _session.InvokeSpotBalancesUpdateAsync(
                UserIdentifier, update));

        private Task HandleOrders(UserDataUpdate<SharedSpotOrder[]> update)
            => _session.DispatchAsync(() => _session.InvokeSpotOrdersUpdateAsync(
                UserIdentifier, update));

        private Task HandleTrades(UserDataUpdate<SharedUserTrade[]> update)
            => _session.DispatchAsync(() => _session.InvokeSpotTradesUpdateAsync(
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
