using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Converters.MessageParsing.DynamicConverters
{
    /// <summary>
    /// Message evaluator
    /// </summary>
    public class MessageEvaluator
    {
        /// <summary>
        /// The priority of this evaluator, higher prio evaluators (with lower Priority number) will be checked for matches before lower ones
        /// </summary>
        public int Priority { get; set; }
        /// <summary>
        /// Whether to immediately match the evaluator when it is matched. Can only be used when the evaluator has a single unique field to look for
        /// </summary>
        public bool ForceIfFound { get; set; }
        /// <summary>
        /// The fields this evaluator has to look for
        /// </summary>
        public MessageFieldReference[] Fields { get; set; }
        /// <summary>
        /// The callback for getting the identifier string
        /// </summary>
        public Func<SearchResult, string>? IdentifyMessageCallback { get; set; }
        /// <summary>
        /// The static identifier string to return when this evaluator is matched
        /// </summary>
        public string? StaticIdentifier { get; set; }
        
        internal string? IdentifyMessage(SearchResult result)
        {
            if (StaticIdentifier != null)
                return StaticIdentifier;

            return IdentifyMessageCallback!(result);
        }

        internal bool Statisfied(SearchResult result)
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
