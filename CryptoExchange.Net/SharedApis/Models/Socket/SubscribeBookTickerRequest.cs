using System.Collections.Generic;

namespace CryptoExchange.Net.SharedApis;

/// <summary>
/// Request to subscribe to book ticker updates
/// </summary>
public record SubscribeBookTickerRequest : SharedSymbolRequest
{
    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="symbol">The symbol to subscribe to</param>
    /// <param name="exchangeParameters">Exchange specific parameters</param>
    public SubscribeBookTickerRequest(SharedSymbol symbol, ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
    {
    }

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="symbols">The symbols to subscribe to</param>
    /// <param name="exchangeParameters">Exchange specific parameters</param>
    public SubscribeBookTickerRequest(IEnumerable<SharedSymbol> symbols, ExchangeParameters? exchangeParameters = null) : base(symbols, exchangeParameters)
    {
    }

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="symbols">The symbols to subscribe to</param>
    public SubscribeBookTickerRequest(params SharedSymbol[] symbols) : base(symbols, null)
    {
    }
}
