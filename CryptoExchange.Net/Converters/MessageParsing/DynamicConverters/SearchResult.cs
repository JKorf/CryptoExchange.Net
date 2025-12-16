using System;
using System.Collections.Generic;

namespace CryptoExchange.Net.Converters.MessageParsing.DynamicConverters
{
    /// <summary>
    /// The results of a search for fields in a JSON message
    /// </summary>
    public class SearchResult
    {
        private List<SearchResultItem> _items = new List<SearchResultItem>();

        /// <summary>
        /// Get the value of a field
        /// </summary>
        public string? FieldValue(string searchName)
        {
            foreach (var item in _items)
            {
                if (item.Field.SearchName.Equals(searchName, StringComparison.Ordinal))
                    return item.Value;
            }

            throw new Exception($"No field value found for {searchName}");
        }

        /// <summary>
        /// The number of found search field values
        /// </summary>
        public int Count => _items.Count;

        /// <summary>
        /// Clear the search result
        /// </summary>
        public void Clear() => _items.Clear();

        /// <summary>
        /// Whether the value for a specific field was found
        /// </summary>
        public bool Contains(MessageFieldReference field)
        {
            foreach (var item in _items)
            {
                if (item.Field == field)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Write a value to the result
        /// </summary>
        public void Write(MessageFieldReference field, string? value) => _items.Add(new SearchResultItem
        {
            Field = field,
            Value = value
        });
    }
}
