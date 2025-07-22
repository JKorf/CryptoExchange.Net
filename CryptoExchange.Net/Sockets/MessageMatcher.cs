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
    /// Message id matching type
    /// </summary>
    public enum MessageIdMatchType
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
        /// Checkers in this matcher
        /// </summary>
        public MessageCheck[] Checkers { get; }

        /// <summary>
        /// ctor
        /// </summary>
        private MessageMatcher(params MessageCheck[] checkers)
        {
            Checkers = checkers;
        }

        /// <summary>
        /// Create message matcher
        /// </summary>
        public static MessageMatcher Create<T>(string value)
        {
            return new MessageMatcher(new MessageCheck<T>(MessageIdMatchType.Full, value, (con, msg) => CallResult.SuccessResult));
        }

        /// <summary>
        /// Create message matcher
        /// </summary>
        public static MessageMatcher Create<T>(string value, Func<SocketConnection, DataEvent<T>, CallResult> handler)
        {
            return new MessageMatcher(new MessageCheck<T>(MessageIdMatchType.Full, value, handler));
        }

        /// <summary>
        /// Create message matcher
        /// </summary>
        public static MessageMatcher Create<T>(IEnumerable<string> values, Func<SocketConnection, DataEvent<T>, CallResult> handler)
        {
            return new MessageMatcher(values.Select(x => new MessageCheck<T>(MessageIdMatchType.Full, x, handler)).ToArray());
        }

        /// <summary>
        /// Create message matcher
        /// </summary>
        public static MessageMatcher Create<T>(MessageIdMatchType type, string value, Func<SocketConnection, DataEvent<T>, CallResult> handler)
        {
            return new MessageMatcher(new MessageCheck<T>(type, value, handler));
        }

        /// <summary>
        /// Create message matcher
        /// </summary>
        public static MessageMatcher Create(params MessageCheck[] checkers)
        {
            return new MessageMatcher(checkers);
        }

        /// <summary>
        /// Whether this matcher contains a specific check
        /// </summary>
        public bool ContainsCheck(MessageCheck check) => Checkers.Any(x => x.Type == check.Type && x.Value == check.Value);

        /// <summary>
        /// Check whether this listen id matches this
        /// </summary>
        public List<MessageCheck> GetListeners(string listenId) => Checkers.Where(x => x.Check(listenId)).ToList();

        /// <inheritdoc />
        public override string ToString() => string.Join(",", Checkers.Select(x => x.ToString()));
    }

    /// <summary>
    /// Message matching check
    /// </summary>
    public abstract class MessageCheck
    {
        /// <summary>
        /// Type of check
        /// </summary>
        public MessageIdMatchType Type { get; }
        /// <summary>
        /// String value of the check
        /// </summary>
        public string Value { get; }
        /// <summary>
        /// Deserialization type
        /// </summary>
        public abstract Type GetDeserializationType(IMessageAccessor accessor);

        /// <summary>
        /// ctor
        /// </summary>
        public MessageCheck(MessageIdMatchType type, string value)
        {
            Type = type;
            Value = value;
        }

        /// <summary>
        /// Whether this listen id matches this checker
        /// </summary>
        public bool Check(string listenId)
        {
            if (Type == MessageIdMatchType.Full)
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
    /// Message matching check
    /// </summary>
    public class MessageCheck<TServer>: MessageCheck
    {
        private Func<SocketConnection, DataEvent<TServer>, CallResult> _handler;

        /// <inheritdoc />
        public override Type GetDeserializationType(IMessageAccessor accessor) => typeof(TServer);

        /// <summary>
        /// ctor
        /// </summary>
        public MessageCheck(string value, Func<SocketConnection, DataEvent<TServer>, CallResult> handler)
            : this(MessageIdMatchType.Full, value, handler)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        public MessageCheck(MessageIdMatchType type, string value, Func<SocketConnection, DataEvent<TServer>, CallResult> handler)
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

    /// <summary>
    /// Message matching check
    /// </summary>
    public class MessageCheck<TServer, TResult> : MessageCheck
    {
        private Func<SocketConnection, DataEvent<TServer>, CallResult<TResult>> _handler;

        /// <inheritdoc />
        public override Type GetDeserializationType(IMessageAccessor accessor) => typeof(TServer);

        /// <inheritdoc />
        public MessageCheck(MessageIdMatchType type, string value, Func<SocketConnection, DataEvent<TServer>, CallResult<TResult>> handler)
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
