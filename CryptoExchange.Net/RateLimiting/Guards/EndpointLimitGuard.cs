using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.RateLimiting
{
    public class EndpointLimitGuard : IRateLimitGuard
    {
        public string Name => "EndpointLimitGuard";

        private readonly string _endpoint;
        private readonly RateLimitTracker _tracker;
        private HttpMethod? _method;

        public EndpointLimitGuard(string endpoint, int limit, TimeSpan timespan, HttpMethod? method)
        {
            _tracker = new RateLimitTracker(limit, timespan);
            _endpoint = endpoint;
            _method = method;
        }


        public TimeSpan Check(ILogger logger, Uri url, HttpMethod method, bool signed, SecureString? apiKey, int requestWeight)
        {
            if (!string.Equals(url.AbsolutePath, _endpoint, StringComparison.OrdinalIgnoreCase))
                return TimeSpan.Zero;

            if (_method != null && _method != method)
                return TimeSpan.Zero;

            return _tracker.ProcessTopic(requestWeight);
        }

        public void Enter(Uri url, HttpMethod method, bool signed, SecureString? apiKey, int requestWeight)
        {
            _tracker.AddEntry(requestWeight);
        }

        public string GetState(Uri url, HttpMethod method, bool signed, SecureString? apiKey, int requestWeight)
            => $"Current: {_tracker.Current}, limit {_tracker.Limit}";
    }
}
