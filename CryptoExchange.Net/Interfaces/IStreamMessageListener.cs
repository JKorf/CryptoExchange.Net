using CryptoExchange.Net.Objects.Sockets;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Interfaces
{
    internal interface IStreamMessageListener
    {
        int Priority { get; }
        bool MessageMatches(StreamMessage message);
        Task ProcessAsync(StreamMessage message);
    }
}
