using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Converters.MessageParsing.DynamicConverters
{

    public class MessageEvaluator
    {
        public int Priority { get; set; }
        public bool ForceIfFound { get; set; }

        public MessageFieldReference[] Fields { get; set; }

        public Func<SearchResult, string> IdentifyMessageCallback { get; set; }
        public string? StaticIdentifier { get; set; }

        public string? IdentifyMessage(SearchResult result)
        {
            if (StaticIdentifier != null)
                return StaticIdentifier;

            return IdentifyMessageCallback(result);
        }

        public bool Statisfied(SearchResult result)
        {
            foreach(var field in Fields)
            {
                if (!result.Contains(field))
                    return false;
            }

            return true;
        }
    }

    public abstract class MessageFieldReference
    {
        public string SearchName { get; set; }
        public int Depth { get; set; } = 1;

        public Func<string?, bool>? Constraint { get; set; }

        public MessageFieldReference(string searchName)
        {
            SearchName = searchName;
        }
    }

    public class PropertyFieldReference : MessageFieldReference
    {
        public byte[] PropertyName { get; set; }
        public bool ArrayValues { get; set; }

        public PropertyFieldReference(string propertyName) : base(propertyName)
        {
            PropertyName = Encoding.UTF8.GetBytes(propertyName);
        }
    }

    public class ArrayFieldReference : MessageFieldReference
    {
        public int ArrayIndex { get; set; }

        public ArrayFieldReference(string searchName, int depth, int index) : base(searchName)
        {
            Depth = depth;
            ArrayIndex = index;
        }
    }

    public class MessageEvalutorFieldReference
    {
        public bool SkipReading { get; set; }
        public bool OverlappingField { get; set; }
        public MessageFieldReference Field { get; set; }
        public MessageEvaluator? ForceEvaluator { get; set; }
    }

    public class SearchResult
    {
        private List<SearchResultItem> _items = new List<SearchResultItem>();

        public string FieldValue(string searchName)
        {
            foreach(var item in _items)
            {
                if (item.Field.SearchName.Equals(searchName, StringComparison.Ordinal))
                    return item.Value;
            }

            throw new Exception(""); // TODO
        }

        public int Count => _items.Count;

        public void Clear() => _items.Clear();

        public bool Contains(MessageFieldReference field)
        {
            foreach(var item in _items)
            {
                if (item.Field == field)
                    return true;
            }

            return false;
        }

        public void Write(MessageFieldReference field, string? value) => _items.Add(new SearchResultItem
        {
            Field = field,
            Value = value
        });
    }

    public struct SearchResultItem
    {
        public MessageFieldReference Field { get; set; }
        public string? Value { get; set; }
    }
}
