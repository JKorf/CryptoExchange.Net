using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Collections;
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
        public static MessageRouter CreateWithoutHandler<T>(string typeIdentifier)
        {
            return new MessageRouter(new MessageRoute<T>(typeIdentifier, (string?)null, (con, receiveTime, originalData, msg) => new CallResult<string>(default, null, null)));
        }

        /// <summary>
        /// Create message matcher
        /// </summary>
        public static MessageRouter CreateWithoutTopicFilter<T>(IEnumerable<string> values, Func<SocketConnection, DateTime, string?, T, CallResult> handler)
        {
            return new MessageRouter(values.Select(x => new MessageRoute<T>(x, (string?)null, handler)).ToArray());
        }

        /// <summary>
        /// Create message matcher
        /// </summary>
        public static MessageRouter CreateWithoutHandler<T>(string typeIdentifier, string topicFilter)
        {
            return new MessageRouter(new MessageRoute<T>(typeIdentifier, topicFilter, (con, receiveTime, originalData, msg) => new CallResult<string>(default, null, null)));
        }

        /// <summary>
        /// Create message matcher
        /// </summary>
        public static MessageRouter CreateWithoutTopicFilter<T>(string typeIdentifier, Func<SocketConnection, DateTime, string?, T, CallResult> handler)
        {
            return new MessageRouter(new MessageRoute<T>(typeIdentifier, (string?)null, handler));
        }

        /// <summary>
        /// Create message matcher
        /// </summary>
        public static MessageRouter CreateWithTopicFilter<T>(string typeIdentifier, string topicFilter, Func<SocketConnection, DateTime, string?, T, CallResult> handler)
        {
            return new MessageRouter(new MessageRoute<T>(typeIdentifier, topicFilter, handler));
        }

        /// <summary>
        /// Create message matcher
        /// </summary>
        public static MessageRouter CreateWithOptionalTopicFilter<T>(string typeIdentifier, string? topicFilter, Func<SocketConnection, DateTime, string?, T, CallResult> handler)
        {
            return new MessageRouter(new MessageRoute<T>(typeIdentifier, topicFilter, handler));
        }

        /// <summary>
        /// Create message matcher
        /// </summary>
        public static MessageRouter CreateWithOptionalTopicFilters<T>(string typeIdentifier, IEnumerable<string>? topicFilters, Func<SocketConnection, DateTime, string?, T, CallResult> handler)
        {
            var routes = new List<MessageRoute>();
            if (topicFilters?.Count() > 0)
            {
                foreach (var filter in topicFilters)
                    routes.Add(new MessageRoute<T>(typeIdentifier, filter, handler));
            }
            else
            {
                routes.Add(new MessageRoute<T>(typeIdentifier, null, handler));
            }
                
            return new MessageRouter(routes.ToArray());
        }

        /// <summary>
        /// Create message matcher
        /// </summary>
        public static MessageRouter CreateWithOptionalTopicFilters<T>(IEnumerable<string> typeIdentifiers, IEnumerable<string>? topicFilters, Func<SocketConnection, DateTime, string?, T, CallResult> handler)
        {
            var routes = new List<MessageRoute>();
            foreach (var typeIdentifier in typeIdentifiers)
            {
                if (topicFilters?.Count() > 0)
                {
                    foreach (var filter in topicFilters)
                        routes.Add(new MessageRoute<T>(typeIdentifier, filter, handler));
                }
                else
                {
                    routes.Add(new MessageRoute<T>(typeIdentifier, null, handler));
                }
            }

            return new MessageRouter(routes.ToArray());
        }

        /// <summary>
        /// Create message matcher
        /// </summary>
        public static MessageRouter CreateWithTopicFilters<T>(string typeIdentifier, IEnumerable<string> topicFilters, Func<SocketConnection, DateTime, string?, T, CallResult> handler)
        {
            var routes = new List<MessageRoute>();
            foreach (var filter in topicFilters)
                routes.Add(new MessageRoute<T>(typeIdentifier, filter, handler));

            return new MessageRouter(routes.ToArray());
        }

        /// <summary>
        /// Create message matcher
        /// </summary>
        public static MessageRouter CreateWithTopicFilters<T>(IEnumerable<string> typeIdentifiers, IEnumerable<string> topicFilters, Func<SocketConnection, DateTime, string?, T, CallResult> handler)
        {
            var routes = new List<MessageRoute>();
            foreach(var type in typeIdentifiers)
            {
                foreach (var filter in topicFilters)
                    routes.Add(new MessageRoute<T>(type, filter, handler));
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
        public string? TopicFilter { get; set; }
        /// <summary>
        /// Deserialization type
        /// </summary>
        public abstract Type DeserializationType { get; }
        
        public MessageRoute(string typeIdentifier, string? topicFilter)
        {
            TypeIdentifier = typeIdentifier;
            TopicFilter = topicFilter;
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
        internal MessageRoute(string typeIdentifier, string? topicFilter, Func<SocketConnection, DateTime, string?, TMessage, CallResult> handler)
            : base(typeIdentifier, topicFilter)
        {
            _handler = handler;
        }

        public static MessageRoute<TMessage> CreateWithoutTopicFilter(string typeIdentifier, Func<SocketConnection, DateTime, string?, TMessage, CallResult> handler)
        {
            return new MessageRoute<TMessage>(typeIdentifier, null, handler);
        }

        public static MessageRoute<TMessage> CreateWithOptionalTopicFilter(string typeIdentifier, string? topicFilter, Func<SocketConnection, DateTime, string?, TMessage, CallResult> handler)
        {
            return new MessageRoute<TMessage>(typeIdentifier, topicFilter, handler);
        }

        public static MessageRoute<TMessage> CreateWithTopicFilter(string typeIdentifier, string topicFilter, Func<SocketConnection, DateTime, string?, TMessage, CallResult> handler)
        {
            return new MessageRoute<TMessage>(typeIdentifier, topicFilter, handler);
        }

        /// <inheritdoc />
        public override CallResult Handle(SocketConnection connection, DateTime receiveTime, string? originalData, object data)
        {
            return _handler(connection, receiveTime, originalData, (TMessage)data);
        }
    }

}
