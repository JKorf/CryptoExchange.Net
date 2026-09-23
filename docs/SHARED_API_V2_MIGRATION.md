# Migrating from Shared API V1 to V2

Shared API V2 introduces fine-grained capability interfaces. You can migrate one operation at a time: exchange REST and socket clients still expose their V1 `SharedClient` facades, while their `SharedApi` properties expose V2 capabilities. You do not need to replace the exchange clients or migrate every call in one release.

This guide describes the current source API. Rebuild your application after updating CryptoExchange.Net and the exchange libraries together; retaining V1 usage does not guarantee binary compatibility with older assemblies.

The snippets are illustrative. Configure credentials and the appropriate environment before trying a trading call. In the order examples, `request` means your populated `PlaceFuturesOrderRequest`.

## The V2 pieces used in this guide

V2 has two ways to reach a capability:

- **Known API surface:** `restClient.SpotApi.SharedApi` is the V2 aggregate for that specific REST API. Its type lists the capabilities that API implements, so use its methods or cast it to a capability interface when the exchange and API surface are already known. The adjacent `SharedClient` property is the V1 facade and remains available during migration.
- **Exchange-wide selection:** `I[Exchange]SharedApiClient` combines an exchange's full Shared API REST and socket V2 surface. `GetCapability` and `GetCapabilities` are methods on this exchange-wide client used for dynamically resolving capabilities. 

For the exchange-wide examples below, `sharedClient` is obtained through DI like this (taking Binance as an example):

```csharp
using Binance.Net.Interfaces.Clients;
using CryptoExchange.Net.SharedApis;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddBinance(); // Configure credentials and environment in the options for real requests.
using var provider = services.BuildServiceProvider();
IBinanceSharedApiClient sharedClient = provider.GetRequiredService<IBinanceSharedApiClient>();
```

`SharedCapabilities` is a catalog of **strongly typed lookup references**, not a list of endpoints guaranteed to exist on Binance. For example, `SharedCapabilities.Orders.Futures.PlaceOrder` identifies the transport-agnostic `IPlaceFuturesOrder` capability; its `.Rest` and `.Socket` references select `IPlaceFuturesOrderRest` and `IPlaceFuturesOrderSocket`. The reference tells `GetCapability` which interface to look for. Lookup checks this client's implemented and supported capabilities, and returns `null` if none match; it does not send an exchange request.

```csharp
var placementCapabilityResolution = sharedClient.GetCapability(
    SharedCapabilities.Orders.Futures.PlaceOrder.Rest,
    TradingMode.PerpetualLinear);

if (placementCapabilityResolution is not null)
{
    IPlaceFuturesOrderRest api = placement.Capability;
    // placement.Options describes parameter and trading-mode support for this implementation.
}
```

Use a typed `...Rest`/`...Socket` property or direct capability injection when you already know the exchange and API surface. Use `GetCapability` when that choice is made at runtime or support may vary; supply a trading mode when more than one futures API may match. It returns one resolution containing `Capability` and `Options`. Use `GetCapabilities` to inspect every matching implementation. For transport-agnostic lookup, one result is selected according to `PreferredTransport` (`BinanceOptions.SharedApi.PreferredTransport`, which defaults to REST); a `.Rest` or `.Socket` reference removes that transport ambiguity but may still match more than one API surface.

## 1. Keep existing calls running, then choose an operation

For example, the V1 `ISpotOrderRestClient` combines placement, cancellation, retrieval, and user trades. In V2, request the specific operation you use:

| V1 usage | V2 capability |
| --- | --- |
| `ISpotOrderRestClient.PlaceSpotOrderAsync` | `IPlaceSpotOrderRest.PlaceSpotOrderAsync` |
| `ISpotOrderRestClient.CancelSpotOrderAsync` | `ICancelSpotOrderRest.CancelSpotOrderAsync` |
| `ISpotTickerRestClient.GetSpotTickerAsync` | `IGetTickerRest.GetTickerAsync` |
| `ISpotTickerRestClient.GetSpotTickersAsync` | `IGetAllTickersRest.GetAllTickersAsync` |
| `IFuturesOrderRestClient.PlaceFuturesOrderAsync` | `IPlaceFuturesOrderRest.PlaceFuturesOrderAsync` |
| `IFuturesOrderRestClient.CancelFuturesOrderAsync` | `ICancelFuturesOrderRest.CancelFuturesOrderAsync` |
| `IFuturesOrderRestClient.GetPositionsAsync` | `IGetPositionsRest.GetPositionsAsync` |

All of these types are in `CryptoExchange.Net.SharedApis`. The exact V2 interfaces exposed by an exchange are listed on its typed `...SharedApi` interface. An exchange can implement some capabilities without implementing every operation from the former V1 aggregate.

For a known exchange and API surface, the typed property gives compile-time discovery:

```csharp
using Binance.Net.Clients;
using CryptoExchange.Net.SharedApis;

var restClient = new BinanceRestClient();
IPlaceSpotOrderRest placement = restClient.SpotApi.SharedApi;

var symbol = new SharedSymbol(TradingMode.Spot, "BTC", "USDT");
var request = new PlaceSpotOrderRequest(
    symbol,
    SharedOrderSide.Buy,
    SharedOrderType.Market,
    SharedQuantity.Base(0.01m));

var result = await placement.PlaceSpotOrderAsync(request);
if (!result.Success)
    Console.WriteLine(result.Error);
```

Keep using `restClient.SpotApi.SharedClient` for V1 calls while migrating other operations. `SharedClient` and `SharedApi` are different views of the same exchange API, not a requirement to run two separate exchange clients.

### Migrating a service using V1 Shared APIs

Previously, a service could inject `IFuturesOrderRestClient` for both futures order placement and cancellation. For example, with Kraken registered in DI:

```csharp
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;

var services = new ServiceCollection();
services.AddKraken(); // Configure credentials and environment in the options for real requests.

public class FuturesOrderServiceV1
{
    private readonly IFuturesOrderRestClient _orders;

    public FuturesOrderServiceV1(IFuturesOrderRestClient orders)
    {
        _orders = orders;
    }
	
    public Task<HttpResult<SharedId>> PlaceAsync(PlaceFuturesOrderRequest request, CancellationToken ct)
        => _orders.PlaceFuturesOrderAsync(request, ct);

    public Task<HttpResult<SharedId>> CancelAsync(CancelOrderRequest request, CancellationToken ct)
        => _orders.CancelFuturesOrderAsync(request, ct);
}
```

In V2, inject the two REST capabilities separately. The request types, method names, and REST result type remain the same for these two calls, so the service's callers need not change (using the same imports as above):

```csharp
public sealed class FuturesOrderServiceV2
{
    private readonly IPlaceFuturesOrderRest _placement;
    private readonly ICancelFuturesOrderRest _cancellation;

    public FuturesOrderServiceV2(
        IPlaceFuturesOrderRest placement,
        ICancelFuturesOrderRest cancellation)
    {
        _placement = placement;
        _cancellation = cancellation;
    }

    public Task<HttpResult<SharedId>> PlaceAsync(PlaceFuturesOrderRequest request, CancellationToken ct)
        => _placement.PlaceFuturesOrderAsync(request, ct);

    public Task<HttpResult<SharedId>> CancelAsync(CancelOrderRequest request, CancellationToken ct)
        => _cancellation.CancelFuturesOrderAsync(request, ct);
}
```

`services.AddKraken()` registers these V2 capability interfaces as well as the V1 interface; no additional registration is needed.

This direct injection pattern is clearest when the DI container has one intended implementation for each capability. An exchange with multiple API surfaces for the same operation (for example, Binance USD- and coin-margined futures order management), or an application registering multiple exchanges, may register several different interface implementations. Plain single-service resolution does not express which surface you want and could pair different surfaces. In that case, inject the exchange-specific shared aggregate. For example, if a service receives an `IBinanceSharedApiClient client`, both operations can be selected from its USD-futures REST API:

```csharp
IPlaceFuturesOrderRest placement = client.UsdFuturesRest;
ICancelFuturesOrderRest cancellation = client.UsdFuturesRest;
```

If the surface is selected dynamically instead, use `GetCapability` as shown below and handle its `null` result. Injecting `IEnumerable<IPlaceFuturesOrderRest>` is another option when you want to examine every DI registration yourself.

Moving ticker requesting from V1 to V2:

```csharp
var symbol = new SharedSymbol(TradingMode.Spot, "BTC", "USDT");

// V1 call, still available.
var oldTicker = await restClient.SpotApi.SharedClient.GetSpotTickerAsync(
    new GetTickerRequest(symbol));

// V2 call on the same REST client.
var newTicker = await restClient.SpotApi.SharedApi.GetTickerAsync(
    new GetTickerRequest(symbol));
```


## 2. Select a transport when it matters

Some actions have both a transport-agnostic interface and transport-specific interfaces. For example, `IPlaceSpotOrder` returns `IExchangeCallResult<SharedId>`; `IPlaceSpotOrderRest` returns `HttpResult<SharedId>` and `IPlaceSpotOrderSocket` returns `QueryResult<SharedId>`. Select a transport-specific interface if your code needs transport-specific result data or require a certain transport.

When dynamically resolving a transport-agnostic capability, the exchange shared client prefers its configured `PreferredTransport`. Specify a transport-specific reference or interface if that preference should not determine your call.

The same operation can be requested with or without a fixed transport:

```csharp
// One preferred transport. The capability method returns IExchangeCallResult<SharedId>.
var preferred = sharedClient.GetCapability(
    SharedCapabilities.Orders.Spot.PlaceOrder);

// REST only. The capability method returns HttpResult<SharedId>.
var rest = sharedClient.GetCapability(
    SharedCapabilities.Orders.Spot.PlaceOrder.Rest);

// Socket only. The capability method returns QueryResult<SharedId>.
var socket = sharedClient.GetCapability(
    SharedCapabilities.Orders.Spot.PlaceOrder.Socket);
```

## 3. Replace broad `Supported` checks with capability lookup

For runtime discovery, use an exchange's shared client and the `SharedCapabilities` catalog. A lookup returns `null` when the requested operation is not available for that API or trading mode.

```csharp
var resolution = sharedClient.GetCapability(
    SharedCapabilities.Orders.Futures.PlaceOrder.Rest,
    TradingMode.PerpetualLinear);

if (resolution is not null)
{
    var result = await resolution.Capability.PlaceFuturesOrderAsync(request);
    // Inspect result.Success and result.Error before using result.Data.
}
```

`GetCapability` selects one preferred implementation. `GetCapabilities` on an exchange shared client returns all matching implementations, which may include multiple API surfaces or transports. Passing a `TradingMode` is particularly important for exchanges with separate linear and inverse futures APIs.

For example, the Binance shared client can expose both USD- and coin-margined futures. Specify the mode when selecting one; omit it only when you intentionally want to inspect every match:

```csharp
var inverse = sharedClient.GetCapability(
    SharedCapabilities.Orders.Futures.PlaceOrder.Rest,
    TradingMode.PerpetualInverse);

var allOrderPlacements = sharedClient.GetCapabilities<IPlaceFuturesOrder>();
foreach (var match in allOrderPlacements)
{
    Console.WriteLine($"{match.Transport}: " +
        string.Join(", ", match.Options.SupportedTradingModes));
}
```

## 4. Inspect options for parameter support

The presence of a capability does not imply support for every field in its request model. Inspect the resolved `Options` or the capability's typed options property:

- `RequestParameterRules`: each shared request field's `Required`, `Optional`, or `NotSupported` status for this implementation.
- `ExchangeParameterRules`: required or optional exchange-specific parameters supplied through `ExchangeParameters`. Parameters absent from these rules are not advertised as supported.
- `SupportedTradingModes`: the modes supported by this capability, which can be narrower than those supported by its containing API.

For instance, check whether `Leverage` can be supplied when placing a Binance futures order. `NotSupported` here means you must use a separate leverage operation if the exchange provides one:

```csharp
var placement = sharedClient.GetCapability(
    SharedCapabilities.Orders.Futures.PlaceOrder.Rest,
    TradingMode.PerpetualLinear);

var leverageRule = placement?.Options.RequestParameterRules
    .FirstOrDefault(rule => rule.Name == nameof(PlaceFuturesOrderRequest.Leverage));

if (leverageRule?.Support == RequestParameterSupport.NotSupported)
    Console.WriteLine("Set leverage separately before placing the order.");

if (placement is not null)
{
    foreach (var parameter in placement.Options.ExchangeParameterRules)
        Console.WriteLine($"{parameter.Name}: {parameter.Requirement}");
}
```

## 5. Review changed request and result semantics

- The V2 ticker operations are `GetTickerAsync` and `GetAllTickersAsync`, returning `SharedTicker` rather than separate spot/futures ticker models. The V1 spot/futures ticker methods remain on their legacy facades. For bulk ticker calls, provide a trading mode where the capability requires one; consult its `RequestParameterRules` rather than assuming all exchanges use the same requirement.
- V2 socket order updates use `SharedSpotOrderUpdate` or `SharedFuturesOrderUpdate`. Code using update-only fields such as `LastTrade` should use the update model; ordinary order retrieval remains on `SharedSpotOrder` or `SharedFuturesOrder`.
- A transport-agnostic operation generally returns `IExchangeCallResult<T>`, which includes the exchange name. The spot and futures symbol catalog operations still return `ICallResult<T>`. Switch to a REST or socket interface if existing code expects `HttpResult<T>` or `QueryResult<T>`.
- V2 `ICloseFullPosition` closes the entire position. It is not a drop-in replacement for V1 `ClosePositionAsync` with `ClosePositionRequest.Quantity`; use an order operation for partial closes when supported by the exchange.
The full-close distinction is visible in the request shape: unlike V1 `ClosePositionRequest`, `CloseFullPositionRequest` has no quantity parameter. Check for the capability before using it because an exchange need not provide a full-close operation:
```csharp
var close = sharedClient.GetCapability(
    SharedCapabilities.Positions.CloseFullPosition.Rest,
    TradingMode.PerpetualLinear);

if (close is not null)
{
    var symbol = new SharedSymbol(TradingMode.PerpetualLinear, "BTC", "USDT");
    var result = await close.Capability.CloseFullPositionAsync(
        new CloseFullPositionRequest(symbol));
}
```

## 6. If using CryptoClients.Net

The existing `IExchangeRestClient` and `IExchangeSocketClient` remain available. CryptoClients.Net also registers `IExchangeSharedApiClient` for V2 cross-exchange capability lookup:

```csharp
var resolution = sharedApis.GetCapability(
    Exchange.Binance,
    SharedCapabilities.Orders.Futures.PlaceOrder.Rest,
    TradingMode.PerpetualLinear);
```

Use an exchange-specific property such as `sharedApis.Binance` when you want its typed `SpotRest`, `SpotSocket`, or futures API properties. For cross-exchange queries, `GetCapabilities(...)` returns **one preferred match per exchange**. Use `GetImplementations(...)` when you need every matching transport and API surface for each exchange. 

With DI, the lookup can be used without replacing existing REST/socket registrations:

```csharp
using CryptoClients.Net.Enums;
using CryptoClients.Net.Interfaces;
using CryptoExchange.Net.SharedApis;
using Microsoft.Extensions.DependencyInjection;

services.AddCryptoClients();

// Later, in a service receiving IExchangeSharedApiClient through DI:
var preferredPerExchange = sharedApis.GetCapabilities(
    SharedCapabilities.Orders.Futures.PlaceOrder,
    TradingMode.PerpetualLinear,
    exchanges: [Exchange.Binance]);

var everyBinanceImplementation = sharedApis.GetImplementations(
    SharedCapabilities.Orders.Futures.PlaceOrder,
    exchanges: [Exchange.Binance]);
```

`preferredPerExchange` contains at most one matching result for Binance; `everyBinanceImplementation` can contain both REST and socket implementations for USD and coin futures. If you know which API you need, the typed property remains the clearest choice:

```csharp
IPlaceFuturesOrderRest usdFutures = sharedApis.Binance.UsdFuturesRest;
IPlaceFuturesOrderSocket coinFuturesSocket = sharedApis.Binance.CoinFuturesSocket;
```

### Migrating cross-exchange async enumeration

V1 `IExchangeRestClient` offers operation-specific `...AsyncEnumerable` methods. For example, a service receiving that interface could stream spot ticker results from selected exchanges as their requests finish:

```csharp
using CryptoClients.Net.Enums;
using CryptoClients.Net.Interfaces;
using CryptoExchange.Net.SharedApis;
using System;
using System.Threading;

async Task PrintTickersV1Async(IExchangeRestClient client, SharedSymbol symbol, CancellationToken ct)
{
    await foreach (var result in client.GetSpotTickerAsyncEnumerable(
        new GetTickerRequest(symbol),
        [Exchange.Binance, Exchange.Bybit],
        ct))
    {
        Console.WriteLine($"{result.Exchange}: {result.Data?.LastPrice}");
    }
}
```

In V2, inject `IExchangeSharedApiClient` instead. Select one REST ticker capability per exchange, start the calls, then use `ParallelEnumerateAsync` to yield results in completion order:

```csharp
using CryptoClients.Net; // ParallelEnumerateAsync
using CryptoClients.Net.Enums;
using CryptoClients.Net.Interfaces;
using CryptoExchange.Net.SharedApis;
using System;
using System.Linq;
using System.Threading;

async Task PrintTickersV2Async(IExchangeSharedApiClient sharedApis, SharedSymbol symbol, CancellationToken ct)
{
    var capabilities = sharedApis.GetCapabilities(
        SharedCapabilities.Tickers.GetTicker.Rest,
        TradingMode.Spot,
        exchanges: [Exchange.Binance, Exchange.Bybit]);

    var tasks = capabilities.Select(match =>
        match.Capability.GetTickerAsync(new GetTickerRequest(symbol), ct));

    await foreach (var result in tasks.ParallelEnumerateAsync())
    {
        Console.WriteLine($"{result.Exchange}: {result.Data?.LastPrice}");
    }
}
```

The V2 results contain `SharedTicker`, rather than V1's `SharedSpotTicker`. `GetCapabilities` omits exchanges without a matching supported capability and returns one preferred match per exchange; use `GetImplementations` if you need every matching API surface. `ParallelEnumerateAsync` is a CryptoClients.Net extension on `IEnumerable<Task<T>>`, not a method on `IExchangeSharedApiClient`. Passing `ct` to each call cancels its underlying request; the enumeration helper itself has no cancellation-token parameter.

## Compatibility checklist

- Existing V1 `SharedClient` methods can be migrated incrementally, but some legacy option property types and concrete option names have changed. Code that explicitly names those types may require edits even when it keeps calling V1 methods.
- `EndpointOptions` and `EndpointName` are retained as obsolete compatibility members. Move discovery code to `CapabilityOptions` and `OperationName` when practical.
- Review code that assumes a broad interface implies every operation is supported.
- Test each exchange/trading-mode combination used by your application, especially bulk market-data requests, futures order placement, and socket order updates.
