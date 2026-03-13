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
        Hmac,
        /// <summary>
        /// RSA credentials
        /// </summary>
        Rsa,
        /// <summary>
        /// ED25519 credentials
        /// </summary>
        Ed25519,
        /// <summary>
        /// ECDSA credentials
        /// </summary>
        Ecdsa,
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
