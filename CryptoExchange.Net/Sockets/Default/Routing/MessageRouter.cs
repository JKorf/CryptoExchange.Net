using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoExchange.Net.Sockets.Default.Routing
{
    /// <summary>
    /// Message router
    /// </summary>
    public class MessageRouter
    {
        private ProcessorRouter? _routingTable;

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
        /// Build the route mapping
        /// </summary>
        public void BuildQueryRouter()
        {
            _routingTable = new QueryRouter(Routes);
        }

        /// <summary>
        /// Build the route mapping
        /// </summary>
        public void BuildSubscriptionRouter()
        {
            _routingTable = new SubscriptionRouter(Routes);
        }

        /// <summary>
        /// Handle message
        /// </summary>
        public bool Handle(string typeIdentifier, string? topicFilter, SocketConnection connection, DateTime receiveTime, string? originalData, object data, out CallResult? result)
        {
            var routeCollection = (_routingTable ?? throw new NullReferenceException("Routing table not build before handling")).GetRoutes(typeIdentifier);
            if (routeCollection == null)
                throw new InvalidOperationException($"No routes for {typeIdentifier} message type");

            return routeCollection.Handle(topicFilter, connection, receiveTime, originalData, data, out result);
        }

        /// <summary>
        /// Create message router without specific message handler
        /// </summary>
        public static MessageRouter CreateWithoutHandler<T>(string typeIdentifier, bool multipleReaders = false)
        {
            return new MessageRouter(new MessageRoute<T>(typeIdentifier, null, (con, receiveTime, originalData, msg) => new CallResult<T>(default, null, null), multipleReaders));
        }

        /// <summary>
        /// Create message router without specific message handler
        /// </summary>
        public static MessageRouter CreateWithoutHandler<T>(string typeIdentifier, string topicFilter, bool multipleReaders = false)
        {
            return new MessageRouter(new MessageRoute<T>(typeIdentifier, topicFilter, (con, receiveTime, originalData, msg) => new CallResult<string>(default, null, null), multipleReaders));
        }

        /// <summary>
        /// Create message router without topic filter
        /// </summary>
        public static MessageRouter CreateWithoutTopicFilter<T>(IEnumerable<string> values, Func<SocketConnection, DateTime, string?, T, CallResult?> handler, bool multipleReaders = false)
        {
            return new MessageRouter(values.Select(x => new MessageRoute<T>(x, null, handler, multipleReaders)).ToArray());
        }

        /// <summary>
        /// Create message router without topic filter
        /// </summary>
        public static MessageRouter CreateWithoutTopicFilter<T>(string typeIdentifier, Func<SocketConnection, DateTime, string?, T, CallResult?> handler, bool multipleReaders = false)
        {
            return new MessageRouter(new MessageRoute<T>(typeIdentifier, null, handler, multipleReaders));
        }

        /// <summary>
        /// Create message router with topic filter
        /// </summary>
        public static MessageRouter CreateWithTopicFilter<T>(string typeIdentifier, string topicFilter, Func<SocketConnection, DateTime, string?, T, CallResult?> handler, bool multipleReaders = false)
        {
            return new MessageRouter(new MessageRoute<T>(typeIdentifier, topicFilter, handler, multipleReaders));
        }

        /// <summary>
        /// Create message router with topic filter
        /// </summary>
        public static MessageRouter CreateWithTopicFilter<T>(IEnumerable<string> typeIdentifiers, string topicFilter, Func<SocketConnection, DateTime, string?, T, CallResult?> handler, bool multipleReaders = false)
        {
            var routes = new List<MessageRoute>();
            foreach (var type in typeIdentifiers)
                routes.Add(new MessageRoute<T>(type, topicFilter, handler, multipleReaders));

            return new MessageRouter(routes.ToArray());
        }

        /// <summary>
        /// Create message router with topic filter
        /// </summary>
        public static MessageRouter CreateWithTopicFilters<T>(string typeIdentifier, IEnumerable<string> topicFilters, Func<SocketConnection, DateTime, string?, T, CallResult?> handler, bool multipleReaders = false)
        {
            var routes = new List<MessageRoute>();
            foreach (var filter in topicFilters)
                routes.Add(new MessageRoute<T>(typeIdentifier, filter, handler, multipleReaders));

            return new MessageRouter(routes.ToArray());
        }

        /// <summary>
        /// Create message router with topic filter
        /// </summary>
        public static MessageRouter CreateWithTopicFilters<T>(IEnumerable<string> typeIdentifiers, IEnumerable<string> topicFilters, Func<SocketConnection, DateTime, string?, T, CallResult?> handler, bool multipleReaders = false)
        {
            var routes = new List<MessageRoute>();
            foreach (var type in typeIdentifiers)
            {
                foreach (var filter in topicFilters)
                    routes.Add(new MessageRoute<T>(type, filter, handler, multipleReaders));
            }

            return new MessageRouter(routes.ToArray());
        }

        /// <summary>
        /// Create message router with optional topic filter
        /// </summary>
        public static MessageRouter CreateWithOptionalTopicFilter<T>(string typeIdentifier, string? topicFilter, Func<SocketConnection, DateTime, string?, T, CallResult?> handler, bool multipleReaders = false)
        {
            return new MessageRouter(new MessageRoute<T>(typeIdentifier, topicFilter, handler, multipleReaders));
        }

        /// <summary>
        /// Create message router with optional topic filter
        /// </summary>
        public static MessageRouter CreateWithOptionalTopicFilters<T>(string typeIdentifier, IEnumerable<string>? topicFilters, Func<SocketConnection, DateTime, string?, T, CallResult?> handler, bool multipleReaders = false)
        {
            var routes = new List<MessageRoute>();
            if (topicFilters?.Count() > 0)
            {
                foreach (var filter in topicFilters)
                    routes.Add(new MessageRoute<T>(typeIdentifier, filter, handler, multipleReaders));
            }
            else
            {
                routes.Add(new MessageRoute<T>(typeIdentifier, null, handler, multipleReaders));
            }
                
            return new MessageRouter(routes.ToArray());
        }

        /// <summary>
        /// Create message router with optional topic filter
        /// </summary>
        public static MessageRouter CreateWithOptionalTopicFilters<T>(IEnumerable<string> typeIdentifiers, IEnumerable<string>? topicFilters, Func<SocketConnection, DateTime, string?, T, CallResult?> handler, bool multipleReaders = false)
        {
            var routes = new List<MessageRoute>();
            foreach (var typeIdentifier in typeIdentifiers)
            {
                if (topicFilters?.Count() > 0)
                {
                    foreach (var filter in topicFilters)
                        routes.Add(new MessageRoute<T>(typeIdentifier, filter, handler, multipleReaders));
                }
                else
                {
                    routes.Add(new MessageRoute<T>(typeIdentifier, null, handler, multipleReaders));
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
}
