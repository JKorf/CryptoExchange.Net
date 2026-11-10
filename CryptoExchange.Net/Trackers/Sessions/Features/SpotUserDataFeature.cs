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
    public sealed class SpotUserDataFeature : ISessionFeature
    {
        private IUserSpotDataTracker _tracker;
        private readonly ISpotUserDataUpdateHandler _handler;
        private bool _attached;
        private bool _disposed;

        private readonly Func<CancellationToken, Task<CallResult<IUserSpotDataTracker>>> _trackerFactory;

        public IUserSpotDataTracker? Tracker => _tracker;
        public string UserIdentifier { get; }

        public SpotUserDataFeature(
            string userIdentifier,
            Func<CancellationToken, Task<CallResult<IUserSpotDataTracker>>> trackerFactory,
            ISpotUserDataUpdateHandler handler)
        {
            UserIdentifier = userIdentifier;
            _trackerFactory = trackerFactory
                ?? throw new ArgumentNullException(nameof(trackerFactory));
            _handler = handler
                ?? throw new ArgumentNullException(nameof(handler));
        }

        public async Task<CallResult> StartAsync(CancellationToken ct)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SpotUserDataFeature));

            if (!_attached)
            {
                var trackerResult = await _trackerFactory(ct).ConfigureAwait(false);
                if (!trackerResult.Success)
                    return trackerResult;

                _tracker = trackerResult.Data;

                _tracker.Balances.OnUpdate += HandleBalances;
                _tracker.Orders.OnUpdate += HandleOrders;
                if (_tracker.Trades != null)
                    _tracker.Trades.OnUpdate += HandleTrades;
                _attached = true;
            }
            return await _tracker.StartAsync(ct).ConfigureAwait(false);
        }

#warning only push if session is started
        private Task HandleBalances(UserDataUpdate<SharedBalance[]> update)
            => _handler.HandleSpotBalancesUpdateAsync(UserIdentifier, update);

        private Task HandleOrders(UserDataUpdate<SharedSpotOrder[]> update)
            => _handler.HandleSpotOrdersUpdateAsync(UserIdentifier, update);

        private Task HandleTrades(UserDataUpdate<SharedUserTrade[]> update)
            => _handler.HandleSpotTradesUpdateAsync(UserIdentifier, update);

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
                    _tracker.Balances.OnUpdate -= HandleBalances;
                    _tracker.Orders.OnUpdate -= HandleOrders;
                    if (_tracker.Trades != null)
                        _tracker.Trades.OnUpdate -= HandleTrades;
                    _tracker = null;
                    _attached = false;
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;
            await StopAsync().ConfigureAwait(false);
            _disposed = true;
        }
    }
}
