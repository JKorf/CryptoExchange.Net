namespace CryptoExchange.Net.SharedApis;

/// <summary>
/// Price direction for trigger order
/// </summary>
public enum SharedTriggerPriceType
{
    /// <summary>
    /// Last traded price
    /// </summary>
    LastPrice,
    /// <summary>
    /// Mark price
    /// </summary>
    MarkPrice,
    /// <summary>
    /// Index price
    /// </summary>
    IndexPrice
}
