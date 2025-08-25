using System;

namespace CryptoExchange.Net.SharedApis;

/// <summary>
/// Futures symbol info
/// </summary>
public record SharedFuturesSymbol : SharedSpotSymbol
{
    /// <summary>
    /// The size of a single contract
    /// </summary>
    public decimal? ContractSize { get; set; }
    /// <summary>
    /// Delivery time of the contract
    /// </summary>
    public DateTime? DeliveryTime { get; set; }
    /// <summary>
    /// Max short leverage setting
    /// </summary>
    public decimal? MaxShortLeverage { get; set; }
    /// <summary>
    /// Max long leverage setting
    /// </summary>
    public decimal? MaxLongLeverage { get; set; }

    /// <summary>
    /// ctor
    /// </summary>
    public SharedFuturesSymbol(TradingMode symbolType, string baseAsset, string quoteAsset, string symbol, bool trading) : base(baseAsset, quoteAsset, symbol, trading, symbolType)
    {
    }

    /// <inheritdoc />
    public override SharedSymbol SharedSymbol => new SharedSymbol(TradingMode, BaseAsset.ToUpperInvariant(), QuoteAsset.ToUpperInvariant(), DeliveryTime) { SymbolName = Name };
}
