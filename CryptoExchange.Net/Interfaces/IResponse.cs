using System.IO;

namespace CryptoExchange.Net.Interfaces
{
    public interface IResponse
    {
        Stream GetResponseStream();
    }
}
