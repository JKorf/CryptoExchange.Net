namespace CryptoExchange.Net.Converters.MessageParsing.DynamicConverters
{
    /// <summary>
    /// Search result value
    /// </summary>
    public struct SearchResultItem
    {
        /// <summary>
        /// The field the values is for
        /// </summary>
        public MessageFieldReference Field { get; set; }
        /// <summary>
        /// The value of the field
        /// </summary>
        public string? Value { get; set; }
    }
}
