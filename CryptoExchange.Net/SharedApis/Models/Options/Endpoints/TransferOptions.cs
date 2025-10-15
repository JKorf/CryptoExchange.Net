using CryptoExchange.Net.Objects;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting a transfer
    /// </summary>
    public class TransferOptions : EndpointOptions<TransferRequest>
    {
        /// <summary>
        /// Supported account types
        /// </summary>
        public SharedAccountType[] SupportedAccountTypes { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public TransferOptions(SharedAccountType[] accountTypes) : base(true)
        {
            SupportedAccountTypes = accountTypes;
        }

        /// <summary>
        /// Validate a request
        /// </summary>
        public new Error? ValidateRequest(
            string exchange,
            TransferRequest request,
            TradingMode? tradingMode,
            TradingMode[] supportedApiTypes)
        {
            if (!SupportedAccountTypes.Contains(request.FromAccountType))
                return ArgumentError.Invalid(nameof(request.FromAccountType), "Invalid FromAccountType");

            if (!SupportedAccountTypes.Contains(request.ToAccountType))
                return ArgumentError.Invalid(nameof(request.FromAccountType), "Invalid ToAccountType");

            return base.ValidateRequest(exchange, request, tradingMode, supportedApiTypes);
        }
    }
}
