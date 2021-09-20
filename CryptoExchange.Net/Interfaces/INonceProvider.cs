namespace CryptoExchange.Net.Interfaces
{
    /// <summary>
    /// A provider for a nonce value used when signing requests
    /// </summary>
    public interface INonceProvider
    {
        /// <summary>
        /// Get nonce value. Nonce value should be unique and incremental for each call
        /// </summary>
        /// <returns>Nonce value</returns>
        long GetNonce();
    }
}
