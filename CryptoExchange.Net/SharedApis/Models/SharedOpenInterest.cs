using System;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Open interest
    /// </summary>
    public record SharedOpenInterest
    {
        /// <summary>
        /// Current open interest
        /// </summary>
        [Obsolete("Use `OpenInterests` instead")]
        public decimal OpenInterest => OpenInterests.QuantityInBaseAsset ?? OpenInterests.QuantityInContracts ?? 0;
        /// <summary>
        /// Current open interest
        /// </summary>
        public SharedOrderQuantity OpenInterests { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedOpenInterest(SharedOrderQuantity openInterest)
        {
            OpenInterests = openInterest;
        }
    }
}
