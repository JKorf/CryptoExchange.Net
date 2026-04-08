using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;

namespace CryptoExchange.Net.Sockets.Default.Routing
{
    internal class QueryRouter : ProcessorRouter<QueryRouteCollection>
    {
        public QueryRouter(IEnumerable<MessageRoute> routes) : base(routes)
        {            
        }

        public override Dictionary<string, QueryRouteCollection> BuildFromRoutes(IEnumerable<MessageRoute> routes)
        {
            var newMap = new Dictionary<string, QueryRouteCollection>();
            foreach (var route in routes)
            {
                if (!newMap.TryGetValue(route.TypeIdentifier, out var typeMap))
                {
                    typeMap = new QueryRouteCollection(route.DeserializationType);
                    newMap.Add(route.TypeIdentifier, typeMap);
                }

                typeMap.AddRoute(route.TopicFilter, route);
            }

            foreach (var subEntry in newMap.Values)
                subEntry.Build();

            return newMap;
        }
    }

    internal class QueryRouteCollection : RouteCollection
    {
        public bool MultipleReaders { get; private set; }

        public QueryRouteCollection(Type routeType) : base(routeType)
        {
        }

        public override void AddRoute(string? topicFilter, MessageRoute route)
        {
            base.AddRoute(topicFilter, route);

            if (route.MultipleReaders)
                MultipleReaders = true;
        }

        public override bool Handle(string? topicFilter, SocketConnection connection, DateTime receiveTime, string? originalData, object data, out CallResult? result)
        {
            result = null;

            // Routes without topic filter handle both when the message topic is empty and when it is not, so we always call them
            var handled = false;
            foreach (var route in _routesWithoutTopicFilter)
            {
                var thisResult = route.Handle(connection, receiveTime, originalData, data);
                if (thisResult != null)
                    result ??= thisResult;

                handled = true;
            }

            // Forward to routes with matching topic filter, if any
            if (topicFilter == null)
                return handled;
            
            var matchingTopicRoutes = GetRoutesWithMatchingTopicFilter(topicFilter);
            if (matchingTopicRoutes == null)
                return handled;
            
            foreach (var route in matchingTopicRoutes)
            {
                var thisResult = route.Handle(connection, receiveTime, originalData, data);
                handled = true;

                if (thisResult != null)
                {
                    result ??= thisResult;

                    if (!MultipleReaders)
                        break;
                }
            }

            return handled;
        }
    }
}