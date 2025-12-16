namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Trigger order status
    /// </summary>
    public enum SharedTriggerOrderStatus
    {
        /// <summary>
        /// Order is active
        /// </summary>
        Active,
        /// <summary>
        /// Order has been filled
        /// </summary>
        Filled,
        /// <summary>
        /// Trigger canceled, can be user cancelation or system cancelation due to an error
        /// </summary>
        CanceledOrRejected,
        /// <summary>
        /// Trigger order has been triggered. Resulting order might be filled or not.
        /// </summary>
        Triggered
    }
}
