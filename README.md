# CryptoExchange.Net
![Build status](https://travis-ci.com/JKorf/CryptoExchange.Net.svg?branch=master) ![Nuget version](https://img.shields.io/nuget/v/CryptoExchange.Net.svg)  ![Nuget downloads](https://img.shields.io/nuget/dt/CryptoExchange.Net.svg)

CryptoExchange.Net is a base package which can be used to easily implement crypto currency exchange API's in C#. This library offers base classes for creating rest and websocket clients, and includes additional features like an automatically synchronizing order book implementation, error handling and automatic reconnects on websocket connections.

## Implementations
By me:
<table>
<tr>
<td><a href="https://github.com/JKorf/Binance.Net"><img src="https://github.com/JKorf/Binance.Net/blob/master/Binance.Net/Icon/icon.png?raw=true"></a>
<br />
<a href="https://github.com/JKorf/Binance.Net">Binance</a>
</td>
<td><a href="https://github.com/JKorf/Bittrex.Net"><img src="https://github.com/JKorf/Bittrex.Net/blob/master/Bittrex.Net/Icon/icon.png?raw=true"></a>
<br />
<a href="https://github.com/JKorf/Bittrex.Net">Bittrex</a>
</td>
<td><a href="https://github.com/JKorf/Bitfinex.Net"><img src="https://github.com/JKorf/Bitfinex.Net/blob/master/Bitfinex.Net/Icon/icon.png?raw=true"></a>
<br />
<a href="https://github.com/JKorf/Bitfinex.Net">Bitfinex</a>
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
<td><a href="https://github.com/JKorf/FTX.Net"><img src="https://github.com/JKorf/FTX.Net/blob/main/FTX.Net/Icon/icon.png?raw=true"></a>
<br />
<a href="https://github.com/JKorf/FTX.Net">FTX</a>
</td>
</tr>
</table>
By third parties:
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
<td><a href="https://github.com/ridicoulous/Bitmex.Net"><img src="https://github.com/ridicoulous/Bitmex.Net/blob/master/Bitmex.Net/Icon/icon.png?raw=true"></a>
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
<td><a href="https://github.com/burakoner/Thodex.Net"><img src="https://github.com/burakoner/Thodex.Net/blob/master/Thodex.Net/Icon/icon.png?raw=true"></a>
<br />
<a href="https://github.com/burakoner/Thodex.Net">Thodex</a>
</td>
<td><a href="https://github.com/d-ugarov/Exante.Net"><img src="https://github.com/d-ugarov/Exante.Net/blob/master/Exante.Net/Icon/icon.png?raw=true"></a>
<br />
<a href="https://github.com/d-ugarov/Exante.Net">Exante</a>
</td>
<td><a href="https://github.com/rodrigobelo/wootrade-dotnet"><img src="https://github.com/rodrigobelo/wootrade-dotnet/blob/main/wootrade-dotnet-icon.png?raw=true"></a>
<br />
<a href="https://github.com/rodrigobelo/wootrade-dotnet">Wootrade</a>
</td>
</tr>
</table>

## Discord
A Discord server is available [here](https://discord.gg/MSpeEtSY8t). Feel free to join for discussion and/or questions around the CryptoExchange.Net and implementation libraries.

## Donate / Sponsor
I develop and maintain this package on my own for free in my spare time. Donations are greatly appreciated. If you prefer to donate any other currency please contact me.

**Btc**:  12KwZk3r2Y3JZ2uMULcjqqBvXmpDwjhhQS  
**Eth**:  0x069176ca1a4b1d6e0b7901a6bc0dbf3bb0bf5cc2  
**Nano**: xrb_1ocs3hbp561ef76eoctjwg85w5ugr8wgimkj8mfhoyqbx4s1pbc74zggw7gs  

Alternatively, sponsor me on Github using [Github Sponsors](https://github.com/sponsors/JKorf)  

## Implementation usage
### Clients
The CryptoExchange.Net library offers 2 base clients which should be implemented in each implementation library. The `RestClient` and the `SocketClient`.

**RestClient**

The `RestClient`, as the name suggests, handles requests to the exchange REST API. Typically the `RestClient` implementation name is [ExchangeName]Client. So for the Binance exchange this would be called the `BinanceClient`, and for Bittrex the client type name is `BittrexClient`.

The `RestClient` implementations can be used in either a `using` or with a static instance:
````C#
using (var binanceClient = new BinanceClient())
{
  var exchangeInfoResult = binanceClient.Spot.System.GetExchangeInfoAsync();
}
```` 
or 
````C#
var client = new BinanceClient();
var exchangeInfoResult = client.Spot.System.GetExchangeInfoAsync();
````
If you're opting for the `using` syntax, a `HttpClient` should be provided in the client options to prevent each client creating it's own `HttpClient` instance.

Calls made on the `RestClient` will return a `WebCallResult<T>` object which will contain the following properties:
|Property|Description|Available when
|---|---|---|
|`Success`|Whether or not the call was successfully executed.|Always
|`Data`|The data the server sent us as response.| When `Success` is `true`
|`Error`|Details on the error that happened during the call.| When `Success` is `false`
|`OriginalData`|The originally received Json data which was received before being deserialized into an object.| When `OutputOriginalData` is enabled in the client options
|`ResponseStatusCode`|The Http status code received as answer on the request.|Always
|`ResponseHeaders`|The headers received in the response.|Always

The `RestClient` implementation should implement the `IExchangeClient` interface, which offers some basic methods for interacting with an exchange without having to know the implementation.

**SocketClient**

The `SocketClient` can be used to connect to websocket streams offered by the API, and receive callbacks whenever new data is received. `SocketClient` implementations are typically named [ExchangeName]SocketClient, so in case of Binance this would be `BinanceSocketClient`.  

To use the `SocketClient` to connect to a stream simply call the `SubscribeXXX` method for the data you're interested in and pass in a delegate for handling updates. For example, to subscribe to the Binance ticker stream call `SubscribeToAllSymbolTickerUpdatesAsync` method:
````C#
var socketClient = new BinanceSocketClient();
var subscribeResult = socketClient.Spot.SubscribeToAllSymbolTickerUpdatesAsync(data => {
  // Handle updates received here
});
````
or
````C#
var socketClient = new BinanceSocketClient();
var subscribeResult = socketClient.Spot.SubscribeToAllSymbolTickerUpdatesAsync(HandleData);

private void HandleData(DataEvent<IEnumerable<BinanceTick>> data)
{
  // Handle updates received here
}
````

Make sure to check the result of the subscribe call to ensure it was successful. The Subscribe methods will return a `CallResult<UpdateSubscription>` object with the following properties:
|Property|Description|Available when
|---|---|---|
|`Success`|Whether or not the call was successfully executed.|Always
|`Data`|The `UpdateSubscription` object for this stream. This can be used to for listening to connection changed events and unsubscribing | When `Success` is `true`
|`Error`|Details on the error that happened during the subscription. | When `Success` is `false`

To unsubscribe from a stream `Unsubscribe()` method can be used, with the `UpdateSubscription` received in the `SubscribeXXX` call as parameter, or use the `UnsubscribeAll()` method to close all subscriptions:
````C#
// Subscribe
var client = new BinanceSocketClient();
var subResult = client.Spot.SubscribeToOrderBookUpdates("BTCUSDT", data => {});

// Unsubscribe
client.Unsubscribe(subResult.Data);
````
The `SocketClient` handles connection management to the server internally and will close a connection if there are no more subscriptions on that connection.

*[WARNING] Do not use `using` statements in combination with constructing a `SocketClient`. Doing so will dispose the `SocketClient` instance when the subscription is done, which will result in the connection getting closed. Instead assign the socket client to a variable outside of the method scope.*

### Client options
Options for a client can be provided in the constructor or using the static SetDefaultOptions method.
````C#
var client = new BinanceClient(new BinanceClientOptions()
{
});
````
or
````C#
BinanceClient.SetDefaultOptions(new BinanceClientOptions()
{
});
````
When providing options in the constructor the options will only apply for that specific client. When using the `SetDefaultOptions` method the options will be applied to any client created after that call which doesn't have any options provided to it via the constructor. Providing options in the constructor means any options set using the `SetDefaultOptions` method will be reset to default unless overwritten in the provided options.

**Options for all clients**

| Property | Description | Default |
| ----------- | ----------- | ---------|
| `LogWriters`| A list of `ILogger`s to handle log messages. | `new List<ILogger> { new DebugLogger() }` |
| `LogLevel`| The minimum log level before passing messages to the `LogWriters`. Messages with a more verbose level than the one specified here will be ignored. Setting this to `null` will pass all messages to the `LogWriters`.| `LogLevel.Information`
|`OutputOriginalData`|If set to `true` the originally received Json data will be output as well as the deserialized object. For `RestClient` calls the data will be in the `WebCallResult<T>.OriginalData` property, for `SocketClient` subscriptions the data will be available in the `DataEvent<T>.OriginalData` property when receiving an update. | `false`
|`BaseAddress`|The base address to the API. All calls to the API will use this base address as basis for the endpoints. This allows for swapping to test API's or swapping to a different cluster for example.| Depends on implementation
|`ApiCredentials`| The API credentials to use for accessing protected endpoints. Typically a key/secret combination.| `null`
|`Proxy`|The proxy to use for connecting to the API.| `null`

**Options for RestClients**
| Property | Description | Default |
| ----------- | ----------- | ---------|
| `RateLimiters`| A list of `IRateLimiter`s to use.| `new List<IRateLimiter>()` |
| `RateLimitingBehaviour`| What should happen when a rate limit is reached.| `RateLimitingBehaviour.Wait` |
| `RequestTimeout`| The time out to use for requests.| `TimeSpan.FromSeconds(30)` |
| `HttpClient`| The `HttpClient` instance to use for making requests. When creating multiple `RestClient` instances a single `HttpClient` should be provided to prevent each client instance from creating its own. *[WARNING] When providing the `HttpClient` instance in the options both the `RequestTimeout` and `Proxy` client options will be ignored and should be set on the provided `HttpClient` instance.*| `null` |

**Options for SocketClients**
| Property | Description | Default |
| ----------- | ----------- | ---------|
|`AutoReconnect`|Whether or not the socket should automatically reconnect when disconnected.|`true`
|`ReconnectInterval`|The time to wait between connection tries when reconnecting.|`TimeSpan.FromSeconds(5)`
|`SocketResponseTimeout`|The time in which a response is expected on a request before giving a timeout.|`TimeSpan.FromSeconds(10)`
|`SocketNoDataTimeout`|If no data is received after this timespan then assume the connection is dropped. This is mainly used for API's which have some sort of ping/keepalive system. For example; the Bitfinex API will sent a heartbeat message every 15 seconds, so the `SocketNoDataTimeout` could be set to 20 seconds. On API's without such a mechanism this might not work because there just might not be any update while still being fully connected. | `default(TimeSpan)` (no timeout)
|`SocketSubscriptionsCombineTarget`|The amount of subscriptions that should be made on a single socket connection. Not all exchanges support multiple subscriptions on a single socket. Setting this to a higher number increases subscription speed because not every subscription needs to connect to the server, but having more subscriptions on a single connection will also increase the amount of traffic on that single connection, potentially leading to issues.| Depends on implementation
|`MaxReconnectTries`|The maximum amount of tries for reconnecting|`null` (infinite)
|`MaxResubscribeTries`|The maximum amount of tries for resubscribing after successfully reconnecting the socket|5
|`MaxConcurrentResubscriptionsPerSocket`|The maximum number of concurrent resubscriptions per socket when resubscribing after reconnecting|5

### Order book
The library implementations provide a `SymbolOrderBook` implementation. This implementation can be used to keep an updated order book without having to think about synchronization. This example is from the Binance.Net library, but the implementation is similar for each library:
````C#
var orderBook = new BinanceSymbolOrderBook("BTCUSDT", new BinanceOrderBookOptions(20));
orderBook.OnStatusChange += (oldStatus, newStatus) => Console.WriteLine($"Book state changed from {oldStatus} to {newStatus}");
orderBook.OnOrderBookUpdate += ((changedBids, changedAsks)) => Console.WriteLine("Book updated");
var startResult = await orderBook.StartAsync();
if(!startResult.Success)
{
	Console.WriteLine("Error starting order book synchronization: " + startResult.Error);
	return;
}

var status = orderBook.Status; // The current status. Note that the order book is only up to date when the status is Synced
var askCount = orderBook.AskCount; // The current number of asks in the book
var bidCount = orderBook.BidCount; // The current number of bids in the book
var asks = orderBook.Asks; // All asks
var bids = orderBook.Bids; // All bids
var bestBid = orderBook.BestBid; // The best bid available in the book
var bestAsk = orderBook.BestAsk; // The best ask available in the book
````
The order book will automatically reconnect when the connection is lost and resync if it detects that it is out of sync. Make sure to check the Status property to see it the book is currently in sync.

To stop synchronizing an order book use the `Stop` method.

## Helper methods
The static `ExchangeHelpers` class offers some helper methods for adjusting value like rounding and adjusting values to fit a certain step size or precision. 

## Creating an implementation
Implementations should implement at least the following:  
**[Exchange]Client based on the RestClient base class**  
Containing calls to the different endpoints, internally using the `SendRequest<T>` method of the `RestClient`.

**[Exchange]SocketClient based on the SocketClient base class**  
Containing methods to subscribe to different streams using the `Subscribe<T>` method of the `SocketClient`.
Implement exchange specific handling of requests/messages by overriding these methods:  
`HandleQueryResponse`: Check if the data received from the websocket matches the sent query.  
`HandleSubscriptionResponse`: Check if the data received from the websocket matches the subscription request.  
`MessageMatchesHandler`: Check if the data received from the websocket matches the handler/subscription.  
`AuthenticateSocket`: Authenticate the connection to be able to subscribe to protected streams.  
`Unsubscribe`: Unsubscribe from a stream, typically by sending an Unsubscribe message.  

**[Exchange]SymbolOrderBook based on the SymbolOrderBook base class**  
An implementation of an automatically synchronized order book. Implement exchange specific behavior by implementing these methods:  
`DoStart`: Start the order book and sync process  
`DoResync`: Resync the order book after a reconnection  
`DoReset`: Reset the state of the orderbook, called when the connection is lost.  
`DoChecksum`: [Optional] Validate the order book with a checksum.

**[Exchange]AuthenticationProvider**  
An implementation of the AuthenticationProvider base class. Should contain the logic for authenticating requests from the RestClient on protected endpoints. Override these methods as needed:  
`AddAuthenticationToParameters`: Will be called before `AddAuthenticationToHeaders`, allows the implementation to add specific parameters to the request which are needed for protected endpoints.  
`AddAuthenticationToHeaders`: Will be called after `AddAuthenticationToParameters`, allows the implementation to add specific headers to the request message.  
If you have any issues or questions regarding implementing an exchange using CryptoExchange.Net hop into the Discord or open an issue.

## FAQ
**I sometimes get NullReferenceException, what's wrong?**  
You probably don't check the result status of a call and just assume the data is always there. `NullReferenceExecption`s will happen when you have code like this `var symbol = client.GetTickersAync().Result.Data.Symbol` because the `Data` property is null when the call fails. Instead check if the call is successful like this:
````C#
var tickerResult = await client.GetTickersAync();
if(!tickerResult.Success)
{
  // Handle error
}
else
{
  // Handle data, it is now safe to access the data
  var symbol = tickerResult.Data.Symbol;
}
````
**The socket client stops sending updates after a little while**  
You probably didn't keep a reference to the socket client and it got disposed.
Instead of subscribing like this:
````C#
private void SomeMethod()
{
  var socketClient = new BinanceSocketClient();
  socketClient.Spot.SubscribeToOrderBookUpdates("BTCUSDT", data => {
	// Handle data
  });
}
````
Subscribe like this:
````C#
private BinanceSocketClient _socketClient;

// .. rest of the class

private void SomeMethod()
{
  if(_socketClient == null)
    _socketClient = new BinanceSocketClient();

  _socketClient.Spot.SubscribeToOrderBookUpdates("BTCUSDT", data => {
	// Handle data
  });
}

````

## Release notes
* Version 4.2.8 - 08 Oct 2021
    * Fixed deadlock in socket receive
    * Fixed issue in reconnection handling when the client is disconnected again during resubscribing
    * Added some additional checking of socket state to prevent sending/expecting data when socket is not connected

* Version 4.2.7 - 06 Oct 2021
    * Made receivedMessages protected again to allow implementations with custom transport (Bittrex) to use it again

* Version 4.2.6 - 06 Oct 2021
    * Fixed an issue causing socket client to stop processing data in .NET Framework

* Version 4.2.5 - 05 Oct 2021
    * Added custom async wait event implementation as previous method seems to not work 100% of the time

* Version 4.2.4 - 30 Sep 2021
    * Fix for InvalidOperationExceptions when running socket connections from .Net framework

* Version 4.2.3 - 29 Sep 2021
    * Added IncomingKbps property to socket/socket client
    * Updated logging
    * Socket performance improvements

* Version 4.2.2 - 23 Sep 2021
    * Restored missing request parameters log

* Version 4.2.1 - 22 Sep 2021
    * Fìx for websocket not automatically reconnecting when connection is closed unexpectedly

* Version 4.2.0 - 20 Sep 2021
    * Prevent reconnect spamming when invalid checksum is calculated in SymbolOrderBook
    * Added default nonce provider implementation

* Version 4.1.0 - 15 Sep 2021
    * Added overload for UnsubscribeAsync with id parameter
    * Added parameter position configuration per HttpMethod type
    * Added option to send custom headers with each requets
    * Added option to send custom headers with individual requests
    * Added debug data on error

* Version 4.0.8 - 26 Aug 2021
    * Added rate limiting option for outgoing messages per socket

* Version 4.0.7 - 24 Aug 2021
    * Additional error info on websocket exception

* Version 4.0.6 - 24 Aug 2021
    * Removed some debug logging

* Version 4.0.5 - 24 Aug 2021
    * Added ConnectionClosed event on UpdateSubscriptions to signal the connection was closed and no reconnecting is happening

* Version 4.0.4 - 24 Aug 2021
    * Websocket connection fixes/improvements
    * Added ChecksumValidationEnabled option for controlling checksum validation in SymbolOrderBook
    * Added MaxReconnectTries option
    * Added MaxResubscribeTries option
    * Added MaxConcurrentResubscriptionsPerSocket option
    * Fix for TimestampSecondsConverter rounding to nearest millisecond

* Version 4.0.3 - 20 Aug 2021
    * Fix for concurrent sent socket issue

* Version 4.0.2 - 20 Aug 2021
    * Fixed socket client continuing before the send/receive loops have been started, which could cause issues when doing concurrent connections

* Version 4.0.1 - 13 Aug 2021
    * Fixed OperationCancelledException when closing socket from a project targeting .net framework

* Version 4.0.0 - 12 Aug 2020
	* Release version, summed up changes from previous beta releases:
		* Removed `Websocket4Net` dependency in favor of a `ClientWebSocket` native implementation for websocket connections
		* Socket events now always come wrapped in a `DataEvent<>` object which contain the timestamp of the data, and optionally the originally received json string
		* Implemented usage of the `Microsoft.Extensions.Logging.Abstractions` `ILogger` interface instead of a custom implementation
		* Added some properties to the `IExchangeClient` interface
			* `ICommonOrder.CommonOrderTime`
			* `ICommonOrder.CommonOrderStatus` enum
			* `ICommonTrade.CommonTradeTime`
		* Added `OnOrderPlaced` and `OnOrderCanceled` events on the `IExchangeClient` interface
		* Added `ExchangeHelpers` static class for various helper methods
		* Removed non-async methods due to too much overhead in development/maintainance
			* If you were previously using non-async methods you can add `.Result` to the end of the async call to get the same result
		* Added `Book` property to `SymbolOrderBook` for a book snapshot
		* Added `CalculateAverageFillPrice` to `SymbolOrderBook` to calculate the average fill price for an order with the current order book state
		* Various fixes

* Version 4.0.0-beta15 - 12 Aug 2021
    * Conditional version Logging.Abstractions

* Version 4.0.0-beta14 - 09 Aug 2021
    * Fix for bug in processing order in SymbolOrderBook

* Version 4.0.0-beta13 - 31 Jul 2021
    * Fix for socket connection

* Version 4.0.0-beta12 - 26 Jul 2021
    * Fix for socket connection

* Version 4.0.0-beta11 - 09 Jul 2021
    * Added CalculateAverageFillPrice to SymbolOrderBook
    * Added Book property to SymbolOrderBook
    * Added Async postfix to async methods

* Version 4.0.0-beta10 - 07 Jul 2021
    * Updated BaseConverter to be case sensitive
    * Added ExchangeHelpers class containing some helper methods
    * Fixed responses not being logged on Trace log level
    * Added some code docs

* Version 4.0.0-beta9 - 17 Jun 2021
    * Small fixes

* Version 4.0.0-beta8 - 08 Jun 2021
    * Fixed exception socket buffer size in .net framework

* Version 4.0.0-beta7 - 07 Jun 2021
    * Added CommonOrderTime to IOrder
    * Added OrderStatus enum for IOrder
    * Added OnOrderPlaced and OnOrderCanceled events on IExchangeClient
    * Added CommonTradeTime to ICommonTrade

* Version 4.0.0-beta6 - 01 jun 2021
    * Some logging adjustments
    * Fixed some async issues

* Version 4.0.0-beta5 - 26 May 2021
    * Added DataEvent wrapper for socket updates
    * Added optional original json output
    * Changed logging implementation to use ILogger

* Version 4.0.0-beta4 - 06 mei 2021
    * Added analyzers
    * Fixed some warnings

* Version 4.0.0-beta3 - 30 Apr 2021
    * Updated socket closing

* Version 4.0.0-beta2 - 30 apr 2021
    * Fix for closing socket without timeout task

* Version 4.0.0-beta1 - 30 apr 2021
    * Removed Websocket4Net dependency
    * Added custom ClientWebSocket implementation
    * Renamed handler -> subscription internally
    * Renamed socket -> socketConenction when type is socketConnection

* Version 3.9.0 - 28 apr 2021
    * Added optional JsonSerializer parameter to SendRequest to use during deserialization
    * Fix for unhandled message warning when unsubscribing a socket subscription

* Version 3.8.1 - 19 apr 2021
    * Added debug logs
	* Added ValidateNullOrNotEmpty extension method

* Version 3.8.0 - 30 mrt 2021
    * Better handling of json errors while deserializing stream
    * Added string datetime converter

* Version 3.7.1 - 10 mrt 2021
    * Performance improvemnt for the ArrayConverter

* Version 3.7.0 - 01 mrt 2021
    * Changed GetResponse in RestClient to protected
    * Added configuration for deterministic build

* Version 3.6.1 - 16 feb 2021
    * Fix for timing related exception when stopping an symbol order book

* Version 3.6.0 - 22 jan 2021
    * Added CommonVolume and CommonOpenTime to ICommonKline interface

* Version 3.5.0 - 11 jan 2021
    * Additional info on exception messages
    * Added support for rate limiting using credits

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