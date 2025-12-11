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
        public Func<string?, bool>? Constraint { get; private set; }

        /// <summary>
        /// Check whether the value is one of the string values in the set
        /// </summary>
        public MessageFieldReference WithFilterConstraint(HashSet<string?> set)
        {
            Constraint = set.Contains;
            return this;
        }

        /// <summary>
        /// Check whether the value is equal to a string
        /// </summary>
        public MessageFieldReference WithEqualConstraint(string compare)
        {
            Constraint = x => x != null && x.Equals(compare, StringComparison.Ordinal);
            return this;
        }

        /// <summary>
        /// Check whether the value is not equal to a string
        /// </summary>
        public MessageFieldReference WithNotEqualConstraint(string compare)
        {
            Constraint = x => x == null || !x.Equals(compare, StringComparison.Ordinal);
            return this;
        }

        /// <summary>
        /// Check whether the value is not null
        /// </summary>
        public MessageFieldReference WithNotNullConstraint()
        {
            Constraint = x => x != null;
            return this;
        }

        /// <summary>
        /// Check whether the value starts with a certain string
        /// </summary>
        public MessageFieldReference WithStartsWithConstraint(string start)
        {
            Constraint = x => x != null && x.StartsWith(start, StringComparison.Ordinal);
            return this;
        }

        /// <summary>
        /// Check whether the value starts with a certain string
        /// </summary>
        public MessageFieldReference WithStartsWithConstraints(params string[] startValues)
        {
            Constraint = x =>
            {
                if (x == null)
                    return false;

                foreach (var item in startValues)
                {
                    if (x!.StartsWith(item, StringComparison.Ordinal))
                        return true;
                }

                return false;
            };
            return this;
        }

        /// <summary>
        /// Check whether the value starts with a certain string
        /// </summary>
        public MessageFieldReference WithCustomConstraint(Func<string?, bool> constraint)
        {
            Constraint = constraint;
            return this;
        }

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
