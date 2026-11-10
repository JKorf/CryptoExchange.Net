using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Trackers.Sessions.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.Sessions.Features
{
    internal sealed class SubscriptionFeature : ISessionFeature
    {
        private readonly Func<CancellationToken, Task<WebSocketResult<UpdateSubscription>>> _starter;
        private UpdateSubscription? _subscription;

        public SubscriptionFeature(Func<CancellationToken, Task<WebSocketResult<UpdateSubscription>>> starter)
        {
            _starter = starter;
        }

        public async Task<CallResult> StartAsync(CancellationToken ct)
        {
            var result = await _starter(ct).ConfigureAwait(false);
            if (result.Success)
                _subscription = result.Data;
            return result.Success ? CallResult.Ok() : CallResult.Fail(result.Error);
        }

        public async Task StopAsync()
        {
            if (_subscription == null)
                return;

            await _subscription.CloseAsync().ConfigureAwait(false);
            _subscription = null;
        }

        public ValueTask DisposeAsync() => new ValueTask(StopAsync());
    }
}
