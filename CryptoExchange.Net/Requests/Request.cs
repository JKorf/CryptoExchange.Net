using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using CryptoExchange.Net.Interfaces;

namespace CryptoExchange.Net.Requests
{
    /// <summary>
    /// Request object
    /// </summary>
    public class Request : IRequest
    {
        private readonly WebRequest request;

        /// <summary>
        /// Create request object for web request
        /// </summary>
        /// <param name="request"></param>
        public Request(WebRequest request)
        {
            this.request = request;
        }

        /// <inheritdoc />
        public WebHeaderCollection Headers
        {
            get => request.Headers;
            set => request.Headers = value;
        }

        /// <inheritdoc />
        public string ContentType
        {
            get => request.ContentType;
            set => request.ContentType = value;
        }

        /// <inheritdoc />
        public string Content { get; set; }

        /// <inheritdoc />
        public string Accept
        {
            get => ((HttpWebRequest)request).Accept;
            set => ((HttpWebRequest)request).Accept = value;
        }

        /// <inheritdoc />
        public long ContentLength
        {
            get => ((HttpWebRequest)request).ContentLength;
            set => ((HttpWebRequest)request).ContentLength = value;
        }

        /// <inheritdoc />
        public string Method
        {
            get => request.Method;
            set => request.Method = value;
        }

        /// <inheritdoc />
        public TimeSpan Timeout
        {
            get => TimeSpan.FromMilliseconds(request.Timeout);
            set => request.Timeout = (int)Math.Round(value.TotalMilliseconds);
        }

        /// <inheritdoc />
        public Uri Uri => request.RequestUri;

        /// <inheritdoc />
        public void SetProxy(string host, int port, string login, string password)
        {
            request.Proxy = new WebProxy(host, port);
            if(!string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(password)) request.Proxy.Credentials = new NetworkCredential(login, password);
        }

        /// <inheritdoc />
        public async Task<Stream> GetRequestStream()
        {
            return await request.GetRequestStreamAsync().ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<IResponse> GetResponse()
        {
            return new Response((HttpWebResponse)await request.GetResponseAsync().ConfigureAwait(false));
        }
    }
}
