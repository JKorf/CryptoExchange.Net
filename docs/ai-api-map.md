# CryptoExchange.Net Shared API V2 map

This is a compact map for choosing Shared API V2 types. All listed types are in `CryptoExchange.Net.SharedApis`. Exchange libraries implement subsets; the map describes available abstractions, not guaranteed exchange support.

## Entry points

| Need | Use |
| --- | --- |
| Known exchange and API surface | Its typed `.SharedApi` property |
| Runtime selection within one exchange | Exchange-wide `I[Exchange]SharedApiClient` |
| Strongly typed runtime lookup key | `SharedCapabilities` |
| One preferred match | `GetCapability(...)` |
| Every matching surface/transport in one exchange | `GetCapabilities(...)` |
| V1 compatibility during migration | `.SharedClient` |

Prefer a direct typed `.SharedApi` when the API surface is known. Dynamic lookup returns `SharedCapabilityResolution<T>?`; handle `null`.

```csharp
IGetTickerRest ticker = restClient.SpotApi.SharedApi;
var result = await ticker.GetTickerAsync(request);
```

```csharp
var match = sharedClient.GetCapability(
    SharedCapabilities.Tickers.GetTicker.Rest,
    TradingMode.Spot);
```

## Interface suffixes and results

| Interface shape | Transport | Typical result |
| --- | --- | --- |
| Operation with no suffix, e.g. `IPlaceSpotOrder` | selected/preferred | `IExchangeCallResult<T>` |
| `...Rest` | REST | `HttpResult<T>` |
| socket command, e.g. `IPlaceSpotOrderSocket` | WebSocket | `QueryResult<T>` |
| subscription, e.g. `ISubscribeTickerSocket` | WebSocket | `WebSocketResult<UpdateSubscription>` |

When a `SharedCapabilities` entry supports multiple transports, use the base entry for preferred transport, `.Rest` for REST, or `.Socket` for socket. Exchange-wide preferred transport normally defaults to REST.

## Market data

| Operation | Capability interface | Lookup reference |
| --- | --- | --- |
| Get one ticker | `IGetTickerRest` | `SharedCapabilities.Tickers.GetTicker.Rest` |
| Get all tickers | `IGetAllTickersRest` | `SharedCapabilities.Tickers.GetAllTickers.Rest` |
| Subscribe to one ticker | `ISubscribeTickerSocket` | `SharedCapabilities.Tickers.SubscribeTicker` |
| Subscribe to all tickers | `ISubscribeAllTickersSocket` | `SharedCapabilities.Tickers.SubscribeAllTickers` |
| Subscribe to book ticker | `ISubscribeBookTickerSocket` | `SharedCapabilities.Tickers.SubscribeBookTicker` |
| Get order book | `IGetOrderBookRest` | `SharedCapabilities.OrderBooks.GetOrderBook.Rest` |
| Get book ticker | `IGetBookTickerRest` | `SharedCapabilities.OrderBooks.GetBookTicker.Rest` |
| Subscribe to order book | `ISubscribeOrderBookSocket` | `SharedCapabilities.OrderBooks.SubscribeOrderBook` |
| Subscribe to incremental book | `ISubscribeIncrementalOrderBookSocket` | `SharedCapabilities.OrderBooks.SubscribeIncrementalOrderBook` |
| Get klines | `IGetKlinesRest` | `SharedCapabilities.Klines.GetKlines.Rest` |
| Subscribe to klines | `ISubscribeKlinesSocket` | `SharedCapabilities.Klines.SubscribeKlines` |
| Get recent trades | `IGetRecentTradesRest` | `SharedCapabilities.Trades.GetRecentTrades.Rest` |
| Subscribe to trades | `ISubscribeTradesSocket` | `SharedCapabilities.Trades.SubscribeTrades` |
| Get spot symbols | `IGetSpotSymbolsRest` | `SharedCapabilities.Symbols.GetSpotSymbols.Rest` |
| Get futures symbols | `IGetFuturesSymbolsRest` | `SharedCapabilities.Symbols.GetFuturesSymbols.Rest` |

Ticker V2 methods are `GetTickerAsync` and `GetAllTickersAsync`; both use `SharedTicker` for spot and futures. Use separate mark-price, index-price, funding, or open-interest capabilities for derivatives-specific data.

## Spot orders

| Operation | Capability family | Lookup reference |
| --- | --- | --- |
| Place | `IPlaceSpotOrder`, `IPlaceSpotOrderRest`, `IPlaceSpotOrderSocket` | `SharedCapabilities.Orders.Spot.PlaceOrder[.Rest/.Socket]` |
| Edit | `IEditSpotOrder`, `IEditSpotOrderRest`, `IEditSpotOrderSocket` | `SharedCapabilities.Orders.Spot.EditOrder[.Rest/.Socket]` |
| Cancel | `ICancelSpotOrder`, `ICancelSpotOrderRest`, `ICancelSpotOrderSocket` | `SharedCapabilities.Orders.Spot.CancelOrder[.Rest/.Socket]` |
| Get one | `IGetSpotOrderRest` | `SharedCapabilities.Orders.Spot.GetOrder.Rest` |
| Get open | `IGetOpenSpotOrdersRest` | `SharedCapabilities.Orders.Spot.GetOpenOrders.Rest` |
| Get closed | `IGetClosedSpotOrdersRest` | `SharedCapabilities.Orders.Spot.GetClosedOrders.Rest` |
| Subscribe to updates | `ISubscribeSpotOrdersSocket` | `SharedCapabilities.Orders.Spot.SubscribeOrders` |

Client-order-id variants exist for edit, cancel, and get. Batch placement and cancel-all capabilities also exist; resolve them only when the application needs those operations and the exchange supports them.

## Futures orders

| Operation | Capability family | Lookup reference |
| --- | --- | --- |
| Place | `IPlaceFuturesOrder`, `IPlaceFuturesOrderRest`, `IPlaceFuturesOrderSocket` | `SharedCapabilities.Orders.Futures.PlaceOrder[.Rest/.Socket]` |
| Edit | `IEditFuturesOrder`, `IEditFuturesOrderRest`, `IEditFuturesOrderSocket` | `SharedCapabilities.Orders.Futures.EditOrder[.Rest/.Socket]` |
| Cancel | `ICancelFuturesOrder`, `ICancelFuturesOrderRest`, `ICancelFuturesOrderSocket` | `SharedCapabilities.Orders.Futures.CancelOrder[.Rest/.Socket]` |
| Get one | `IGetFuturesOrderRest` | `SharedCapabilities.Orders.Futures.GetOrder.Rest` |
| Get open | `IGetOpenFuturesOrdersRest` | `SharedCapabilities.Orders.Futures.GetOpenOrders.Rest` |
| Get closed | `IGetClosedFuturesOrdersRest` | `SharedCapabilities.Orders.Futures.GetClosedOrders.Rest` |
| Subscribe to updates | `ISubscribeFuturesOrdersSocket` | `SharedCapabilities.Orders.Futures.SubscribeOrders` |

Specify `TradingMode.PerpetualLinear`, `PerpetualInverse`, or the appropriate delivery mode when multiple futures APIs can match.

## Account and derivatives

Use the following capability families for common account workflows:

| Area | Common interfaces |
| --- | --- |
| Balances | `IGetBalancesRest`, `ISubscribeBalancesSocket` |
| Positions | `IGetPositionsRest`, `ISubscribePositionsSocket`, `IGetPositionHistoryRest` |
| User trades | `IGetSpotUserTradeHistoryRest`, `IGetFuturesUserTradeHistoryRest`, `ISubscribeUserTradesSocket` |
| Fees | `IGetFeesRest` |
| Deposits | `IGetDepositAddressesRest`, `IGetDepositHistoryRest` |
| Withdrawals | `IWithdrawRest`, `IGetWithdrawalHistoryRest` |
| Transfers | `ITransferRest`, `IGetTransferHistoryRest` |
| Leverage | `IGetLeverageRest`, `ISetLeverageRest`, `IGetLeverageTiersRest` |
| Funding | `IGetFundingInfoRest`, `IGetFundingRateHistoryRest`, `IGetUserFundingHistoryRest` |
| Mark/index data | `IGetMarkPriceRest`, `IGetIndexPriceRest` and their all-market/subscription variants |
| Open interest | `IGetOpenInterestRest` |

The `SharedCapabilities` catalog groups these under matching plural categories such as `Balances`, `Positions`, `Funding`, `Leverage`, `MarkPrices`, and `IndexPrices`.

## Parameter support map

Every successful runtime resolution includes `Options`:

| Property | Meaning |
| --- | --- |
| `RequestParameterRules` | Shared request fields that are required, optional, or unsupported |
| `ExchangeParameterRules` | Exchange-specific fields accepted through `ExchangeParameters` |
| `SupportedTradingModes` | Modes supported by this capability implementation |
| `NeedsAuthentication` | Whether credentials are required |

Capability presence is not proof that all properties on its request model are accepted. Inspect rules for dynamically generated requests.

## Common V1 to V2 mappings

| V1 call | V2 capability call |
| --- | --- |
| `ISpotTickerRestClient.GetSpotTickerAsync` | `IGetTickerRest.GetTickerAsync` |
| `ISpotTickerRestClient.GetSpotTickersAsync` | `IGetAllTickersRest.GetAllTickersAsync` |
| `ISpotOrderRestClient.PlaceSpotOrderAsync` | `IPlaceSpotOrderRest.PlaceSpotOrderAsync` |
| `ISpotOrderRestClient.CancelSpotOrderAsync` | `ICancelSpotOrderRest.CancelSpotOrderAsync` |
| `IFuturesOrderRestClient.PlaceFuturesOrderAsync` | `IPlaceFuturesOrderRest.PlaceFuturesOrderAsync` |
| `IFuturesOrderRestClient.CancelFuturesOrderAsync` | `ICancelFuturesOrderRest.CancelFuturesOrderAsync` |
| `IFuturesOrderRestClient.GetPositionsAsync` | `IGetPositionsRest.GetPositionsAsync` |

V1 uses `.SharedClient`; V2 uses `.SharedApi`. Migrate operation by operation.

## Selection checklist

1. Use the exchange-native API if portability is unnecessary.
2. For portable code, identify the single operation capability required.
3. Use the typed `.SharedApi` directly when the surface is known.
4. Otherwise resolve through the exchange-wide shared client and handle `null`.
5. Specify trading mode and transport when ambiguity matters.
6. Inspect parameter rules for dynamic requests.
7. Check `Success` before using `Data`.

For migration edge cases and CryptoClients.Net cross-exchange lookup, see `SHARED_API_V2_MIGRATION.md`.
