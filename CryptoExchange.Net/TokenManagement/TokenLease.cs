using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.TokenManagement
{
    /// <summary>
    /// A token lease
    /// </summary>
    public class TokenLease
    {
        private readonly TokenRegistry _registry;
        private int _released;

        internal TokenLease(TokenRegistry registry, TokenScope scope, Guid ownerId, TokenInfo token)
        {
            _registry = registry;
            Scope = scope;
            OwnerId = ownerId;
            Token = token;
        }

        internal Guid OwnerId { get; }
        internal TokenScope Scope { get; }

        /// <summary>
        /// Token info
        /// </summary>
        public TokenInfo Token { get; }

        /// <summary>
        /// Release the lease for this token
        /// </summary>
        /// <returns></returns>
        public Task ReleaseAsync()
        {
            if (Interlocked.Exchange(ref _released, 1) == 1)
                return Task.CompletedTask;

            return _registry.ReleaseAsync(this);
        }
    }
}
