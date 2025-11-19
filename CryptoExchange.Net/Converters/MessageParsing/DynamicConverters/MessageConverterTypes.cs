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

        public Func<SearchResult, string> MessageIdentifier { get; set; }

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
        public string? PropertyName { get; set; }

        public PropertyFieldReference(string propertyName) : base(propertyName)
        {
            PropertyName = propertyName;
        }
    }

    public class ArrayFieldReference : MessageFieldReference
    {
        public int? ArrayIndex { get; set; }

        public ArrayFieldReference(string searchName) : base(searchName)
        {
        }
    }

    public class MessageEvalutorFieldReference
    {
        public MessageFieldReference Field { get; set; }
        public MessageEvaluator? ForceEvaluator { get; set; }
    }

    public class SearchResult
    {
        private List<SearchResultItem> _items = new List<SearchResultItem>();

        public string FieldValue(string searchName) => _items.First(x => x.Field.SearchName == searchName).Value;

        public int Count => _items.Count;

        public void Clear() => _items.Clear();

        public bool Contains(MessageFieldReference field) => _items.Any(x => x.Field == field);

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
