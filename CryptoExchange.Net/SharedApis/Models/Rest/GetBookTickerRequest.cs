namespace CryptoExchange.Net.SharedApis;

/// <summary>
/// Request to retrieve best bid/ask info for a symbol
/// </summary>
public record GetBookTickerRequest : SharedSymbolRequest
{
    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="symbol">Symbol to retrieve book ticker for</param>
    /// <param name="exchangeParameters">Exchange specific parameters</param>
    public GetBookTickerRequest(SharedSymbol symbol, ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
    {
    }
}
