namespace CryptoExchange.Net.SharedApis;

/// <summary>
/// Request to cancel a take profit / stop loss
/// </summary>
public record CancelTpSlRequest : SharedSymbolRequest
{
    /// <summary>
    /// Id of order to cancel
    /// </summary>
    public string? OrderId { get; set; }

    /// <summary>
    /// Position mode
    /// </summary>
    public SharedPositionMode? PositionMode { get; set; }
    /// <summary>
    /// Position side
    /// </summary>
    public SharedPositionSide? PositionSide { get; set; }
    /// <summary>
    /// Take profit / Stop loss side
    /// </summary>
    public SharedTpSlSide? TpSlSide { get; set; }
    /// <summary>
    /// Margin mode
    /// </summary>
    public SharedMarginMode? MarginMode { get; set; }

    /// <summary>
    /// ctor for canceling by order id
    /// </summary>
    /// <param name="symbol">Symbol the order is on</param>
    /// <param name="orderId">Id of the order to close</param>
    /// <param name="exchangeParameters">Exchange specific parameters</param>
    public CancelTpSlRequest(SharedSymbol symbol, string orderId, ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
    {
        OrderId = orderId;
    }

    /// <summary>
    /// ctor for canceling without order id
    /// </summary>
    /// <param name="symbol">Symbol the order is on</param>
    /// <param name="mode">The position mode of the account</param>
    /// <param name="positionSide">The side of the position</param>
    /// <param name="tpSlSide">The side to cancel</param>
    /// <param name="exchangeParameters">Exchange specific parameters</param>
    public CancelTpSlRequest(SharedSymbol symbol, SharedPositionMode mode, SharedPositionSide positionSide, SharedTpSlSide tpSlSide, ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
    {
        PositionMode = mode;
        PositionSide = positionSide;
        TpSlSide = tpSlSide;
    }
}
