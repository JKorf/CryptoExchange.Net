using CryptoExchange.Net.RateLimiting.Guards;
using CryptoExchange.Net.RateLimiting.Trackers;
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
    public class PartialEndpointTotalLimitGuard : LimitGuard, IRateLimitGuard
    {
        public string Name => "PartialEndpointLimitGuard";

        private readonly string _endpoint;
        private WindowTracker _tracker;

        public PartialEndpointTotalLimitGuard(string endpoint, int limit, TimeSpan timespan): base(limit, timespan)
        {
            _endpoint = endpoint;
        }


        public TimeSpan Check(ILogger logger, Uri url, HttpMethod method, bool signed, SecureString? apiKey, int requestWeight)
        {
            if (!url.AbsolutePath.StartsWith(_endpoint))
                return TimeSpan.Zero;

            if (_tracker == null)
                _tracker = CreateTracker();

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
