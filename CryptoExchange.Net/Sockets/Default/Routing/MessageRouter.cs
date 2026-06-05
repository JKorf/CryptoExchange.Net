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
        /// Create a void handler
        /// </summary>
        public static MessageRouter CreateVoid<TMessage>(string typeIdentifier)
        {
            return new MessageRouter(new EventRoute<TMessage>(typeIdentifier, null, (con, time, originalData, msg) => CallResult<TMessage>.Ok(default!)));
        }

        /// <summary>
        /// Create a router for handling event messages
        /// </summary>
        public static MessageRouter CreateForEvent<TMessage>(string typeIdentifier, Func<SocketConnection, DateTime, string?, TMessage, CallResult?> handler, bool multipleReaders = false)
        {
            return new MessageRouter(new EventRoute<TMessage>(typeIdentifier, null, handler)
            {
                MultipleReaders = multipleReaders
            });
        }

        /// <summary>
        /// Create a router for handling event messages
        /// </summary>
        public static MessageRouter CreateForEvent<TMessage>(IEnumerable<string> typeIdentifier, Func<SocketConnection, DateTime, string?, TMessage, CallResult?> handler, bool multipleReaders = false)
        {
            return new MessageRouter(typeIdentifier.Select(x => new EventRoute<TMessage>(x, null, handler)
            {
                MultipleReaders = multipleReaders
            }).ToArray());
        }

        /// <summary>
        /// Create a router for handling event messages
        /// </summary>
        public static MessageRouter CreateForEvent<TMessage>(string typeIdentifier, string? topicFilter, Func<SocketConnection, DateTime, string?, TMessage, CallResult?> handler, bool multipleReaders = false)
        {
            return new MessageRouter(new EventRoute<TMessage>(typeIdentifier, topicFilter, handler)
            {
                MultipleReaders = multipleReaders
            });
        }

        /// <summary>
        /// Create a router for handling event messages
        /// </summary>
        public static MessageRouter CreateForEvent<TMessage>(string typeIdentifier, IEnumerable<string> topicFilters, Func<SocketConnection, DateTime, string?, TMessage, CallResult?> handler, bool multipleReaders = false)
        {
            return new MessageRouter(topicFilters.Select(x => new EventRoute<TMessage>(typeIdentifier, x, handler)
            {
                MultipleReaders = multipleReaders
            }).ToArray());
        }

        /// <summary>
        /// Create a router for handling query responses
        /// </summary>
        public static MessageRouter CreateForQuery<TMessage>(IEnumerable<string> typeIdentifier, Func<SocketConnection, DateTime, string?, TMessage, CallResult<TMessage>?> handler, bool multipleReaders = false)
        {
            return new MessageRouter(typeIdentifier.Select(x => new QueryRoute<TMessage>(x, null, handler)
            {
                MultipleReaders = multipleReaders
            }).ToArray());
        }

        /// <summary>
        /// Create a router for handling query responses
        /// </summary>
        public static MessageRouter CreateForQuery<TMessage>(string typeIdentifier, Func<SocketConnection, DateTime, string?, TMessage, CallResult<TMessage>?> handler, bool multipleReaders = false)
        {
            return new MessageRouter(new QueryRoute<TMessage>(typeIdentifier, null, handler)
            {
                MultipleReaders = multipleReaders
            });
        }

        /// <summary>
        /// Create a router for handling query responses
        /// </summary>
        public static MessageRouter CreateForQuery<TMessage>(string typeIdentifier, string? topicFilter, Func<SocketConnection, DateTime, string?, TMessage, CallResult<TMessage>?> handler, bool multipleReaders = false)
        {
            return new MessageRouter(new QueryRoute<TMessage>(typeIdentifier, topicFilter, handler)
            {
                MultipleReaders = multipleReaders
            });
        }

        /// <summary>
        /// Create a router for handling query responses
        /// </summary>
        public static MessageRouter CreateForQuery<TMessage, TResult>(string typeIdentifier, Func<SocketConnection, DateTime, string?, TMessage, CallResult<TResult>?> handler, bool multipleReaders = false)
        {
            return new MessageRouter(new QueryRoute<TMessage, TResult>(typeIdentifier, null, handler)
            {
                MultipleReaders = multipleReaders
            });
        }

        /// <summary>
        /// Create a router for handling query responses
        /// </summary>
        public static MessageRouter CreateForQuery<TMessage, TResult>(string typeIdentifier, string? topicFilter, Func<SocketConnection, DateTime, string?, TMessage, CallResult<TResult>?> handler, bool multipleReaders = false)
        {
            return new MessageRouter(new QueryRoute<TMessage, TResult>(typeIdentifier, topicFilter, handler)
            {
                MultipleReaders = multipleReaders
            });
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
