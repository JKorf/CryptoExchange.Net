using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.Converters.MessageParsing.DynamicConverters
{

    public class MessageEvaluator
    {
        public int Priority { get; set; }
        public bool ForceIfFound { get; set; }

        public MessageFieldReference[] Fields { get; set; }

        public Func<Dictionary<string, string>, string> MessageIdentifier { get; set; }

        public bool Statisfied(Dictionary<string, string> result)
        {
            foreach(var field in Fields)
            {
                if (!result.ContainsKey(field.Name))
                    return false;
            }

            return true;
        }
    }

    public enum FieldType
    {
        ArrayIndex,
        Property
    }

    public class MessageFieldReference
    {

        public FieldType FieldType { get; set; }
        public int? Depth { get; set; }
        public int? MaxDepth { get; set; }
        public Type Type { get; set; }

        // For FieldType.Property
        public string Name { get; set; }
        // For FieldType.ArrayIndex
        public int Index { get; set; }
    }

    public class MessageEvalutorFieldReference
    {
        public MessageFieldReference Field { get; set; }
        public MessageEvaluator? ForceEvaluator { get; set; }
    }
}
