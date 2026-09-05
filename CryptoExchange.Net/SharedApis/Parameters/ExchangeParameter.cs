namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Exchange parameter
    /// </summary>
    public class ExchangeParameter
    {
        /// <summary>
        /// Exchange name
        /// </summary>
        public string Exchange { get; set; }
        /// <summary>
        /// Parameter name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Parameter value
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Create a new exchange parameter
        /// </summary>
        /// <param name="exchange">Exchange name</param>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Parameter value</param>
        public ExchangeParameter(string exchange, string name, object value)
        {
            Exchange = exchange;
            Name = name;
            Value = value;
        }
    }
}
