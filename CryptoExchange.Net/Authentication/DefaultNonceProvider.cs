using CryptoExchange.Net.Interfaces;
using System;

namespace CryptoExchange.Net.Authentication
{
    /// <summary>
    /// Default nonce provider
    /// </summary>
    public class DefaultNonceProvider : INonceProvider
    {
        private static readonly object nonceLock = new object();
        private static long? lastNonce;

        /// <summary>
        /// Create a new default nonce provider
        /// </summary>
        /// <param name="startNonce">Start with an incremental value from this start nonce. If not provided `DataTime.UtcNow.Ticks` will be used</param>
        public DefaultNonceProvider(long? startNonce = null)
        {
            lastNonce = startNonce;
        }

        /// <inheritdoc />
        public long GetNonce()
        {
            lock (nonceLock)
            {
                long nonce;
                if (lastNonce == null)
                    nonce = DateTime.UtcNow.Ticks;
                else
                    nonce = lastNonce.Value + 1;                
                lastNonce = nonce;
                return nonce;
            }
        }
    }
}
