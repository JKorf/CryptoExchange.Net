using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// References for a capability available through REST.
    /// </summary>
    public sealed class SharedRestCapability<TCapability, TRest>
        where TCapability : ISharedApiCapability
        where TRest : TCapability, ISharedRest
    {
        /// <summary>
        /// Transport-agnostic capability.
        /// </summary>
        public SharedCapabilityReference<TCapability> Any { get; } = new();

        /// <summary>
        /// REST capability.
        /// </summary>
        public SharedCapabilityReference<TRest> Rest { get; } = new();

        internal SharedRestCapability()
        {
        }
    }
}
