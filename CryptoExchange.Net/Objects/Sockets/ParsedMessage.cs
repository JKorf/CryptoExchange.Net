namespace CryptoExchange.Net.Objects.Sockets
{
    /// <summary>
    /// Parsed message object
    /// </summary>
    public abstract class BaseParsedMessage
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
        /// If parsed
        /// </summary>
        public bool Parsed { get; set; }
    }

    public class ParsedMessage<T> : BaseParsedMessage
    {
        /// <summary>
        /// Parsed data object
        /// </summary>
        public T? Data { get; set; }

        public ParsedMessage(T? data)
        {
            Data = data;
        }
    }
}
