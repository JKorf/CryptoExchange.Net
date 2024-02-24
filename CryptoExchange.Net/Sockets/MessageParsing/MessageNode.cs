namespace CryptoExchange.Net.Sockets.MessageParsing
{
    /// <summary>
    /// Node accessor
    /// </summary>
    public struct NodeAccessor
    {
        /// <summary>
        /// Value
        /// </summary>
        public object? Value { get; }
        /// <summary>
        /// Type (0 = int, 1 = string, 2 = prop name)
        /// </summary>
        public int Type { get; }

        private NodeAccessor(object? value, int type)
        {
            Value = value;
            Type = type;
        }

        /// <summary>
        /// Create an int node accessor
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static NodeAccessor Int(int value) { return new NodeAccessor(value, 0); }

        /// <summary>
        /// Create a string node accessor
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static NodeAccessor String(string value) { return new NodeAccessor(value, 1); }

        /// <summary>
        /// Create a property name node accessor
        /// </summary>
        /// <returns></returns>
        public static NodeAccessor PropertyName() { return new NodeAccessor(null, 2); }
    }

}
