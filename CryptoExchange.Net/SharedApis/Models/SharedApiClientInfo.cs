using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Discovery information for a Shared API client.
    /// </summary>
    public class SharedApiClientInfo
    {
        /// <summary>
        /// Preferred transport when multiple implementations of a capability are available.
        /// </summary>
        public SharedTransport PreferredTransport { get; }

        /// <summary>
        /// Shared APIs available on the client.
        /// </summary>
        public SharedClientInfo[] SharedApis { get; }

        internal SharedApiClientInfo(
            SharedTransport preferredTransport,
            SharedClientInfo[] sharedApis)
        {
            PreferredTransport = preferredTransport;
            SharedApis = sharedApis;
        }

        /// <inheritdoc />
        public override string ToString() => ToString(false);

        /// <summary>
        /// Create a string representation of the available Shared APIs.
        /// </summary>
        /// <param name="detailed">Whether to include detailed capability information.</param>
        public string ToString(bool detailed)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Preferred transport: {PreferredTransport}");

            foreach (var sharedApi in SharedApis)
            {
                builder.AppendLine();
                builder.Append(sharedApi.ToString(detailed));
            }

            return builder.ToString();
        }
    }
}
