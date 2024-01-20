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
        public object Value { get; }
        /// <summary>
        /// Type (true = int, false = string)
        /// </summary>
        public bool Type { get; }

        private NodeAccessor(object value, bool type)
        {
            Value = value;
            Type = type;
        }

        /// <summary>
        /// Create an int node accessor
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static NodeAccessor Int(int value) { return new NodeAccessor(value, true); }

        /// <summary>
        /// Create a string node accessor
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static NodeAccessor String(string value) { return new NodeAccessor(value, false); }
    }

}
