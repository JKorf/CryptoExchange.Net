using CryptoExchange.Net.Objects;
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
        /// Environments supported by this client
        /// </summary>
        public string[] SupportedEnvironments { get; set; } = [];
        /// <summary>
        /// Supported trading modes
        /// </summary>
        public TradingMode[] SupportedTradingModes { get; init; } = [];
        /// <summary>
        /// Centralization type of the exchange
        /// </summary>
        public CentralizationType CentralizationType { get; set; }
        /// <summary>
        /// Endpoint/subscription info
        /// </summary>
        public EndpointOptions[] Features { get; init; } = [];

        /// <summary>
        /// Create a string representation for this client
        /// </summary>
        public override string ToString()
            => ToString(false);

        /// <summary>
        /// Create a string representation for this client
        /// </summary>
        public string ToString(bool detailed)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Exchange: {Exchange}");
            sb.AppendLine($"Client: {TypeName}");
            sb.AppendLine($"Supported environments: {string.Join(", ", SupportedEnvironments)}");
            sb.AppendLine($"Supported trading modes: {string.Join(", ", SupportedTradingModes)}");
            sb.AppendLine($"Centralization type: {CentralizationType}");
            sb.AppendLine($"Features:");
            foreach (var feature in Features)
            {
                if (detailed)
                {
                    var stringRep = feature.ToString();
                    foreach(var line in stringRep!.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                        sb.AppendLine($"  {line}");
                }
                else
                {
                    sb.AppendLine($"  {feature.EndpointName}");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }

}
