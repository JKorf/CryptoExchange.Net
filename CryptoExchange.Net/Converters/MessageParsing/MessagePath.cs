using System.Collections;
using System.Collections.Generic;

namespace CryptoExchange.Net.Converters.MessageParsing
{
    /// <summary>
    /// Message access definition
    /// </summary>
    public struct MessagePath : IEnumerable<NodeAccessor>
    {
        private List<NodeAccessor> _path;

        internal void Add(NodeAccessor node)
        {
            _path.Add(node);
        }

        /// <summary>
        /// ctor
        /// </summary>
        public MessagePath()
        {
            _path = new List<NodeAccessor>();
        }

        /// <summary>
        /// Create a new message path
        /// </summary>
        /// <returns></returns>
        public static MessagePath Get()
        {
            return new MessagePath();
        }

        /// <summary>
        /// IEnumerable implementation
        /// </summary>
        /// <returns></returns>
        public IEnumerator<NodeAccessor> GetEnumerator()
        {
            for (var i = 0; i < _path.Count; i++)
                yield return _path[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
