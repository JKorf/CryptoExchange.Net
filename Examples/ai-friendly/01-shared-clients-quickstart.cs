// 01-shared-clients-quickstart.cs
//
// Demonstrates: the SharedApis pattern — same code calling multiple exchanges.
//
// Setup:
//   dotnet add package Binance.Net
//   dotnet add package JK.OKX.Net

using Binance.Net.Clients;
using OKX.Net.Clients;
using CryptoExchange.Net.SharedApis;

// ---- THE PATTERN ----
// Each exchange library exposes `.SharedClient` properties on its API surfaces.
// Those implement common interfaces from CryptoExchange.Net.SharedApis.
// You write code against the interface — it works against any exchange.

ISpotTickerRestClient binance = new BinanceRestClient().SpotApi.SharedClient;
ISpotTickerRestClient okx     = new OKXRestClient().UnifiedApi.SharedClient;

// ---- SYMBOL NORMALIZATION ----
// Different exchanges use different formats: "BTCUSDT" (Binance), "BTC-USDT" (OKX).
// SharedSymbol normalizes — pass it instead of raw strings to shared methods.
var btcusdt = new SharedSymbol(TradingMode.Spot, "BTC", "USDT");

// ---- AGNOSTIC METHOD — runs against any exchange ----
async Task PrintTicker(ISpotTickerRestClient client, SharedSymbol symbol)
{
    var result = await client.GetSpotTickerAsync(new GetTickerRequest(symbol));
    if (!result.Success)
    {
        Console.WriteLine($"[{client.Exchange}] Failed: {result.Error}");
        return;
    }
    // SharedSpotTicker has a unified shape regardless of the source exchange
    Console.WriteLine($"[{client.Exchange}] {result.Data.Symbol}: last={result.Data.LastPrice}, 24h-vol={result.Data.Volume}");
}

await PrintTicker(binance, btcusdt);
await PrintTicker(okx, btcusdt);

// ---- WEBSOCKET PATTERN ----
ITickerSocketClient binanceTickerSocket = new BinanceSocketClient().SpotApi.SharedClient;
ITickerSocketClient okxTickerSocket     = new OKXSocketClient().UnifiedApi.SharedClient;

var sub1 = await binanceTickerSocket.SubscribeToTickerUpdatesAsync(
    new SubscribeTickerRequest(btcusdt),
    update => Console.WriteLine($"[{binanceTickerSocket.Exchange}] {update.Data.Symbol}: {update.Data.LastPrice}"));

var sub2 = await okxTickerSocket.SubscribeToTickerUpdatesAsync(
    new SubscribeTickerRequest(btcusdt),
    update => Console.WriteLine($"[{okxTickerSocket.Exchange}] {update.Data.Symbol}: {update.Data.LastPrice}"));

Console.WriteLine("Press Enter to exit");
Console.ReadLine();

if (sub1.Success) await sub1.Data.CloseAsync();
if (sub2.Success) await sub2.Data.CloseAsync();

// Common variations:
//   Add Bybit:           ISpotTickerRestClient bybit = new BybitRestClient().V5Api.SharedClient;
//   Add Kraken:          ISpotTickerRestClient kraken = new KrakenRestClient().SpotApi.SharedClient;
//   Add Coinbase:        ISpotTickerRestClient cb = new CoinbaseRestClient().AdvancedTradeApi.SharedClient;
//   Other interfaces:    ISpotOrderRestClient (place/cancel orders), IBalanceRestClient (balances),
//                        IFuturesOrderRestClient, IPositionRestClient, IOrderBookSocketClient, etc.
