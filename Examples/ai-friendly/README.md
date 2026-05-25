# AI-Friendly Examples

Cross-exchange examples using `CryptoExchange.Net.SharedApis`. These examples are optimized for AI coding assistants and quick onboarding.

## Files

| File | What it shows |
|---|---|
| `01-shared-clients-quickstart.cs` | Same code calling Binance and OKX via SharedApis |
| `02-multi-exchange-tickers.cs` | Aggregating ticker data across N exchanges concurrently |
| `03-cross-exchange-arbitrage-skeleton.cs` | Pattern for building a price difference scanner |

## Running

```bash
dotnet new console -n MyMultiExchangeApp
cd MyMultiExchangeApp

# Add the exchange libraries you want
dotnet add package Binance.Net
dotnet add package JK.OKX.Net
dotnet add package Bybit.Net

# Copy example file content into Program.cs and run
dotnet run
```

These are public market data examples — no API keys needed.
