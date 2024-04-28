namespace CryptoExchange.Net.Objects.Options
{
    /// <summary>
    /// Base for order book options
    /// </summary>
    public class OrderBookOptions
    {
        /// <summary>
        /// Whether or not checksum validation is enabled. Default is true, disabling will ignore checksum messages.
        /// </summary>
        public bool ChecksumValidationEnabled { get; set; } = true;

        /// <summary>
        /// Create a copy of this options
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Copy<T>() where T : OrderBookOptions, new()
        {
            return new T
            {
                ChecksumValidationEnabled = ChecksumValidationEnabled,
            };
        }
    }
}
