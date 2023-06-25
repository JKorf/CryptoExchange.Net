using System;
using System.Collections.Generic;
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
    /// Request object, wrapper for HttpRequestMessage
    /// </summary>
    public class Request : IRequest
    {
        private readonly HttpRequestMessage _request;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Create request object for web request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="client"></param>        
        /// <param name="requestId"></param>        
        public Request(HttpRequestMessage request, HttpClient client, int requestId)
        {
            _httpClient = client;
            _request = request;
            RequestId = requestId;
        }
        
        /// <inheritdoc />
        public string? Content { get; private set; }

        /// <inheritdoc />
        public string Accept
        {
            set => _request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(value));
        }

        /// <inheritdoc />
        public HttpMethod Method
        {
            get => _request.Method;
            set => _request.Method = value;
        }

        /// <inheritdoc />
        public Uri Uri => _request.RequestUri;

        /// <inheritdoc />
        public int RequestId { get; }

        /// <inheritdoc />
        public void SetContent(string data, string contentType)
        {
            Content = data;
            _request.Content = new StringContent(data, Encoding.UTF8, contentType);
        }

        /// <inheritdoc />
        public void AddHeader(string key, string value)
        {
            _request.Headers.Add(key, value);
        }

        /// <inheritdoc />
        public Dictionary<string, IEnumerable<string>> GetHeaders()
        {
            return _request.Headers.ToDictionary(h => h.Key, h => h.Value);
        }

        /// <inheritdoc />
        public void SetContent(byte[] data)
        {
            _request.Content = new ByteArrayContent(data);
        }

        /// <inheritdoc />
        public async Task<IResponse> GetResponseAsync(CancellationToken cancellationToken)
        {
            return new Response(await _httpClient.SendAsync(_request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false));
        }
    }
}
