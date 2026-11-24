using Binance.Net.Clients;
using Bybit.Net.Clients;
using CryptoExchange.Net.SharedApis;
using OKX.Net.Clients;

using var binance = new BinanceRestClient();
using var okx = new OKXRestClient();
using var bybit = new BybitRestClient();
var tickers = new IGetTickerRest[]
{
    binance.SpotApi.SharedApi, okx.UnifiedApi.SharedApi, bybit.V5Api.SharedApi
};
var symbol = new SharedSymbol(TradingMode.Spot, "BTC", "USDT");
await Task.WhenAll(tickers.Select(async ticker =>
{
    var result = await ticker.GetTickerAsync(new GetTickerRequest(symbol));
    if (!result.Success)
        Console.WriteLine($"{ticker.Exchange}: {result.Error}");
    else
        Console.WriteLine($"{ticker.Exchange}: {result.Data.LastPrice}, base volume: {result.Data.Volumes.QuantityInBaseAsset}");
}));
