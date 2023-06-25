---
title: Client options
nav_order: 4
---

## Setting client options

Each implementation can be configured using client options. There are 2 ways to provide these, either via `[client].SetDefaultOptions([options]);`, or in the constructor of the client. The examples here use the `BinanceClient`, but usage is the same for each client.

*Set the default options to use for new clients*
```csharp

BinanceClient.SetDefaultOptions(options =>
{
	options.OutputOriginalData = true;
	options.ApiCredentials = new ApiCredentials("KEY", "SECRET");
});

```

*Set the options to use for a single new client*
```csharp

var client = new BinanceClient(options =>
{
	options.OutputOriginalData = true;
	options.ApiCredentials = new ApiCredentials("KEY", "SECRET");
});

```

When calling `SetDefaultOptions` each client created after that will use the options that were set, unless the specific option is overriden in the options that were provided to the client. Consider the following example:
```csharp

BinanceClient.SetDefaultOptions(options =>
{
	options.OutputOriginalData = true;
});

var client = new BinanceClient(options =>
{
	options.OutputOriginalData = false;
});

```

The client instance will have the following options:  
`OutputOriginalData = false`  

## Api options
The options are divided in two categories. The basic options, which will apply to everything the client does, and the Api options, which is limited to the specific API client (see [Clients](https://jkorf.github.io/CryptoExchange.Net/Clients.html)).

```csharp

var client = new BinanceRestClient(options =>
{
	options.ApiCredentials = new ApiCredentials("GENERAL-KEY", "GENERAL-SECRET"),
	options.SpotOptions.ApiCredentials = new ApiCredentials("SPOT-KEY", "SPOT-SECRET");
});

```

The options provided in the SpotApiOptions are only applied to the SpotApi (`client.SpotApi.XXX` endpoints), while the base options are applied to everything. This means that the spot endpoints will use the "SPOT-KEY" credentials, while all other endpoints (`client.UsdFuturesApi.XXX` / `client.CoinFuturesApi.XXX`) will use the "GENERAL-KEY" credentials.

## CryptoExchange.Net options definitions
All clients have access to the following options, specific implementations might have additional options.

**Base client options**  

|Option|Description|Default|
|------|-----------|-------|
|`OutputOriginalData`|If set to `true` the originally received Json data will be output as well as the deserialized object. For `RestClient` calls the data will be in the `WebCallResult<T>.OriginalData` property, for `SocketClient` subscriptions the data will be available in the `DataEvent<T>.OriginalData` property when receiving an update. | `false`
|`ApiCredentials`| The API credentials to use for accessing protected endpoints. Can either be an API key/secret using Hmac encryption or an API key/private key using RSA encryption for exchanges that support that. See [Credentials](#credentials). Note that this is a `default` value for all API clients, and can be overridden per API client. See the `Base Api client options`| `null`
|`Proxy`|The proxy to use for connecting to the API.| `null`
|`RequestTimeout`|The timeout for client requests to the server| `TimeSpan.FromSeconds(20)`

**Rest client options (extension of base client options)** 
 
|Option|Description|Default|
|------|-----------|-------|
|`AutoTimestamp`|Whether or not the library should attempt to sync the time between the client and server. If the time between server and client is not in sync authentication errors might occur. This option should be disabled when the client time sure to be in sync.|`true`|
|`TimestampRecalculationInterval`|The interval of how often the time synchronization between client and server should be executed| `TimeSpan.FromHours(1)`
|`Environment`|The environment the library should talk to. Some exchanges have testnet/sandbox environments which can be used instead of the real exchange. The environment option can be used to switch between different trade environments|`Live environment`

**Socket client options (extension of base client options)**  

|Option|Description|Default|
|------|-----------|-------|
|`AutoReconnect`|Whether or not the socket should attempt to automatically reconnect when disconnected.|`true`
|`ReconnectInterval`|The time to wait between connection tries when reconnecting.|`TimeSpan.FromSeconds(5)`
|`SocketResponseTimeout`|The time in which a response is expected on a request before giving a timeout.|`TimeSpan.FromSeconds(10)`
|`SocketNoDataTimeout`|If no data is received after this timespan then assume the connection is dropped. This is mainly used for API's which have some sort of ping/keepalive system. For example; the Bitfinex API will sent a heartbeat message every 15 seconds, so the `SocketNoDataTimeout` could be set to 20 seconds. On API's without such a mechanism this might not work because there just might not be any update while still being fully connected. | `default(TimeSpan)` (no timeout)
|`SocketSubscriptionsCombineTarget`|The amount of subscriptions that should be made on a single socket connection. Not all exchanges support multiple subscriptions on a single socket. Setting this to a higher number increases subscription speed because not every subscription needs to connect to the server, but having more subscriptions on a single connection will also increase the amount of traffic on that single connection, potentially leading to issues.| Depends on implementation
|`MaxConcurrentResubscriptionsPerSocket`|The maximum number of concurrent resubscriptions per socket when resubscribing after reconnecting|5
|`MaxSocketConnections`|The maximum amount of distinct socket connections|`null`
|`DelayAfterConnect`|The time to wait before sending messages after connecting to the server.|`TimeSpan.Zero`
|`Environment`|The environment the library should talk to. Some exchanges have testnet/sandbox environments which can be used instead of the real exchange. The environment option can be used to switch between different trade environments|`Live environment`

**Base Api client options**  

|Option|Description|Default|
|------|-----------|-------|
|`ApiCredentials`|If set to `true` the originally received Json data will be output as well as the deserialized object. For `RestClient` calls the data will be in the `WebCallResult<T>.OriginalData` property, for `SocketClient` subscriptions the data will be available in the `DataEvent<T>.OriginalData` property when receiving an update. Overrides the Base client options `OutputOriginalData` option if set| `false`
|`OutputOriginalData`|The base address to the API. All calls to the API will use this base address as basis for the endpoints. This allows for swapping to test API's or swapping to a different cluster for example. Available base addresses are defined in the [Library]ApiAddresses helper class, for example `KucoinApiAddresses`|Depends on implementation

**Options for Rest Api Client (extension of base api client options)**  

|Option|Description|Default|
|------|-----------|-------|
|`RateLimiters`|A list of `IRateLimiter`s to use.|`new List<IRateLimiter>()`|
|`RateLimitingBehaviour`|What should happen when a rate limit is reached.|`RateLimitingBehaviour.Wait`|
|`AutoTimestamp`|Whether or not the library should attempt to sync the time between the client and server. If the time between server and client is not in sync authentication errors might occur. This option should be disabled when the client time sure to be in sync. Overrides the Rest client options `AutoTimestamp` option if set|`null`|
|`TimestampRecalculationInterval`|The interval of how often the time synchronization between client and server should be executed. Overrides the Rest client options `TimestampRecalculationInterval` option if set| `TimeSpan.FromHours(1)`

**Options for Socket Api Client (extension of base api client options)**  

|Option|Description|Default|
|------|-----------|-------|
|`SocketNoDataTimeout`|If no data is received after this timespan then assume the connection is dropped. This is mainly used for API's which have some sort of ping/keepalive system. For example; the Bitfinex API will sent a heartbeat message every 15 seconds, so the `SocketNoDataTimeout` could be set to 20 seconds. On API's without such a mechanism this might not work because there just might not be any update while still being fully connected. Overrides the Socket client options `SocketNoDataTimeout` option if set | `default(TimeSpan)` (no timeout)
|`MaxSocketConnections`|The maximum amount of distinct socket connections. Overrides the Socket client options `MaxSocketConnections` option if set |`null`

## Credentials
Credentials are supported in 3 formats in the base library:  

|Type|Description|Example|
|----|-----------|-------|
|`Hmac`|An API key + secret combination. The API key is send with the request and the secret is used to sign requests. This is the default authentication method on all exchanges. |`options.ApiCredentials = new ApiCredentials("51231f76e-9c503548-8fabs3f-rfgf12mkl3", "556be32-d563ba53-faa2dfd-b3n5c", CredentialType.Hmac);`|
|`RsaPem`|An API key + a public and private key pair generated by the user. The public key is shared with the exchange, while the private key is used to sign requests. This CredentialType expects the private key to be in .pem format and is only supported in .netstandard2.1 due to limitations of the framework|`options.ApiCredentials = new ApiCredentials("432vpV8daAaXAF4Qg", ""-----BEGIN PRIVATE KEY-----[PRIVATEKEY]-----END PRIVATE KEY-----", CredentialType.RsaPem);`|
|`RsaXml`|An API key + a public and private key pair generated by the user. The public key is shared with the exchange, while the private key is used to sign requests. This CredentialType expects the private key to be in xml format and is supported in .netstandard2.0 and .netstandard2.1, but it might mean the private key needs to be converted from the original format to xml|`options.ApiCredentials = new ApiCredentials("432vpV8daAaXAF4Qg", "<RSAKeyValue>[PRIVATEKEY]</RSAKeyValue>", CredentialType.RsaXml);`|