namespace CryptoExchange.Net.TokenManagement
{
    /// <summary>
    /// Defines how a token is retained after its last lease is released
    /// </summary>
    public enum TokenRetentionPolicy
    {
        /// <summary>
        /// Remove and stop the token when its last lease is released
        /// </summary>
        RemoveWhenUnused,
        /// <summary>
        /// Keep the token available for reuse until its validity period expires
        /// </summary>
        RetainUntilExpired
    }
}
