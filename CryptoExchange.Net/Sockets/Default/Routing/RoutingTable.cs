using CryptoExchange.Net.Sockets.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif

namespace CryptoExchange.Net.Sockets.Default.Routing
{
    /// <summary>
    /// Routing table
    /// </summary>
    public class RoutingTable
    {
#if NET8_0_OR_GREATER
        private FrozenDictionary<string, TypeRoutingCollection> _typeRoutingCollections = new Dictionary<string, TypeRoutingCollection>().ToFrozenDictionary();
#else
        private Dictionary<string, TypeRoutingCollection> _typeRoutingCollections = new();
#endif

        /// <summary>
        /// Update the routing table
        /// </summary>
        /// <param name="processors"></param>
        public void Update(IEnumerable<IMessageProcessor> processors)
        {
            var newTypeMap = new Dictionary<string, TypeRoutingCollection>();
            foreach (var entry in processors)
            {
                foreach (var route in entry.MessageRouter.Routes)
                {
                    if (!newTypeMap.ContainsKey(route.TypeIdentifier))
                        newTypeMap.Add(route.TypeIdentifier, new TypeRoutingCollection(route.DeserializationType));

                    if (!newTypeMap[route.TypeIdentifier].Handlers.Contains(entry))
                        newTypeMap[route.TypeIdentifier].Handlers.Add(entry);
                }
            }

#if NET8_0_OR_GREATER
            _typeRoutingCollections = newTypeMap.ToFrozenDictionary();
#else
            _typeRoutingCollections = newTypeMap;
#endif
        }

        /// <summary>
        /// Get route table entry for a type identifier
        /// </summary>
        public TypeRoutingCollection? GetRouteTableEntry(string typeIdentifier)
        {
            return _typeRoutingCollections.TryGetValue(typeIdentifier, out var entry) ? entry : null;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            string GetFormattedName(Type type)
            {
                if(type.IsGenericType)
                {
                    string genericArguments = type.GetGenericArguments()
                                        .Select(x => x.Name)
                                        .Aggregate((x1, x2) => $"{x1}, {x2}");
                    return $"{type.Name.Substring(0, type.Name.IndexOf("`"))}"
                         + $"<{genericArguments}>";
                }
                return type.Name;
            }

            string GetRouteType(MessageRoute route)
            {
                var routeTypeName = route.GetType().Name;
                return routeTypeName.Contains("QueryRoute") ? "Query" : routeTypeName.Contains("EventRoute") ? "Subscription" : "";
            }

            var sb = new StringBuilder();
            foreach (var entry in _typeRoutingCollections)
            {
                sb.AppendLine($"TypeIdentifier: `{entry.Key}`, DeserializationType: `{GetFormattedName(entry.Value.DeserializationType)}`");
                foreach(var item in entry.Value.Handlers)
                {
                    var typeRoutes = item.MessageRouter.Routes.Where(x => x.TypeIdentifier == entry.Key).ToList();
                    if (typeRoutes.Count == 1)
                    {
                        var route = typeRoutes[0];
                        sb.Append($" - {GetRouteType(route)}");
                        if (route.TopicFilter != null)
                            sb.AppendLine($" with filter `{route.TopicFilter}`");
                        else
                            sb.AppendLine(" without filter");
                    }
                    else
                    {
                        sb.AppendLine(" - ");
                        foreach (var route in typeRoutes)
                        {
                            if (route.TopicFilter == null)
                                sb.AppendLine($"   {GetRouteType(route)} Route without topic filter");
                            else
                                sb.AppendLine($"   {GetRouteType(route)} Route with topic filter `{route.TopicFilter}`");                            
                        }
                    }

                }
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Routing table entry
    /// </summary>
    public record TypeRoutingCollection
    {
        /// <summary>
        /// Whether the deserialization type is string
        /// </summary>
        public bool IsStringOutput { get; set; }
        /// <summary>
        /// The deserialization type
        /// </summary>
        public Type DeserializationType { get; set; }
        /// <summary>
        /// Message processors
        /// </summary>
        public List<IMessageProcessor> Handlers { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public TypeRoutingCollection(Type deserializationType)
        {
            IsStringOutput = deserializationType == typeof(string);
            DeserializationType = deserializationType;
            Handlers = new List<IMessageProcessor>();
        }
    }
}
