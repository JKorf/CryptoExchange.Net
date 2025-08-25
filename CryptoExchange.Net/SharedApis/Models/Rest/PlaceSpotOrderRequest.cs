namespace CryptoExchange.Net.SharedApis;

/// <summary>
/// Request to place a new spot order
/// </summary>
public record PlaceSpotOrderRequest : SharedSymbolRequest
{
    /// <summary>
    /// Type of the order
    /// </summary>
    public SharedOrderType OrderType { get; set; }
    /// <summary>
    /// Side of the order
    /// </summary>
    public SharedOrderSide Side { get; set; }
    /// <summary>
    /// Time in force of the order
    /// </summary>
    public SharedTimeInForce? TimeInForce { get; set; }
    /// <summary>
    /// Quantity of the order
    /// </summary>
    public SharedQuantity? Quantity { get; set; }
    /// <summary>
    /// Price of the order
    /// </summary>
    public decimal? Price { get; set; }
    /// <summary>
    /// Client order id
    /// </summary>
    public string? ClientOrderId { get; set; }

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="symbol">Symbol to place the order on</param>
    /// <param name="side">Side of the order</param>
    /// <param name="orderType">Type of the order</param>
    /// <param name="quantity">Quantity of the order</param>
    /// <param name="price">Price of the order</param>
    /// <param name="timeInForce">Time in force</param>
    /// <param name="clientOrderId">Client order id</param>
    /// <param name="exchangeParameters">Exchange specific parameters</param>
    public PlaceSpotOrderRequest(
        SharedSymbol symbol,
        SharedOrderSide side,
        SharedOrderType orderType,
        SharedQuantity? quantity = null,
        decimal? price = null,
        SharedTimeInForce? timeInForce = null,
        string? clientOrderId = null,
        ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
    {
        OrderType = orderType;
        Side = side;
        Quantity = quantity;
        Price = price;
        TimeInForce = timeInForce;
        ClientOrderId = clientOrderId;
    }
}
