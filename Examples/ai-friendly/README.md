# AI-Friendly Examples

Cross-exchange examples using `CryptoExchange.Net.SharedApis`. Each file is an independent top-level console program using Shared API V2.

## Files

| File | What it shows |
|---|---|
| [01-shared-clients-quickstart.cs](01-shared-clients-quickstart.cs) | Same code calling Binance and OKX via SharedApis |
| [02-multi-exchange-tickers.cs](02-multi-exchange-tickers.cs) | Aggregating ticker data across N exchanges concurrently |
| [03-cross-exchange-arbitrage-skeleton.cs](03-cross-exchange-arbitrage-skeleton.cs) | Pattern for building a price difference scanner |

## Running

```bash
dotnet new console -n MyMultiExchangeApp
cd MyMultiExchangeApp

# Add the exchange libraries you want
dotnet add package Binance.Net
dotnet add package JK.OKX.Net
dotnet add package Bybit.Net

# Copy ONE example file into Program.cs, then compile without contacting exchanges
dotnet build

# Optional: run the public market-data example
dotnet run
```

These are public market data examples â€” no API keys needed.

The subscription example listens for 30 seconds and closes all successful subscriptions before disposing its clients. The price comparison is one read-only pass; its gross spread is not a profitability estimate.

See the [consumer skill](../../.claude/skills/cryptoexchange-net/SKILL.md) and the [complete Shared API project](../SharedClients/SharedClients.csproj).
