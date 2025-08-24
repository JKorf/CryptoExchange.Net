using CryptoExchange.Net.Interfaces;

namespace CryptoExchange.Net.SharedApis;

/// <summary>
/// Order book info
/// </summary>
public record SharedOrderBook
{
    /// <summary>
    /// Asks list
    /// </summary>
    public ISymbolOrderBookEntry[] Asks { get; set; }
    /// <summary>
    /// Bids list
    /// </summary>
    public ISymbolOrderBookEntry[] Bids { get; set; }

    /// <summary>
    /// ctor
    /// </summary>
    public SharedOrderBook(ISymbolOrderBookEntry[] asks, ISymbolOrderBookEntry[] bids)
    {
        Asks = asks;
        Bids = bids;
    }
}
