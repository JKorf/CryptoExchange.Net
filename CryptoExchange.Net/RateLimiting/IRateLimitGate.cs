using CryptoExchange.Net.Objects;
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
    public interface IRateLimitGate
    {
        event Action<string, HttpMethod, TimeSpan> RateLimitTriggered;

        IRateLimitGate AddGuard(IRateLimitGuard guard, int initialCount = 0);
        IRateLimitGate WithWindowType(RateLimitWindowType type);

        Task SetRetryAfterGuardAsync(DateTime retryAfter);
        Task<CallResult> ProcessAsync(ILogger logger, RateLimitType type, Uri url, HttpMethod? method, bool signed, SecureString? apiKey, int requestWeight, RateLimitingBehaviour behaviour, CancellationToken ct);
    }
}
