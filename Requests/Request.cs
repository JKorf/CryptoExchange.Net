using System;
using System.Net;
using CryptoExchange.Net.Interfaces;

namespace CryptoExchange.Net.Requests
{
    public class Request : IRequest
    {
        private readonly WebRequest request;

        public Request(WebRequest request)
        {
            this.request = request;
        }

        public WebHeaderCollection Headers
        {
            get => request.Headers;
            set => request.Headers = value;
        }
        public string Method
        {
            get => request.Method;
            set => request.Method = value;
        }
        public Uri Uri
        {
            get => request.RequestUri;
        }

        public void SetProxy(string host, int port)
        {
            request.Proxy = new WebProxy(host, port); ;
        }

        public IResponse GetResponse()
        {
            return new Response(request.GetResponse());
        }
    }
}
