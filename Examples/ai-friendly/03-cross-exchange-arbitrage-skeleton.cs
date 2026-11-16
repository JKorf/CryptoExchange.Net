using Binance.Net.Clients;
using Bybit.Net.Clients;
using CryptoExchange.Net.SharedApis;
using OKX.Net.Clients;

// Public, read-only gross-price comparison. No order execution.
using var binance = new BinanceRestClient();
using var okx = new OKXRestClient();
using var bybit = new BybitRestClient();
var books = new IGetBookTickerRest[]
{
    binance.SpotApi.SharedApi, okx.UnifiedApi.SharedApi, bybit.V5Api.SharedApi
};
var symbol = new SharedSymbol(TradingMode.Spot, "BTC", "USDT");
var quotes = await Task.WhenAll(books.Select(async book =>
{
    var result = await book.GetBookTickerAsync(new GetBookTickerRequest(symbol));
    if (!result.Success)
    {
        Console.WriteLine($"{book.Exchange}: {result.Error}");
        return null;
    }
    return new Quote(book.Exchange, result.Data.BestBidPrice, result.Data.BestAskPrice);
}));
var valid = quotes.OfType<Quote>().Where(q => q.Bid > 0 && q.Ask > 0).ToArray();
var pairs = from buy in valid
            from sell in valid
            where buy.Exchange != sell.Exchange
            orderby sell.Bid / buy.Ask descending
            select new { Buy = buy, Sell = sell };
var best = pairs.FirstOrDefault();
if (best is not null)
{
    var grossSpread = (best.Sell.Bid / best.Buy.Ask - 1) * 100;
    Console.WriteLine($"Buy {best.Buy.Exchange}, sell {best.Sell.Exchange}: {grossSpread:F3}% gross");
    // Quotes are not atomic. Fees, size, inventory, and transfer constraints can remove any apparent spread.
}
record Quote(string Exchange, decimal Bid, decimal Ask);
