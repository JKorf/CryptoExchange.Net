using Binance.Net.Clients;
using BitMart.Net.Clients;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using OKX.Net.Clients;

var symbol = new SharedSymbol(TradingMode.Spot, "ETH", "USDT");
using var binanceRest = new BinanceRestClient();
var binanceSpotRestClient = binanceRest.SpotApi.SharedApi;
using var okxRest = new OKXRestClient();
var okxSpotRestClient = okxRest.UnifiedApi.SharedApi;
using var bitmartRest = new BitMartRestClient();
var bitmartSpotRestClient = bitmartRest.SpotApi.SharedApi;

using var binanceSocket = new BinanceSocketClient();
var binanceSpotSocketClient = binanceSocket.SpotApi.SharedApi;
using var okxSocket = new OKXSocketClient();
var okxSpotSocketClient = okxSocket.UnifiedApi.SharedApi;
using var bitmartSocket = new BitMartSocketClient();
var bitmartSpotSocketClient = bitmartSocket.SpotApi.SharedApi;

await GetLastTradePriceAsync(binanceSpotRestClient, symbol);
await GetLastTradePriceAsync(okxSpotRestClient, symbol);
await GetLastTradePriceAsync(bitmartSpotRestClient, symbol);

Console.WriteLine();
Console.WriteLine("Press enter to start websocket");
Console.ReadLine();

var subscriptions = new List<UpdateSubscription>();
try
{
    await SubscribeTickerUpdatesAsync(binanceSpotSocketClient, symbol, subscriptions);
    await SubscribeTickerUpdatesAsync(okxSpotSocketClient, symbol, subscriptions);
    await SubscribeTickerUpdatesAsync(bitmartSpotSocketClient, symbol, subscriptions);

    Console.WriteLine("Press enter to stop websocket updates");
    Console.ReadLine();
}
finally
{
    await Task.WhenAll(subscriptions.Select(subscription => subscription.CloseAsync()));
}

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
