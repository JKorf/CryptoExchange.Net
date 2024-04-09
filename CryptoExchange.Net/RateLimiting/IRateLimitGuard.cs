using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using CryptoExchange.Net.RateLimiting.Trackers;

namespace CryptoExchange.Net.RateLimiting
{
    public interface IRateLimitGuard
    {
        string Name { get; }

        TimeSpan Check(ILogger logger, RateLimitType type, Uri url, HttpMethod? method, bool signed, SecureString? apiKey, int requestWeight);
        void Enter(RateLimitType type, Uri url, HttpMethod? method, bool signed, SecureString? apiKey, int requestWeight);

        WindowTracker? GetTracker(RateLimitType type, Uri url, HttpMethod? method, bool signed, SecureString? apiKey);
    }
}
