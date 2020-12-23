# CryptoExchange.Net 

![Build status](https://travis-ci.org/JKorf/CryptoExchange.Net.svg?branch=master)

A base library for easy implementation of cryptocurrency API's. Include:
* REST API calls and error handling
* Websocket subscriptions, error handling and automatic reconnecting
* Order book implementations automatically synchronizing and updating
* Automatic rate limiting

**If you think something is broken, something is missing or have any questions, please open an [Issue](https://github.com/JKorf/CryptoExchange.Net/issues)**

---
## Implementations
<table>
<tr>
<td><a href="https://github.com/JKorf/Bittrex.Net"><img src="https://github.com/JKorf/Bittrex.Net/blob/master/Bittrex.Net/Icon/icon.png?raw=true"></a>
<br />
<a href="https://github.com/JKorf/Bittrex.Net">Bittrex</a>
</td>
<td><a href="https://github.com/JKorf/Bitfinex.Net"><img src="https://github.com/JKorf/Bitfinex.Net/blob/master/Bitfinex.Net/Icon/icon.png?raw=true"></a>
<br />
<a href="https://github.com/JKorf/Bitfinex.Net">Bitfinex</a>
</td>
<td><a href="https://github.com/JKorf/Binance.Net"><img src="https://github.com/JKorf/Binance.Net/blob/master/Binance.Net/Icon/icon.png?raw=true"></a>
<br />
<a href="https://github.com/JKorf/Binance.Net">Binance</a>
</td>
<td><a href="https://github.com/JKorf/CoinEx.Net"><img src="https://github.com/JKorf/CoinEx.Net/blob/master/CoinEx.Net/Icon/icon.png?raw=true"></a>
<br />
<a href="https://github.com/JKorf/CoinEx.Net">CoinEx</a>
</td>
<td><a href="https://github.com/JKorf/Huobi.Net"><img src="https://github.com/JKorf/Huobi.Net/blob/master/Huobi.Net/Icon/icon.png?raw=true"></a>
<br />
<a href="https://github.com/JKorf/Huobi.Net">Huobi</a>
</td>
<td><a href="https://github.com/JKorf/Kucoin.Net"><img src="https://github.com/JKorf/Kucoin.Net/blob/master/Kucoin.Net/Icon/icon.png?raw=true"></a>
<br />
<a href="https://github.com/JKorf/Kucoin.Net">Kucoin</a>
</td>
<td><a href="https://github.com/JKorf/Kraken.Net"><img src="https://github.com/JKorf/Kraken.Net/blob/master/Kraken.Net/Icon/icon.png?raw=true"></a>
<br />
<a href="https://github.com/JKorf/Kraken.Net">Kraken</a>
</td>
</tr>
</table>

Implementations from third parties
<table>
<tr>
<td><a href="https://github.com/Zaliro/Switcheo.Net"><img src="https://github.com/Zaliro/Switcheo.Net/blob/master/Resources/switcheo-coin.png?raw=true"></a>
<br />
<a href="https://github.com/Zaliro/Switcheo.Net">Switcheo</a>
</td>
<td><a href="https://github.com/ridicoulous/LiquidQuoine.Net"><img src="https://github.com/ridicoulous/LiquidQuoine.Net/blob/master/Resources/icon.png?raw=true"></a>
<br />
<a href="https://github.com/ridicoulous/LiquidQuoine.Net">Liquid</a>
</td>
<td><a href="https://github.com/ridicoulous/Bitmex.Net"><img src="https://github.com/ridicoulous/Bitmex.Net/blob/master/Bitmex.Net/Icon/icon.png"></a>
<br />
<a href="https://github.com/ridicoulous/Bitmex.Net">Bitmex</a>
</td>
<td><a href="https://github.com/intelligences/HitBTC.Net"><img src="https://github.com/intelligences/HitBTC.Net/blob/master/src/HitBTC.Net/Icon/icon.png?raw=true"></a>
<br />
<a href="https://github.com/intelligences/HitBTC.Net">HitBTC</a>
</td>
<td><a href="https://github.com/EricGarnier/LiveCoin.Net"><img src="https://github.com/EricGarnier/LiveCoin.Net/blob/master/LiveCoin.Net/Icon/icon.png?raw=true"></a>
<br />
<a href="https://github.com/EricGarnier/LiveCoin.Net">LiveCoin</a>
</td>
<td><a href="https://github.com/burakoner/OKEx.Net"><img src="https://github.com/burakoner/OKEx.Net/blob/master/Okex.Net/Icon/icon.png?raw=true"></a>
<br />
<a href="https://github.com/burakoner/OKEx.Net">OKEx</a>
</td>
<td><a href="https://github.com/burakoner/Chiliz.Net"><img src="https://github.com/burakoner/Chiliz.Net/blob/master/Chiliz.Net/Icon/icon.png?raw=true"></a>
<br />
<a href="https://github.com/burakoner/Chiliz.Net">Chiliz</a>
</td>
<td><a href="https://github.com/burakoner/BtcTurk.Net"><img src="https://github.com/burakoner/BtcTurk.Net/blob/master/BtcTurk.Net/Icon/icon.png?raw=true"></a>
<br />
<a href="https://github.com/burakoner/BtcTurk.Net">BtcTurk</a>
</td>
<td><a href="https://github.com/burakoner/Thodex.Net"><img src="https://github.com/burakoner/Thodex.Net/blob/main/Thodex.Net/Icon/icon.png?raw=true"></a>
<br />
<a href="https://github.com/burakoner/Thodex.Net">Thodex</a>
</td>
</tr>
</table>

Planned implementations (no timeline or specific order):
* Bitstamp
* CoinFalcon
* Binance DEX

## Donations
Donations are greatly appreciated and a motivation to keep improving.

**Btc**:  12KwZk3r2Y3JZ2uMULcjqqBvXmpDwjhhQS  
**Eth**:  0x069176ca1a4b1d6e0b7901a6bc0dbf3bb0bf5cc2  
**Nano**: xrb_1ocs3hbp561ef76eoctjwg85w5ugr8wgimkj8mfhoyqbx4s1pbc74zggw7gs  

## Usage
Most API methods are available in two flavors, sync and async, see the example using the `BinanceClient`:
````C#
public void NonAsyncMethod()
{
    using(var client = new BinanceClient())
    {
        var result = client.Spot.Market.Get24HPrices();
    }
}

public async Task AsyncMethod()
{
    using(var client = new BinanceClient())
    {
        var result2 = await client.Spot.Market.Get24HPricesAsync();
    }
}
````

## Response handling
All API requests will respond with a (Web)CallResult object. This object contains whether the call was successful, the data returned from the call and an error if the call wasn't successful. As such, one should always check the Success flag when processing a response.
For example:
```C#
using(var client = new BinanceClient())
{
	var result = client.Spot.System.GetServerTime();
	if (result.Success)
		Console.WriteLine($"Server time: {result.Data}");
	else
		Console.WriteLine($"Error: {result.Error}");
}
```

## Options & Authentication
The default behavior of the clients can be changed by providing options to the constructor, or using the `SetDefaultOptions` before creating a new client. Api credentials can be provided in the options.
Credentials can be provided 2 ways:
* Providing key and secret:
````C#
	BinanceClient.SetDefaultOptions(new BinanceClientOptions
	{
		ApiCredentials = new ApiCredentials("apiKey", "apiSecret")
	});
````
* Providing a (file)stream containing the key/secret
````C#
using (var stream = File.OpenRead("/path/to/credential-file"))
{
	BinanceClient.SetDefaultOptions(new BinanceClientOptions
	{
		ApiCredentials = new ApiCredentials(stream)
	});
}
````
Note that when using a file it can provide credentials for multiple exchanges by providing the identifierKey and identifierSecret parameters:
````
// File content:
{
	"binanceKey": "actualBinanceApiKey",
	"binanceSecret": "actualBinanceApiSecret",
	"bittrexKey": "actualBittrexApiKey",
	"bittrexSecret": "actualBittrexApiSecret",
}

// Loading:
using (var stream = File.OpenRead("/path/to/credential-file"))
{
	BinanceClient.SetDefaultOptions(new BinanceClientOptions
	{
		ApiCredentials = new ApiCredentials(stream, "binanceKey", "binanceSecret")
	});
	BittrexClient.SetDefaultOptions(new BittrexClientOptions
	{
		ApiCredentials = new ApiCredentials(stream, "bittrexKey", "bittrexSecret")
	});
}
````

## Websockets
Most implementations have a websocket client. The client will manage the websocket connection for you, all you have to do is call a subscribe method. The client will automatically handle reconnecting when losing a connection.

When using a subscribe method it will return an `UpdateSubscription` object. This object has 3 events: ConnectionLost/ConnectionRestored and Exception. Use the connection lost/restored to be notified when the socket has lost it's connection and when it was reconnected. The Exception event is thrown when there was an exception within the data handling callback.

To unsubscribe use the client.Unsubscribe method and pass the UpdateSubscription received when subscribing:
````C#
// Subscribe
var client = new BinanceSocketClient();
var subResult = client.Spot.SubscribeToOrderBookUpdates("BTCUSDT", data => {});

// Unsubscribe
client.Unsubscribe(subResult.Data);
````
To unsubscribe all subscriptions the `client.UnsubscribeAll()` method can be used.

## Order books
The library implementations provide a `SymbolOrderBook` implementation. This implementation can be used to keep an updated order book without having to think about synchronizing it. This example is from the Binance.Net library, 
but the implementation is similar for each library:
````C#
var orderBook = new BinanceSymbolOrderBook("BTCUSDT", new BinanceOrderBookOptions(20));
orderBook.OnStatusChange += (oldStatus, newStatus) => Console.WriteLine($"Book state changed from {oldStatus} to {newStatus}");
orderBook.OnOrderBookUpdate += (changedBids, changedAsks) => Console.WriteLine("Book updated");
var startResult = await orderBook.StartAsync();
if(!startResult.Success)
{
	Console.WriteLine("Error starting order book synchronization: " + startResult.Error);
	return;
}

var status = orderBook.Status; // The current status. Note that the order book is only current when the status is Synced
var askCount = orderBook.AskCount; // The current number of asks in the book
var bidCount = orderBook.BidCount; // The current number of bids in the book
var asks = orderBook.Asks; // All asks
var bids = orderBook.Bids; // All bids
var bestBid = orderBook.BestBid; // The best bid available in the book
var bestAsk = orderBook.BestAsk; // The best ask available in the book

````
The order book will automatically reconnect when the connection is lost and resync if it detects the sequence is off. Make sure to check the Status property to see it the book is currently in sync.

To stop synchronizing an order book use the `Stop` method.

## Shared IExchangeClient interface
Most implementations have an implementation of the `IExchangeClient` interface. It is a shared interface, which makes it easier to re-use the same code for multiple exchanges. It offers basic functionality only, for exchange specific calls use the actual client interface, for example `IBinanceClient` or `IBitfinexClient`.

The IExchangeClient interface supports the following methods:

`string GetSymbolName(string baseAsset, string quoteAsset);` - Based on a base and quote asset return the symbol name for the exchange.

`GetSymbolsAsync();` - Get a list of supported symbols.

`GetTickersAsync();` - Get a list of tickers (High/Low/volume data for the last 24 hours) for all symbols.

`GetTickerAsync(string symbol);` - Get a specific ticker.

`GetKlinesAsync(string symbol, TimeSpan timespan, DateTime? startTime = null, DateTime? endTime = null, int? limit = null);` - Get candlestick data for a symbol.

`GetOrderBookAsync(string symbol);` - Get the order book for a symbol.

`GetRecentTradesAsync(string symbol);` - Get a list of the most recent trades for a symbol.

`PlaceOrderAsync(string symbol, OrderSide side, OrderType type, decimal quantity, decimal? price = null, string? accountId = null);` - \[Authenticated\] Place an order.

`GetOrderAsync(string orderId, string? symbol = null);` - \[Authenticated\] Get details on an order.

`GetTradesAsync(string orderId, string? symbol = null);` - \[Authenticated\] Get executed trades for an order.

`GetOpenOrdersAsync(string? symbol = null);` - \[Authenticated\] Get a list of open orders.

`GetClosedOrdersAsync(string? symbol = null);` - \[Authenticated\] Get a list of closed orders.

`CancelOrderAsync(string orderId, string? symbol = null);` - \[Authenticated\] Cancel an order.

`GetBalancesAsync(string? accountId = null);` - \[Authenticated\] Get a list of balances.


Example usage:
````C#
static async Task Main(string[] args)
{
	var clients = new List<IExchangeClient>()
	{
		{ new BinanceClient(new BinanceClientOptions(){ LogVerbosity = LogVerbosity.Debug, ApiCredentials = new ApiCredentials("BinanceKey", "BinanceSecret") }) },
		{ new BitfinexClient(new BitfinexClientOptions(){ LogVerbosity = LogVerbosity.Debug, ApiCredentials = new ApiCredentials("BitfinexKey", "BitfinexSecret")  }) },
		{ new BittrexClientV3(new BittrexClientOptions(){ LogVerbosity = LogVerbosity.Debug, ApiCredentials = new ApiCredentials("BittrexKey", "BittrexSecret")  }) },
	};

	await Task.WhenAll(clients.Select(GetExchangeData));
	Console.WriteLine("Done");
	Console.ReadLine();
}

static async Task GetExchangeData(IExchangeClient client)
{
	var symbols = await client.GetSymbolsAsync();
	Console.WriteLine($"{((RestClient)client).ClientName}: {symbols.Data?.Count()} symbols returned, first = {symbols.Data?.First().CommonName}");

	var balances = await client.GetBalancesAsync();
	var btcBalance = balances.Data?.Where(b => b.CommonAsset.ToUpperInvariant() == "BTC").FirstOrDefault();
	Console.WriteLine($"{((RestClient)client).ClientName}: {balances.Data?.Count()} balance returned, BTC balance = {btcBalance?.CommonTotal}");

	var symbolName = client.GetSymbolName("BTC", "USDT");
	var klines = await client.GetKlinesAsync(symbolName, TimeSpan.FromHours(1));
	Console.WriteLine($"{((RestClient)client).ClientName}: {klines.Data?.Count()} klines returned, first klines @ {klines.Data?.First().CommonClose}");
}
````

## Release notes
* Version 3.4.0 - 21 dec 2020
    * Updated IExchangeClient interface
    * Fix for dropping message after timeout on socket
    * Added virtual HandleUnhandledMessage method in SocketClient

* Version 3.3.0 - 10 dec 2020
    * Added client name
    * Added common interfaces
    * Fixed api key plain text storing in RateLimitterApiKey

* Version 3.2.1 - 19 nov 2020
    * Fixed error code parsing

* Version 3.2.0 - 19 nov 2020
    * Fix for multiple socket subscriptions re-using the same socket connection
    * Updated errors

* Version 3.1.0 - 08 Oct 2020
    * Added CallResult without type parameter for calls which don't return data
    * Added GetErrorOrResult method on CallResult to support proper nullability checking
    * Fix for reading credentials from file
    * Fix for setting custom base addresses in clients

* Version 3.0.15 - 06 Oct 2020
    * Changed default ShouldCheckObjects to false to prevent spam in logging

* Version 3.0.14 - 24 Aug 2020
    * Updated exception message logging

* Version 3.0.13 - 24 Aug 2020
    * Added request tracing id for logging
    * Added shared HttpClient option

* Version 3.0.12 - 12 Aug 2020
    * Named parameters on SymbolOrderBook events

* Version 3.0.11 - 20 Jun 2020
	* Added support for checksum in SymbolOrderBook

* Version 3.0.10 - 16 Jun 2020
    * Fix for order book synchronization

* Version 3.0.9 - 07 Jun 2020
	* Added arraySerialization and postParameterPosition to AuthenticationProvider interface
	* Fixed array serialization in request body

* Version 3.0.8 - 02 Jun 2020
	* Added requestBodyEmptyContent setting for rest client
	* Added TryParseError for rest implementations to check for error with success status code

* Version 3.0.7 - 20 May 2020
    * Added error debug output
    * Fix for unsubscribe causing possible deadlock

* Version 3.0.6 - 03 Mar 2020
    * Added BestOffer to SymbolOrderBook, removed invalid check on proxy

* Version 3.0.5 - 05 Feb 2020
    * Added PausedActivity events on socket subscriptions

* Version 3.0.4 - 29 Jan 2020
	* Removed unnecessary json serialization

* Version 3.0.3 - 23 Jan 2020
    * Added OnBestOffersChanged event to order book implementations

* Version 3.0.2 - 10 Dec 2019
    * Removed invalid check for unauthenticated proxy

* Version 3.0.1 - 14 Nov 2019
    * Re-enabled debug response logging

* Version 3.0.0 - 23 Oct 2019
	* Updated to C# 8.0
	* Added .NetStandard2.1 support
	* Added Nullability support
	* Now using HttpClient instead of WebRequest, should result in faster consequtive requests
	* Added CancellationToken support
	* Added bool compare override to CallResult (now possible to `if(callresult)` instead of `if(callresult.Success)`)
	* Added input validation methods
		* Wrong input will now throw exceptions rather than error results
	* OnOrderBookUpdate event added to `SymbolOrderBook`


* Version 2.1.8 - 29 Aug 2019
    * Added array serialization options for implementations

* Version 2.1.7 - 07 Aug 2019
    * Fixed bug with socket connection not being disposed after lost connection
    * Resubscribing after reconnecting socket now in parallel

* Version 2.1.6 - 06 Aug 2019
    * Fix for missing subscription events if they are also a request response, added code docs

* Version 2.1.5 - 09 jul 2019
	* Updated SymbolOrderBook

* Version 2.1.4 - 24 jun 2019
	* Added checks for json deserialization issues

* Version 2.1.3 - 16 may 2019
	* Refactored SymbolOrderBook
	* Added BestBid/BestAsk properties for order book

* Version 2.1.2 - 14 may 2019
	* Added order book base class for easy implementation
	* Added additional constructor to ApiCredentials to be able to read from file
