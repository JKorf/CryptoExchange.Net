# Copilot Instructions for CryptoExchange.Net

This repository is **CryptoExchange.Net** — the base library powering 28+ cryptocurrency exchange wrappers in C#/.NET (Binance.Net, Bybit.Net, OKX.Net, Kraken.Net, Coinbase.Net, etc.).

When generating code in this ecosystem, follow these conventions:

## You don't install CryptoExchange.Net directly

Install the exchange-specific library you need (`Binance.Net`, `JK.OKX.Net`, `Bybit.Net`, ...) or `CryptoClients.Net` for the bundle. CryptoExchange.Net is pulled in as a dependency.

## Multi-exchange code uses SharedApis

For code that must work against multiple exchanges, use `CryptoExchange.Net.SharedApis` interfaces accessed via `.SharedClient` properties on each exchange's API surface:

```csharp
using CryptoExchange.Net.SharedApis;

ISpotTickerRestClient binance = new BinanceRestClient().SpotApi.SharedClient;
ISpotTickerRestClient okx     = new OKXRestClient().UnifiedApi.SharedClient;

var symbol = new SharedSymbol(TradingMode.Spot, "BTC", "USDT");
var ticker = await binance.GetSpotTickerAsync(new GetTickerRequest(symbol));
```

Same code works on every exchange that implements the interface. Use `Task.WhenAll` for concurrent multi-exchange calls.

## Single-exchange code uses the exchange's own client

For Binance-only code, use `BinanceRestClient` directly (see Binance.Net repo `AGENTS.md`). SharedApis is for portability — use it when you need that.

## Result pattern

Every method returns `WebCallResult<T>` or `CallResult<T>`. Check `.Success` before `.Data`. `.Error` has structured info. `.Exchange` on shared clients identifies which exchange responded.

## Available shared interfaces

REST: tickers, symbols, orderbook, klines, trades, orders (spot/futures, trigger, TP-SL), balances, positions, fees, deposits/withdrawals, transfers.
WebSocket: tickers, book tickers, orderbook, trades, klines, user data.

Each exchange library implements a subset. Check exchange docs for support matrix.

## Avoid

- Installing `CryptoExchange.Net` alone and trying to call exchange APIs (need exchange-specific packages)
- Mixing exchange-native models in cross-exchange code (use Shared* types)
- Synchronous `.Result` / `.Wait()` (use `await`)
- Instantiating clients per-request (use DI, reuse instances)
- Sequential per-exchange calls when parallel is fine (`Task.WhenAll`)

## Reference

For detailed patterns see `AGENTS.md` and `llms.txt` in repo root, `examples/ai-friendly/` for compilable examples.
