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
        IRateLimitGate AddGuard(IRateLimitGuard guard);
        IRateLimitGate WithLimitBehaviour(RateLimitingBehaviour behaviour);
        Task<CallResult> ProcessAsync(ILogger logger, Uri url, HttpMethod method, bool signed, SecureString? apiKey, int requestWeight, CancellationToken ct);
    }
}
