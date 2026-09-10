using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Capability lookup result
    /// </summary>
    /// <typeparam name="T">Capability type</typeparam>
    public class SharedCapabilityResolution<T>
        where T : ISharedApiCapability
    {
        /// <summary>
        /// The capability
        /// </summary>
        public T Capability { get; }
        /// <summary>
        /// The capability options
        /// </summary>
        public CapabilityOptions Options { get; }

        internal SharedCapabilityResolution(
            T capability,
            CapabilityOptions options)
        {
            Capability = capability;
            Options = options;
        }
    }
}
