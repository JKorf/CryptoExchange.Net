using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// References for a capability available through REST and WebSocket.
    /// </summary>
    public sealed class SharedRestSocketCapability<TCapability, TRest, TSocket>
        : SharedCapabilityReference<TCapability>
        where TCapability : ISharedApiCapability
        where TRest : TCapability, ISharedRest
        where TSocket : TCapability, ISharedSocket
    {
        /// <summary>
        /// REST capability.
        /// </summary>
        public SharedCapabilityReference<TRest> Rest { get; } = new();

        /// <summary>
        /// WebSocket capability.
        /// </summary>
        public SharedCapabilityReference<TSocket> Socket { get; } = new();

        internal SharedRestSocketCapability()
        {
        }
    }
}
