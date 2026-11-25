using CryptoExchange.Net.OrderBook.V2.Interfaces;
using CryptoExchange.Net.SharedApis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.OrderBook.V2
{
    /// <summary>
    /// Client order book factory
    /// </summary>
    public class ClientOrderBookFactory : IClientOrderBookFactory
    {
        private readonly ILoggerFactory _logger;
        private readonly ISharedApiClientBase _api;

        /// <summary>
        /// ctor
        /// </summary>
        public ClientOrderBookFactory(ILoggerFactory logger, ISharedApiClientBase api)
        {
            _logger = logger;
            _api = api;
        }

        /// <summary>
        /// Create a new order book
        /// </summary>
        public IClientOrderBook Create(SharedSymbol symbol)
        {
#warning TODO options
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
