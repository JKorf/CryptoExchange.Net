using Binance.Net.Clients;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using OKX.Net.Clients;

using var binanceRest = new BinanceRestClient();
using var okxRest = new OKXRestClient();
using var binanceSocket = new BinanceSocketClient();
using var okxSocket = new OKXSocketClient();
var symbol = new SharedSymbol(TradingMode.Spot, "BTC", "USDT");
foreach (var ticker in new IGetTickerRest[] { binanceRest.SpotApi.SharedApi, okxRest.UnifiedApi.SharedApi })
{
    var result = await ticker.GetTickerAsync(new GetTickerRequest(symbol));
    if (!result.Success)
        Console.WriteLine($"{ticker.Exchange}: {result.Error}");
    else
        Console.WriteLine($"{ticker.Exchange}: {result.Data.LastPrice}");
}

var subscriptions = new List<UpdateSubscription>();
try
{
    foreach (var ticker in new ISubscribeTickerSocket[] { binanceSocket.SpotApi.SharedApi, okxSocket.UnifiedApi.SharedApi })
    {
        var result = await ticker.SubscribeToTickerUpdatesAsync(new SubscribeTickerRequest(symbol),
            update => Console.WriteLine($"{ticker.Exchange}: {update.Data.LastPrice}"));
        if (!result.Success)
            Console.WriteLine($"{ticker.Exchange}: {result.Error}");
        else
            subscriptions.Add(result.Data);
    }
    await Task.Delay(TimeSpan.FromSeconds(30));
}
finally
{
    await Task.WhenAll(subscriptions.Select(subscription => subscription.CloseAsync()));
}
