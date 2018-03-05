using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Interfaces
{
    public interface IRequest
    {
        Uri Uri { get; }
        WebHeaderCollection Headers { get; set; }
        string Method { get; set; }

        void SetProxy(string host, int port);

        string ContentType { get; set; }
        string Accept { get; set; }
        long ContentLength { get; set; }

        Task<Stream> GetRequestStream();
        Task<IResponse> GetResponse();
    }
}
