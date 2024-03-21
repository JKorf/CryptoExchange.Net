namespace CryptoExchange.Net.Converters.MessageParsing
{
    /// <summary>
    /// Node accessor
    /// </summary>
    public struct NodeAccessor
    {
        /// <summary>
        /// Index
        /// </summary>
        public int? Index { get; }
        /// <summary>
        /// Property name
        /// </summary>
        public string? Property { get; }

        /// <summary>
        /// Type (0 = int, 1 = string, 2 = prop name)
        /// </summary>
        public int Type { get; }

        private NodeAccessor(int? index, string? property, int type)
        {
            Index = index;
            Property = property;
            Type = type;
        }

        /// <summary>
        /// Create an int node accessor
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static NodeAccessor Int(int value) { return new NodeAccessor(value, null, 0); }

        /// <summary>
        /// Create a string node accessor
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static NodeAccessor String(string value) { return new NodeAccessor(null, value, 1); }

        /// <summary>
        /// Create a property name node accessor
        /// </summary>
        /// <returns></returns>
        public static NodeAccessor PropertyName() { return new NodeAccessor(null, null, 2); }
    }
}
