---
name: cryptoexchange-net
description: Use CryptoExchange.Net abstractions when generating C#/.NET code that needs to work across MULTIPLE cryptocurrency exchanges (Binance + Bybit + OKX + Kraken + Coinbase + ...) — for arbitrage, best-execution routing, multi-exchange portfolio dashboards, exchange-agnostic trading bots, or comparison tools. Triggers on requests mentioning multi-exchange, cross-exchange, arbitrage, exchange-agnostic, or unified crypto API in C#. Also triggers when the user is implementing a new exchange library following the CryptoExchange.Net pattern.
---

# CryptoExchange.Net Skill

## When to use

CryptoExchange.Net is the **base library** powering 28+ exchange-specific libraries (Binance.Net, Bybit.Net, OKX.Net, Kraken.Net, Coinbase.Net, etc.). You don't install it directly — you install the exchange libraries, which depend on it.

**Three usage modes:**

1. **You target ONE exchange** → use that exchange's library directly (e.g., Binance.Net), see its CLAUDE.md.
2. **You target MULTIPLE exchanges** → install each library you need + use `CryptoExchange.Net.SharedApis` interfaces — write code once, runs against any exchange. **This is the main use case for this skill.**
3. **You want ALL exchanges in one package** → install `CryptoClients.Net`, get `ExchangeRestClient` and `ExchangeSocketClient` with everything bundled.

## Installation

For a multi-exchange project:

```bash
dotnet add package Binance.Net
dotnet add package JK.OKX.Net
dotnet add package Bybit.Net
# ... etc
```

Or the bundle:

```bash
dotnet add package CryptoClients.Net
```

## Core Pattern: Shared Interfaces

Every exchange library exposes `.SharedClient` properties on its API surfaces. These implement the same interfaces from `CryptoExchange.Net.SharedApis`.

```csharp
using Binance.Net.Clients;
using OKX.Net.Clients;
using Bybit.Net.Clients;
using CryptoExchange.Net.SharedApis;

// All three implement ISpotTickerRestClient
ISpotTickerRestClient binance = new BinanceRestClient().SpotApi.SharedClient;
ISpotTickerRestClient okx     = new OKXRestClient().UnifiedApi.SharedClient;
ISpotTickerRestClient bybit   = new BybitRestClient().V5Api.SharedClient;

// Single agnostic call — works against any of them
var symbol = new SharedSymbol(TradingMode.Spot, "BTC", "USDT");
var ticker = await binance.GetSpotTickerAsync(new GetTickerRequest(symbol));
// ticker.Data.LastPrice, ticker.Data.HighPrice, etc. — same model regardless of exchange
```

## Core Pattern: SharedSymbol

Different exchanges format symbols differently — Binance uses `BTCUSDT`, OKX uses `BTC-USDT`, others may have other formats. `SharedSymbol` normalizes this:

```csharp
var btcusdt = new SharedSymbol(TradingMode.Spot, "BTC", "USDT");
// Each exchange library translates SharedSymbol → its native format internally.

// For futures:
var btcusdtPerp = new SharedSymbol(TradingMode.PerpetualLinear, "BTC", "USDT");
```

For exchanges that use exotic asset names, see the AssetAliases configuration.

## Available Shared Interfaces

**REST:**

- Market data: `ISpotTickerRestClient`, `IBookTickerRestClient`, `ISpotSymbolRestClient`, `IFuturesSymbolRestClient`, `IOrderBookRestClient`, `IRecentTradeRestClient`, `IKlineRestClient`
- Orders: `ISpotOrderRestClient`, `IFuturesOrderRestClient`, `ISpotOrderClientIdRestClient`, `IFuturesOrderClientIdRestClient`, `ISpotTriggerOrderRestClient`, `IFuturesTriggerOrderRestClient`, `IFuturesTpSlRestClient`
- Account: `IBalanceRestClient`, `IPositionRestClient`, `IFeeRestClient`, `ITransferRestClient`, `IDepositRestClient`, `IWithdrawalRestClient`

**WebSocket:**

- `ITickerSocketClient`, `IBookTickerSocketClient`
- `IOrderBookSocketClient`, `ITradeSocketClient`, `IKlineSocketClient`
- `IUserTradeSocketClient`, `ISpotOrderSocketClient`, `IFuturesOrderSocketClient`, `IPositionSocketClient`, `IBalanceSocketClient`

Each exchange documents which interfaces it implements (some exchanges don't support every operation).

## Core Pattern: Result Handling

Same as exchange-specific libraries — `WebCallResult<T>` (REST) or `CallResult<T>` (WebSocket) with `.Success`, `.Data`, `.Error`. Always check `.Success` first.

```csharp
var result = await sharedClient.GetSpotTickerAsync(new GetTickerRequest(symbol));
if (!result.Success)
{
    Console.WriteLine($"[{sharedClient.Exchange}] Error: {result.Error}");
    return;
}
Console.WriteLine($"[{sharedClient.Exchange}] {result.Data.LastPrice}");
```

`.Exchange` property on every shared client tells you which exchange you're talking to — useful for logging.

## Core Pattern: Multi-Exchange Aggregation

```csharp
var clients = new ISpotTickerRestClient[]
{
    new BinanceRestClient().SpotApi.SharedClient,
    new OKXRestClient().UnifiedApi.SharedClient,
    new BybitRestClient().V5Api.SharedClient,
};

var symbol = new SharedSymbol(TradingMode.Spot, "BTC", "USDT");

// Fetch concurrently from all exchanges
var tasks = clients.Select(c => c.GetSpotTickerAsync(new GetTickerRequest(symbol))).ToArray();
var results = await Task.WhenAll(tasks);

for (int i = 0; i < clients.Length; i++)
{
    if (results[i].Success)
        Console.WriteLine($"{clients[i].Exchange}: {results[i].Data!.LastPrice}");
}
```

## Per-Exchange Setup

Each exchange library has its own credentials class and options. See each library's CLAUDE.md for specifics. The pattern is consistent: `XxxRestClient(options => { options.ApiCredentials = new XxxCredentials(...); })`.

## Dependency Injection

Each exchange library has its own `services.AddXxx(...)` extension. They all share the same option-builder pattern. Register only the ones you use:

```csharp
services.AddBinance(restOpts => { /*...*/ }, socketOpts => { /*...*/ });
services.AddOKX(restOpts => { /*...*/ }, socketOpts => { /*...*/ });
// Inject IBinanceRestClient, IOKXRestClient, etc.
```

For one-package access: `services.AddCryptoClients(...)` from `CryptoClients.Net`.

## Common Pitfalls — AVOID

- **Do NOT install `CryptoExchange.Net` and try to call exchange APIs directly** — it's a base abstraction; you need an exchange library.
- **Do NOT try to use one exchange's models with another's client** — use the SharedApis types (`SharedSymbol`, `SharedSpotTicker`, `SharedSpotOrder`, etc.) for cross-exchange code.
- **Do NOT block on async operations** — use `await` throughout. `Task.WhenAll` for parallelism across exchanges.
- **Do NOT assume every exchange supports every operation** — check exchange docs or the library's implementation. Operations may return errors like "not supported on this exchange".
- **Do NOT instantiate clients per-request** — reuse via DI.
- **Do NOT iterate exchanges sequentially when concurrency is fine** — use `Task.WhenAll` for ~Nx speedup.

## Implementing a New Exchange Library

If you're building a NEW exchange wrapper following the CryptoExchange.Net pattern (rare but valuable):

- Inherit from `RestApiClient` and `SocketApiClient` base classes
- Define your own `XxxCredentials` extending `ApiCredentials` (or use `ApiCredentials` directly)
- Implement `AuthenticationProvider` for the exchange's signing scheme
- Implement the relevant `Shared*` interfaces on your API client classes for cross-exchange support
- Follow the same `XxxRestOptions` / `XxxSocketOptions` pattern

See existing libraries (Binance.Net, Bybit.Net) as reference implementations.

## Reference

- Source: https://github.com/JKorf/CryptoExchange.Net
- Documentation: https://cryptoexchange.jkorf.dev/
- SharedApis docs: https://cryptoexchange.jkorf.dev/CryptoExchange.Net/idocs_shared.html
- Bundle (all exchanges): https://github.com/JKorf/CryptoClients.Net
- Demo app: https://github.com/JKorf/CryptoManager.Net
- Discord: https://discord.gg/MSpeEtSY8t
