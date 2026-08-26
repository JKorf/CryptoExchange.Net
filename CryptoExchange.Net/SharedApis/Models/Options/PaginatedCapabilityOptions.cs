using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options and description of a Shared API capability which returns paginated data.
    /// </summary>
    public abstract class PaginatedCapabilityOptions<
#if NET5_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        TRequest, TCapability> : CapabilityOptions<TRequest, TCapability>
            where TRequest : SharedRequest
            where TCapability : ISharedApiCapability
    {
        /// <summary>
        /// Whether ascending data retrieval and pagination is available
        /// </summary>
        public bool SupportsAscending { get; set; }
        /// <summary>
        /// Whether descending data retrieval and pagination is available
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
        public PaginatedCapabilityOptions(
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
            sb.AppendLine($"  Ascending retrieval supported:  {SupportsAscending}");
            sb.AppendLine($"  Descending retrieval supported: {SupportsDescending}");
            sb.AppendLine($"  Time period filter supported:   {TimePeriodFilterSupport}");
            sb.AppendLine($"  Max limit:                      {MaxLimit}");
            if (MaxAge.HasValue)
                sb.AppendLine($"  Max age:                        {MaxAge}");
            return sb.ToString();
        }
    }
}
