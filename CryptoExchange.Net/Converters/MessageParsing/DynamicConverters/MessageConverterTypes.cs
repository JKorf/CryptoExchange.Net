using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Converters.MessageParsing.DynamicConverters
{
    /// <summary>
    /// Message type definition
    /// </summary>
    public class MessageTypeDefinition
    {
        /// <summary>
        /// Whether to immediately select the definition when it is matched. Can only be used when the evaluator has a single unique field to look for
        /// </summary>
        public bool ForceIfFound { get; set; }
        /// <summary>
        /// The fields a message needs to contain for this definition
        /// </summary>
        public MessageFieldReference[] Fields { get; set; } = [];
        /// <summary>
        /// The callback for getting the identifier string
        /// </summary>
        public Func<SearchResult, string>? TypeIdentifierCallback { get; set; }
        /// <summary>
        /// The static identifier string to return when this evaluator is matched
        /// </summary>
        public string? StaticIdentifier { get; set; }
        
        internal string? GetMessageType(SearchResult result)
        {
            if (StaticIdentifier != null)
                return StaticIdentifier;

            return TypeIdentifierCallback!(result);
        }

        internal bool Satisfied(SearchResult result)
        {
            foreach(var field in Fields)
            {
                if (!result.Contains(field))
                    return false;
            }

            return true;
        }
    }
}
