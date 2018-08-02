using System;
using System.Collections.Generic;
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
        Dictionary<string, object> Parameters { get; set; }

        byte[] CustomBody { get; set; }
        bool WriteCustomBody { get; set; }

        void SetProxy(string host, int port);

        string ContentType { get; set; }
        string Accept { get; set; }
        long ContentLength { get; set; }

        Task<Stream> GetRequestStream();
        Task<IResponse> GetResponse();
    }
}
