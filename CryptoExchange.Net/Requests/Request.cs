using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Interfaces;

namespace CryptoExchange.Net.Requests
{
    /// <summary>
    /// Request object
    /// </summary>
    public class Request : IRequest
    {
        private readonly HttpRequestMessage request;
        private readonly HttpClient httpClient;

        /// <summary>
        /// Create request object for web request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="client"></param>
        public Request(HttpRequestMessage request, HttpClient client)
        {
            httpClient = client;
            this.request = request;
        }
        
        /// <inheritdoc />
        public string? Content { get; private set; }

        /// <inheritdoc />
        public string Accept
        {
            set => request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(value));
        }

        /// <inheritdoc />
        public HttpMethod Method
        {
            get => request.Method;
            set => request.Method = value;
        }

        /// <inheritdoc />
        public Uri Uri => request.RequestUri;

        /// <inheritdoc />
        public void SetContent(string data, string contentType)
        {
            Content = data;
            request.Content = new StringContent(data, Encoding.UTF8, contentType);
        }

        /// <inheritdoc />
        public void AddHeader(string key, string value)
        {
            request.Headers.Add(key, value);
        }

        /// <inheritdoc />
        public void SetContent(byte[] data)
        {
            request.Content = new ByteArrayContent(data);
        }

        /// <inheritdoc />
        public async Task<IResponse> GetResponse(CancellationToken cancellationToken)
        {
            return new Response(await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false));
        }
    }
}
