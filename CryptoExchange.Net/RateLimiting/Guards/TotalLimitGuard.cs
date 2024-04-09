using CryptoExchange.Net.RateLimiting.Trackers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.RateLimiting.Guards
{
    public class TotalLimitGuard : LimitGuard, IRateLimitGuard
    {
        public string Name => "TotalLimitGuard";

        private WindowTracker _tracker;

        public TotalLimitGuard(int limit, TimeSpan timespan) : base(limit, timespan)
        {
        }


        public TimeSpan Check(ILogger logger, RateLimitType type, Uri url, HttpMethod method, bool signed, SecureString? apiKey, int requestWeight)
        {
            _tracker ??= CreateTracker();
            return _tracker.ProcessTopic(requestWeight);
        }

        public void Enter(RateLimitType type, Uri url, HttpMethod method, bool signed, SecureString? apiKey, int requestWeight)
        {
            _tracker.AddEntry(requestWeight);
        }

        public WindowTracker? GetTracker(RateLimitType type, Uri url, HttpMethod method, bool signed, SecureString? apiKey) => _tracker;
    }
}
