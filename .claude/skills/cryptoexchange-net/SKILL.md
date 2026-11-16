---
name: cryptoexchange-net
description: Build exchange-agnostic C#/.NET integrations using CryptoExchange.Net Shared API V2 capabilities, runtime discovery, shared symbols and quantities, REST and WebSocket results, and multi-exchange workflows. Use when consuming portable contracts across exchange packages or implementing their shared capabilities and infrastructure.
---

# CryptoExchange.Net

CryptoExchange.Net supplies contracts and infrastructure; it has no concrete exchange connection. For infrastructure development or direct use of its helpers:

```shell
dotnet add package CryptoExchange.Net
```

For exchange access, install the concrete exchange package instead, for example:

```shell
dotnet add package Binance.Net
```

Use multiple concrete packages for a fixed set of exchanges, or `CryptoClients.Net` for a bundle. No additional skill is required.

## Choose the API

Use native exchange clients for exchange-specific workflows. For portable code on a known surface, assign its typed `.SharedApi` to a narrow operation interface such as `IGetTickerRest`. Use Shared API V2 for new code; `.SharedClient` and broad V1 interfaces are compatibility APIs.

For runtime selection, use the exchange-wide shared client and `SharedCapabilities` references. A missing resolution is `null`; support depends on operation, market, transport, and parameter rules. Inspect capability options before constructing dynamic requests. CryptoClients.Net's cross-exchange aggregate belongs to that separate package.

Credentials and environments are configured on the concrete exchange client. There is no universal exchange credential or test environment configuration in this package. Use shared requests and models with shared capabilities, and native types with native APIs.

Check `Success` before accessing `Data`, and report `Error` with the exchange identity. Keep clients alive for their intended workload, retain successful subscriptions, and close them before disposing their owning socket client. Respect DI ownership when clients are container-managed.

## Read as needed

- [API surfaces](references/api-surfaces.md): operation routing, discovery, parameter support, symbol catalogs, quantity semantics, and package selection.
- [Usage](references/usage.md): compilable setup, authentication, account reads, explicit trading functions, subscriptions, DI, pagination, and rate-limit admission.
- [Operational considerations](references/safety.md): account-changing calls, partial multi-exchange outcomes, retry decisions, and state reconciliation.

Checkout-only: [repository development](../../../AGENTS.md) and [complete examples](../../../Examples/README.md). The references above contain the guidance needed when only this packaged skill is available.
