using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.TokenManagement
{
    internal class TokenOperations
    {
        public TokenOperations(
            Func<TokenScope, CancellationToken, Task<CallResult<string>>> startToken,
            Func<TokenInfo, CancellationToken, Task<CallResult>>? keepAliveToken,
            Func<TokenInfo, CancellationToken, Task<CallResult>>? stopToken)
        {
            StartToken = startToken;
            KeepAliveToken = keepAliveToken;
            StopToken = stopToken;
        }

        public Func<TokenScope, CancellationToken, Task<CallResult<string>>> StartToken { get; }
        public Func<TokenInfo, CancellationToken, Task<CallResult>>? KeepAliveToken { get; }
        public Func<TokenInfo, CancellationToken, Task<CallResult>>? StopToken { get; }
    }
}
