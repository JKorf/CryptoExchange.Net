---
title: Migrate v5 to v6
nav_order: 10
---

## Migrating from version 5 to version 6
When updating your code from version 5 implementations to version 6 implementations you will encounter some breaking changes. Here is the general outline of changes made in the CryptoExchange.Net library. For more specific changes for each library visit the library migration guide.

*NOTE when updating it is not possible to have some client implementations use a V5 version and some clients a V6. When updating all libraries should be migrated*

## Rest client name
To be more clear about different clients for different API's the rest client implementations have been renamed from [Exchange]Client to [Exchange]RestClient. This makes it more clear that it only implements the Rest API and the [Exchange]SocketClient the Socket API.

## Options
Option parameters have been changed to a callback instead of an options object. This makes processing of the options easier and is in line with how dotnet handles option configurations.

**BaseAddress**  
The BaseAddress option has been replaced by the Environment option. The Environment options allows for selection/switching between different trade environments more easily. For example the environment can be switched between a testnet and live by changing only a single line instead of having to change all BaseAddresses.  

**LogLevel/LogWriters**  
The logging options have been removed and are now inherited by the DI configuration. See [Logging](https://jkorf.github.io/CryptoExchange.Net/Logging.html) for more info.  

**HttpClient**
The HttpClient will now be received by the DI container instead of having to pass it manually. When not using DI it is still possible to provide a HttpClient, but it is now located in the client constructor.

*V5*  
```csharp
var client = new BinanceClient(new BinanceClientOptions(){
    OutputOriginalData = true,
    SpotApiOptions = new RestApiOptions {
      BaseAddress = BinanceApiAddresses.TestNet.RestClientAddress
    }
    // Other options
});
```

*V6*  
```csharp
var client = new BinanceClient(options => {
    options.OutputOriginalData = true;
    options.Environment = BinanceEnvironment.Testnet;
    // Other options
});
```

## Socket api name
As socket API's are often more than just streams to subscribe to the name of the socket API clients have been changed from [Topic]Streams to [Topic]Api which matches the rest API client names. For example `SpotStreams` has become `SpotApi`, so `binanceSocketClient.UsdFuturesStreams.SubscribeXXX` has become `binanceSocketClient.UsdFuturesApi.SubscribeXXX`.

## Add[Exchange] extension method
With the change in options providing the DI extension methods for the IServiceCollection have also been changed slightly. Also the socket clients will now be registered as Singleton by default instead of Scoped.

*V5*  
```csharp
builder.Services.AddKucoin((restOpts, socketOpts) =>
{
    restOpts.LogLevel = LogLevel.Debug;
    restOpts.ApiCredentials = new KucoinApiCredentials("KEY", "SECRET", "PASS");
    socketOpts.LogLevel = LogLevel.Debug;
    socketOpts.ApiCredentials = new KucoinApiCredentials("KEY", "SECRET", "PASS");
}, ServiceLifetime.Singleton);
```

*V6*  
```csharp
builder.Services.AddKucoin((restOpts) =>
{
    restOpts.ApiCredentials = new KucoinApiCredentials("KEY", "SECRET", "PASS");
},
(socketOpts) =>
{
    socketOpts.ApiCredentials = new KucoinApiCredentials("KEY", "SECRET", "PASS");
});
```