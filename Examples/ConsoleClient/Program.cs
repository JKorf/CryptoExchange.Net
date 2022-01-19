using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleClient.Exchanges;

namespace ConsoleClient
{
    internal class Program
    {
        static Dictionary<string, IExchange> _exchanges = new Dictionary<string, IExchange>
        {
            { "Binance", new BinanceExchange() }
        };

        static async Task Main(string[] args)
        {
            Console.WriteLine("> Available commands: PlaceOrder, GetOrders, GetPrice");
            while (true)
            {
                var input = Console.ReadLine();

                switch (input)
                {
                    case "PlaceOrder":

                        break;
                    case "GetOrders":

                        break;
                    case "GetPrice":
                        await ProcessGetPrice();
                        break;
                    default:
                        Console.WriteLine("> Unknown command");
                        break;
                }
            }
        }

        static async Task ProcessGetPrice()
        {
            Console.WriteLine("> Exchange?");
            var exchange = Console.ReadLine();

            Console.WriteLine("> Symbol?");
            var symbol = Console.ReadLine();

            var price = await _exchanges[exchange].GetPrice(symbol);
            Console.WriteLine($"> {exchange} price for {symbol}: {price}");
        }
    }
}
