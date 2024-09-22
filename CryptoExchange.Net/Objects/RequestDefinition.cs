using CryptoExchange.Net.RateLimiting.Interfaces;
using System.Net.Http;

namespace CryptoExchange.Net.Objects
{
    /// <summary>
    /// The definition of a rest request
    /// </summary>
    public class RequestDefinition
    {
        private string? _stringRep;

        // Basics

        /// <summary>
        /// Path of the request
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// Http method of the request
        /// </summary>
        public HttpMethod Method { get; set; }
        /// <summary>
        /// Is the request authenticated
        /// </summary>
        public bool Authenticated { get; set; }


        // Formating

        /// <summary>
        /// The body format for this request
        /// </summary>
        public RequestBodyFormat? RequestBodyFormat { get; set; }
        /// <summary>
        /// The position of parameters for this request
        /// </summary>
        public HttpMethodParameterPosition? ParameterPosition { get; set; }
        /// <summary>
        /// The array serialization type for this request
        /// </summary>
        public ArrayParametersSerialization? ArraySerialization { get; set; }

        // Rate limiting
        
        /// <summary>
        /// Request weight
        /// </summary>
        public int Weight { get; set; } = 1;

        /// <summary>
        /// Rate limit gate to use
        /// </summary>
        public IRateLimitGate? RateLimitGate { get; set; }

        /// <summary>
        /// Individual endpoint rate limit guard to use
        /// </summary>
        public IRateLimitGuard? LimitGuard { get; set; }


        /// <summary>
        /// Whether this request should never be cached
        /// </summary>
        public bool PreventCaching { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="path"></param>
        /// <param name="method"></param>
        public RequestDefinition(string path, HttpMethod method)
        {
            Path = path;
            Method = method;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return _stringRep ??= $"{Method} {Path}{(Authenticated ? " authenticated" : "")}";
        }
    }
}
