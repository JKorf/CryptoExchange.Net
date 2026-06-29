namespace CryptoExchange.Net.TokenManagement
{
    /// <summary>
    /// Defines how tokens are managed
    /// </summary>
    public enum TokenManagementType
    {
        /// <summary>
        /// Tokens represent an active server side resource. They are kept alive while in use and stopped when the last lease is released.
        /// </summary>
        Active,
        /// <summary>
        /// Tokens represent a temporary credential. They are cached for reuse until their validity period expires and don't require background maintenance.
        /// </summary>
        Cached
    }
}
