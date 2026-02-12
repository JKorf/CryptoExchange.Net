using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for paginated endpoints
    /// </summary>
    /// <typeparam name="T"></typeparam>
#if NET5_0_OR_GREATER
    public class PageEndpointOptions<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>  : EndpointOptions<T> where T : SharedRequest
#else
    public class PageEndpointOptions<T> : EndpointOptions<T> where T : SharedRequest
#endif
    {
        

        /// <summary>
        /// ctor
        /// </summary>
        public PageEndpointOptions(bool needsAuthentication) : base(needsAuthentication)
        {
        }

        /// <inheritdoc />
        public override string ToString(string exchange)
        {
            var sb = new StringBuilder(base.ToString(exchange));

            return sb.ToString();
        }
    }
}
