using CryptoExchange.Net.Objects;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for paginated endpoints
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PaginatedEndpointOptions<T> : EndpointOptions<T> where T : SharedRequest
    {
        /// <summary>
        /// Type of pagination supported
        /// </summary>
        public SharedPaginationSupport PaginationSupport { get; }

        /// <summary>
        /// Whether filtering based on start/end time is supported
        /// </summary>
        public bool TimePeriodFilterSupport { get; }

        /// <summary>
        /// Max amount of results that can be requested
        /// </summary>
        public int MaxLimit { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public PaginatedEndpointOptions(SharedPaginationSupport paginationType, bool timePeriodSupport, int maxLimit, bool needsAuthentication) : base(needsAuthentication)
        {
            PaginationSupport = paginationType;
            TimePeriodFilterSupport = timePeriodSupport;
            MaxLimit = maxLimit;
        }

        /// <inheritdoc />
        public override string ToString(string exchange)
        {
            var sb = new StringBuilder(base.ToString(exchange));
            sb.AppendLine($"Pagination type: {PaginationSupport}");
            sb.AppendLine($"Time period filter support: {TimePeriodFilterSupport}");
            sb.AppendLine($"Max limit: {MaxLimit}");
            return sb.ToString();
        }
    }
}
