using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for paginated endpoints
    /// </summary>
    /// <typeparam name="T"></typeparam>
#if NET5_0_OR_GREATER
    public class PaginatedEndpointOptions<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>  : EndpointOptions<T> where T : SharedRequest
#else
    public class PaginatedEndpointOptions<T> : EndpointOptions<T> where T : SharedRequest
#endif
    {
        /// <summary>
        /// Whether ascending data retrieval and pagination is available
        /// </summary>
        public bool SupportsAscending { get; set; }
        /// <summary>
        /// Whether ascending data retrieval and pagination is available
        /// </summary>
        public bool SupportsDescending { get; set; }

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
        public PaginatedEndpointOptions(
            bool supportsAscending,
            bool supportsDescending,
            bool timePeriodSupport,
            int maxLimit,
            bool needsAuthentication) : base(needsAuthentication)
        {
            SupportsAscending = supportsAscending;
            SupportsDescending = supportsDescending;
            TimePeriodFilterSupport = timePeriodSupport;
            MaxLimit = maxLimit;
        }

        /// <inheritdoc />
        public override string ToString(string exchange)
        {
            var sb = new StringBuilder(base.ToString(exchange));
            sb.AppendLine($"Ascending retrieval supported: {SupportsAscending}");
            sb.AppendLine($"Descending retrieval supported: {SupportsDescending}");
            sb.AppendLine($"Time period filter support: {TimePeriodFilterSupport}");
            sb.AppendLine($"Max limit: {MaxLimit}");
            return sb.ToString();
        }
    }
}
