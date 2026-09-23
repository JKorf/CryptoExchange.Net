# Copilot Instructions for CryptoExchange.Net

CryptoExchange.Net is the base library behind exchange-specific C#/.NET clients such as Binance.Net, Bybit.Net, OKX.Net, Kraken.Net, and Coinbase.Net.

## Package choice

Do not install CryptoExchange.Net alone to call an exchange. Install the exchange-specific package, or use `CryptoClients.Net` for the full bundle. For single-exchange code, prefer that library's native client. For portable multi-exchange code, use `CryptoExchange.Net.SharedApis`.

## Generate Shared API V2 code

Use fine-grained V2 capabilities through `.SharedApi`:

```csharp
using CryptoExchange.Net.SharedApis;

IGetTickerRest binance = new BinanceRestClient().SpotApi.SharedApi;
IGetTickerRest okx = new OKXRestClient().UnifiedApi.SharedApi;

var symbol = new SharedSymbol(TradingMode.Spot, "BTC", "USDT");
var result = await binance.GetTickerAsync(new GetTickerRequest(symbol));
```

Choose the interface for the operation: for example, `IGetTickerRest`, `IGetOrderBookRest`, `IPlaceSpotOrderRest`, `ICancelFuturesOrderRest`, or `ISubscribeTradesSocket`. V1 broad interfaces remain on `.SharedClient` for migration, but new code should use `.SharedApi`.

Use `SharedSymbol`; do not hard-code exchange-native symbol formatting in shared code.

## Runtime capability selection

When the API surface is known, assign its typed `.SharedApi` directly. When support or the API surface is selected at runtime, use the exchange-wide `I[Exchange]SharedApiClient`:

```csharp
var match = sharedClient.GetCapability(
    SharedCapabilities.Orders.Futures.PlaceOrder.Rest,
    TradingMode.PerpetualLinear);

if (match is null)
    return;

var result = await match.Capability.PlaceFuturesOrderAsync(request);
```

`GetCapability` returns one preferred `SharedCapabilityResolution<T>` or `null`; `GetCapabilities` returns all matching implementations. Include `TradingMode` when an exchange can expose multiple futures surfaces. `SharedCapabilities` entries are lookup references, not guarantees of support.

Before constructing dynamic requests, inspect `match.Options.RequestParameterRules`, `ExchangeParameterRules`, and `SupportedTradingModes`. A capability may exist while a particular request field is unsupported.

## Results and transports

- Transport-agnostic operation: `IExchangeCallResult<T>`
- REST capability: `HttpResult<T>`
- WebSocket command capability: `QueryResult<T>`
- WebSocket subscription capability: `WebSocketResult<UpdateSubscription>`

Select `.Rest` or `.Socket` when transport-specific behavior matters. Otherwise exchange-wide selection uses `PreferredTransport`, normally REST. Always check `.Success` before `.Data`; use `.Error` and `.Exchange` for diagnostics.

## Current V2 semantics

- Ticker operations are `GetTickerAsync` and `GetAllTickersAsync`, returning `SharedTicker` for both spot and futures.
- Socket order streams use `SharedSpotOrderUpdate` and `SharedFuturesOrderUpdate`.
- `ICloseFullPosition` closes a complete position, not a partial quantity.
- Exchange support varies by operation, transport, trading mode, and request parameter.

## Engineering conventions

- Reuse clients through dependency injection.
- Use `await`; never use `.Result` or `.Wait()`.
- Use `Task.WhenAll` for independent requests across exchanges.
- Keep exchange-native models out of portable services.
- Do not infer feature support from a broad interface or request model.

See `AGENTS.md`, `docs/ai-api-map.md`, and `docs/SHARED_API_V2_MIGRATION.md` for expanded guidance.
