using System.IO;
using System.Net;

namespace CryptoExchange.Net.Interfaces
{
    public interface IResponse
    {
        HttpStatusCode StatusCode { get; }
        Stream GetResponseStream();
        void Close();
    }
}
