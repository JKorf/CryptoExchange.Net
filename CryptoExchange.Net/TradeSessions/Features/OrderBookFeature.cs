using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.TradeSessions.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.TradeSessions.Features
{
    public sealed class OrderBookFeature : ITradingSessionFeature
    {
        private readonly ISymbolOrderBook _book;
        private readonly SharedSymbol _symbol;
        private bool _disposed;

        public SharedSymbol Symbol => _symbol with { };
        public ISymbolOrderBook OrderBook => _book;

        public OrderBookFeature(ISymbolOrderBook book, SharedSymbol symbol)
        {
            _book = book ?? throw new ArgumentNullException(nameof(book));
            _symbol = (symbol ?? throw new ArgumentNullException(nameof(symbol))) with { };
        }

        public Task<CallResult> StartAsync(CancellationToken ct)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(OrderBookFeature));

            return _book.StartAsync(ct);
        }

        public Task StopAsync() => _disposed ? Task.CompletedTask : _book.StopAsync();

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;
            await StopAsync().ConfigureAwait(false);
            if (_book is IDisposable disposable)
                disposable.Dispose();
            _disposed = true;
        }
    }
}
