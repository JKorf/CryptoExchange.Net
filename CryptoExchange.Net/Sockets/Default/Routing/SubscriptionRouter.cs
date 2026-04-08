using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif

namespace CryptoExchange.Net.Sockets.Default.Routing
{
    internal class SubscriptionRouter : ProcessorRouter<SubscriptionRouteCollection>
    {
        public SubscriptionRouter(IEnumerable<MessageRoute> routes) : base(routes)
        {            
        }

        public override Dictionary<string, SubscriptionRouteCollection> BuildFromRoutes(IEnumerable<MessageRoute> routes)
        {
            var newMap = new Dictionary<string, SubscriptionRouteCollection>();
            foreach (var route in routes)
            {
                if (!newMap.TryGetValue(route.TypeIdentifier, out var typeMap))
                {
                    typeMap = new SubscriptionRouteCollection(route.DeserializationType);
                    newMap.Add(route.TypeIdentifier, typeMap);
                }

                typeMap.AddRoute(route.TopicFilter, route);
            }

            foreach (var subEntry in newMap.Values)
                subEntry.Build();

            return newMap;
        }
    }

    internal class SubscriptionRouteCollection : RouteCollection
    {
        public SubscriptionRouteCollection(Type routeType) : base(routeType)
        {
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
            if (topicFilter != null)
            {
                var matchingTopicRoutes = GetRoutesWithMatchingTopicFilter(topicFilter);
                foreach (var route in matchingTopicRoutes ?? [])
                {
                    var thisResult = route.Handle(connection, receiveTime, originalData, data);
                    handled = true;

                    if (thisResult != null)                    
                        result ??= thisResult;
                }
            }

            return handled;
        }
    }
}