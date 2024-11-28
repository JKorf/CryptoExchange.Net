namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting position history
    /// </summary>
    public class GetPositionHistoryOptions : PaginatedEndpointOptions<GetPositionHistoryRequest>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GetPositionHistoryOptions(SharedPaginationSupport paginationType, bool timeFilterSupported, int maxLimit) : base(paginationType, timeFilterSupported, maxLimit, true)
        {
        }
    }
}
