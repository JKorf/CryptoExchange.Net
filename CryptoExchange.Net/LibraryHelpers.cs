namespace CryptoExchange.Net;

/// <summary>
/// Helpers for client libraries
/// </summary>
public static class LibraryHelpers
{
    /// <summary>
    /// Client order id separator
    /// </summary>
    public const string ClientOrderIdSeparator = "JK";

    /// <summary>
    /// Apply broker id to a client order id
    /// </summary>
    /// <param name="clientOrderId"></param>
    /// <param name="brokerId"></param>
    /// <param name="maxLength"></param>
    /// <param name="allowValueAdjustment"></param>
    /// <returns></returns>
    public static string ApplyBrokerId(string? clientOrderId, string brokerId, int maxLength, bool allowValueAdjustment)
    {
        var reservedLength = brokerId.Length + ClientOrderIdSeparator.Length;

        if ((clientOrderId?.Length + reservedLength) > maxLength)
            return clientOrderId!;

        if (!string.IsNullOrEmpty(clientOrderId))
        {
            if (allowValueAdjustment)
                clientOrderId = brokerId + ClientOrderIdSeparator + clientOrderId;

            return clientOrderId!;
        }
        else
        {
            clientOrderId = ExchangeHelpers.AppendRandomString(brokerId + ClientOrderIdSeparator, maxLength);
        }

        return clientOrderId;
    }
}
