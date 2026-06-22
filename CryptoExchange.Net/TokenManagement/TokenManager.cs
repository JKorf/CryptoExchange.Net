using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets.Default;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.TokenManagement
{
    /// <summary>
    /// Token manager
    /// </summary>
    public class TokenManager
    {
        private readonly TokenRegistry _registry;
        private readonly TokenOperations _operations;
        private readonly ILogger _logger;
        private readonly TimeSpan _timeValid;
        private readonly TimeSpan _refreshInterval;
        private readonly bool _removeTokensWithoutLease;

        /// <summary>
        /// ctor
        /// </summary>
        public TokenManager(
            string registryKey,
            ILoggerFactory? loggerFactory,
            TimeSpan refreshInterval,
            TimeSpan timeValid,
            Func<TokenScope, CancellationToken, Task<CallResult<string>>> startToken,
            Func<TokenInfo, CancellationToken, Task<CallResult>>? keepAliveToken = null,
            Func<TokenInfo, CancellationToken, Task<CallResult>>? stopToken = null,
            bool removeTokensWithoutLease = true)
        {
            _logger = loggerFactory?.CreateLogger(registryKey + "." + nameof(TokenManager)) ?? NullLogger.Instance;
            _refreshInterval = refreshInterval;
            _timeValid = timeValid;
            _registry = TokenRegistryProvider.GetRegistry(registryKey, _logger);
            _operations = new TokenOperations(startToken, keepAliveToken, stopToken);
            _removeTokensWithoutLease = removeTokensWithoutLease;
        }

        /// <summary>
        /// Acquire a token for the provided scope
        /// </summary>
        public Task<CallResult<TokenLease>> AcquireAsync(TokenScope scope, CancellationToken ct = default)
            => _registry.AcquireAsync(_logger, scope, _refreshInterval, _timeValid, _operations, _removeTokensWithoutLease, ct);

        /// <summary>
        /// Acquire a token and replace the current token of the subscription with the new one. 
        /// </summary>
        public async Task<CallResult<TokenLease>> AcquireAndReplaceAsync(Subscription subscription, TokenScope scope, CancellationToken ct = default)
        {
            var acquireResult = await _registry.AcquireAsync(_logger, scope, _refreshInterval, _timeValid, _operations, _removeTokensWithoutLease, ct).ConfigureAwait(false);
            if (!acquireResult.Success)
                return acquireResult;

            var oldLease = subscription.TokenLease;
            subscription.TokenLease = acquireResult.Data;
            if (oldLease != null)
                await oldLease.ReleaseAsync().ConfigureAwait(false);

            return acquireResult;
        }
    }

}
