using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{

    /// <summary>
    /// Client information
    /// </summary>
    public class SharedClientInfo
    {
        /// <summary>
        /// Exchange name
        /// </summary>
        public string Exchange { get; init; } = string.Empty;
        /// <summary>
        /// The client type name
        /// </summary>
        public string TypeName { get; init; } = string.Empty;
        /// <summary>
        /// Supported trading modes
        /// </summary>
        public TradingMode[] SupportedTradingModes { get; init; } = [];
        /// <summary>
        /// Endpoint/subscription info
        /// </summary>
        public EndpointOptions[] Features { get; init; } = [];

        /// <summary>
        /// Create a string representation for this client
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Exchange: {Exchange}");
            sb.AppendLine($"Client: {TypeName}");
            sb.AppendLine($"Supported trading modes: {string.Join(", ", SupportedTradingModes)}");
            sb.AppendLine($"Features:");
            foreach (var feature in Features.Where(x => x.Supported))
            {
                sb.AppendLine($"  {feature.EndpointName}");
            }

            return sb.ToString();
        }
    }

}
