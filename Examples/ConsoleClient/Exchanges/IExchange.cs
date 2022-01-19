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
    }
}
