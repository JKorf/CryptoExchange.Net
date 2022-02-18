namespace CryptoExchange.Net.CommonObjects
{
    /// <summary>
    /// Balance data
    /// </summary>
    public class Balance: BaseCommonObject
    {
        /// <summary>
        /// The asset name
        /// </summary>
        public string Asset { get; set;  } = string.Empty;
        /// <summary>
        /// Quantity available
        /// </summary>
        public decimal? Available { get; set; }
        /// <summary>
        /// Total quantity
        /// </summary>
        public decimal? Total { get; set;  }
    }
}
