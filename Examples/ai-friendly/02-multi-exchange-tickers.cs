// 02-multi-exchange-tickers.cs
//
// Demonstrates: aggregating ticker data across N exchanges concurrently.
// Pattern is foundational for arbitrage scanners, best-execution routers,
// portfolio dashboards, and cross-exchange comparison tools.
//
// Setup:
//   dotnet add package Binance.Net
//   dotnet add package JK.OKX.Net
//   dotnet add package Bybit.Net

using Binance.Net.Clients;
using OKX.Net.Clients;
using Bybit.Net.Clients;
using CryptoExchange.Net.SharedApis;

// ---- BUILD A LIST OF EXCHANGE CLIENTS ----
// All implement ISpotTickerRestClient, so we can iterate uniformly.
var exchanges = new List<ISpotTickerRestClient>
{
    new BinanceRestClient().SpotApi.SharedClient,
    new OKXRestClient().UnifiedApi.SharedClient,
    new BybitRestClient().V5Api.SharedClient,
    // Add as many as you want — same interface
};

var symbol = new SharedSymbol(TradingMode.Spot, "BTC", "USDT");

// ---- CONCURRENT FETCH ----
// Fire all requests in parallel, await all together.
// Each request runs on its own connection — no inter-exchange interference.
var tasks = exchanges
    .Select(c => FetchAsync(c, symbol))
    .ToList();

var results = await Task.WhenAll(tasks);

// ---- PRINT SORTED BY PRICE ----
// Highest bid first — useful for "where to sell" decisions.
foreach (var r in results.Where(r => r != null).OrderByDescending(r => r!.LastPrice))
{
    Console.WriteLine($"{r!.Exchange,-12} {r.LastPrice,15} (24h vol: {r.Volume:F2})");
}

// ---- HELPER ----
async Task<TickerSnapshot?> FetchAsync(ISpotTickerRestClient client, SharedSymbol sym)
{
    var result = await client.GetSpotTickerAsync(new GetTickerRequest(sym));
    if (!result.Success)
    {
        Console.WriteLine($"[{client.Exchange}] error: {result.Error}");
        return null;
    }

    return new TickerSnapshot(
        Exchange:  client.Exchange,
        Symbol:    result.Data.Symbol,
        LastPrice: result.Data.LastPrice ?? 0,
        Volume:    result.Data.Volume);
}

record TickerSnapshot(string Exchange, string Symbol, decimal LastPrice, decimal Volume);

// Common variations:
//   Periodic polling:        wrap in `while(true) { await ...; await Task.Delay(...); }`
//                            Better: use ITickerSocketClient for push updates instead of polling
//   With timeout per call:   pass `ct: cts.Token` and use `CancellationTokenSource(timeout)`
//   With retry:              wrap FetchAsync in retry policy (see Binance.Net 05-error-handling.cs)
//   Different metric:        use IBookTickerRestClient for tighter best-bid/ask data
//   Spread analysis:         instead of ticker, use IOrderBookRestClient and compute mid/spread
