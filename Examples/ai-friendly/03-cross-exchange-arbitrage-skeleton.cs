// 03-cross-exchange-arbitrage-skeleton.cs
//
// Demonstrates: skeleton pattern for a cross-exchange spot arbitrage scanner.
// This is a structural example — production arbitrage requires also:
//   - real-time WebSocket feeds (not REST polling)
//   - orderbook depth analysis (not just ticker)
//   - slippage / fees modeling
//   - withdrawal availability and timing
//   - inventory management on both sides
// Use this as a starting structure, not a deployable bot.
//
// Setup:
//   dotnet add package Binance.Net
//   dotnet add package JK.OKX.Net
//   dotnet add package Bybit.Net

using Binance.Net.Clients;
using OKX.Net.Clients;
using Bybit.Net.Clients;
using CryptoExchange.Net.SharedApis;

// ---- CONFIGURATION ----
// Symbols to monitor and minimum profit threshold (gross, before fees)
var symbols = new[]
{
    new SharedSymbol(TradingMode.Spot, "BTC", "USDT"),
    new SharedSymbol(TradingMode.Spot, "ETH", "USDT"),
    new SharedSymbol(TradingMode.Spot, "SOL", "USDT"),
};

const decimal minSpreadBps = 30; // 0.30% — must exceed total fees on both legs

// ---- USE BOOK TICKER FOR TIGHTER SPREADS ----
// IBookTickerRestClient gives best bid/ask, narrower than 24h ticker.
// For real arbitrage you'd use IOrderBookSocketClient for depth + push updates.
var exchanges = new List<IBookTickerRestClient>
{
    new BinanceRestClient().SpotApi.SharedClient,
    new OKXRestClient().UnifiedApi.SharedClient,
    new BybitRestClient().V5Api.SharedClient,
};

// ---- MAIN LOOP (simplified: REST polling, 5-second intervals) ----
// In production: replace with concurrent WebSocket subscriptions.
while (true)
{
    foreach (var symbol in symbols)
    {
        await ScanSymbolAsync(symbol, exchanges);
    }

    Console.WriteLine($"--- waiting 5s --- ({DateTime.UtcNow:HH:mm:ss})");
    await Task.Delay(TimeSpan.FromSeconds(5));
}

// ---- SCAN ONE SYMBOL ACROSS ALL EXCHANGES ----
async Task ScanSymbolAsync(SharedSymbol symbol, List<IBookTickerRestClient> clients)
{
    // Fetch best bid/ask from every exchange in parallel
    var tasks = clients.Select(c => GetBookAsync(c, symbol)).ToArray();
    var quotes = (await Task.WhenAll(tasks)).Where(q => q != null).Cast<Quote>().ToList();

    if (quotes.Count < 2) return;

    // Find best buy venue (lowest ask) and best sell venue (highest bid)
    var bestBuy  = quotes.OrderBy(q => q.AskPrice).First();
    var bestSell = quotes.OrderByDescending(q => q.BidPrice).First();

    if (bestBuy.Exchange == bestSell.Exchange) return; // no cross-venue arbitrage

    // Spread in basis points
    var spreadBps = (bestSell.BidPrice - bestBuy.AskPrice) / bestBuy.AskPrice * 10_000;

    if (spreadBps >= minSpreadBps)
    {
        Console.WriteLine(
            $"[{symbol.BaseAsset}/{symbol.QuoteAsset}] BUY {bestBuy.Exchange}@{bestBuy.AskPrice} " +
            $"SELL {bestSell.Exchange}@{bestSell.BidPrice} " +
            $"spread={spreadBps:F1}bps");

        // Production hooks would go here:
        //   - check available inventory on both venues
        //   - simulate execution against orderbook depth
        //   - compute net P&L after fees
        //   - if profitable, execute via ISpotOrderRestClient on both venues
    }
}

async Task<Quote?> GetBookAsync(IBookTickerRestClient client, SharedSymbol symbol)
{
    var result = await client.GetBookTickerAsync(new GetBookTickerRequest(symbol));
    if (!result.Success || result.Data == null)
        return null;

    return new Quote(
        Exchange: client.Exchange,
        BidPrice: result.Data.BestBidPrice,
        AskPrice: result.Data.BestAskPrice);
}

record Quote(string Exchange, decimal BidPrice, decimal AskPrice);

// Production checklist (NOT in this skeleton):
//   ✓ Use WebSocket book tickers (IBookTickerSocketClient) instead of REST polling
//   ✓ Track full orderbook depth (IOrderBookSocketClient) to estimate fill price for size > top-of-book
//   ✓ Model fees per exchange per pair (taker vs maker, BNB discount, etc.)
//   ✓ Track inventory on both venues — can't sell what you don't have
//   ✓ Account for withdrawal delays if rebalancing inventory
//   ✓ Set hard P&L stops, position limits, maximum exposure per pair
//   ✓ Use ISpotOrderRestClient with exchange-supported IOC/fill-or-kill order options where available
//   ✓ Monitor connection health and have failover logic
//   ✓ Log everything — arbitrage P&L analysis requires complete audit trails
