using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Symbol model
    /// </summary>
    public record SharedSymbolModel
    {
        /// <summary>
        /// SharedSymbol, only filled when the related GetSpotSymbolsAsync or GetFuturesSymbolsAsync method has been called previously
        /// </summary>
        public SharedSymbol? SharedSymbol { get; set; }
        /// <summary>
        /// Symbol name 
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedSymbolModel(SharedSymbol? sharedSymbol, string symbol)
        {
            Symbol = symbol;
            SharedSymbol = sharedSymbol;
        }
    }
}
