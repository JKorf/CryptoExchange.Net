using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sessions;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.Sessions.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.Sessions.Features
{
    public sealed class OrderBookFeature : ITradingSessionFeature
    {
        private readonly ISymbolOrderBook _book;
        private readonly SharedSymbol _symbol;
        private readonly IOrderBookUpdateHandler _handler;
        private readonly TradingStrategy _strategy;
        private bool _attached;
        private bool _disposed;

        public SharedSymbol Symbol => _symbol;
        public ISymbolOrderBook OrderBook => _book;

        public OrderBookFeature(ISymbolOrderBook book, SharedSymbol symbol, IOrderBookUpdateHandler handler)
        {
            _book = book ?? throw new ArgumentNullException(nameof(book));
            _symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _strategy = handler as TradingStrategy
                ?? throw new ArgumentException("The handler must be a trading strategy.", nameof(handler));
        }

        public Task<CallResult> StartAsync(CancellationToken ct)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(OrderBookFeature));

            if (!_attached)
            {
                _book.OnOrderBookUpdate += HandleUpdate;
                _attached = true;
            }
            return _book.StartAsync(ct);
        }

        private void HandleUpdate((ISymbolOrderBookEntry[] Bids, ISymbolOrderBookEntry[] Asks) update)
        {
            if (_book.Status == OrderBookStatus.Synced)
                _strategy.Dispatch(() => _handler.HandleOrderBookUpdate(_symbol, _book));
        }

        public async Task StopAsync()
        {
            if (_disposed)
                return;
            try
            {
                await _book.StopAsync().ConfigureAwait(false);
            }
            finally
            {
                if (_attached)
                {
                    _book.OnOrderBookUpdate -= HandleUpdate;
                    _attached = false;
                }
            }
        }

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
