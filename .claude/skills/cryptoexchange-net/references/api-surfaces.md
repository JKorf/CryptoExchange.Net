# API surfaces and routing

## Contracts and concrete implementations

CryptoExchange.Net defines `BaseRestClient`, `RestApiClient`, `BaseSocketClient`, and `SocketApiClient` infrastructure. Concrete exchange libraries supply their native endpoints and typed Shared API surfaces. For example, Binance has `SpotApi.SharedApi`, OKX has `UnifiedApi.SharedApi`, and Bybit has `V5Api.SharedApi`; these are not properties of this base library.

| Need | Entry point |
| --- | --- |
| Known exchange, market, and transport | Concrete API surface's typed `.SharedApi` |
| Runtime selection within one exchange | Its exchange-wide `I[Exchange]SharedApiClient`, implementing `ISharedApiClientBase` |
| Typed lookup key | `SharedCapabilities` |
| One preferred match | `GetCapability(...)`, returning a nullable `SharedCapabilityResolution<T>` |
| All matching implementations on one exchange | `GetCapabilities(...)` |
| Discovery without making an exchange request | `Discover(...)` and `SharedApis` on the exchange aggregate |
| Dynamic selection across exchanges | `IExchangeSharedApiClient` from the separate CryptoClients.Net package |

A resolution contains `Capability` and `Options`. Pass the trading mode when multiple markets can match. The aggregate's `PreferredTransport` selects among eligible transports; use a `.Rest` or `.Socket` descriptor to require one. Do not treat a descriptor as proof that an exchange implements it.

CryptoClients.Net's cross-exchange aggregate offers `GetClient`, `GetCapability`, `GetCapabilities` (preferred matches by exchange), and `GetImplementations` (all matches). These are bundle APIs, not members of CryptoExchange.Net's base client interface. Choose the concrete exchange and environment before executing an account operation.

## Transports and results

| Interface shape | Result |
| --- | --- |
| Transport-independent operation, e.g. `IPlaceSpotOrder` | `IExchangeCallResult<T>` |
| REST operation, e.g. `IPlaceSpotOrderRest` | `HttpResult<T>` |
| Socket command, e.g. `IPlaceSpotOrderSocket` | `QueryResult<T>` |
| Subscription, e.g. `ISubscribeTickerSocket` | `WebSocketResult<UpdateSubscription>` |

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


## Symbols and catalogs

Construct `SharedSymbol` from trading mode, base asset, and quote asset for portable formatting; supply delivery time for a dated contract. Its native-symbol constructor sets `SymbolName` and bypasses portable formatting, so use it only with a known exchange-specific symbol. Asset aliases and `SharedSymbol.UsdOrStable` support matching preferences; they do not guarantee a listed market.

Successful symbol retrieval populates the implementation's `SpotSymbolCatalog` or `FuturesSymbolCatalog`. Catalogs describe that API/environment snapshot, not every exchange. Use catalog filters and symbol availability (`Trading`) to select markets. Classification distinguishes crypto/stablecoin, traditional-finance equity/commodity, fiat, and unspecified assets; missing classification must not be guessed.

## Quantities

Requests use `SharedQuantity.Base(...)`, `.Quote(...)`, or `.Contracts(...)`. Inspect the operation's supported quantity types before choosing units. Responses use `SharedOrderQuantity` with nullable `QuantityInBaseAsset`, `QuantityInQuoteAsset`, and `QuantityInContracts`; an absent unit is not zero. `WithCalculatedQuantities(price, contractSize)` derives additional units and does not make them exchange-reported values. Order-book quantity units are described by `SharedOrderBook.QuantityType`.

## Options, pagination, and implementation boundaries

`RequestParameterRules` specify required, optional, or unsupported shared fields, including a default support policy. `ExchangeParameterRules` describe exchange-specific parameters. Read those rules along with `Description`, `RequestNotes`, `NeedsAuthentication`, `SupportedTradingModes`, multiple-symbol support, and operation-specific limits. A nullable request property does not imply support. Older required/optional parameter lists are compatibility views.

`SetDefaultExchangeParameter` and `ResetDefaultExchangeParameters` configure defaults on a shared API; per-request exchange parameters override defaults. Scope these settings to the intended client rather than changing a shared instance during concurrent calls.

Paginated results carry `NextPageRequest`. Pass it unchanged to the same operation with the original request and context. Options describe ascending/descending support, time-period filters, maximum limit, and maximum age; see [usage](usage.md#pagination). `ICloseFullPosition` means the complete position; a partial close requires an appropriate order operation where supported. Socket order streams use `SharedSpotOrderUpdate` and `SharedFuturesOrderUpdate`, distinct from REST order models.

Use native APIs when exact account, margin, position mode, conditional order, or response semantics cannot be represented by a shared capability. Implementers derive from the base clients, implement the relevant V2 interfaces, publish accurate capability options, and register their actual REST/socket surfaces and exchange aggregate. Do not infer a universal registration or authentication scheme from another exchange.

Checkout-only source references: [base client interfaces](../../../../CryptoExchange.Net/Interfaces/Clients), [V2 interfaces](../../../../CryptoExchange.Net/SharedApis/V2), [runtime descriptors](../../../../CryptoExchange.Net/SharedApis/CapabilityReferences), and [aggregate contracts](../../../../CryptoExchange.Net/SharedApis/SharedApiClientBase.cs).

## Ecosystem packages

| Repository | NuGet package |
| --- | --- |
| Aster.Net | `JKorf.Aster.Net` |
| Binance.Net | `Binance.Net` |
| BingX.Net | `JK.BingX.Net` |
| Bitfinex.Net | `Bitfinex.Net` |
| Bitget.Net | `JK.Bitget.Net` |
| BitMart.Net | `BitMart.Net` |
| BitMEX.Net | `JKorf.BitMEX.Net` |
| Bitstamp.Net | `Bitstamp.Net` |
| BloFin.Net | `BloFin.Net` |
| Bybit.Net | `Bybit.Net` |
| Coinbase.Net | `JKorf.Coinbase.Net` |
| CoinEx.Net | `CoinEx.Net` |
| CoinGecko.Net | `CoinGecko.Net` |
| CoinW.Net | `CoinW.Net` |
| CryptoClients.Net | `CryptoClients.Net` |
| CryptoCom.Net | `CryptoCom.Net` |
| CryptoExchange.Net | `CryptoExchange.Net` |
| DeepCoin.Net | `DeepCoin.Net` |
| GateIo.Net | `GateIo.Net` |
| HTX.Net | `JKorf.HTX.Net` |
| HyperLiquid.Net | `HyperLiquid.Net` |
| Kraken.Net | `KrakenExchange.Net` |
| Kucoin.Net | `Kucoin.Net` |
| LBank.Net | `LBank.Net` |
| Lighter.Net | `JKorf.Lighter.Net` |
| Mexc.Net | `JK.Mexc.Net` |
| OKX.Net | `JK.OKX.Net` |
| Pionex.Net | `Pionex.Net` |
| Polymarket.Net | `Polymarket.Net` |
| Tapbit.Net | `Tapbit.Net` |
| Toobit.Net | `Toobit.Net` |
| Upbit.Net | `JKorf.Upbit.Net` |
| Weex.Net | `Weex.Net` |
| WhiteBit.Net | `WhiteBit.Net` |
| XT.Net | `XT.Net` |
