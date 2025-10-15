using CryptoExchange.Net.Objects;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting a transfer
    /// </summary>
    public class GetBalancesOptions : EndpointOptions<GetBalancesRequest>
    {
        /// <summary>
        /// Supported account types
        /// </summary>
        public AccountTypeFilter[] SupportedAccountTypes { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public GetBalancesOptions(params AccountTypeFilter[] accountTypes) : base(true)
        {
            SupportedAccountTypes = accountTypes;
        }

        /// <summary>
        /// Validate a request
        /// </summary>
        public Error? ValidateRequest(
            string exchange,
            GetBalancesRequest request,
            TradingMode[] supportedApiTypes)
        {
            if (request.AccountType != null && !IsValid(request.AccountType.Value))
                return ArgumentError.Invalid(nameof(request.AccountType), "Invalid AccountType");

            return base.ValidateRequest(exchange, request, null, supportedApiTypes);
        }

        /// <summary>
        /// Is the account type valid for this client
        /// </summary>
        /// <param name="accountType"></param>
        /// <returns></returns>
        public bool IsValid(SharedAccountType accountType)
        {
            if (accountType == SharedAccountType.Funding)
                return SupportedAccountTypes.Contains(AccountTypeFilter.Funding);

            if (accountType == SharedAccountType.Spot) 
                return SupportedAccountTypes.Contains(AccountTypeFilter.Spot);

            if (accountType == SharedAccountType.PerpetualLinearFutures
                || accountType == SharedAccountType.PerpetualInverseFutures
                || accountType == SharedAccountType.DeliveryLinearFutures
                || accountType == SharedAccountType.DeliveryInverseFutures)
            {
                return SupportedAccountTypes.Contains(AccountTypeFilter.Futures);
            }


            if (accountType == SharedAccountType.CrossMargin
                || accountType == SharedAccountType.IsolatedMargin)
            {
                return SupportedAccountTypes.Contains(AccountTypeFilter.Margin);
            }

            return SupportedAccountTypes.Contains(AccountTypeFilter.Option);
        }
    }

    /// <summary>
    /// Account type filter
    /// </summary>
    public enum AccountTypeFilter
    {
        /// <summary>
        /// Funding account
        /// </summary>
        Funding,
        /// <summary>
        /// Spot account
        /// </summary>
        Spot,
        /// <summary>
        /// Futures account
        /// </summary>
        Futures,
        /// <summary>
        /// Margin account
        /// </summary>
        Margin,
        /// <summary>
        /// Option account
        /// </summary>
        Option
    }
}
