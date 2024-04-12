using System;

namespace CryptoExchange.Net.Converters
{
    /// <summary>
    /// Mark property as an index in the array
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ArrayPropertyAttribute : Attribute
    {
        /// <summary>
        /// The index in the array
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="index"></param>
        public ArrayPropertyAttribute(int index)
        {
            Index = index;
        }
    }
}
