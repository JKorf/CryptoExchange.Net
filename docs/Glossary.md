---
title: Glossary
nav_order: 10
---
## Terms and definitions

|Definition|Synonyms|Meaning|
|----------|--------|-------|
|Symbol|Market|An asset pair, for example `BTC-ETH`|
|Asset|Currency, Coin|A coin for which you can hold balance and which makes up Symbols. For example both `BTC`, `ETH` or `USD`|
|Trade|Execution, fill|The (partial) execution of an order. Orders can have multiple trades|
|Quantity|Amount, Size|The amount of asset|
|Fee|Commission|The fee paid for an order or trade|
|Kline|Candlestick, OHLC|K-line data, used for candlestick charts. Contains Open/High/Low/Close/Volume|
|KlineInterval|The time period of a single kline|
|Open order|Active order, Unexecuted order|An order which has not yet been fully filled|
|Closed order|Completed order, executed order|An order which is no longer active. Can be canceled or fully filled|
|Network|Chain|The network of an asset. For example `ETH` allows multiple networks like `ERC20` and `BEP2`|
|Order book|Market depth|A list of (the top rows of) the current best bids and asks|
|Ticker|Stats|Statistics over the last 24 hours|
|Client implementation|Library|An implementation of the `CrytpoExchange.Net` library. For example `Binance.Net` or `FTX.Net`|

### Other naming conventions
#### PlaceOrderAsync
Methods for creating an order are always named `PlaceOrderAsync`, with and optional additional name for the type of order, for example `PlaceMarginOrderAsync`.

#### GetOrdersAsync/GetOpenOrdersAsync/GetClosedOrdersAsync
`GetOpenOrdersAsync` only retrieves orders which are still active, `GetClosedOrdersAsync` only retrieves orders which are canceled/closed. `GetOrdersAsync` retrieves both open and closed orders.