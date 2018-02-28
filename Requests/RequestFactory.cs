using System.Net;
using CryptoExchange.Net.Interfaces;

namespace CryptoExchange.Net.Requests
{
    public class RequestFactory : IRequestFactory
    {
        public IRequest Create(string uri)
        {
            return new Request(WebRequest.Create(uri));
        }
    }
}
