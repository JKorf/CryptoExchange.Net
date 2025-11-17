using System;

namespace CryptoExchange.Net.Converters.MessageParsing
{
    /// <summary>
    /// Message info
    /// </summary>
    public ref struct MessageInfo
    {
        /// <summary>
        /// The deserialization type
        /// </summary>
        public Type? DeserializationType { get; set; }
        /// <summary>
        /// The listen identifier
        /// </summary>
        public string? Identifier { get; set; }
    }

}
