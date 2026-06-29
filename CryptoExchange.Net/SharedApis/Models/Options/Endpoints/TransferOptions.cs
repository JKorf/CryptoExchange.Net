using CryptoExchange.Net.Objects;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting a transfer
    /// </summary>
    public class TransferOptions : EndpointOptions<TransferRequest, ITransferRestClient>
    {
        /// <summary>
        /// Supported account types
        /// </summary>
        public SharedAccountType[] SupportedAccountTypes { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public TransferOptions(string exchange, SharedAccountType[] accountTypes) : base(exchange, true, nameof(ITransferRestClient.TransferAsync))
        {
            SupportedAccountTypes = accountTypes;
        }

        /// <summary>
        /// Validate a request
        /// </summary>
        public override Error? ValidateRequest(
            TransferRequest request,
            ITransferRestClient client)
        {
            if (!SupportedAccountTypes.Contains(request.FromAccountType))
                return ArgumentError.Invalid(nameof(request.FromAccountType), "Invalid FromAccountType");

            if (!SupportedAccountTypes.Contains(request.ToAccountType))
                return ArgumentError.Invalid(nameof(request.FromAccountType), "Invalid ToAccountType");

            return base.ValidateRequest(request, client);
        }
    }
}
