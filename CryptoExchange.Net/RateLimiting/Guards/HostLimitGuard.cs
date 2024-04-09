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
        private RateLimitType _types;

        public HostLimitGuard(string host, int limit, TimeSpan timespan, RateLimitType types = RateLimitType.Request) : base(limit, timespan)
        {
            _host = host;
            _types = types;
        }

        public TimeSpan Check(ILogger logger, RateLimitType type, Uri url, HttpMethod method, bool signed, SecureString? apiKey, int requestWeight)
        {
            if (!_types.HasFlag(type))
                return TimeSpan.Zero;

            if (!string.Equals(url.Host, _host, StringComparison.OrdinalIgnoreCase))
                return TimeSpan.Zero;

            if (_tracker == null)
                _tracker = CreateTracker();

            return _tracker.ProcessTopic(requestWeight);
        }

        public void Enter(RateLimitType type, Uri url, HttpMethod method, bool signed, SecureString? apiKey, int requestWeight)
        {
            if (!_types.HasFlag(type))
                return;

            if (!string.Equals(url.Host, _host, StringComparison.OrdinalIgnoreCase))
                return;

            _tracker.AddEntry(requestWeight);
        }

        public WindowTracker? GetTracker(RateLimitType type, Uri url, HttpMethod method, bool signed, SecureString? apiKey) => _tracker;
    }
}
