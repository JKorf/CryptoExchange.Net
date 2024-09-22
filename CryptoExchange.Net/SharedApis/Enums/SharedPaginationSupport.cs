namespace CryptoExchange.Net.SharedApis.Enums
{
    /// <summary>
    /// Supported pagination type
    /// </summary>
    public enum SharedPaginationSupport
    {
        /// <summary>
        /// Pagination is not supported for this exchange request
        /// </summary>
        NotSupported,
        /// <summary>
        /// Pagination is in ascending order
        /// </summary>
        Ascending,
        /// <summary>
        /// Pagination is in descending order
        /// </summary>
        Descending
    }
}
