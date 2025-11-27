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
    public class MessageRouter
    {
        /// <summary>
        /// 
        /// </summary>
        public MessageRoute[] Routes { get; }

        // <summary>
        /// ctor
        /// </summary>
        private MessageRouter(params MessageRoute[] routes)
        {
            Routes = routes;
        }

        /// <summary>
        /// Create message matcher
        /// </summary>
        public static MessageRouter Create(string typeIdentifier, string? topicFilter = null)
        {
            return new MessageRouter(new MessageRoute<string>(typeIdentifier, topicFilter, (con, receiveTime, originalData, msg) => new CallResult<string>(default, null, null)));
        }

        /// <summary>
        /// Create message matcher
        /// </summary>
        public static MessageRouter Create<T>(string typeIdentifier)
        {
            return new MessageRouter(new MessageRoute<T>(typeIdentifier, (string?)null, (con, receiveTime, originalData, msg) => new CallResult<string>(default, null, null)));
        }

        /// <summary>
        /// Create message matcher
        /// </summary>
        public static MessageRouter Create<T>(IEnumerable<string> values, Func<SocketConnection, DateTime, string?, T, CallResult> handler)
        {
            return new MessageRouter(values.Select(x => new MessageRoute<T>(x, (string?)null, handler)).ToArray());
        }

        /// <summary>
        /// Create message matcher
        /// </summary>
        public static MessageRouter Create<T>(string typeIdentifier, string topicFilter)
        {
            return new MessageRouter(new MessageRoute<T>(typeIdentifier, topicFilter, (con, receiveTime, originalData, msg) => new CallResult<string>(default, null, null)));
        }

        /// <summary>
        /// Create message matcher
        /// </summary>
        public static MessageRouter Create<T>(string typeIdentifier, Func<SocketConnection, DateTime, string?, T, CallResult> handler)
        {
            return new MessageRouter(new MessageRoute<T>(typeIdentifier, (string?)null, handler));
        }

        /// <summary>
        /// Create message matcher
        /// </summary>
        public static MessageRouter Create<T>(string typeIdentifier, string? topicFilter, Func<SocketConnection, DateTime, string?, T, CallResult> handler)
        {
            return new MessageRouter(new MessageRoute<T>(typeIdentifier, topicFilter, handler));
        }

        /// <summary>
        /// Create message matcher
        /// </summary>
        public static MessageRouter Create<T>(string typeIdentifier, IEnumerable<string>? topicFilters, Func<SocketConnection, DateTime, string?, T, CallResult> handler)
        {
            return new MessageRouter(new MessageRoute<T>(typeIdentifier, topicFilters, handler));
        }

        /// <summary>
        /// Create message matcher
        /// </summary>
        public static MessageRouter Create<T>(IEnumerable<string> typeIdentifiers, IEnumerable<string>? topicFilters, Func<SocketConnection, DateTime, string?, T, CallResult> handler)
        {
            var routes = new List<MessageRoute>();
            foreach(var type in typeIdentifiers)
            {
                if (topicFilters?.Count() > 0)
                {
                    foreach (var filter in topicFilters ?? [])
                        routes.Add(new MessageRoute<T>(type, filter, handler));
                }
                else
                {
                    routes.Add(new MessageRoute<T>(type, (string?)null, handler));
                }
            }

            return new MessageRouter(routes.ToArray());
        }

        /// <summary>
        /// Create message matcher
        /// </summary>
        public static MessageRouter Create(params MessageRoute[] linkers)
        {
            return new MessageRouter(linkers);
        }

        /// <summary>
        /// Whether this matcher contains a specific link
        /// </summary>
        public bool ContainsCheck(MessageRoute link) => Routes.Any(x => x.TypeIdentifier == link.TypeIdentifier && x.TopicFilter == link.TopicFilter);
    }

    public abstract class MessageRoute
    {
        public string TypeIdentifier { get; set; }
        public HashSet<string>? TopicFilter { get; set; }
        /// <summary>
        /// Deserialization type
        /// </summary>
        public abstract Type DeserializationType { get; }
        
        public MessageRoute(string typeIdentifier, string? topicFilter)
        {
            TypeIdentifier = typeIdentifier;
            TopicFilter = topicFilter == null ? null : new HashSet<string>() { topicFilter };
        }

        public MessageRoute(string typeIdentifier, IEnumerable<string>? topicFilters)
        {
            TypeIdentifier = typeIdentifier;
            TopicFilter = topicFilters == null ? null : new HashSet<string>(topicFilters);
        }

        /// <summary>
        /// Message handler
        /// </summary>
        public abstract CallResult Handle(SocketConnection connection, DateTime receiveTime, string? originalData, object data);
    }

    public class MessageRoute<TMessage> : MessageRoute
    {
        private Func<SocketConnection, DateTime, string?, TMessage, CallResult> _handler;

        /// <inheritdoc />
        public override Type DeserializationType => typeof(TMessage);

        /// <summary>
        /// ctor
        /// </summary>
        public MessageRoute(string typeIdentifier, string? topicFilter, Func<SocketConnection, DateTime, string?, TMessage, CallResult> handler)
            : base(typeIdentifier, topicFilter)
        {
            _handler = handler;
        }

        /// <summary>
        /// ctor
        /// </summary>
        public MessageRoute(string typeIdentifier, IEnumerable<string>? topicFilter, Func<SocketConnection, DateTime, string?, TMessage, CallResult> handler)
            : base(typeIdentifier, topicFilter)
        {
            _handler = handler;
        }

        /// <inheritdoc />
        public override CallResult Handle(SocketConnection connection, DateTime receiveTime, string? originalData, object data)
        {
            return _handler(connection, receiveTime, originalData, (TMessage)data);
        }
    }

}
