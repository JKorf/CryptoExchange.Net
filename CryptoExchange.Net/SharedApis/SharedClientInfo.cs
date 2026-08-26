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
        private CapabilityOptions[] _capabilities = [];

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
        /// Use Capabilities instead
        /// </summary>
        [Obsolete("Use Capabilities instead")]
        public EndpointOptions[] Features
        {
            get => _capabilities
                .OfType<EndpointOptions>()
                .ToArray();

            init => _capabilities = value;
        }

        /// <summary>
        /// Client capabilities
        /// </summary>
        public CapabilityOptions[] Capabilities
        {
            get => _capabilities;
            init => _capabilities = value;
        }

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
            sb.AppendLine($"Capabilities:");
            foreach (var capability in Capabilities)
            {
                if (detailed)
                {
                    var stringRep = capability.ToString();
                    foreach(var line in stringRep!.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                        sb.AppendLine($"  {line}");
                }
                else
                {
                    sb.AppendLine($"  {capability.OperationName}");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }

}
