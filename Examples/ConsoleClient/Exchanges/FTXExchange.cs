using ConsoleClient.Models;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using FTX.Net.Clients;
using FTX.Net.Interfaces.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleClient.Exchanges
{
    internal class FTXExchange : IExchange
    {
        private IFTXSocketClient _socketClient = new FTXSocketClient();

        public async Task<WebCallResult> CancelOrder(string symbol, string id)
        {
            using var client = new FTXClient();
            var result = await client.TradeApi.Trading.CancelOrderAsync(long.Parse(id));
            return result.AsDataless();
        }

        public async Task<Dictionary<string, decimal>> GetBalances()
        {
            using var client = new FTXClient();
            var result = await client.TradeApi.Account.GetBalancesAsync();
            return result.Data.ToDictionary(d => d.Asset, d => d.Total);
        }

        public async Task<IEnumerable<OpenOrder>> GetOpenOrders()
        {
            using var client = new FTXClient();
            var order = await client.TradeApi.Trading.GetOpenOrdersAsync();
            return order.Data.Select(o => new OpenOrder
            {
                Symbol = o.Symbol,
                OrderSide = o.Side.ToString(),
                OrderStatus = o.Status.ToString(),
                OrderTime = o.CreateTime,
                OrderType = o.Type.ToString(),
                Price = o.Price ?? 0,
                Quantity = o.Quantity,
                QuantityFilled = o.QuantityFilled ?? 0
            });
        }

        public async Task<decimal> GetPrice(string symbol)
        {
            using var client = new FTXClient();
            var result = await client.TradeApi.ExchangeData.GetSymbolAsync(symbol);
            return result.Data.LastPrice ?? 0;
        }

        public async Task<WebCallResult<string>> PlaceOrder(string symbol, string side, string type, decimal quantity, decimal? price)
        {
            using var client = new FTXClient();
            var result = await client.TradeApi.Trading.PlaceOrderAsync(
                symbol,
                side.ToLower() == "buy" ? FTX.Net.Enums.OrderSide.Buy : FTX.Net.Enums.OrderSide.Sell,
                type == "market" ? FTX.Net.Enums.OrderType.Market : FTX.Net.Enums.OrderType.Limit,
                quantity,
                price: price);
            return result.As(result.Data?.Id.ToString());
        }

        public async Task<UpdateSubscription> SubscribePrice(string symbol, Action<decimal> handler)
        {
            var sub = await _socketClient.Streams.SubscribeToTickerUpdatesAsync(symbol, data => handler(data.Data.LastPrice ?? 0));
            return sub.Data;
        }
    }
}
