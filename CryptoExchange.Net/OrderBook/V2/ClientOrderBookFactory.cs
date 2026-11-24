using CryptoExchange.Net.OrderBook.V2.Interfaces;
using CryptoExchange.Net.SharedApis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.OrderBook.V2
{
    public class ClientOrderBookFactory : IClientOrderBookFactory
    {
        private readonly ILoggerFactory _logger;
        private readonly ISharedApiClientBase _api;

        public ClientOrderBookFactory(ILoggerFactory logger, ISharedApiClientBase api)
        {
            _logger = logger;
            _api = api;
        }

        public IClientOrderBook Create(SharedSymbol symbol)
        {
            return new ClientOrderBook(
                _logger,
                symbol,
                _api.GetCapability<ISubscribeOrderBookSocket>(symbol.TradingMode)?.Capability,
                _api.GetCapability<ISubscribeIncrementalOrderBookSocket>(symbol.TradingMode)?.Capability,
                _api.GetCapability<IGetOrderBookRest>(symbol.TradingMode)?.Capability
                );
        }
    }
}
