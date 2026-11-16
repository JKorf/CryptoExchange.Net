# Consumer examples

The examples below use Binance.Net to supply concrete clients. Install `Binance.Net`; it brings CryptoExchange.Net transitively. Each C# block is independent and includes its imports. Account-changing calls are inside functions that are defined but never invoked by the examples. Configure the intended account and inspect capability support before calling them; see [operational considerations](safety.md).

## Public setup and results

```csharp
using Binance.Net.Clients;
using CryptoExchange.Net.SharedApis;

using var client = new BinanceRestClient();
IGetTickerRest ticker = client.SpotApi.SharedApi;
var symbol = new SharedSymbol(TradingMode.Spot, "BTC", "USDT");
var result = await ticker.GetTickerAsync(new GetTickerRequest(symbol));
if (!result.Success)
    Console.WriteLine($"{ticker.Exchange}: {result.Error}");
else
    Console.WriteLine($"{result.Data.Symbol}: {result.Data.LastPrice}");
```

## Authentication and account reads

This credential type belongs to Binance.Net. Other exchange packages can require additional credentials, passphrases, signers, or account identifiers.

```csharp
using Binance.Net;
using Binance.Net.Clients;
using CryptoExchange.Net.SharedApis;

var key = Environment.GetEnvironmentVariable("BINANCE_API_KEY")
    ?? throw new InvalidOperationException("Missing API key");
var secret = Environment.GetEnvironmentVariable("BINANCE_API_SECRET")
    ?? throw new InvalidOperationException("Missing API secret");
using var client = new BinanceRestClient(options =>
    options.ApiCredentials = new BinanceCredentials(key, secret));
IGetBalancesRest balances = client.SpotApi.SharedApi;
var result = await balances.GetBalancesAsync(new GetBalancesRequest(TradingMode.Spot));
if (!result.Success)
    Console.WriteLine(result.Error);
else
    Console.WriteLine($"Received balances from {balances.Exchange}");
```

## Explicit trading function

The caller supplies the configured capability, validated symbol, quantity in base units, and price. This example only defines the function; it does not submit an order.

```csharp
using CryptoExchange.Net.SharedApis;

static async Task SubmitLimitOrderAsync(
    IPlaceSpotOrderRest orders, SharedSymbol symbol,
    decimal baseQuantity, decimal limitPrice, CancellationToken ct)
{
    var request = new PlaceSpotOrderRequest(
        symbol, SharedOrderSide.Buy, SharedOrderType.Limit,
        SharedQuantity.Base(baseQuantity), price: limitPrice);
    var result = await orders.PlaceSpotOrderAsync(request, ct);
    if (!result.Success)
        throw new InvalidOperationException($"{orders.Exchange}: {result.Error}");
    Console.WriteLine(result.Data);
}
```

## Runtime capability lookup

Inject or pass the actual exchange aggregate. This function accepts the common aggregate contract, so no concrete property name is assumed.

```csharp
using CryptoExchange.Net.SharedApis;

static async Task PrintTickerAsync(ISharedApiClientBase aggregate, CancellationToken ct)
{
    var match = aggregate.GetCapability(
        SharedCapabilities.Tickers.GetTicker.Rest, TradingMode.Spot);
    if (match is null)
        return;
    Console.WriteLine($"Authentication required: {match.Options.NeedsAuthentication}");
    var symbol = new SharedSymbol(TradingMode.Spot, "BTC", "USDT");
    var result = await match.Capability.GetTickerAsync(new GetTickerRequest(symbol), ct);
    if (!result.Success)
        Console.WriteLine($"{aggregate.Exchange}: {result.Error}");
    else
        Console.WriteLine(result.Data.LastPrice);
}
```

For transport-independent commands use the base descriptor; choose `.Rest` or `.Socket` when their result semantics matter. CryptoClients.Net adds cross-exchange selection through `IExchangeSharedApiClient`: `GetCapability` selects one match, `GetCapabilities` selects preferred matches per exchange, and `GetImplementations` exposes all eligible implementations. That interface is supplied by the bundle, not this package. Consult its installed interfaces for overloads and registration.

## Subscription lifecycle

```csharp
using Binance.Net.Clients;
using CryptoExchange.Net.SharedApis;

using var client = new BinanceSocketClient();
ISubscribeTickerSocket ticker = client.SpotApi.SharedApi;
var symbol = new SharedSymbol(TradingMode.Spot, "BTC", "USDT");
var result = await ticker.SubscribeToTickerUpdatesAsync(
    new SubscribeTickerRequest(symbol), update => Console.WriteLine(update.Data.LastPrice));
if (!result.Success)
{
    Console.WriteLine(result.Error);
    return;
}
try
{
    await Task.Delay(TimeSpan.FromSeconds(30));
}
finally
{
    await result.Data.CloseAsync();
}
```

A shared capability does not transfer ownership of its underlying socket client. In a multi-subscription setup, retain each successful handle as it arrives and clean up in `finally`, including when later setup fails.

## Dependency injection

Register one intended capability explicitly when a service needs a single implementation. Concrete packages also provide their own `Add[Exchange]` extensions; inspect their registrations when multiple surfaces or exchanges are involved. A plain single-service resolution cannot express which exchange you intended.

```csharp
using Binance.Net.Clients;
using CryptoExchange.Net.SharedApis;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddSingleton<BinanceRestClient>(_ => new BinanceRestClient());
services.AddSingleton<IGetTickerRest>(provider =>
    provider.GetRequiredService<BinanceRestClient>().SpotApi.SharedApi);
using var provider = services.BuildServiceProvider();
var ticker = provider.GetRequiredService<IGetTickerRest>();
var result = await ticker.GetTickerAsync(
    new GetTickerRequest(new SharedSymbol(TradingMode.Spot, "BTC", "USDT")));
if (!result.Success)
    Console.WriteLine(result.Error);
else
    Console.WriteLine(result.Data.LastPrice);
```

## Pagination

Keep the request and continuation tied to the same API, account, environment, and operation. `ExchangeHelpers.ExecutePages` is an alternative async page iterator; process errors per page. Inspect pagination options before selecting time ranges or limits.

```csharp
using CryptoExchange.Net.SharedApis;

static async Task ReadClosedOrdersAsync(
    IGetClosedSpotOrdersRest orders, SharedSymbol symbol, CancellationToken ct)
{
    var request = new GetClosedOrdersRequest(symbol, limit: 100);
    var page = await orders.GetClosedSpotOrdersAsync(request, ct: ct);
    while (true)
    {
        if (!page.Success)
        {
            Console.WriteLine(page.Error);
            return;
        }
        Console.WriteLine($"Received a page from {orders.Exchange}");
        if (page.NextPageRequest is null)
            return;
        page = await orders.GetClosedSpotOrdersAsync(
            request, nextPageToken: page.NextPageRequest, ct: ct);
    }
}
```

## Rate-limit admission

The callback scope applies to requests started on the same async flow; it does not reorder queued requests. Coalesced public GETs can reuse an in-flight request without another rate-limit slot.

```csharp
using CryptoExchange.Net;
using CryptoExchange.Net.RateLimiting;
using CryptoExchange.Net.SharedApis;

static async Task ReadWithinBudgetAsync(IGetTickerRest ticker, CancellationToken ct)
{
    var request = new GetTickerRequest(new SharedSymbol(TradingMode.Spot, "BTC", "USDT"));
    var result = await ticker.WithRateLimitAdmissionAsync(
        RateLimitAdmission.WithMaxUtilizationRatio(0.8),
        client => client.GetTickerAsync(request, ct));
    if (!result.Success)
        Console.WriteLine(result.Error);
    else
        Console.WriteLine(result.Data.LastPrice);
}
```

Checkout-only complete examples: [multi-exchange console project](../../../../Examples/SharedClients/SharedClients.csproj) and [standalone programs and setup](../../../../Examples/ai-friendly/README.md). These examples contact exchanges only when run, not when compiled.
