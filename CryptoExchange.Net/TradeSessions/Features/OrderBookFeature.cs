using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.TradeSessions.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.TradeSessions.Features
{
    /// <summary>
    /// Order book feature
    /// </summary>
    public sealed class OrderBookFeature : ITradeSessionFeature
    {
        private readonly ISymbolOrderBook _book;
        private readonly SharedSymbol _symbol;
        private bool _disposed;

        /// <summary>
        /// The symbol the book is for
        /// </summary>
        public SharedSymbol Symbol => _symbol with { };
        /// <summary>
        /// The order book
        /// </summary>
        public ISymbolOrderBook OrderBook => _book;

        /// <summary>
        /// ctor
        /// </summary>
        public OrderBookFeature(ISymbolOrderBook book, SharedSymbol symbol)
        {
            _book = book ?? throw new ArgumentNullException(nameof(book));
            _symbol = (symbol ?? throw new ArgumentNullException(nameof(symbol))) with { };
        }

        /// <inheritdoc />
        public Task<CallResult> StartAsync(CancellationToken ct)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(OrderBookFeature));

            return _book.StartAsync(ct);
        }

        /// <inheritdoc />
        public Task StopAsync() => _disposed ? Task.CompletedTask : _book.StopAsync();

        /// <inheritdoc />
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
