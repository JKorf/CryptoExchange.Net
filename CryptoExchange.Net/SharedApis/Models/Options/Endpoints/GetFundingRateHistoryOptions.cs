namespace CryptoExchange.Net.SharedApis;

/// <summary>
/// Options for requesting funding rate history
/// </summary>
public class GetFundingRateHistoryOptions : PaginatedEndpointOptions<GetFundingRateHistoryRequest>
{
    /// <summary>
    /// ctor
    /// </summary>
    public GetFundingRateHistoryOptions(SharedPaginationSupport paginationType, bool timeFilterSupported, int maxLimit, bool needsAuthentication) : base(paginationType, timeFilterSupported, maxLimit, needsAuthentication)
    {
    }
}
