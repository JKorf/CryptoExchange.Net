using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Strongly typed reference to a Shared API capability.
    /// </summary>
    public class SharedCapabilityReference<T>
        where T : ISharedApiCapability
    {
        /// <summary>
        /// Capability interface type.
        /// </summary>
        public Type CapabilityType => typeof(T);

        internal SharedCapabilityReference()
        {
        }

        /// <inheritdoc />
        public override string ToString() => typeof(T).Name;
    }
}
