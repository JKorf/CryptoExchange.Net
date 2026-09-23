---
name: cryptoexchange-net
description: Use CryptoExchange.Net Shared API V2 abstractions when generating C#/.NET code for multiple cryptocurrency exchanges, including arbitrage, routing, portfolio aggregation, exchange-agnostic bots, comparison tools, and new exchange-library implementations.
---

# CryptoExchange.Net Skill

## When to use

CryptoExchange.Net is the base library behind exchange-specific libraries such as Binance.Net, Bybit.Net, OKX.Net, Kraken.Net, and Coinbase.Net. Do not install it alone to call an exchange.

Choose one of these approaches:

1. One exchange: install and use that exchange's library directly.
2. Multiple exchanges: install the required exchange libraries and use `CryptoExchange.Net.SharedApis` V2 capabilities.
3. All exchanges in one package: install `CryptoClients.Net` and use its combined clients and shared capability lookup.

For new cross-exchange code, use Shared API V2. V1 aggregate interfaces remain available through `.SharedClient` for incremental migration.

## Installation

```bash
dotnet add package Binance.Net
dotnet add package JK.OKX.Net
dotnet add package Bybit.Net
```

Or install the bundle:

```bash
dotnet add package CryptoClients.Net
```

## Core pattern: fine-grained capabilities

Each exchange API surface exposes a typed `.SharedApi` aggregate. Assign it to the capability for the single operation being used:

```csharp
using Binance.Net.Clients;
using OKX.Net.Clients;
using Bybit.Net.Clients;
using CryptoExchange.Net.SharedApis;

IGetTickerRest binance = new BinanceRestClient().SpotApi.SharedApi;
IGetTickerRest okx = new OKXRestClient().UnifiedApi.SharedApi;
IGetTickerRest bybit = new BybitRestClient().V5Api.SharedApi;

var symbol = new SharedSymbol(TradingMode.Spot, "BTC", "USDT");
var result = await binance.GetTickerAsync(new GetTickerRequest(symbol));

if (!result.Success)
    Console.WriteLine($"[{result.Exchange}] {result.Error}");
else
    Console.WriteLine($"[{result.Exchange}] {result.Data!.LastPrice}");
```

V2 interfaces describe operations, not broad feature groups. Examples include `IGetTickerRest`, `IGetOrderBookRest`, `IPlaceSpotOrderRest`, `ICancelFuturesOrderRest`, and `ISubscribeTradesSocket`. An exchange may implement one operation without implementing adjacent operations.

## Shared symbols

Use `SharedSymbol`; never pass exchange-native symbol strings to shared requests:

```csharp
var spot = new SharedSymbol(TradingMode.Spot, "BTC", "USDT");
var linearPerpetual = new SharedSymbol(
    TradingMode.PerpetualLinear,
    "BTC",
    "USDT");
```

Each exchange library converts the shared symbol to its native format. For unusual exchange asset names, configure asset aliases.

## Known surface versus runtime selection

Use the typed `.SharedApi` surface when the exchange and API are known. This is the simplest and most discoverable approach:

```csharp
IPlaceSpotOrderRest orders = restClient.SpotApi.SharedApi;
```

For runtime selection, inject the exchange-wide `I[Exchange]SharedApiClient` registered by `services.Add[Exchange](...)`. Resolve a capability with the strongly typed `SharedCapabilities` catalog:

```csharp
var match = sharedClient.GetCapability(
    SharedCapabilities.Orders.Futures.PlaceOrder.Rest,
    TradingMode.PerpetualLinear);

if (match is null)
    return; // This exchange/API/mode does not provide the operation.

var result = await match.Capability.PlaceFuturesOrderAsync(request);
```

`GetCapability` returns one `SharedCapabilityResolution<T>` containing `Capability` and `Options`, or `null` if no supported implementation matches. `GetCapabilities` returns every match on that exchange-wide client. Always specify `TradingMode` when multiple spot, linear, inverse, or delivery surfaces could match.

`SharedCapabilities` references identify interface types; they do not guarantee that an exchange implements the operation and do not send a request.

## Transport selection and result types

Some commands have three interfaces:

- Transport-agnostic, such as `IPlaceSpotOrder`, returns `IExchangeCallResult<T>`.
- REST-specific, such as `IPlaceSpotOrderRest`, returns `HttpResult<T>`.
- socket-specific, such as `IPlaceSpotOrderSocket`, returns `QueryResult<T>`.

Socket subscription capabilities such as `ISubscribeTickerSocket` return `WebSocketResult<UpdateSubscription>`.

Use `.Rest` or `.Socket` on a capability reference when transport matters. Without a transport filter, exchange-wide lookup uses `PreferredTransport`, configured through the exchange's `SharedApi.PreferredTransport` option and normally defaulting to REST.

Always check `.Success` before `.Data`. Use `.Error` for failures and the result or capability `.Exchange` value for multi-exchange logging.

## Inspect parameter support

The presence of a capability does not mean every shared request property is accepted by every exchange. Inspect its `CapabilityOptions`:

- `RequestParameterRules`: whether each shared request field is `Required`, `Optional`, or `NotSupported`.
- `ExchangeParameterRules`: required or optional exchange-specific values supplied through the request's `ExchangeParameters`.
- `SupportedTradingModes`: the trading modes supported by this implementation.

```csharp
var match = sharedClient.GetCapability(
    SharedCapabilities.Orders.Futures.PlaceOrder.Rest,
    TradingMode.PerpetualLinear);

var leverageRule = match?.Options.RequestParameterRules
    .FirstOrDefault(x => x.Name == nameof(PlaceFuturesOrderRequest.Leverage));
```

Do not infer support from the request model alone. Handle a `null` resolution and exchange-specific parameter rules before issuing dynamic trading calls.

## Common capabilities

- Market data: `IGetTickerRest`, `IGetAllTickersRest`, `IGetOrderBookRest`, `IGetKlinesRest`, `IGetRecentTradesRest`; ticker, trade, kline, and order-book socket subscriptions.
- Trading: fine-grained spot and futures place, edit, cancel, get, open-order, closed-order, and order-update capabilities.
- Account: balances, positions, user trades, fees, deposits, withdrawals, and transfers.
- Futures data: funding, open interest, leverage, mark price, and index price capabilities.

Check the typed exchange Shared API surface or use capability lookup instead of assuming universal support.

## Multi-exchange aggregation

Run independent exchange requests concurrently:

```csharp
var clients = new IGetTickerRest[] { binance, okx, bybit };
var symbol = new SharedSymbol(TradingMode.Spot, "BTC", "USDT");

var tasks = clients.Select(client =>
    client.GetTickerAsync(new GetTickerRequest(symbol)));
var results = await Task.WhenAll(tasks);

foreach (var result in results.Where(x => x.Success))
    Console.WriteLine($"{result.Exchange}: {result.Data!.LastPrice}");
```

Reuse clients through dependency injection; do not instantiate them per request.

## Dependency injection

Each exchange library provides `services.Add[Exchange](...)`. Registration includes its native clients, V1 shared interfaces, V2 operation capabilities, and exchange-wide shared client.

Inject a capability directly only when the container has one intended implementation:

```csharp
public sealed class TickerService(IGetTickerRest ticker)
{
    public Task<HttpResult<SharedTicker>> GetAsync(
        SharedSymbol symbol,
        CancellationToken ct = default)
        => ticker.GetTickerAsync(new GetTickerRequest(symbol), ct);
}
```

If multiple exchanges or multiple API surfaces register the same capability, inject the exchange-specific shared client and select its typed API property or call `GetCapability`. Plain single-service resolution does not express which implementation you want.

For the bundle, use `services.AddCryptoClients(...)`. CryptoClients.Net provides `IExchangeSharedApiClient` for capability lookup across exchanges.

## V1 migration

V1 `.SharedClient` facades and broad interfaces such as `ISpotTickerRestClient` remain available. Migrate one operation at a time:

```csharp
// V1
await restClient.SpotApi.SharedClient.GetSpotTickerAsync(request);

// V2
await restClient.SpotApi.SharedApi.GetTickerAsync(request);
```

Important semantic changes:

- V2 ticker methods are `GetTickerAsync` and `GetAllTickersAsync` and return `SharedTicker` for both spot and futures.
- WebSocket order updates use `SharedSpotOrderUpdate` and `SharedFuturesOrderUpdate`; REST order retrieval keeps the ordinary order models.
- `ICloseFullPosition` closes the complete position. Use an order capability for partial closes where supported.
- Transport-agnostic operations return `IExchangeCallResult<T>`; select the REST or socket interface when code needs a transport-specific result.

See `docs/SHARED_API_V2_MIGRATION.md` for detailed mappings.

## Common pitfalls

- Do not install `CryptoExchange.Net` alone and expect exchange endpoints.
- Do not use `.SharedClient` for new V2 code; use `.SharedApi` and fine-grained capabilities.
- Do not mix exchange-native request or response models into cross-exchange services.
- Do not assume capability presence or request-parameter support; resolve and inspect it.
- Do not rely on preferred transport when REST or socket semantics matter.
- Do not block with `.Result` or `.Wait()`; use async calls throughout.
- Do not query exchanges sequentially when requests are independent.

## Implementing a new exchange library

- Derive API clients from `RestApiClient` and `SocketApiClient`.
- Follow the exchange credentials and options patterns used by existing libraries.
- Implement the relevant fine-grained V2 capability interfaces on typed Shared API classes.
- Publish accurate `CapabilityOptions`, including supported trading modes and request/exchange parameter rules.
- Register REST/socket Shared APIs and the exchange-wide shared client with the library's DI extension.
- Retain V1 facades only where compatibility with existing consumers is required.

Use maintained exchange libraries such as Binance.Net and Bybit.Net as implementation references.

## Reference

- Source: https://github.com/JKorf/CryptoExchange.Net
- Documentation: https://cryptoexchange.jkorf.dev/
- Shared API migration: https://github.com/JKorf/CryptoExchange.Net/blob/master/docs/SHARED_API_V2_MIGRATION.md
- Bundle: https://github.com/JKorf/CryptoClients.Net
- Demo app: https://github.com/JKorf/CryptoManager.Net
- Discord: https://discord.gg/MSpeEtSY8t
