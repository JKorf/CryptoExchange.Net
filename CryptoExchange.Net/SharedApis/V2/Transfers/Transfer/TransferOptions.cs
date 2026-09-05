using CryptoExchange.Net.Objects;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting a transfer
    /// </summary>
    public class TransferOptions : CapabilityOptions<TransferRequest, ITransferRest>
    {
        /// <inheritdoc />
        public override string Description => "Transfer funds between account types";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<TransferRequest>.Required(x => x.Asset, "The asset to transfer", "ETH"),
            RequestParameterRule<TransferRequest>.Required(x => x.Quantity, "The quantity to transfer", 1m),
            RequestParameterRule<TransferRequest>.Optional(x => x.FromSymbol, "The symbol of the source account", "ETH-USDT"),
            RequestParameterRule<TransferRequest>.Optional(x => x.ToSymbol, "The symbol of the destination account", "ETH-USDT"),
            RequestParameterRule<TransferRequest>.Required(x => x.FromAccountType, "The source account type", SharedAccountType.Spot),
            RequestParameterRule<TransferRequest>.Required(x => x.ToAccountType, "The destination account type", SharedAccountType.Funding),
        };

        /// <summary>
        /// Supported account types
        /// </summary>
        public SharedAccountType[] SupportedAccountTypes { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public TransferOptions(string exchange, SharedAccountType[] accountTypes) : base(exchange, true, nameof(ITransferRest.TransferAsync), _defaultParameterRules)
        {
            SupportedAccountTypes = accountTypes;
        }

        /// <summary>
        /// Validate a request
        /// </summary>
        public override Error? ValidateRequest(
            TransferRequest request,
            ITransferRest client)
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
