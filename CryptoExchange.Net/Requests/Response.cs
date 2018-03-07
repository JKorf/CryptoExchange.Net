using System.IO;
using System.Net;
using CryptoExchange.Net.Interfaces;

namespace CryptoExchange.Net.Requests
{
    public class Response : IResponse
    {
        private readonly WebResponse response;

        public Response(WebResponse response)
        {
            this.response = response;
        }

        public Stream GetResponseStream()
        {
            return response.GetResponseStream();
        }
    }
}
