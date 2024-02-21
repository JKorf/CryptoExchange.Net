using ConsoleClient.Models;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleClient.Exchanges
{
    public interface IExchange
    {
        Task<decimal> GetPrice(string symbol);
        Task<IEnumerable<OpenOrder>> GetOpenOrders();
        Task<Dictionary<string, decimal>> GetBalances();
        Task<WebCallResult> CancelOrder(string symbol, string id);
        Task<WebCallResult<string>> PlaceOrder(string symbol, string side, string type, decimal quantity, decimal? price );
        Task<UpdateSubscription> SubscribePrice(string symbol, Action<decimal> handler);
    }
}
