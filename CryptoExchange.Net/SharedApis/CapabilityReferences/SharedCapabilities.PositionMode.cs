using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    public static partial class SharedCapabilities
    {
        /// <summary>
        /// Position mode capabilities.
        /// </summary>
        public static class PositionMode
        {
            /// <summary>
            /// Get position mode capability.
            /// </summary>
            public static SharedRestCapability<IGetPositionMode, IGetPositionModeRest> GetPositionMode { get; } = new();

            /// <summary>
            /// Set position mode capability.
            /// </summary>
            public static SharedRestCapability<ISetPositionMode, ISetPositionModeRest> SetPositionMode { get; } = new();
        }
    }
}
