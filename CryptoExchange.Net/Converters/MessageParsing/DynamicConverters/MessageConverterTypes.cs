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
                if (!result.ContainsKey(field.SearchName))
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
        private string _searchName;

        public string SearchName
        {
            get => _searchName ?? PropertyName;
            set => _searchName = value;
        }
        public FieldType FieldType { get; set; }
        public int? Depth { get; set; }
        public int? MaxDepth { get; set; }

        // For FieldType.Property
        public string? PropertyName { get; set; }
        // For FieldType.ArrayIndex
        public int? ArrayIndex { get; set; }

        public Func<string, bool>? Constraint { get; set; }
    }

    public class MessageEvalutorFieldReference
    {
        public MessageFieldReference Field { get; set; }
        public MessageEvaluator? ForceEvaluator { get; set; }
    }
}
