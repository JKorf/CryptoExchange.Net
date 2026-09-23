using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    public static partial class SharedCapabilities
    {
        /// <summary>
        /// Position capabilities.
        /// </summary>
        public static class Positions
        {
            /// <summary>
            /// Get positions capability.
            /// </summary>
            public static SharedRestCapability<IGetPositions, IGetPositionsRest> GetPositions { get; } = new();

            /// <summary>
            /// Get position history capability.
            /// </summary>
            public static SharedRestCapability<IGetPositionHistory, IGetPositionHistoryRest> GetPositionHistory { get; } = new();

            /// <summary>
            /// Close a full position capability.
            /// </summary>
            public static SharedRestCapability<ICloseFullPosition, ICloseFullPositionRest> CloseFullPosition { get; } = new();

            /// <summary>
            /// Subscribe to position updates capability.
            /// </summary>
            public static SharedCapabilityReference<ISubscribePositionsSocket> SubscribePositions { get; } = new();
        }
    }
}
