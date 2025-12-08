namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Account type
    /// </summary>
    public enum SharedAccountType
    {
        /// <summary>
        /// Unified account, combined account for multiple different types of trading
        /// </summary>
        Unified,
        
        /// <summary>
        /// Funding account, where withdrawals and deposits are made from and to
        /// </summary>
        Funding,

        /// <summary>
        /// Spot trading account
        /// </summary>
        Spot,

        /// <summary>
        /// Cross margin account
        /// </summary>
        CrossMargin,
        /// <summary>
        /// Isolated margin account
        /// </summary>
        IsolatedMargin,

        /// <summary>
        /// Perpetual linear futures account
        /// </summary>
        PerpetualLinearFutures,
        /// <summary>
        /// Delivery linear futures account
        /// </summary>
        DeliveryLinearFutures,
        /// <summary>
        /// Perpetual inverse futures account
        /// </summary>
        PerpetualInverseFutures,
        /// <summary>
        /// Delivery inverse futures account
        /// </summary>
        DeliveryInverseFutures,

        /// <summary>
        /// Option account
        /// </summary>
        Option,

        /// <summary>
        /// Other
        /// </summary>
        Other
    }
}
