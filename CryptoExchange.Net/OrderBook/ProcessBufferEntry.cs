using CryptoExchange.Net.Interfaces;
using System;

namespace CryptoExchange.Net.OrderBook;

/// <summary>
/// Buffer entry with a first and last update id
/// </summary>
public class ProcessBufferRangeSequenceEntry
{
    /// <summary>
    /// First sequence number in this update
    /// </summary>
    public long FirstUpdateId { get; set; }

    /// <summary>
    /// Last sequence number in this update
    /// </summary>
    public long LastUpdateId { get; set; }

    /// <summary>
    /// List of changed/new asks
    /// </summary>
    public ISymbolOrderBookEntry[] Asks { get; set; } = Array.Empty<ISymbolOrderBookEntry>();

    /// <summary>
    /// List of changed/new bids
    /// </summary>
    public ISymbolOrderBookEntry[] Bids { get; set; } = Array.Empty<ISymbolOrderBookEntry>();
}
