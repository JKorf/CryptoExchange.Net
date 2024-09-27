namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Time in force for an order
    /// </summary>
    public enum SharedTimeInForce
    {
        /// <summary>
        /// Order is good until canceled
        /// </summary>
        GoodTillCanceled,
        /// <summary>
        /// Order should execute immediately, not executed part is canceled
        /// </summary>
        ImmediateOrCancel,
        /// <summary>
        /// Order should execute fully immediately or is fully canceled
        /// </summary>
        FillOrKill
    }
}
