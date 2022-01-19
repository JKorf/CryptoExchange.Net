using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Binance.Net.Clients;

namespace ConsoleClient.Exchanges
{
    internal class BinanceExchange : IExchange
    {
        public async Task<decimal> GetPrice(string symbol)
        {
            using var client = new BinanceClient();
            var result = await client.SpotApi.ExchangeData.GetPriceAsync(symbol);
            // Should check result success status here
            return result.Data.Price;
        }
    }
}
