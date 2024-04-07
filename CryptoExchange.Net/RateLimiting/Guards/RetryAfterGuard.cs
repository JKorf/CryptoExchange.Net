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
    public class RetryAfterGuard : IRateLimitGuard
    {
        public string Name => "RetryAfterGuard";

        private readonly DateTime _after;

        public RetryAfterGuard(DateTime after)
        {
            _after = after;
        }


        public TimeSpan Check(ILogger logger, Uri url, HttpMethod method, bool signed, SecureString? apiKey, int requestWeight)
        {
            var dif = _after - DateTime.UtcNow;
            if (dif < TimeSpan.Zero)
                return TimeSpan.Zero;

            return dif;
        }

        public void Enter(Uri url, HttpMethod method, bool signed, SecureString? apiKey, int requestWeight)
        {
        }

        public string GetState(Uri url, HttpMethod method, bool signed, SecureString? apiKey, int requestWeight)
            => $"Limited until {_after}";
    }
}
