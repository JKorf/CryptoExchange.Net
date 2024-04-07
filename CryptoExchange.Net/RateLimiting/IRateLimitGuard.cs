using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CryptoExchange.Net.RateLimiting
{
    public interface IRateLimitGuard
    {
        string Name { get; }

        TimeSpan Check(ILogger logger, Uri url, HttpMethod method, bool signed, SecureString? apiKey, int requestWeight);
        void Enter(Uri url, HttpMethod method, bool signed, SecureString? apiKey, int requestWeight);

        string GetState(Uri url, HttpMethod method, bool signed, SecureString? apiKey, int requestWeight);
    }
}
