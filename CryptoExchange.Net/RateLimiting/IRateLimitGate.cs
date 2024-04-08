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

        IRateLimitGate AddGuard(IRateLimitGuard guard);
        IRateLimitGate WithLimitBehaviour(RateLimitingBehaviour behaviour);
        IRateLimitGate WithWindowType(RateLimitWindowType type);

        Task SetRetryAfterGuardAsync(DateTime retryAfter);
        Task<CallResult> ProcessAsync(ILogger logger, Uri url, HttpMethod method, bool signed, SecureString? apiKey, int requestWeight, CancellationToken ct);
    }
}
