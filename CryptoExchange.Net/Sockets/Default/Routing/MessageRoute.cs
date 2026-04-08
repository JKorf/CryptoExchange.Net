using CryptoExchange.Net.Objects;
using System;

namespace CryptoExchange.Net.Sockets.Default.Routing
{
    /// <summary>
    /// Message route
    /// </summary>
    public abstract class MessageRoute
    {
        /// <summary>
        /// Type identifier
        /// </summary>
        public string TypeIdentifier { get; set; }
        /// <summary>
        /// Optional topic filter
        /// </summary>
        public string? TopicFilter { get; set; }

        /// <summary>
        /// Whether responses to this route might be read by multiple listeners
        /// </summary>
        public bool MultipleReaders { get; set; } = false;

        /// <summary>
        /// Deserialization type
        /// </summary>
        public abstract Type DeserializationType { get; }

        /// <summary>
        /// ctor
        /// </summary>
        public MessageRoute(string typeIdentifier, string? topicFilter)
        {
            TypeIdentifier = typeIdentifier;
            TopicFilter = topicFilter;
        }

        /// <summary>
        /// Message handler
        /// </summary>
        public abstract CallResult? Handle(SocketConnection connection, DateTime receiveTime, string? originalData, object data);
    }

    /// <summary>
    /// Message route
    /// </summary>
    public class MessageRoute<TMessage> : MessageRoute
    {
        private Func<SocketConnection, DateTime, string?, TMessage, CallResult?> _handler;

        /// <inheritdoc />
        public override Type DeserializationType { get; } = typeof(TMessage);

        /// <summary>
        /// ctor
        /// </summary>
        internal MessageRoute(string typeIdentifier, string? topicFilter, Func<SocketConnection, DateTime, string?, TMessage, CallResult?> handler, bool multipleReaders = false)
            : base(typeIdentifier, topicFilter)
        {
            _handler = handler;
            MultipleReaders = multipleReaders;
        }

        /// <summary>
        /// Create route without topic filter
        /// </summary>
        public static MessageRoute<TMessage> CreateWithoutTopicFilter(string typeIdentifier, Func<SocketConnection, DateTime, string?, TMessage, CallResult?> handler, bool multipleReaders = false)
        {
            return new MessageRoute<TMessage>(typeIdentifier, null, handler)
            {
                MultipleReaders = multipleReaders
            };
        }

        /// <summary>
        /// Create route with optional topic filter
        /// </summary>
        public static MessageRoute<TMessage> CreateWithOptionalTopicFilter(string typeIdentifier, string? topicFilter, Func<SocketConnection, DateTime, string?, TMessage, CallResult?> handler, bool multipleReaders = false)
        {
            return new MessageRoute<TMessage>(typeIdentifier, topicFilter, handler)
            {
                MultipleReaders = multipleReaders
            };
        }

        /// <summary>
        /// Create route with topic filter
        /// </summary>
        public static MessageRoute<TMessage> CreateWithTopicFilter(string typeIdentifier, string topicFilter, Func<SocketConnection, DateTime, string?, TMessage, CallResult?> handler, bool multipleReaders = false)
        {
            return new MessageRoute<TMessage>(typeIdentifier, topicFilter, handler)
            {
                MultipleReaders = multipleReaders
            };
        }

        /// <inheritdoc />
        public override CallResult? Handle(SocketConnection connection, DateTime receiveTime, string? originalData, object data)
        {
            return _handler(connection, receiveTime, originalData, (TMessage)data);
        }
    }
}
