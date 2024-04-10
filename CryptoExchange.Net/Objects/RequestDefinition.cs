using CryptoExchange.Net.RateLimiting;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace CryptoExchange.Net.Objects
{
    public class RequestDefinition
    {
        // Basics
        public Uri Uri { get; set; }
        public HttpMethod Method { get; set; }
        public bool Authenticated { get; set; }

        // Formating
        public RequestBodyFormat? RequestBodyFormat { get; set; }
        public HttpMethodParameterPosition? ParameterPosition { get; set; }
        public ArrayParametersSerialization? ArraySerialization { get; set; }

        // Rate limiting
        public int Weight { get; set; } = 1;
        public IRateLimitGate RateLimitGate { get; set; }
        public int? EndpointLimitCount { get; set; }
        public TimeSpan? EndpointLimitPeriod { get; set; }

        public RequestDefinition(HttpMethod method, Uri uri, bool signed = false)
        {
            Uri = uri;
            Method = method;
        }

        public RequestDefinition(HttpMethod method, Uri uri, IRateLimitGate rateLimitGate, int weight = 1, bool authenticated = false)
        {
            Uri = uri;
            Method = method;
            RateLimitGate = rateLimitGate;
            Weight = weight;
            Authenticated = authenticated;
        }

        public RequestDefinition(HttpMethod method, Uri uri, IRateLimitGate rateLimitGate, int endpointLimitCount, TimeSpan endpointLimitPeriod, int weight = 1, bool authenticated = false)
        {
            Uri = uri;
            Method = method;
            RateLimitGate = rateLimitGate;
            EndpointLimitCount = endpointLimitCount;
            EndpointLimitPeriod = endpointLimitPeriod;
            Weight = weight;
            Authenticated = authenticated;
        }
    }
}
