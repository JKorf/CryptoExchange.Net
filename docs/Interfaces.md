---
title: Common interfaces
nav_order: 5
---

## Shared interfaces
Clients have a common interface implementation to allow a shared code base to use the same functionality for different exchanges. The interface is implemented at the `API` level, for example:
```csharp
var binanceClient = new BinanceClient();
ISpotClient spotClient = binanceClient.SpotApi.CommonSpotClient;
IFuturesClient futuresClient = binanceClient.UsdFuturesApi.CommonFuturesClient;
```

For examples on this see the Examples folder.

## ISpotClient
The `ISpotClient` interface is implemented on Spot API clients. The interface exposes basic functionality like retrieving market data and managing orders. The `ISpotClient` interface will be available via the `CommonSpotClient` property on the Api client.
The spot client has the following members:

*Properties*  
```csharp
// The name of the exchange this client interacts with
string ExchangeName { get; }
```

*Events*  
```csharp
// Event when placing an order with this ISpotClient. Note that this is not an event handler listening on the exchange, just an event handler for when the `PlaceOrderAsync` method is called.
event Action<OrderId> OnOrderPlaced;
// Event when canceling an order with this ISpotClient. Note that this is not an event handler listening on the exchange, just an event handler for when the `CancelOrderAsync` method is called.
event Action<OrderId> OnOrderCanceled;

```

*Methods*
```csharp
// Retrieve the name of a symbol based on 2 assets. This will format them in the way the exchange expects them. For example BTC, USDT will return BTCUSDT on Binance and BTC-USDT on Kucoin
string GetSymbolName(string baseAsset, string quoteAsset);

// Get a list of symbols (trading pairs) on the exchange
Task<WebCallResult<IEnumerable<Symbol>>> GetSymbolsAsync();

// Get the ticker (24 hour stats) for a symbol
Task<WebCallResult<Ticker>> GetTickerAsync(string symbol);

// Get a list of tickers for all symbols
Task<WebCallResult<IEnumerable<Ticker>>> GetTickersAsync();

// Get a list klines (candlesticks) for a symbol
Task<WebCallResult<IEnumerable<Kline>>> GetKlinesAsync(string symbol, TimeSpan timespan, DateTime? startTime = null, DateTime? endTime = null, int? limit = null);

// Get the order book for a symbol
Task<WebCallResult<OrderBook>> GetOrderBookAsync(string symbol);

// Get a list of most recent trades
Task<WebCallResult<IEnumerable<Trade>>> GetRecentTradesAsync(string symbol);

// Get balances
Task<WebCallResult<IEnumerable<Balance>>> GetBalancesAsync(string? accountId = null);

// Place an order
Task<WebCallResult<OrderId>> PlaceOrderAsync(string symbol, CommonOrderSide side, CommonOrderType type, decimal quantity, decimal? price = null, string? accountId = null);

// Get order by order id
Task<WebCallResult<Order>> GetOrderAsync(string orderId, string? symbol = null);

// Get the trades for an order
Task<WebCallResult<IEnumerable<UserTrade>>> GetOrderTradesAsync(string orderId, string? symbol = null);

// Get a list of open orders. Some exchanges require a symbol
Task<WebCallResult<IEnumerable<Order>>> GetOpenOrdersAsync(string? symbol = null);

// Get a list of closed orders. Some exchanges require a symbol
Task<WebCallResult<IEnumerable<Order>>> GetClosedOrdersAsync(string? symbol = null);

// Cancel an active order
Task<WebCallResult<OrderId>> CancelOrderAsync(string orderId, string? symbol = null);
```

## IFuturesClient
The `IFuturesClient` interface is implemented on Futures API clients. The interface exposes basic functionality like retrieving market data and managing orders. The `IFuturesClient` interface will be available via the `CommonFuturesClient` property on the Api client.
The spot client has the following members:

*Properties*  
```csharp
// The name of the exchange this client interacts with
string ExchangeName { get; }
```

*Events*  
```csharp
// Event when placing an order with this ISpotClient. Note that this is not an event handler listening on the exchange, just an event handler for when the `PlaceOrderAsync` method is called.
event Action<OrderId> OnOrderPlaced;
// Event when canceling an order with this ISpotClient. Note that this is not an event handler listening on the exchange, just an event handler for when the `CancelOrderAsync` method is called.
event Action<OrderId> OnOrderCanceled;

```

*Methods*
```csharp
// Retrieve the name of a symbol based on 2 assets. This will format them in the way the exchange expects them. For example BTC, USDT will return BTCUSDT on Binance and BTC-USDT on Kucoin
string GetSymbolName(string baseAsset, string quoteAsset);

// Get a list of symbols (trading pairs) on the exchange
Task<WebCallResult<IEnumerable<Symbol>>> GetSymbolsAsync();

// Get the ticker (24 hour stats) for a symbol
Task<WebCallResult<Ticker>> GetTickerAsync(string symbol);

// Get a list of tickers for all symbols
Task<WebCallResult<IEnumerable<Ticker>>> GetTickersAsync();

// Get a list klines (candlesticks) for a symbol
Task<WebCallResult<IEnumerable<Kline>>> GetKlinesAsync(string symbol, TimeSpan timespan, DateTime? startTime = null, DateTime? endTime = null, int? limit = null);

// Get the order book for a symbol
Task<WebCallResult<OrderBook>> GetOrderBookAsync(string symbol);

// Get a list of most recent trades
Task<WebCallResult<IEnumerable<Trade>>> GetRecentTradesAsync(string symbol);

// Get balances
Task<WebCallResult<IEnumerable<Balance>>> GetBalancesAsync(string? accountId = null);

// Get current open positions
Task<WebCallResult<IEnumerable<Position>>> GetPositionsAsync();

// Place an order
Task<WebCallResult<OrderId>> PlaceOrderAsync(string symbol, CommonOrderSide side, CommonOrderType type, decimal quantity, decimal? price = null, int? leverage = null, string? accountId = null);

// Get order by order id
Task<WebCallResult<Order>> GetOrderAsync(string orderId, string? symbol = null);

// Get the trades for an order
Task<WebCallResult<IEnumerable<UserTrade>>> GetOrderTradesAsync(string orderId, string? symbol = null);

// Get a list of open orders. Some exchanges require a symbol
Task<WebCallResult<IEnumerable<Order>>> GetOpenOrdersAsync(string? symbol = null);

// Get a list of closed orders. Some exchanges require a symbol
Task<WebCallResult<IEnumerable<Order>>> GetClosedOrdersAsync(string? symbol = null);

// Cancel an active order
Task<WebCallResult<OrderId>> CancelOrderAsync(string orderId, string? symbol = null);
```