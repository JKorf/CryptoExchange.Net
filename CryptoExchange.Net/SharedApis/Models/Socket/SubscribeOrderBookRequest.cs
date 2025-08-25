using System.Collections.Generic;

namespace CryptoExchange.Net.SharedApis;

/// <summary>
/// Request to subscribe to order book snapshot updates
/// </summary>
public record SubscribeOrderBookRequest : SharedSymbolRequest
{
    /// <summary>
    /// The order book depth
    /// </summary>
    public int? Limit { get; set; }

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="symbol">The symbol to subscribe to</param>
    /// <param name="limit">Order book depth</param>
    /// <param name="exchangeParameters">Exchange specific parameters</param>
    public SubscribeOrderBookRequest(SharedSymbol symbol, int? limit = null, ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
    {
        Limit = limit;
    }

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="symbols">The symbols to subscribe to</param>
    /// <param name="exchangeParameters">Exchange specific parameters</param>
    public SubscribeOrderBookRequest(IEnumerable<SharedSymbol> symbols, ExchangeParameters? exchangeParameters = null) : base(symbols, exchangeParameters)
    {
    }

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="symbols">The symbols to subscribe to</param>
    public SubscribeOrderBookRequest(params SharedSymbol[] symbols) : base(symbols, null)
    {
    }
}
