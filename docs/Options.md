---
title: Client options
nav_order: 3
---

## Setting client options

Each implementation can be configured using client options. There are 2 ways to provide these, either via `[client].SetDefaultOptions([options]);`, or in the constructor of the client. The examples here use the `BinanceClient`, but usage is the same for each client.

*Set the default options to use for new clients*
```csharp

BinanceClient.SetDefaultOptions(new BinanceClientOptions
{
	LogLevel = LogLevel.Trace,
	ApiCredentials = new ApiCredentials("KEY", "SECRET")
});

```

*Set the options to use for a single new client*
```csharp

var client = new BinanceClient(new BinanceClientOptions
{
	LogLevel = LogLevel.Trace,
	ApiCredentials = new ApiCredentials("KEY", "SECRET")
});

```

When calling `SetDefaultOptions` each client created after that will use the options that were set, unless the specific option is overriden in the options that were provided to the client. Consider the following example:
```csharp

BinanceClient.SetDefaultOptions(new BinanceClientOptions
{
	LogLevel = LogLevel.Trace,
	OutputOriginalData = true
});

var client = new BinanceClient(new BinanceClientOptions
{
	LogLevel = LogLevel.Debug,
	ApiCredentials = new ApiCredentials("KEY", "SECRET")
});

```

The client instance will have the following options:  
`LogLevel = Debug`  
`OutputOriginalData = true`  
`ApiCredentials = set`  

## Api options
The options are divided in two categories. The basic options, which will apply to everything the client does, and the Api options, which is limited to the specific API client (see [Clients](https://github.com/JKorf/CryptoExchange.Net/wiki/Clients)).

```csharp

var client = new BinanceClient(new BinanceClientOptions
{
	LogLevel = LogLevel.Debug,
	ApiCredentials = new ApiCredentials("GENERAL-KEY", "GENERAL-SECRET"),
	SpotApiOptions = new BinanceApiClientOptions
	{
		ApiCredentials = new ApiCredentials("SPOT-KEY", "SPOT-SECRET")              ,
		BaseAddress = BinanceApiAddresses.Us.RestClientAddress
	}
});

```

The options provided in the SpotApiOptions are only applied to the SpotApi (`client.SpotApi.XXX` endpoints), while the base options are applied to everything. This means that the spot endpoints will use the "SPOT-KEY" credentials, while all other endpoints (`client.UsdFuturesApi.XXX` / `client.CoinFuturesApi.XXX`) will use the "GENERAL-KEY" credentials.

## CryptoExchange.Net options definitions
All clients have access to the following options, specific implementations might have additional options.

**Base client options**  

|Option|Description|Default|
|------|-----------|-------|
|`LogWriters`| A list of `ILogger`s to handle log messages. | `new List<ILogger> { new DebugLogger() }` |
|`LogLevel`| The minimum log level before passing messages to the `LogWriters`. Messages with a more verbose level than the one specified here will be ignored. Setting this to `null` will pass all messages to the `LogWriters`.| `LogLevel.Information`
|`OutputOriginalData`|If set to `true` the originally received Json data will be output as well as the deserialized object. For `RestClient` calls the data will be in the `WebCallResult<T>.OriginalData` property, for `SocketClient` subscriptions the data will be available in the `DataEvent<T>.OriginalData` property when receiving an update. | `false`
|`ApiCredentials`| The API credentials to use for accessing protected endpoints. Typically a key/secret combination. Note that this is a `default` value for all API clients, and can be overridden per API client. See the `Base Api client options`| `null`
|`Proxy`|The proxy to use for connecting to the API.| `null`

**Rest client options (extension of base client options)** 
 
|Option|Description|Default|
|------|-----------|-------|
|`RequestTimeout`|The time out to use for requests.|`TimeSpan.FromSeconds(30)`|
|`HttpClient`|The `HttpClient` instance to use for making requests. When creating multiple `RestClient` instances a single `HttpClient` should be provided to prevent each client instance from creating its own. *[WARNING] When providing the `HttpClient` instance in the options both the `RequestTimeout` and `Proxy` client options will be ignored and should be set on the provided `HttpClient` instance.*| `null` |

**Socket client options (extension of base client options)**  

|Option|Description|Default|
|------|-----------|-------|
|`AutoReconnect`|Whether or not the socket should automatically reconnect when disconnected.|`true`
|`ReconnectInterval`|The time to wait between connection tries when reconnecting.|`TimeSpan.FromSeconds(5)`
|`SocketResponseTimeout`|The time in which a response is expected on a request before giving a timeout.|`TimeSpan.FromSeconds(10)`
|`SocketNoDataTimeout`|If no data is received after this timespan then assume the connection is dropped. This is mainly used for API's which have some sort of ping/keepalive system. For example; the Bitfinex API will sent a heartbeat message every 15 seconds, so the `SocketNoDataTimeout` could be set to 20 seconds. On API's without such a mechanism this might not work because there just might not be any update while still being fully connected. | `default(TimeSpan)` (no timeout)
|`SocketSubscriptionsCombineTarget`|The amount of subscriptions that should be made on a single socket connection. Not all exchanges support multiple subscriptions on a single socket. Setting this to a higher number increases subscription speed because not every subscription needs to connect to the server, but having more subscriptions on a single connection will also increase the amount of traffic on that single connection, potentially leading to issues.| Depends on implementation
|`MaxReconnectTries`|The maximum amount of tries for reconnecting|`null` (infinite)
|`MaxResubscribeTries`|The maximum amount of tries for resubscribing after successfully reconnecting the socket|5
|`MaxConcurrentResubscriptionsPerSocket`|The maximum number of concurrent resubscriptions per socket when resubscribing after reconnecting|5

**Base Api client options**  

|Option|Description|Default|
|------|-----------|-------|
|`ApiCredentials`|The API credentials to use for this specific API client. Will override any credentials provided in the base client options|
|`BaseAddress`|The base address to the API. All calls to the API will use this base address as basis for the endpoints. This allows for swapping to test API's or swapping to a different cluster for example. Available base addresses are defined in the [Library]ApiAddresses helper class, for example `KucoinApiAddresses`|Depends on implementation

**Options for Rest Api Client (extension of base api client options)**  

|Option|Description|Default|
|------|-----------|-------|
|`RateLimiters`|A list of `IRateLimiter`s to use.|`new List<IRateLimiter>()`|
|`RateLimitingBehaviour`|What should happen when a rate limit is reached.|`RateLimitingBehaviour.Wait`|

**Options for Socket Api Client (extension of base api client options)**  
There are currently no specific options for socket API clients, the base API options are still available.
