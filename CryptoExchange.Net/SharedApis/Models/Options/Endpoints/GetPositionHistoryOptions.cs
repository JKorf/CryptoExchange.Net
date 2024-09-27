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
        public GetPositionHistoryOptions(SharedPaginationSupport paginationType) : base(paginationType, true)
        {
        }
    }
}
