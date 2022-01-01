namespace CryptoExchange.Net.ComonObjects
{
    /// <summary>
    /// Balance data
    /// </summary>
    public class Balance: BaseComonObject
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
