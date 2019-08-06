using System.Net;
using CryptoExchange.Net.Interfaces;

namespace CryptoExchange.Net.Requests
{
    /// <summary>
    /// WebRequest factory
    /// </summary>
    public class RequestFactory : IRequestFactory
    {
        /// <inheritdoc />
        public IRequest Create(string uri)
        {
            return new Request(WebRequest.Create(uri));
        }
    }
}
