namespace CryptoExchange.Net.Objects.Sockets
{
    /// <summary>
    /// Parsed message object
    /// </summary>
    public class ParsedMessage
    {
        /// <summary>
        /// Identifier string
        /// </summary>
        public string Identifier { get; set; } = null!;
        /// <summary>
        /// Original data if the option is enabled
        /// </summary>
        public string? OriginalData { get; set; }
        /// <summary>
        /// Parsed data object
        /// </summary>
        public object? Data { get; set; }
    }
}
