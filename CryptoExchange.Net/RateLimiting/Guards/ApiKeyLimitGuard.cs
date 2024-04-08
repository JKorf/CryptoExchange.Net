using CryptoExchange.Net.Objects;
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
    public class ApiKeyLimitGuard : LimitGuard, IRateLimitGuard
    {
        public string Name => "ApiKeyLimitGuard";

        private readonly Dictionary<string, WindowTracker> _trackers;

        public ApiKeyLimitGuard(int limit, TimeSpan timespan): base(limit, timespan)
        {
            _trackers = new Dictionary<string, WindowTracker>();
        }

        public TimeSpan Check(ILogger logger, Uri url, HttpMethod method, bool signed, SecureString? apiKey, int requestWeight)
        {
            if (apiKey == null)
                return TimeSpan.Zero;

            var ky = apiKey.GetString();
            if(!_trackers.TryGetValue(ky, out var tracker))
            {
                tracker = CreateTracker();
                _trackers[ky] = tracker;
            }    

            return tracker.ProcessTopic(requestWeight);
        }

        public void Enter(Uri url, HttpMethod method, bool signed, SecureString? apiKey, int requestWeight)
        {
            if (apiKey == null)
                return;

            var ky = apiKey.GetString();
            _trackers[ky].AddEntry(requestWeight);
        }

        public string GetState(Uri url, HttpMethod method, bool signed, SecureString? apiKey, int requestWeight)
        {
            var ky = apiKey!.GetString();
            var tracker = _trackers[ky];
            return $"Current: {tracker.Current}, limit {tracker.Limit}";
        }
    }
}
