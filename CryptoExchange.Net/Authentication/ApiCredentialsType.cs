namespace CryptoExchange.Net.Authentication
{
    /// <summary>
    /// Credentials type
    /// </summary>
    public enum ApiCredentialsType
    {
        /// <summary>
        /// HMAC credentials
        /// </summary>
        HMAC,
        /// <summary>
        /// RSA credentials
        /// </summary>
        RSA,
        /// <summary>
        /// Ed25519 credentials
        /// </summary>
        Ed25519,
        /// <summary>
        /// ECDsa credentials
        /// </summary>
        ECDsa,
        /// <summary>
        /// API key credentials
        /// </summary>
        ApiKey,
        /// <summary>
        /// Custom / exchange specific
        /// </summary>
        Custom
    }
}
