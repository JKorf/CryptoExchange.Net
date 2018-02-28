using System;
using System.Net;

namespace CryptoExchange.Net.Interfaces
{
    public interface IRequest
    {
        Uri Uri { get; }
        WebHeaderCollection Headers { get; set; }
        string Method { get; set; }

        void SetProxy(string host, int port);
        IResponse GetResponse();
    }
}
