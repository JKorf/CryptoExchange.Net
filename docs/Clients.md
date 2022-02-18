---
title: General usage
nav_order: 2
---

## How to use the library

Each implementation generally provides two different clients, which will be the access point for the API's. First of the rest client, which is typically available via [ExchangeName]Client, and a socket client, which is generally named [ExchangeName]SocketClient. For example `BinanceClient` and `BinanceSocketClient`.

## Rest client
The rest client gives access to the Rest endpoint of the API. Rest endpoints are accessed by sending an HTTP request and receiving a response. The client is split in different sub-clients, which are named API Clients. These API clients are then again split in different topics. Typically a Rest client will look like this:

- KucoinClient
	- SpotApi
		- Account
		- ExchangeData
		- Trading
	- FuturesApi
		- Account
		- ExchangeData
		- Trading
		
This rest client has 2 different API clients, the `SpotApi` and the `FuturesApi`, each offering their own set of endpoints.  
*Requesting ticker info on the spot API*
```csharp
var client = new KucoinClient();
var tickersResult = kucoinClient.SpotApi.ExchangeData.GetTickersAsync();
```

Structuring the client like this should make it easier to find endpoints and allows for separate options and functionality for different API clients. For example, some API's have totally separate API's for futures, with different base addresses and different API credentials, while other API's have implemented this in the same API. Either way, this structure can facilitate a similar interface.

### Rest API client
The Api clients are parts of the total API with a common identifier. In the previous Kucoin example, it separates the Spot and the Futures API. This again is then separated into topics. Most Rest clients implement the following structure:  

**Account**  
Endpoints related to the user account. This can for example be endpoints for accessing account settings, or getting account balances. The endpoints in this topic will require API credentials to be provided in the client options.

**ExchangeData**  
Endpoints related to exchange data. Exchange data can be tied to the exchange, for example retrieving the symbols supported by the exchange and what the trading rules are, or can be more general market endpoints, such as getting the most recent trades for a symbol.
These endpoints generally don't require API credentials as they are publicly available.

**Trading**  
Endpoints related to trading. These are endpoints for placing and retrieving orders and retrieving trades made by the user. The endpoints in this topic will require API credentials to be provided in the client options.

### Processing request responses
Each request will return a WebCallResult<T> with the following properties:  
`ResponseHeaders`: The headers returned from the server
`ResponseStatusCode`: The status code as returned by the server
`Success`: Whether or not the call was successful. If successful the `Data` property will contain the resulting data, if not successful the `Error` property will contain more details about what the issue was
`Error`: Details on what went wrong with a call. Only filled when `Success` == `false`
`Data`: Data returned by the server

When processing the result of a call it should always be checked for success. Not doing so will result in `NullReference` exceptions.

*Check call result*
```csharp
var callResult = await kucoinClient.SpotApi.ExchangeData.GetTickersAsync();
if(!callResult.Success)
{
	Console.WriteLine("Request failed: " + callResult.Error);
	return;
}

Console.WriteLine("Result: " + callResult.Data);
```

## Socket client
The socket client gives access to the websocket API of an exchange. Websocket API's offer streams to which updates are pushed to which a client can listen. Some exchanges also offer some degree of functionality by allowing clients to give commands via the websocket, but most exchanges only allow this via the Rest API.
Just like the Rest client is divided in Rest Api clients, the Socket client is divided into Socket Api clients, each with their own range of API functionality. Socket Api clients are generally not divided into topics since the number of methods isn't as big as with the Rest client. To use the Kucoin client as example again, it looks like this:

```csharp

- KucoinSocketClient
	- SpotStreams
	- FuturesStreams

```
*Subscribing to updates for all tickers on the Spot Api*
```csharp
var subscribeResult = kucoinSocketClient.SpotStreams.SubscribeToAllTickerUpdatesAsync(DataHandler);
```

Subscribe methods require a data handler parameter, which is the method which will be called when an update is received from the server. This can be the name of a method or a lambda expression.  

*Method reference*
```csharp
await kucoinSocketClient.SpotStreams.SubscribeToAllTickerUpdatesAsync(DataHandler);

private static void DataHandler(DataEvent<KucoinStreamTick> updateData)
{
	// Process updateData
}
```

*Lambda*
```csharp
await kucoinSocketClient.SpotStreams.SubscribeToAllTickerUpdatesAsync(updateData =>
{
	// Process updateData
});
```

All updates are wrapped in a `DataEvent<>` object, which contain a `Timestamp`, `OriginalData`, `Topic`, and a `Data` property. The `Timestamp` is the timestamp when the data was received (not send!). `OriginalData` will contain the originally received data if this has been enabled in the client options. `Topic` will contain the topic of the update, which is typically the symbol or asset the update is for. The `Data` property contains the received update data.

*[WARNING] Do not use `using` statements in combination with constructing a `SocketClient`. Doing so will dispose the `SocketClient` instance when the subscription is done, which will result in the connection getting closed. Instead assign the socket client to a variable outside of the method scope.*

### Processing subscribe responses
Subscribing to a stream will return a `CallResult<UpdateSubscription>` object. This should be checked for success the same was as the [rest client](#processing-request-responses). The `UpdateSubscription` object can be used to listen for connection events of the socket connection. 
```csharp

var subscriptionResult = await kucoinSocketClient.SpotStreams.SubscribeToAllTickerUpdatesAsync(DataHandler);
if(!subscriptionResult.Success)
{
	Console.WriteLine("Failed to connect: " + subscriptionResult.Error);
	return;
}
subscriptionResult.Data.ConnectionLost += () =>
{
	Console.WriteLine("Connection lost");
};
subscriptionResult.Data.ConnectionRestored += (time) =>
{
	Console.WriteLine("Connection restored");
};

```

### Unsubscribing
When no longer interested in specific updates there are a few ways to unsubscribe. 

**Close subscription**  
Subscribing to an update stream will respond with an `UpdateSubscription` object. You can call the `CloseAsync()` method on this to no longer receive updates from that subscription:
```csharp
var subscriptionResult = await kucoinSocketClient.SpotStreams.SubscribeToAllTickerUpdatesAsync(DataHandler);
await subscriptionResult.Data.CloseAsync();
```

**Cancellation token**  
Passing in a `CancellationToken` as parameter in the subscribe method will allow you to cancel subscriptions by canceling the token. This can be useful when you need to cancel some streams but not others. In this example, both `BTC-USDT` and `ETH-USDT` streams get canceled, while the `KCS-USDT` stream remains active.
```csharp
var cts = new CancellationTokenSource();
var subscriptionResult1 = await kucoinSocketClient.SpotStreams.SubscribeToTickerUpdatesAsync("BTC-USDT", DataHandler, cts.Token);
var subscriptionResult2 = await kucoinSocketClient.SpotStreams.SubscribeToTickerUpdatesAsync("ETH-USDT", DataHandler, cts.Token);
var subscriptionResult3 = await kucoinSocketClient.SpotStreams.SubscribeToTickerUpdatesAsync("KCS-USDT", DataHandler);
Console.ReadLine();
cts.Cancel();
```

**Client unsubscribe**  
Subscriptions can also be closed by calling the `UnsubscribeAsync` method on the client, while providing either the `UpdateSubscription` object or the subscription id:
```csharp
var subscriptionResult = await kucoinSocketClient.SpotStreams.SubscribeToTickerUpdatesAsync("BTC-USDT", DataHandler);
await kucoinSocketClient.UnsubscribeAsync(subscriptionResult.Data);
// OR
await kucoinSocketClient.UnsubscribeAsync(subscriptionResult.Data.Id);
```

When you need to unsubscribe all current subscriptions on a client you can call `UnsubscribeAllAsync` on the client to unsubscribe all streams and close all connections.


## Dependency injection
Each library offers a `Add[Library]` extension method for `IServiceCollection`, which allows you to add the clients to the service collection. It also provides a callback for setting the client options. See this example for adding the `BinanceClient`:
```csharp
public void ConfigureServices(IServiceCollection services)
{
	services.AddBinance((restClientOptions, socketClientOptions) => {
		restClientOptions.ApiCredentials = new ApiCredentials("KEY", "SECRET");
		restClientOptions.LogLevel = LogLevel.Trace;

		socketClientOptions.ApiCredentials = new ApiCredentials("KEY", "SECRET");
	});
}
```
Doing client registration this way will add the `IBinanceClient` as a transient service, and the `IBinanceSocketClient` as a scoped service.

Alternatively, the clients can be registered manually:
```csharp
BinanceClient.SetDefaultOptions(new BinanceClientOptions
{
	ApiCredentials = new ApiCredentials("KEY", "SECRET"),
	LogLevel = LogLevel.Trace
});

BinanceSocketClient.SetDefaultOptions(new BinanceSocketClientOptions
{
	ApiCredentials = new ApiCredentials("KEY", "SECRET"),
});

services.AddTransient<IBinanceClient, BinanceClient>();
services.AddScoped<IBinanceSocketClient, BinanceSocketClient>();
```
