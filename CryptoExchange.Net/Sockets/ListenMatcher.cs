using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.Sockets
{
    /// <summary>
    /// Matching type
    /// </summary>
    public enum ListenMatcherType
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
    public class ListenMatcher
    {
        /// <summary>
        /// Checkers in this matcher
        /// </summary>
        public ListenMatchCheck[] Checkers { get; }

        public Type DeserializationType { get; set; }
        public Func<SocketConnection, DataEvent<object>, CallResult> DataHandler { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public ListenMatcher(string value, Type deserializationType, Func<SocketConnection, DataEvent<object>, CallResult> dataHandler)
            : this(deserializationType, dataHandler, new ListenMatchCheck(ListenMatcherType.Full, value))
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        public ListenMatcher(ListenMatcherType type, string value, Type deserializationType, Func<SocketConnection, DataEvent<object>, CallResult> dataHandler)
            : this(deserializationType, dataHandler, new ListenMatchCheck(type, value))
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        public ListenMatcher(Type deserializationType, Func<SocketConnection, DataEvent<object>, CallResult> dataHandler, params ListenMatchCheck[] checkers)
        {
            Checkers = checkers;
            DeserializationType = deserializationType;
            DataHandler = dataHandler;
        }

        /// <summary>
        /// Whether this matcher contains a specific check
        /// </summary>
        public bool ContainsCheck(ListenMatchCheck check) => Checkers.Any(x => x.Type == check.Type && x.Value == check.Value);

        /// <summary>
        /// Check whether this listen id matches this
        /// </summary>
        public bool Check(string listenId) => Checkers.Any(x => x.Check(listenId));

        /// <inheritdoc />
        public override string ToString() => string.Join(",", Checkers.Select(x => x.ToString()));
    }

    /// <summary>
    /// Matching check
    /// </summary>
    public class ListenMatchCheck
    {
        /// <summary>
        /// Type of check
        /// </summary>
        public ListenMatcherType Type { get; }
        /// <summary>
        /// String value of the check
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// ctor
        /// </summary>
        public ListenMatchCheck(ListenMatcherType type, string value)
        {
            Type = type;
            Value = value;
        }

        /// <summary>
        /// Whether this listen id matches this checker
        /// </summary>
        public bool Check(string listenId)
        {
            if (Type == ListenMatcherType.Full)
                return Value.Equals(listenId, StringComparison.Ordinal);

            return listenId.StartsWith(Value, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override string ToString() => $"{Type} match for \"{Value}\"";
    }
}
