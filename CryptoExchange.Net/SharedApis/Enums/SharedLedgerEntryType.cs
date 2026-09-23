namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Ledger entry type
    /// </summary>
    public enum SharedLedgerEntryType
    {
        /// <summary>
        /// Trade execution
        /// </summary>
        Trade,
        /// <summary>
        /// Withdrawal
        /// </summary>
        Withdrawal,
        /// <summary>
        /// Deposit
        /// </summary>
        Deposit,
        /// <summary>
        /// Transfer
        /// </summary>
        Transfer,
        /// <summary>
        /// Fee payment
        /// </summary>
        Fee,
        /// <summary>
        /// Funding fee payment
        /// </summary>
        FundingFee,
        /// <summary>
        /// Rebate
        /// </summary>
        Rebate,

        /// <summary>
        /// Other or unknown
        /// </summary>
        Unknown
    }
}
