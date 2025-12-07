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
    /// <summary>
    /// Message router
    /// </summary>
    public class MessageRouter
    {
        /// <summary>
        /// The routes registered for this router
        /// </summary>
        public MessageRoute[] Routes { get; }

        /// <summary>
        /// ctor
        /// </summary>
        private MessageRouter(params MessageRoute[] routes)
        {
            Routes = routes;
        }

        /// <summary>
        /// Create message router without specific message handler
        /// </summary>
        public static MessageRouter CreateWithoutHandler<T>(string typeIdentifier)
        {
            return new MessageRouter(new MessageRoute<T>(typeIdentifier, (string?)null, (con, receiveTime, originalData, msg) => new CallResult<string>(default, null, null)));
        }

        /// <summary>
        /// Create message router without specific message handler
        /// </summary>
        public static MessageRouter CreateWithoutHandler<T>(string typeIdentifier, string topicFilter)
        {
            return new MessageRouter(new MessageRoute<T>(typeIdentifier, topicFilter, (con, receiveTime, originalData, msg) => new CallResult<string>(default, null, null)));
        }

        /// <summary>
        /// Create message router without topic filter
        /// </summary>
        public static MessageRouter CreateWithoutTopicFilter<T>(IEnumerable<string> values, Func<SocketConnection, DateTime, string?, T, CallResult> handler)
        {
            return new MessageRouter(values.Select(x => new MessageRoute<T>(x, (string?)null, handler)).ToArray());
        }

        /// <summary>
        /// Create message router without topic filter
        /// </summary>
        public static MessageRouter CreateWithoutTopicFilter<T>(string typeIdentifier, Func<SocketConnection, DateTime, string?, T, CallResult> handler)
        {
            return new MessageRouter(new MessageRoute<T>(typeIdentifier, (string?)null, handler));
        }

        /// <summary>
        /// Create message router with topic filter
        /// </summary>
        public static MessageRouter CreateWithTopicFilter<T>(string typeIdentifier, string topicFilter, Func<SocketConnection, DateTime, string?, T, CallResult> handler)
        {
            return new MessageRouter(new MessageRoute<T>(typeIdentifier, topicFilter, handler));
        }

        /// <summary>
        /// Create message router with topic filter
        /// </summary>
        public static MessageRouter CreateWithTopicFilter<T>(IEnumerable<string> typeIdentifiers, string topicFilter, Func<SocketConnection, DateTime, string?, T, CallResult> handler)
        {
            var routes = new List<MessageRoute>();
            foreach (var type in typeIdentifiers)
                routes.Add(new MessageRoute<T>(type, topicFilter, handler));

            return new MessageRouter(routes.ToArray());
        }

        /// <summary>
        /// Create message router with topic filter
        /// </summary>
        public static MessageRouter CreateWithTopicFilters<T>(string typeIdentifier, IEnumerable<string> topicFilters, Func<SocketConnection, DateTime, string?, T, CallResult> handler)
        {
            var routes = new List<MessageRoute>();
            foreach (var filter in topicFilters)
                routes.Add(new MessageRoute<T>(typeIdentifier, filter, handler));

            return new MessageRouter(routes.ToArray());
        }

        /// <summary>
        /// Create message router with topic filter
        /// </summary>
        public static MessageRouter CreateWithTopicFilters<T>(IEnumerable<string> typeIdentifiers, IEnumerable<string> topicFilters, Func<SocketConnection, DateTime, string?, T, CallResult> handler)
        {
            var routes = new List<MessageRoute>();
            foreach (var type in typeIdentifiers)
            {
                foreach (var filter in topicFilters)
                    routes.Add(new MessageRoute<T>(type, filter, handler));
            }

            return new MessageRouter(routes.ToArray());
        }

        /// <summary>
        /// Create message router with optional topic filter
        /// </summary>
        public static MessageRouter CreateWithOptionalTopicFilter<T>(string typeIdentifier, string? topicFilter, Func<SocketConnection, DateTime, string?, T, CallResult> handler)
        {
            return new MessageRouter(new MessageRoute<T>(typeIdentifier, topicFilter, handler));
        }

        /// <summary>
        /// Create message router with optional topic filter
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
        /// Create message router with optional topic filter
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
        /// Create message matcher with specific routes
        /// </summary>
        public static MessageRouter Create(params MessageRoute[] routes)
        {
            return new MessageRouter(routes);
        }

        /// <summary>
        /// Whether this matcher contains a specific link
        /// </summary>
        public bool ContainsCheck(MessageRoute route) => Routes.Any(x => x.TypeIdentifier == route.TypeIdentifier && x.TopicFilter == route.TopicFilter);
    }

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
        public abstract CallResult Handle(SocketConnection connection, DateTime receiveTime, string? originalData, object data);
    }

    /// <summary>
    /// Message route
    /// </summary>
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

        /// <summary>
        /// Create route without topic filter
        /// </summary>
        public static MessageRoute<TMessage> CreateWithoutTopicFilter(string typeIdentifier, Func<SocketConnection, DateTime, string?, TMessage, CallResult> handler)
        {
            return new MessageRoute<TMessage>(typeIdentifier, null, handler);
        }

        /// <summary>
        /// Create route with optional topic filter
        /// </summary>
        public static MessageRoute<TMessage> CreateWithOptionalTopicFilter(string typeIdentifier, string? topicFilter, Func<SocketConnection, DateTime, string?, TMessage, CallResult> handler)
        {
            return new MessageRoute<TMessage>(typeIdentifier, topicFilter, handler);
        }

        /// <summary>
        /// Create route with topic filter
        /// </summary>
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
