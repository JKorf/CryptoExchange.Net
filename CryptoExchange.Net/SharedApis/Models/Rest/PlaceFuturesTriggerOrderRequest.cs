namespace CryptoExchange.Net.SharedApis;

/// <summary>
/// Request to place a new trigger order
/// </summary>
public record PlaceFuturesTriggerOrderRequest : SharedSymbolRequest
{
    /// <summary>
    /// Client order id
    /// </summary>
    public string? ClientOrderId { get; set; }
    /// <summary>
    /// Direction of the trigger order
    /// </summary>
    public SharedTriggerOrderDirection OrderDirection { get; set; }
    /// <summary>
    /// Price trigger direction
    /// </summary>
    public SharedTriggerPriceDirection PriceDirection { get; set; }
    /// <summary>
    /// Quantity of the order
    /// </summary>
    public SharedQuantity Quantity { get; set; }
    /// <summary>
    /// Price of the order
    /// </summary>
    public decimal? OrderPrice { get; set; }
    /// <summary>
    /// Trigger price
    /// </summary>
    public decimal TriggerPrice { get; set; }
    /// <summary>
    /// Time in force
    /// </summary>
    public SharedTimeInForce? TimeInForce { get; set; }
    /// <summary>
    /// Position mode
    /// </summary>
    public SharedPositionMode? PositionMode { get; set; }
    /// <summary>
    /// Position side
    /// </summary>
    public SharedPositionSide PositionSide { get; set; }
    /// <summary>
    /// Margin mode
    /// </summary>
    public SharedMarginMode? MarginMode { get; set; }
    /// <summary>
    /// Leverage
    /// </summary>
    public decimal? Leverage { get; set; }
    /// <summary>
    /// Trigger price type
    /// </summary>
    public SharedTriggerPriceType? TriggerPriceType { get; set; }

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="symbol">Symbol the order is on</param>
    /// <param name="orderDirection">Direction of the order when triggered</param>
    /// <param name="priceDirection">Price direction</param>
    /// <param name="quantity">Quantity of the order</param>
    /// <param name="positionSide">Position side</param>
    /// <param name="triggerPrice">Price at which the order should activate</param>
    /// <param name="orderPrice">Limit price for the order</param>
    /// <param name="exchangeParameters">Exchange specific parameters</param>
    public PlaceFuturesTriggerOrderRequest(SharedSymbol symbol,
        SharedTriggerPriceDirection priceDirection,
        decimal triggerPrice,
        SharedTriggerOrderDirection orderDirection,
        SharedPositionSide positionSide,
        SharedQuantity quantity,
        decimal? orderPrice = null,
        ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
    {
        PriceDirection = priceDirection;
        PositionSide = positionSide;
        Quantity = quantity;
        OrderPrice = orderPrice;
        TriggerPrice = triggerPrice;
        OrderDirection = orderDirection;
    }
}
