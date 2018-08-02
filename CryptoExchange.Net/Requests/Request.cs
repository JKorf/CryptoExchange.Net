using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
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
        public string ContentType
        {
            get => request.ContentType;
            set => request.ContentType = value;
        }

        public string Accept
        {
            get => ((HttpWebRequest)request).Accept;
            set => ((HttpWebRequest)request).Accept = value;
        }

        public long ContentLength
        {
            get => ((HttpWebRequest)request).ContentLength;
            set => ((HttpWebRequest)request).ContentLength = value;
        }

        public string Method
        {
            get => request.Method;
            set => request.Method = value;
        }

        public Dictionary<string, object> Parameters { get; set; }

        public byte[] CustomBody { get; set; }
        public bool WriteCustomBody { get; set; } = false;

        public Uri Uri => request.RequestUri;

        public void SetProxy(string host, int port)
        {
            request.Proxy = new WebProxy(host, port);
        }

        public async Task<Stream> GetRequestStream()
        {
            return await request.GetRequestStreamAsync().ConfigureAwait(false);
        }

        public async Task<IResponse> GetResponse()
        {
            return new Response(await request.GetResponseAsync().ConfigureAwait(false));
        }
    }
}
