using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to retrieve balance info for the user
    /// </summary>
    public record GetBalancesRequest : SharedRequest
    {
        /// <summary>
        /// Account type
        /// </summary>
        public SharedAccountType? AccountType { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="tradingMode">Trading mode</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetBalancesRequest(TradingMode tradingMode, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            AccountType = tradingMode.ToAccountType();
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="accountType">Account type</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetBalancesRequest(SharedAccountType? accountType = null, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            AccountType = accountType;
        }
    }
}
