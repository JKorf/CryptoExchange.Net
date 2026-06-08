using Binance.Net;
using Binance.Net.Clients;
using Bybit.Net;
using Bybit.Net.Clients;
using CryptoExchange.Net.SharedApis;

var binanceRest = new BinanceRestClient(options =>
{
    options.ApiCredentials = new BinanceCredentials("KEY", "SECRET");
});
var bybitRest = new BybitRestClient(options =>
{
    options.ApiCredentials = new BybitCredentials("KEY", "SECRET");
});
var binanceSocket = new BinanceSocketClient(options =>
{
    options.ApiCredentials = new BinanceCredentials("KEY", "SECRET");
});
var bybitSocket = new BybitSocketClient(options =>
{
    options.ApiCredentials = new BybitCredentials("KEY", "SECRET");
});

var exchanges = new Dictionary<string, ExchangeClients>(StringComparer.OrdinalIgnoreCase)
{
    ["binance"] = new ExchangeClients(
        "Binance",
        binanceRest.SpotApi.SharedClient,
        binanceRest.SpotApi.SharedClient,
        binanceRest.SpotApi.SharedClient,
        binanceRest.SpotApi.SharedClient,
        binanceSocket.SpotApi.SharedClient),
    ["bybit"] = new ExchangeClients(
        "Bybit",
        bybitRest.V5Api.SharedClient,
        bybitRest.V5Api.SharedClient,
        bybitRest.V5Api.SharedClient,
        bybitRest.V5Api.SharedClient,
        bybitSocket.V5SpotApi.SharedClient)
};

PrintHelp();

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input))
        continue;

    var commandArgs = input.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    var command = commandArgs[0].ToLowerInvariant();

    try
    {
        switch (command)
        {
            case "help":
                PrintHelp();
                break;
            case "exit":
            case "quit":
                return;
            case "prices":
                await PrintPricesAsync(commandArgs);
                break;
            case "stream":
                await StreamTickerAsync(commandArgs);
                break;
            case "orderbook":
                await PrintOrderBookAsync(commandArgs);
                break;
            case "balances":
                await PrintBalancesAsync(commandArgs);
                break;
            case "open-orders":
                await PrintOpenOrdersAsync(commandArgs);
                break;
            case "place-limit":
                await PlaceLimitOrderAsync(commandArgs);
                break;
            case "cancel-order":
                await CancelOrderAsync(commandArgs);
                break;
            default:
                Console.WriteLine("Unknown command. Type `help` for examples.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Command failed: {ex.Message}");
    }
}

async Task PrintPricesAsync(string[] args)
{
    var symbol = GetSymbol(args, 1);
    var tasks = exchanges.Values.Select(async exchange =>
    {
        var result = await exchange.TickerRest.GetSpotTickerAsync(new GetTickerRequest(symbol));
        if (!result.Success)
        {
            Console.WriteLine($"{exchange.Name,-8} error: {result.Error}");
            return;
        }

        Console.WriteLine($"{exchange.Name,-8} {result.Data.Symbol,-12} last={result.Data.LastPrice}");
    });

    await Task.WhenAll(tasks);
}

async Task StreamTickerAsync(string[] args)
{
    var exchange = GetExchange(args, 1);
    var symbol = GetSymbol(args, 2);

    using var cts = new CancellationTokenSource();
    var result = await exchange.TickerSocket.SubscribeToTickerUpdatesAsync(
        new SubscribeTickerRequest(symbol),
        update => Console.WriteLine($"{exchange.Name,-8} {update.Data.Symbol,-12} last={update.Data.LastPrice}"),
        cts.Token);

    if (!result.Success)
    {
        Console.WriteLine($"Subscription failed: {result.Error}");
        return;
    }

    Console.WriteLine("Streaming ticker updates. Press Enter to stop.");
    Console.ReadLine();
    cts.Cancel();
    await result.Data.CloseAsync();
}

async Task PrintOrderBookAsync(string[] args)
{
    var exchange = GetExchange(args, 1);
    var symbol = GetSymbol(args, 2);

    var result = await exchange.OrderBookRest.GetOrderBookAsync(new GetOrderBookRequest(symbol, limit: 5));
    if (!result.Success)
    {
        Console.WriteLine($"Order book request failed: {result.Error}");
        return;
    }

    Console.WriteLine($"{exchange.Name} {symbol.BaseAsset}/{symbol.QuoteAsset} top of book");
    for (var i = 0; i < Math.Min(5, Math.Min(result.Data.Bids.Length, result.Data.Asks.Length)); i++)
        Console.WriteLine($"bid {result.Data.Bids[i].Price,15} | ask {result.Data.Asks[i].Price,15}");
}

async Task PrintBalancesAsync(string[] args)
{
    var exchange = GetExchange(args, 1);
    var result = await exchange.BalanceRest.GetBalancesAsync(new GetBalancesRequest(TradingMode.Spot));
    if (!result.Success)
    {
        Console.WriteLine($"Balances request failed: {result.Error}");
        Console.WriteLine("Set API credentials in environment variables before calling private endpoints.");
        return;
    }

    foreach (var balance in result.Data.Where(x => x.Total != 0).OrderBy(x => x.Asset))
        Console.WriteLine($"{balance.Asset,-8} available={balance.Available} total={balance.Total}");
}

async Task PrintOpenOrdersAsync(string[] args)
{
    var exchange = GetExchange(args, 1);
    var symbol = GetSymbol(args, 2);

    var result = await exchange.OrderRest.GetOpenSpotOrdersAsync(new GetOpenOrdersRequest(symbol));
    if (!result.Success)
    {
        Console.WriteLine($"Open orders request failed: {result.Error}");
        return;
    }

    if (result.Data.Length == 0)
    {
        Console.WriteLine("No open orders.");
        return;
    }

    foreach (var order in result.Data)
        Console.WriteLine($"{order.OrderId,-24} {order.Side,-4} {order.OrderType,-6} {order.Status,-12} {order.OrderQuantity?.QuantityInBaseAsset} @ {order.OrderPrice}");
}

async Task PlaceLimitOrderAsync(string[] args)
{
    var exchange = GetExchange(args, 1);
    var symbol = GetSymbol(args, 2);
    var side = ParseSide(args.ElementAtOrDefault(4));
    var quantity = ParseDecimal(args.ElementAtOrDefault(5), "quantity");
    var price = ParseDecimal(args.ElementAtOrDefault(6), "price");

    var request = new PlaceSpotOrderRequest(
        symbol,
        side,
        SharedOrderType.Limit,
        SharedQuantity.Base(quantity),
        price,
        SharedTimeInForce.GoodTillCanceled,
        exchange.OrderRest.GenerateClientOrderId());

    var validationError = exchange.OrderRest.PlaceSpotOrderOptions.ValidateRequest(
        exchange.Name,
        request,
        TradingMode.Spot,
        exchange.OrderRest.SupportedTradingModes,
        exchange.OrderRest.SpotSupportedOrderTypes,
        exchange.OrderRest.SpotSupportedTimeInForce,
        exchange.OrderRest.SpotSupportedOrderQuantity);
    if (validationError != null)
    {
        Console.WriteLine($"Order request is not valid for {exchange.Name}: {validationError}");
        return;
    }

    var result = await exchange.OrderRest.PlaceSpotOrderAsync(request);
    Console.WriteLine(result.Success
        ? $"Order placed. Id: {result.Data.Id}"
        : $"Order failed: {result.Error}");
}

async Task CancelOrderAsync(string[] args)
{
    var exchange = GetExchange(args, 1);
    var symbol = GetSymbol(args, 2);
    var orderId = args.ElementAtOrDefault(4);
    if (string.IsNullOrWhiteSpace(orderId))
        throw new ArgumentException("Missing order id. Example: cancel-order binance BTC USDT 123456");

    var result = await exchange.OrderRest.CancelSpotOrderAsync(new CancelOrderRequest(symbol, orderId));
    Console.WriteLine(result.Success
        ? $"Order canceled. Id: {result.Data.Id}"
        : $"Cancel failed: {result.Error}");
}

ExchangeClients GetExchange(string[] args, int index)
{
    var name = args.ElementAtOrDefault(index);
    if (name == null || !exchanges.TryGetValue(name, out var exchange))
        throw new ArgumentException($"Unknown exchange `{name}`. Use one of: {string.Join(", ", exchanges.Keys)}");

    return exchange;
}

SharedSymbol GetSymbol(string[] args, int index)
{
    var baseAsset = args.ElementAtOrDefault(index) ?? "BTC";
    var quoteAsset = args.ElementAtOrDefault(index + 1) ?? "USDT";
    return new SharedSymbol(TradingMode.Spot, baseAsset.ToUpperInvariant(), quoteAsset.ToUpperInvariant());
}

static decimal ParseDecimal(string? value, string name)
{
    if (!decimal.TryParse(value, out var result))
        throw new ArgumentException($"Invalid {name} value `{value}`.");

    return result;
}

static SharedOrderSide ParseSide(string? value)
{
    return value?.ToLowerInvariant() switch
    {
        "buy" => SharedOrderSide.Buy,
        "sell" => SharedOrderSide.Sell,
        _ => throw new ArgumentException("Side should be `buy` or `sell`.")
    };
}

static void PrintHelp()
{
    Console.WriteLine("CryptoExchange.Net SharedApis console example");
    Console.WriteLine();
    Console.WriteLine("Public market data:");
    Console.WriteLine("  prices [base] [quote]                 e.g. prices BTC USDT");
    Console.WriteLine("  stream [exchange] [base] [quote]      e.g. stream binance ETH USDT");
    Console.WriteLine("  orderbook [exchange] [base] [quote]   e.g. orderbook bybit BTC USDT");
    Console.WriteLine();
    Console.WriteLine("Private endpoints, credentials required:");
    Console.WriteLine("  balances [exchange]");
    Console.WriteLine("  open-orders [exchange] [base] [quote]");
    Console.WriteLine("  place-limit [exchange] [base] [quote] [buy|sell] [quantity] [price] --live");
    Console.WriteLine("  cancel-order [exchange] [base] [quote] [order-id]");
    Console.WriteLine();
    Console.WriteLine("Other:");
    Console.WriteLine("  help");
    Console.WriteLine("  exit");
    Console.WriteLine();
}

internal record ExchangeClients(
    string Name,
    ISpotTickerRestClient TickerRest,
    IOrderBookRestClient OrderBookRest,
    IBalanceRestClient BalanceRest,
    ISpotOrderRestClient OrderRest,
    ITickerSocketClient TickerSocket);
