using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Binance.Net.Clients;
using Bybit.Net.Clients;
using ConsoleClient.Exchanges;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects.Sockets;

namespace ConsoleClient
{
    internal class Program
    {
        static Dictionary<string, IExchange> _exchanges = new Dictionary<string, IExchange>
        {
            { "Binance", new BinanceExchange() },
            { "Bybit", new BybitExchange() }
        };

        static async Task Main(string[] args)
        {
            BinanceRestClient.SetDefaultOptions(options =>
            {
                options.ApiCredentials = new ApiCredentials("APIKEY", "APISECRET");
            });
            BybitRestClient.SetDefaultOptions(options =>
            {
                options.ApiCredentials = new ApiCredentials("APIKEY", "APISECRET");
            });

            while (true)
            {
                Console.WriteLine("> Available commands: PlaceOrder, GetBalances, GetOpenOrders, CancelOrder, GetPrice, SubscribePrice");
                var input = Console.ReadLine();

                switch (input)
                {
                    case "PlaceOrder":
                        await ProcessPlaceOrder();
                        break;
                    case "GetBalances":
                        await ProcessGetBalances();
                        break;
                    case "GetOpenOrders":
                        await ProcessGetOpenOrders();
                        break;
                    case "CancelOrder":
                        await ProcessCancelOrder();
                        break;
                    case "GetPrice":
                        await ProcessGetPrice();
                        break;
                    case "SubscribePrice":
                        var sub = await ProcessSubscribePrice();
                        Console.ReadLine();
                        await sub.CloseAsync();
                        break;
                    default:
                        Console.WriteLine("> Unknown command");
                        break;
                }
            }
        }

        static async Task ProcessPlaceOrder()
        {
            var exchange = GetInput("Exchange?");
            var symbol = GetInput("Symbol?");
            var side = GetInput("Buy/Sell?");
            var type = GetInput("Limit/Market?");
            var quantity = GetDecimalInput("Quantity?");
            decimal? price = null;
            if (type != "Market")
                price = GetDecimalInput("Price?");
            var result = await _exchanges[exchange].PlaceOrder(symbol, side, type, quantity, price);
            if (result.Success)
                Console.WriteLine("Order placed, ID: " + result.Data);
            else
                Console.WriteLine("Failed to place order: " + result.Error);
        }

        static async Task ProcessGetBalances()
        {
            var exchange = GetInput("Exchange?");
            Console.WriteLine("Requesting balances...");
            var balances = await _exchanges[exchange].GetBalances();
            foreach (var balance in balances.Where(b => b.Value != 0))
                Console.WriteLine($"> {balance.Key}: {balance.Value}");            
        }

        static async Task ProcessGetOpenOrders()
        {
            var exchange = GetInput("Exchange?");
            Console.WriteLine("Requesting open orders...");
            var orders = await _exchanges[exchange].GetOpenOrders();
            if (!orders.Any())
                Console.WriteLine("No open orders found");
            else
            {
                foreach(var openOrder in orders)
                    Console.WriteLine($"> {openOrder.OrderTime} - {openOrder.Symbol} {openOrder.OrderType} {openOrder.OrderSide} {openOrder.QuantityFilled}/{openOrder.Quantity} @ {openOrder.Price}");
            }
        }

        static async Task ProcessCancelOrder()
        {
            var exchange = GetInput("Exchange?");
            var symbol = GetInput("Symbol?");
            var id = GetInput("Order id?");
            var result = await _exchanges[exchange].CancelOrder(symbol, id);
            if (result)
                Console.WriteLine("Order canceled");
            else
                Console.WriteLine("Order cancel failed: " + result.Error);
        }

        static async Task ProcessGetPrice()
        {
            var exchange = GetInput("Exchange?");
            var symbol = GetInput("Symbol?");
            Console.WriteLine("Requesting price...");
            var price = await _exchanges[exchange].GetPrice(symbol);
            Console.WriteLine($"> {exchange} price for {symbol}: {price}");
        }

        static async Task<UpdateSubscription> ProcessSubscribePrice()
        {
            var exchange = GetInput("Exchange?");
            var symbol = GetInput("Symbol?");
            Console.WriteLine("Subscribing price...");
            return await _exchanges[exchange].SubscribePrice(symbol, price => HandlePriceUpdate(exchange,symbol, price));
        }

        static void HandlePriceUpdate(string exchange, string symbol, decimal price)
        {
            Console.Clear();
            Console.WriteLine($"> {exchange} price for {symbol}: {price}");
            Console.WriteLine("Press enter to stop live price updates");
        }

        static decimal GetDecimalInput(string question)
        {
            while (true) 
            {
                Console.WriteLine("> " + question);
                var response = Console.ReadLine();
                if (decimal.TryParse(response, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var answer))
                    return answer;
                else
                    Console.WriteLine("Invalid decimal value");
            }
        }

        static string GetInput(string question)
        {
            Console.WriteLine("> " + question);
            return Console.ReadLine();
        }
    }
}
