using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Binance.Net.Clients;
using Binance.Net.Interfaces.Clients;
using ConsoleClient.Models;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;

namespace ConsoleClient.Exchanges
{
    internal class BinanceExchange : IExchange
    {
        private IBinanceSocketClient _socketClient = new BinanceSocketClient();

        public async Task<WebCallResult> CancelOrder(string symbol, string id)
        {
            using var client = new BinanceRestClient();
            var result = await client.SpotApi.Trading.CancelOrderAsync(symbol, long.Parse(id));
            return result.AsDataless();
        }

        public async Task<Dictionary<string, decimal>> GetBalances()
        {
            using var client = new BinanceRestClient();
            var result = await client.SpotApi.Account.GetAccountInfoAsync();
            return result.Data.Balances.ToDictionary(b => b.Asset, b => b.Total);
        }

        public async Task<IEnumerable<OpenOrder>> GetOpenOrders()
        {
            using var client = new BinanceRestClient();
            var result = await client.SpotApi.Trading.GetOpenOrdersAsync();
            // Should check result success status here
            return result.Data.Select(o => new OpenOrder
            {
                Symbol = o.Symbol,
                OrderSide = o.Side.ToString(),
                OrderStatus = o.Status.ToString(),
                OrderTime = o.CreateTime,
                OrderType = o.Type.ToString(),
                Price = o.Price,
                Quantity = o.Quantity,
                QuantityFilled = o.QuantityFilled
            });
        }

        public async Task<decimal> GetPrice(string symbol)
        {
            using var client = new BinanceRestClient();
            var result = await client.SpotApi.ExchangeData.GetPriceAsync(symbol);
            // Should check result success status here
            return result.Data.Price;
        }

        public async Task<WebCallResult<string>> PlaceOrder(string symbol, string side, string type, decimal quantity, decimal? price)
        {
            using var client = new BinanceRestClient();
            var result = await client.SpotApi.Trading.PlaceOrderAsync(
                symbol,
                side.ToLower() == "buy" ? Binance.Net.Enums.OrderSide.Buy: Binance.Net.Enums.OrderSide.Sell, 
                type == "market" ? Binance.Net.Enums.SpotOrderType.Market : Binance.Net.Enums.SpotOrderType.Limit, 
                quantity, 
                price: price,
                timeInForce: type == "market" ? null: Binance.Net.Enums.TimeInForce.GoodTillCanceled);
            return result.As(result.Data?.Id.ToString());
        }

        public async Task<UpdateSubscription> SubscribePrice(string symbol, Action<decimal> handler)
        {
            var sub = await _socketClient.SpotApi.ExchangeData.SubscribeToMiniTickerUpdatesAsync(symbol, data => handler(data.Data.LastPrice));
            return sub.Data;
        }
    }
}
