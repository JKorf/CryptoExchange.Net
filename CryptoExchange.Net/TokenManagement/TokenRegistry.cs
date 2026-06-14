using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.TokenManagement
{
    internal class TokenRegistry
    {
        private readonly string _registryKey;
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly Dictionary<string, ManagedToken> _tokens = new Dictionary<string, ManagedToken>();

        private CancellationTokenSource? _keepAliveCts;
        private Task? _keepAliveTask;

        public TokenRegistry(string registryKey, ILogger logger)
        {
            _registryKey = registryKey;
            _logger = logger;
        }

        public async Task<CallResult<TokenLease>> AcquireAsync(
            ILogger logger,
            TokenScope scope,
            TimeSpan refreshInterval,
            TimeSpan timeValid,
            TokenOperations operations,
            CancellationToken ct)
        {
            if (string.IsNullOrEmpty(scope.ApiKey))
                return CallResult.Fail<TokenLease>(new NoApiCredentialsError());

            _logger.LogDebug("Acquiring token lease for scope {Scope}", scope.ToString());

            ManagedToken? existing;
            var ownerId = Guid.NewGuid();

            await _semaphore.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (_tokens.TryGetValue(scope.Id, out existing)
                    && existing.Info.Status == TokenStatus.Valid
                    && existing.Info.ValidUntil > DateTime.UtcNow)
                {
                    _logger.LogDebug("Existing token found for scope {Scope}, now {Count} leases", scope.ToString(), existing.RefCount + 1);
                    existing.RefCount++;
                    existing.Owners[ownerId] = operations;

                    EnsureKeepAliveLoop(logger);
                    return CallResult.Ok(new TokenLease(this, scope, ownerId, existing.Info));
                }
            }
            finally
            {
                _semaphore.Release();
            }

            _logger.LogDebug("Starting new token for scope {Scope}", scope.ToString());
            var startResult = await operations.StartToken(scope, ct).ConfigureAwait(false);
            if (!startResult.Success)
            {
                _logger.LogDebug("Failed to start new token for scope {Scope}", scope.ToString());
                return CallResult.Fail<TokenLease>(startResult.Error!);
            }

            try
            {
                await _semaphore.WaitAsync(ct).ConfigureAwait(false);
            }
            catch(OperationCanceledException)
            {
                if (operations.StopToken != null)
                    _ = operations.StopToken(new TokenInfo(scope, startResult.Data, refreshInterval, timeValid), CancellationToken.None);
                return CallResult.Fail<TokenLease>(new CancellationRequestedError());
            }
            
            try
            {
                // Another client may have started the same token while this call was in flight.
                if (_tokens.TryGetValue(scope.Id, out existing)
                    && existing.Info.Status == TokenStatus.Valid
                    && existing.Info.ValidUntil > DateTime.UtcNow)
                {
                    _logger.LogDebug("Duplicate token found for scope {Scope}, keeping existing token", scope.ToString());
                    existing.RefCount++;
                    existing.Owners[ownerId] = operations;

                    // The server may have returned the same token. We keep the first tracked instance
                    // and stop the just-created duplicate only if it differs.
                    if (existing.Info.Token != startResult.Data && operations.StopToken != null)
                        _ = operations.StopToken(new TokenInfo(scope, startResult.Data, refreshInterval, timeValid), CancellationToken.None);

                    EnsureKeepAliveLoop(logger);
                    return CallResult.Ok(new TokenLease(this, scope, ownerId, existing.Info));
                }

                var info = new TokenInfo(scope, startResult.Data, refreshInterval, timeValid)
                {
                    CreateTime = DateTime.UtcNow,
                    NextRefreshTime = DateTime.UtcNow.Add(refreshInterval),
                    ValidUntil = DateTime.UtcNow.Add(timeValid)
                };

                var token = new ManagedToken(info)
                {
                    RefCount = 1
                };

                token.Owners[ownerId] = operations;
                _tokens[scope.Id] = token;

                EnsureKeepAliveLoop(logger);
                _logger.LogDebug("Token lease for {Scope}, token {Token} acquired, valid until {ValidUntil}", scope.ToString(), info.Token, info.ValidUntil);
                return CallResult.Ok(new TokenLease(this, scope, ownerId, info));
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task ReleaseAsync(TokenLease lease)
        {
            ManagedToken? tokenToStop = null;
            TokenOperations? stopOperations = null;

            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                if (!_tokens.TryGetValue(lease.Scope.Id, out var token))
                    return;

                if (!token.Owners.TryGetValue(lease.OwnerId, out stopOperations))
                    return; // Lease belongs to an old/replaced token; don't touch current token.

                _logger.LogDebug("Releasing token lease for {Scope}, token {Token}. {Count} leases left", lease.Scope.ToString(), lease.Token.Token, token.RefCount - 1);

                token.Owners.Remove(lease.OwnerId);
                token.RefCount--;
                if (token.RefCount > 0)
                    return;

                _tokens.Remove(lease.Scope.Id);
                tokenToStop = token;

                if (_tokens.Count == 0)
                    StopKeepAliveLoop();
            }
            finally
            {
                _semaphore.Release();
            }

            if (tokenToStop != null && stopOperations?.StopToken != null)
            {
                _logger.LogDebug("No token leases left for token {Token} for scope {Scope}, stopping", lease.Token.Token.ToString(), lease.Scope.ToString());
                await stopOperations.StopToken(tokenToStop.Info, CancellationToken.None).ConfigureAwait(false);
            }
        }

        private void EnsureKeepAliveLoop(ILogger logger)
        {
            if (_keepAliveTask != null && !_keepAliveTask.IsCompleted)
                return;

            _keepAliveCts = new CancellationTokenSource();
            _keepAliveTask = Task.Run(() => ProcessKeepAlivesAsync(logger, _keepAliveCts.Token));
        }

        private void StopKeepAliveLoop()
        {
            _logger.LogDebug("Stopping keep alive loop");
            _keepAliveCts?.Cancel();
            _keepAliveCts?.Dispose();
            _keepAliveCts = null;
            _keepAliveTask = null;
        }

        private async Task ProcessKeepAlivesAsync(ILogger logger, CancellationToken ct)
        {
            logger.LogDebug("Starting keep alive loop");

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), ct).ConfigureAwait(false);

                    List<(ManagedToken Token, TokenOperations Operations)> dueTokens;

                    await _semaphore.WaitAsync(ct).ConfigureAwait(false);
                    try
                    {
                        var now = DateTime.UtcNow;
                        dueTokens = _tokens.Values
                            .Where(x => x.RefCount > 0 && x.Info.NextRefreshTime <= now)
                            .Select(x => (Token: x, Operations: x.Owners.Values.FirstOrDefault(o => o.KeepAliveToken != null)))
                            .Where(x => x.Operations != null)
                            .Select(x => (x.Token, x.Operations!))
                            .ToList();
                    }
                    finally
                    {
                        _semaphore.Release();
                    }

                    if (dueTokens.Count > 0) 
                        logger.LogDebug("Keeping alive {Count} tokens", dueTokens.Count);

                    var expiredTokens = new List<TokenInfo>();
                    foreach (var item in dueTokens)
                    {
                        var result = await item.Operations.KeepAliveToken!(item.Token.Info, ct).ConfigureAwait(false);
                        await _semaphore.WaitAsync(ct).ConfigureAwait(false);
                        try
                        {
                            if (!_tokens.TryGetValue(item.Token.Info.Scope.Id, out var current)
                                || !ReferenceEquals(current, item.Token))
                            {
                                continue;
                            }

                            if (result.Success)
                            {
                                logger.LogDebug("Token {Token} for {Scope} successfully kept alive", current.Info.Token, current.Info.Scope);                    
                                current.Info.Refresh();
                            }
                            else
                            {
                                logger.LogWarning("Token {Token} for {Scope} keep alive failed: {Error}", current.Info.Token, current.Info.Scope, result.Error);                                
                                if (current.Info.ValidUntil <= DateTime.UtcNow)
                                {
                                    _tokens.Remove(current.Info.Scope.Id);

                                    current.Info.MarkExpired();
                                    expiredTokens.Add(current.Info);

                                    if (_tokens.Count == 0)
                                        StopKeepAliveLoop();
                                }
                                else
                                {
                                    // TODO Some smarter way to determine next refresh time based on the remaining valid time
                                    current.Info.NextRefreshTime = DateTime.UtcNow.Add(TimeSpan.FromMinutes(5));
                                }
                            }
                        }
                        finally
                        {
                            _semaphore.Release();
                        }
                    }

                    foreach (var expiredToken in expiredTokens)
                        expiredToken.InvokeExpired();
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Keep alive loop error");
                }
            }

            logger.LogDebug("Keep alive loop stopped");
        }

        private sealed class ManagedToken
        {
            public ManagedToken(TokenInfo info)
            {
                Info = info;
            }

            public TokenInfo Info { get; }
            public int RefCount { get; set; }
            public Dictionary<Guid, TokenOperations> Owners { get; } = new Dictionary<Guid, TokenOperations>();
        }
    }
}
