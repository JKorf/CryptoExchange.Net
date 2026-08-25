using CryptoExchange.Net.Objects;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting a transfer
    /// </summary>
    public class TransferOptions : EndpointOptions<TransferRequest, ITransferEndpoint>
    {
        /// <inheritdoc />
        public override string Description => "Transfer funds between account types";

        /// <summary>
        /// Supported account types
        /// </summary>
        public SharedAccountType[] SupportedAccountTypes { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public TransferOptions(string exchange, SharedAccountType[] accountTypes) : base(exchange, true, nameof(ITransferEndpoint.TransferAsync))
        {
            SupportedAccountTypes = accountTypes;
        }

        /// <summary>
        /// Validate a request
        /// </summary>
        public override Error? ValidateRequest(
            TransferRequest request,
            ITransferEndpoint client)
        {
            if (!SupportedAccountTypes.Contains(request.FromAccountType))
                return ArgumentError.Invalid(nameof(request.FromAccountType), "Invalid FromAccountType");

            if (!SupportedAccountTypes.Contains(request.ToAccountType))
                return ArgumentError.Invalid(nameof(request.FromAccountType), "Invalid ToAccountType");

            return base.ValidateRequest(request, client);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());
            sb.AppendLine($"  Supported accounts:             {string.Join(", ", SupportedAccountTypes)}");
            return sb.ToString();
        }
    }
}
