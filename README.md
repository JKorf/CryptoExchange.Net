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
<td><a href="https://github.com/JKorf/Bittrex.Net"><img src="https://github.com/JKorf/Bittrex.Net/blob/master/Resources/icon.png?raw=true"></a>
<br />
<a href="https://github.com/JKorf/Bittrex.Net">Bittrex</a>
</td>
<td><a href="https://github.com/JKorf/Bitfinex.Net"><img src="https://github.com/JKorf/Bitfinex.Net/blob/master/Resources/icon.png?raw=true"></a>
<br />
<a href="https://github.com/JKorf/Bitfinex.Net">Bitfinex</a>
</td>
<td><a href="https://github.com/JKorf/Binance.Net"><img src="https://github.com/JKorf/Binance.Net/blob/master/Resources/binance-coin.png?raw=true"></a>
<br />
<a href="https://github.com/JKorf/Binance.Net">Binance</a>
</td>
<td><a href="https://github.com/JKorf/CoinEx.Net"><img src="https://github.com/JKorf/CoinEx.Net/blob/master/Resources/icon.png?raw=true"></a>
<br />
<a href="https://github.com/JKorf/CoinEx.Net">CoinEx</a>
</td>
<td><a href="https://github.com/JKorf/Huobi.Net"><img src="https://github.com/JKorf/Huobi.Net/blob/master/Resources/icon.png?raw=true"></a>
<br />
<a href="https://github.com/JKorf/Huobi.Net">Huobi</a>
</td>
<td><a href="https://github.com/JKorf/Kucoin.Net"><img src="https://github.com/JKorf/Kucoin.Net/blob/master/Resources/icon.png?raw=true"></a>
<br />
<a href="https://github.com/JKorf/Kucoin.Net">Kucoin</a>
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
</tr>
</table>

Planned implementations (no timeline or specific order):
* BitMEX
* Bitstamp
* CoinFalcon
* Kraken
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
        var result = client.Ping();
    }
}

public async Task AsyncMethod()
{
    using(var client = new BinanceClient())
    {
        var result2 = await client.PingAsync();
    }
}
````

## Response handling
All API requests will respond with a (Web)CallResult object. This object contains whether the call was successful, the data returned from the call and an error if the call wasn't successful. As such, one should always check the Success flag when processing a response.
For example:
```C#
using(var client = new BinanceClient())
{
	var result = client.GetServerTime();
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
	"binanceKey": "binanceApiKey",
	"binanceSecret": "binanceApiSecret",
	"bittrexKey": "bitrexApiKey",
	"bittrexSecret": "bittrexApiSecret",
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
		ApiCredentials = new ApiCredentials(stream, "BittrexKey", "BittrexSecret")
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
var subResult = client.SubscribeToDepthStream("BTCUSDT", data => {});

// Unsubscribe
client.Unsubscribe(subResult.Data);
````
To unsubscribe all subscriptions the `client.UnsubscribeAll()` method can be used.

## Order books
The library implementations provide a `SymbolOrderBook` implementation. This implementation can be used to keep an updated order book without having to think about synchronizing it. This example is from the Binance.Net library, but is the same for others:
````C#
var orderBook = new BinanceSymbolOrderBook("BTCUSDT", 100);
orderBook.OnStatusChange += (oldStatus, newStatus) => Console.WriteLine($"Book state changed from {oldStatus} to {newStatus}");
var startResult = await orderBook.Start();
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

````
The order book will automatically reconnect when the connection is lost and resync if it detects the sequence is off. Make sure to check the Status property to see it the book is currently in sync.

To stop synchronizing an order book use the `Stop` method.

## Release notes
* Version 2.1.3 - 16 may 2019
	* Refactored SymbolOrderBook
	* Added BestBid/BestAsk properties for order book

* Version 2.1.2 - 14 may 2019
	* Added order book base class for easy implementation
	* Added additional constructor to ApiCredentials to be able to read from file
