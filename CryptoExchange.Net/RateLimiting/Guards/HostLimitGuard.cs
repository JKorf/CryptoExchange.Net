using CryptoExchange.Net.RateLimiting.Trackers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security;
using System.Text;

namespace CryptoExchange.Net.RateLimiting.Guards
{
    public class HostLimitGuard : LimitGuard, IRateLimitGuard
    {
        public string Name => "HostLimitGuard";

        private readonly string _host;
        private WindowTracker _tracker;

        public HostLimitGuard(string host, int limit, TimeSpan timespan) : base(limit, timespan)
        {
            _host = host;
        }

        public TimeSpan Check(ILogger logger, Uri url, HttpMethod method, bool signed, SecureString? apiKey, int requestWeight)
        {
            if (!string.Equals(url.Host, _host, StringComparison.OrdinalIgnoreCase))
                return TimeSpan.Zero;

            if (_tracker == null)
                _tracker = CreateTracker();

            return _tracker.ProcessTopic(requestWeight);
        }

        public void Enter(Uri url, HttpMethod method, bool signed, SecureString? apiKey, int requestWeight)
        {
            if (!string.Equals(url.Host, _host, StringComparison.OrdinalIgnoreCase))
                return;

            _tracker.AddEntry(requestWeight);
        }

        public string GetState(Uri url, HttpMethod method, bool signed, SecureString? apiKey, int requestWeight)
            => $"Current: {_tracker.Current}, limit {_tracker.Limit}";
    }
}
