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

        public static MessageRoute CreateVoid<TMessage>(string typeIdentifier)
        {
            return new EventRoute<TMessage>(typeIdentifier, null, (con, time, originalData, msg) => CallResult<TMessage>.Ok(default!));
        }

        public static MessageRoute CreateForEvent<TMessage>(string typeIdentifier, Func<SocketConnection, DateTime, string?, TMessage, CallResult?> handler, bool multipleReaders = false)
        {
            return new EventRoute<TMessage>(typeIdentifier, null, handler)
            {
                MultipleReaders = multipleReaders
            };
        }

        public static MessageRoute CreateForEvent<TMessage>(string typeIdentifier, string? topicFilter, Func<SocketConnection, DateTime, string?, TMessage, CallResult?> handler, bool multipleReaders = false)
        {
            return new EventRoute<TMessage>(typeIdentifier, topicFilter, handler)
            {
                MultipleReaders = multipleReaders
            };
        }

        public static MessageRoute CreateForQuery<TMessage>(string typeIdentifier, Func<SocketConnection, DateTime, string?, TMessage, CallResult<TMessage>?> handler, bool multipleReaders = false)
        {
            return new QueryRoute<TMessage>(typeIdentifier, null, handler)
            {
                MultipleReaders = multipleReaders
            };
        }

        public static MessageRoute CreateForQuery<TMessage>(string typeIdentifier, string? topicFilter, Func<SocketConnection, DateTime, string?, TMessage, CallResult<TMessage>?> handler, bool multipleReaders = false)
        {
            return new QueryRoute<TMessage>(typeIdentifier, topicFilter, handler)
            {
                MultipleReaders = multipleReaders
            };
        }

        public static MessageRoute CreateForQuery<TMessage, TResult>(string typeIdentifier, Func<SocketConnection, DateTime, string?, TMessage, CallResult<TResult>?> handler, bool multipleReaders = false)
        {
            return new QueryRoute<TMessage, TResult>(typeIdentifier, null, handler)
            {
                MultipleReaders = multipleReaders
            };
        }

        public static MessageRoute CreateForQuery<TMessage, TResult>(string typeIdentifier, string? topicFilter, Func<SocketConnection, DateTime, string?, TMessage, CallResult<TResult>?> handler, bool multipleReaders = false)
        {
            return new QueryRoute<TMessage, TResult>(typeIdentifier, topicFilter, handler)
            {
                MultipleReaders = multipleReaders
            };
        }

        /// <summary>
        /// Message handler
        /// </summary>
        public abstract CallResult? Handle(SocketConnection connection, DateTime receiveTime, string? originalData, object data);
    }

    public class QueryRoute<TMessage, TResult> : MessageRoute
    {
        private Func<SocketConnection, DateTime, string?, TMessage, CallResult<TResult>?> _handler;

        /// <inheritdoc />
        public override Type DeserializationType { get; } = typeof(TMessage);

        /// <summary>
        /// ctor
        /// </summary>
        internal QueryRoute(string typeIdentifier, string? topicFilter, Func<SocketConnection, DateTime, string?, TMessage, CallResult<TResult>?> handler, bool multipleReaders = false)
            : base(typeIdentifier, topicFilter)
        {
            _handler = handler;
            MultipleReaders = multipleReaders;
        }

        /// <inheritdoc />
        public override CallResult? Handle(SocketConnection connection, DateTime receiveTime, string? originalData, object data)
        {
            return _handler(connection, receiveTime, originalData, (TMessage)data);
        }
    }


    /// <summary>
    /// Message route
    /// </summary>
    public class QueryRoute<TMessage> : QueryRoute<TMessage, TMessage>
    {
        /// <summary>
        /// ctor
        /// </summary>
        internal QueryRoute(string typeIdentifier, string? topicFilter, Func<SocketConnection, DateTime, string?, TMessage, CallResult<TMessage>?> handler, bool multipleReaders = false)
            : base(typeIdentifier, topicFilter, handler, multipleReaders)
        {
        }

        /// <summary>
        /// Create route without topic filter
        /// </summary>        
        public static QueryRoute<TMessage> CreateWithoutTopicFilter(string typeIdentifier, Func<SocketConnection, DateTime, string?, TMessage, CallResult<TMessage>?> handler, bool multipleReaders = false)
        {
            return new QueryRoute<TMessage>(typeIdentifier, null, handler)
            {
                MultipleReaders = multipleReaders
            };
        }

        /// <summary>
        /// Create route with optional topic filter
        /// </summary>
        public static QueryRoute<TMessage> CreateWithOptionalTopicFilter(string typeIdentifier, string? topicFilter, Func<SocketConnection, DateTime, string?, TMessage, CallResult<TMessage>?> handler, bool multipleReaders = false)
        {
            return new QueryRoute<TMessage>(typeIdentifier, topicFilter, handler)
            {
                MultipleReaders = multipleReaders
            };
        }

        /// <summary>
        /// Create route with topic filter
        /// </summary>
        public static QueryRoute<TMessage> CreateWithTopicFilter(string typeIdentifier, string topicFilter, Func<SocketConnection, DateTime, string?, TMessage, CallResult<TMessage>?> handler, bool multipleReaders = false)
        {
            return new QueryRoute<TMessage>(typeIdentifier, topicFilter, handler)
            {
                MultipleReaders = multipleReaders
            };
        }
    }


    /// <summary>
    /// Message route
    /// </summary>
    public class EventRoute<TMessage> : MessageRoute
    {
        private Func<SocketConnection, DateTime, string?, TMessage, CallResult?> _handler;

        /// <inheritdoc />
        public override Type DeserializationType { get; } = typeof(TMessage);

        /// <summary>
        /// ctor
        /// </summary>
        internal EventRoute(string typeIdentifier, string? topicFilter, Func<SocketConnection, DateTime, string?, TMessage, CallResult?> handler, bool multipleReaders = false)
            : base(typeIdentifier, topicFilter)
        {
            _handler = handler;
            MultipleReaders = multipleReaders;
        }

        /// <summary>
        /// Create route without topic filter
        /// </summary>        
        public static EventRoute<TMessage> CreateWithoutTopicFilter(string typeIdentifier, Func<SocketConnection, DateTime, string?, TMessage, CallResult?> handler, bool multipleReaders = false)
        {
            return new EventRoute<TMessage>(typeIdentifier, null, handler)
            {
                MultipleReaders = multipleReaders
            };
        }

        /// <summary>
        /// Create route with optional topic filter
        /// </summary>
        public static EventRoute<TMessage> CreateWithOptionalTopicFilter(string typeIdentifier, string? topicFilter, Func<SocketConnection, DateTime, string?, TMessage, CallResult?> handler, bool multipleReaders = false)
        {
            return new EventRoute<TMessage>(typeIdentifier, topicFilter, handler)
            {
                MultipleReaders = multipleReaders
            };
        }

        /// <summary>
        /// Create route with topic filter
        /// </summary>
        public static EventRoute<TMessage> CreateWithTopicFilter(string typeIdentifier, string topicFilter, Func<SocketConnection, DateTime, string?, TMessage, CallResult?> handler, bool multipleReaders = false)
        {
            return new EventRoute<TMessage>(typeIdentifier, topicFilter, handler)
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
