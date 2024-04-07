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
    public class PartialEndpointTotalLimitGuard : IRateLimitGuard
    {
        public string Name => "PartialEndpointLimitGuard";

        private readonly string _endpoint;
        private readonly RateLimitTracker _tracker;
        private readonly int _limit;
        private readonly TimeSpan _timespan;

        public PartialEndpointTotalLimitGuard(string endpoint, int limit, TimeSpan timespan)
        {
            _endpoint = endpoint;
            _tracker = new RateLimitTracker(limit, timespan);
        }


        public TimeSpan Check(ILogger logger, Uri url, HttpMethod method, bool signed, SecureString? apiKey, int requestWeight)
        {
            if (!url.AbsolutePath.StartsWith(_endpoint))
                return TimeSpan.Zero;

            return _tracker.ProcessTopic(requestWeight);
        }

        public void Enter(Uri url, HttpMethod method, bool signed, SecureString? apiKey, int requestWeight)
        {
            if (!url.AbsolutePath.StartsWith(_endpoint))
                return;

            _tracker.AddEntry(requestWeight);
        }

        public string GetState(Uri url, HttpMethod method, bool signed, SecureString? apiKey, int requestWeight)
        {
            return $"Current: {_tracker.Current}, limit {_tracker.Limit}";
        }
    }
}
