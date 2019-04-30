using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using CryptoExchange.Net.Interfaces;

namespace CryptoExchange.Net.Requests
{
    public class Response : IResponse
    {
        private readonly HttpWebResponse response;

        public HttpStatusCode StatusCode => response.StatusCode;

        public Response(HttpWebResponse response)
        {
            this.response = response;
        }

        public Stream GetResponseStream()
        {
            return response.GetResponseStream();
        }

        public IEnumerable<Tuple<string, string>> GetResponseHeaders()
        {
            return response.Headers.ToIEnumerable();
        }

        public void Close()
        {
            response.Close();
        }
    }
}
