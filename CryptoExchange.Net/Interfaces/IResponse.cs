using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace CryptoExchange.Net.Interfaces
{
    public interface IResponse
    {
        HttpStatusCode StatusCode { get; }
        Stream GetResponseStream();
        IEnumerable<Tuple<string, string>> GetResponseHeaders();
        void Close();
    }
}
