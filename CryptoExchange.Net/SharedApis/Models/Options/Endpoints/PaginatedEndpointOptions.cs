using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for paginated endpoints
    /// </summary>
#if NET5_0_OR_GREATER
    public class PaginatedEndpointOptions<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TRequest, TClient> : EndpointOptions<TRequest, TClient> 
        where TRequest : SharedRequest
        where TClient : ISharedClient
#else
    public abstract class PaginatedEndpointOptions<TRequest, TClient> : EndpointOptions<TRequest, TClient>
        where TRequest : SharedRequest
        where TClient : ISharedClient
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
        /// Max age of data that can be requested
        /// </summary>
        public TimeSpan? MaxAge { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public PaginatedEndpointOptions(
            string exchange,
            bool supportsAscending,
            bool supportsDescending,
            bool timePeriodSupport,
            int maxLimit,
            bool needsAuthentication, 
            string requestName) : base(exchange, needsAuthentication, requestName)
        {
            SupportsAscending = supportsAscending;
            SupportsDescending = supportsDescending;
            TimePeriodFilterSupport = timePeriodSupport;
            MaxLimit = maxLimit;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());
            sb.AppendLine($"Ascending retrieval supported: {SupportsAscending}");
            sb.AppendLine($"Descending retrieval supported: {SupportsDescending}");
            sb.AppendLine($"Time period filter supported: {TimePeriodFilterSupport}");
            sb.AppendLine($"Max limit: {MaxLimit}");
            if (MaxAge.HasValue)
                sb.AppendLine($"Max age: {MaxAge}");
            return sb.ToString();
        }
    }
}
