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
        /// ctor
        /// </summary>
        public PaginatedEndpointOptions(SharedPaginationSupport paginationType, bool needsAuthentication) : base(needsAuthentication)
        {
            PaginationSupport = paginationType;
        }

        /// <inheritdoc />
        public override string ToString(string exchange)
        {
            var sb = new StringBuilder(base.ToString(exchange));
            sb.AppendLine($"Pagination type: {PaginationSupport}");
            return sb.ToString();
        }
    }
}
