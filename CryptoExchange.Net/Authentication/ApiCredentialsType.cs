namespace CryptoExchange.Net.Authentication
{
    /// <summary>
    /// Credentials type
    /// </summary>
    public enum ApiCredentialsType
    {
        /// <summary>
        /// Hmac keys credentials
        /// </summary>
        Hmac,
        /// <summary>
        /// Rsa keys credentials in xml format
        /// </summary>
        RsaXml,
        /// <summary>
        /// Rsa keys credentials in pem/base64 format. Only available for .NetStandard 2.1 and up, use xml format for lower.
        /// </summary>
        RsaPem
    }
}
