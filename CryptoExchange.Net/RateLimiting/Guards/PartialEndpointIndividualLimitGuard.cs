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
    public class PartialEndpointIndividualLimitGuard : IRateLimitGuard
    {
        public string Name => "PartialEndpointIndividualLimitGuard";

        private readonly string _endpoint;
        private readonly Dictionary<string, RateLimitTracker> _trackers;
        private readonly int _limit;
        private readonly TimeSpan _timespan;

        public PartialEndpointIndividualLimitGuard(string endpoint, int limit, TimeSpan timespan)
        {
            _endpoint = endpoint;
            _limit = limit;
            _timespan = timespan;
            _trackers = new Dictionary<string, RateLimitTracker>();
        }


        public TimeSpan Check(ILogger logger, Uri url, HttpMethod method, bool signed, SecureString? apiKey, int requestWeight)
        {
            if (!url.AbsolutePath.StartsWith(_endpoint))
                return TimeSpan.Zero;

            if (!_trackers.TryGetValue(url.AbsolutePath, out var tracker))
            {
                tracker = new RateLimitTracker(_limit, _timespan);
                _trackers[url.AbsolutePath] = tracker;
            }

            return tracker.ProcessTopic(requestWeight);
        }

        public void Enter(Uri url, HttpMethod method, bool signed, SecureString? apiKey, int requestWeight)
        {
            if (!url.AbsolutePath.StartsWith(_endpoint))
                return;

            _trackers[url.AbsolutePath].AddEntry(requestWeight);
        }

        public string GetState(Uri url, HttpMethod method, bool signed, SecureString? apiKey, int requestWeight)
        {
            var tracker = _trackers[url.AbsolutePath];
            return $"Current: {tracker.Current}, limit {tracker.Limit}";
        }
    }
}
