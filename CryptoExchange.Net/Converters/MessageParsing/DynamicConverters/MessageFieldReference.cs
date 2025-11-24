using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Converters.MessageParsing.DynamicConverters
{
    /// <summary>
    /// Reference to a message field
    /// </summary>
    public abstract class MessageFieldReference
    {
        /// <summary>
        /// The name for this search field
        /// </summary>
        public string SearchName { get; set; }
        /// <summary>
        /// The depth at which to look for this field
        /// </summary>
        public int Depth { get; set; } = 1;
        /// <summary>
        /// Callback to check if the field value matches an expected constraint
        /// </summary>
        public Func<string?, bool>? Constraint { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public MessageFieldReference(string searchName)
        {
            SearchName = searchName;
        }
    }

    /// <summary>
    /// Reference to a property message field
    /// </summary>
    public class PropertyFieldReference : MessageFieldReference
    {
        /// <summary>
        /// The property name in the JSON
        /// </summary>
        public byte[] PropertyName { get; set; }
        /// <summary>
        /// Whether the property value is array values
        /// </summary>
        public bool ArrayValues { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public PropertyFieldReference(string propertyName) : base(propertyName)
        {
            PropertyName = Encoding.UTF8.GetBytes(propertyName);
        }
    }

    /// <summary>
    /// Reference to an array message field
    /// </summary>
    public class ArrayFieldReference : MessageFieldReference
    {
        /// <summary>
        /// The index in the array
        /// </summary>
        public int ArrayIndex { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public ArrayFieldReference(string searchName, int depth, int index) : base(searchName)
        {
            Depth = depth;
            ArrayIndex = index;
        }
    }

}
