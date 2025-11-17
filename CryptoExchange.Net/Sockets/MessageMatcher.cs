using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Sockets
{
    /// <summary>
    /// Message link type
    /// </summary>
    public enum MessageLinkType
    {
        /// <summary>
        /// Match when the listen id matches fully to the value
        /// </summary>
        Full,
        /// <summary>
        /// Match when the listen id starts with the value
        /// </summary>
        StartsWith
    }

    /// <summary>
    /// Matches a message listen id to a specific listener
    /// </summary>
    public class MessageMatcher
    {
        /// <summary>
        /// Linkers in this matcher
        /// </summary>
        public MessageHandlerLink[] HandlerLinks { get; }

        /// <summary>
        /// ctor
        /// </summary>
        private MessageMatcher(params MessageHandlerLink[] links)
        {
            HandlerLinks = links;
        }

        /// <summary>
        /// Create message matcher
        /// </summary>
        public static MessageMatcher Create<T>(string value)
        {
            return new MessageMatcher(new MessageHandlerLink<T>(MessageLinkType.Full, value, (con, msg) => new CallResult<T>(default, msg.OriginalData, null)));
        }

        /// <summary>
        /// Create message matcher
        /// </summary>
        public static MessageMatcher Create<T>(string value, Func<SocketConnection, DataEvent<T>, CallResult> handler)
        {
            return new MessageMatcher(new MessageHandlerLink<T>(MessageLinkType.Full, value, handler));
        }

        /// <summary>
        /// Create message matcher
        /// </summary>
        public static MessageMatcher Create<T>(IEnumerable<string> values, Func<SocketConnection, DataEvent<T>, CallResult> handler)
        {
            return new MessageMatcher(values.Select(x => new MessageHandlerLink<T>(MessageLinkType.Full, x, handler)).ToArray());
        }

        /// <summary>
        /// Create message matcher
        /// </summary>
        public static MessageMatcher Create<T>(MessageLinkType type, string value, Func<SocketConnection, DataEvent<T>, CallResult> handler)
        {
            return new MessageMatcher(new MessageHandlerLink<T>(type, value, handler));
        }

        /// <summary>
        /// Create message matcher
        /// </summary>
        public static MessageMatcher Create(params MessageHandlerLink[] linkers)
        {
            return new MessageMatcher(linkers);
        }

        /// <summary>
        /// Whether this matcher contains a specific link
        /// </summary>
        public bool ContainsCheck(MessageHandlerLink link) => HandlerLinks.Any(x => x.Type == link.Type && x.Value == link.Value);

        /// <summary>
        /// Get any handler links matching with the listen id
        /// </summary>
        public IEnumerable<MessageHandlerLink> GetHandlerLinks(string listenId) => HandlerLinks.Where(x => x.Check(listenId));

        /// <inheritdoc />
        public override string ToString() => string.Join(",", HandlerLinks.Select(x => x.ToString()));
    }

    /// <summary>
    /// Message handler link
    /// </summary>
    public abstract class MessageHandlerLink
    {
        /// <summary>
        /// Type of check
        /// </summary>
        public MessageLinkType Type { get; }
        /// <summary>
        /// String value of the check
        /// </summary>
        public string Value { get; }
        /// <summary>
        /// Deserialization type
        /// </summary>
        public abstract Type DeserializationType { get; }

        /// <summary>
        /// ctor
        /// </summary>
        public MessageHandlerLink(MessageLinkType type, string value)
        {
            Type = type;
            Value = value;
        }

        /// <summary>
        /// Whether this listen id matches this link
        /// </summary>
        public bool Check(string listenId)
        {
            if (Type == MessageLinkType.Full)
                return Value.Equals(listenId, StringComparison.Ordinal);

            return listenId.StartsWith(Value, StringComparison.Ordinal);
        }

        /// <summary>
        /// Message handler
        /// </summary>
        public abstract CallResult Handle(SocketConnection connection, DataEvent<object> message);

        /// <inheritdoc />
        public override string ToString() => $"{Type} match for \"{Value}\"";
    }

    /// <summary>
    /// Message handler link
    /// </summary>
    public class MessageHandlerLink<TServer>: MessageHandlerLink
    {
        private Func<SocketConnection, DataEvent<TServer>, CallResult> _handler;

        /// <inheritdoc />
        public override Type DeserializationType => typeof(TServer);

        /// <summary>
        /// ctor
        /// </summary>
        public MessageHandlerLink(string value, Func<SocketConnection, DataEvent<TServer>, CallResult> handler)
            : this(MessageLinkType.Full, value, handler)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        public MessageHandlerLink(MessageLinkType type, string value, Func<SocketConnection, DataEvent<TServer>, CallResult> handler)
            : base(type, value)
        {
            _handler = handler;
        }


        /// <inheritdoc />
        public override CallResult Handle(SocketConnection connection, DataEvent<object> message)
        {
            return _handler(connection, message.As((TServer)message.Data));
        }
    }
}
