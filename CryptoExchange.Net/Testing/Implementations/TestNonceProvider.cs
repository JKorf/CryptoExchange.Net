using CryptoExchange.Net.Interfaces;

namespace CryptoExchange.Net.Testing.Implementations
{
    /// <summary>
    /// Test implementation for nonce provider, returning a prespecified nonce
    /// </summary>
    public class TestNonceProvider : INonceProvider
    {
        private readonly long _nonce;

        /// <summary>
        /// ctor
        /// </summary>
        public TestNonceProvider(long nonce)
        {
            _nonce = nonce;
        }

        /// <inheritdoc />
        public long GetNonce() => _nonce;
    }
}
