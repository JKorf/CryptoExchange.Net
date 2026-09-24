using Binance.Net.Clients;
using BitMart.Net.Clients;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using OKX.Net.Clients;

var symbol = new SharedSymbol(TradingMode.Spot, "ETH", "USDT");
var binanceSpotRestClient = new BinanceRestClient().SpotApi.SharedApi;
var okxSpotRestClient = new OKXRestClient().UnifiedApi.SharedApi;
var bitmartSpotRestClient = new BitMartRestClient().SpotApi.SharedApi;

var binanceSpotSocketClient = new BinanceSocketClient().SpotApi.SharedApi;
var okxSpotSocketClient = new OKXSocketClient().UnifiedApi.SharedApi;
var bitmartSpotSocketClient = new BitMartSocketClient().SpotApi.SharedApi;

await GetLastTradePriceAsync(binanceSpotRestClient, symbol);
await GetLastTradePriceAsync(okxSpotRestClient, symbol);
await GetLastTradePriceAsync(bitmartSpotRestClient, symbol);

Console.WriteLine();
Console.WriteLine("Press enter to start websocket");
Console.ReadLine();

var subscriptions = new List<UpdateSubscription>();
await SubscribeTickerUpdatesAsync(binanceSpotSocketClient, symbol, subscriptions);
await SubscribeTickerUpdatesAsync(okxSpotSocketClient, symbol, subscriptions);
await SubscribeTickerUpdatesAsync(bitmartSpotSocketClient, symbol, subscriptions);

Console.WriteLine("Press enter to stop websocket updates");
Console.ReadLine();
foreach (var subscription in subscriptions)
    await subscription.CloseAsync();

async Task GetLastTradePriceAsync(IGetTickerRest client, SharedSymbol symbol)
{
    var result = await client.GetTickerAsync(new GetTickerRequest(symbol));
    if (!result.Success)
    {
        Console.WriteLine($"Failed to get ticker: {result.Error}");
        return;
    }

    Console.WriteLine($"{client.Exchange} {result.Data.Symbol}: {result.Data.LastPrice}");
}

async Task SubscribeTickerUpdatesAsync(ISubscribeTickerSocket client, SharedSymbol symbol, ICollection<UpdateSubscription> subscriptions)
{
    var result = await client.SubscribeToTickerUpdatesAsync(new SubscribeTickerRequest(symbol), update =>
    {
        Console.WriteLine($"{client.Exchange} {update.Data.Symbol} {update.Data.LastPrice}");
    });

    if (!result.Success)
    {
        Console.WriteLine($"Failed to subscribe ticker: {result.Error}");
        return;
    }

    subscriptions.Add(result.Data);
}
