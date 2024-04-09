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
    public class PartialEndpointIndividualLimitGuard : LimitGuard, IRateLimitGuard
    {
        public string Name => "PartialEndpointIndividualLimitGuard";

        private readonly string _endpoint;
        private readonly Dictionary<string, WindowTracker> _trackers;

        public PartialEndpointIndividualLimitGuard(string endpoint, int limit, TimeSpan timespan) : base(limit, timespan)
        {
            _endpoint = endpoint;
            _trackers = new Dictionary<string, WindowTracker>();
        }


        public TimeSpan Check(ILogger logger, RateLimitType type, Uri url, HttpMethod method, bool signed, SecureString? apiKey, int requestWeight)
        {
            if (!url.AbsolutePath.StartsWith(_endpoint))
                return TimeSpan.Zero;

            if (!_trackers.TryGetValue(url.AbsolutePath, out var tracker))
            {
                tracker = CreateTracker();
                _trackers[url.AbsolutePath] = tracker;
            }

            return tracker.ProcessTopic(requestWeight);
        }

        public void Enter(RateLimitType type, Uri url, HttpMethod method, bool signed, SecureString? apiKey, int requestWeight)
        {
            if (!url.AbsolutePath.StartsWith(_endpoint))
                return;

            _trackers[url.AbsolutePath].AddEntry(requestWeight);
        }

        public WindowTracker? GetTracker(RateLimitType type, Uri url, HttpMethod method, bool signed, SecureString? apiKey) => _trackers[url.AbsolutePath];
        
    }
}
