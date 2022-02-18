---
title: Migrate v4 to v5
nav_order: 9
---

## Migrating from version 4 to version 5
When updating your code from version 4 implementations to version 5 implementations you will encounter a fair bit of breaking changes. Here is the general outline for changes made in the CryptoExchange.Net library. For more specific changes for each library visit the library migration guide.

*NOTE when updating it is not possible to have some client implementations use a V4 version and some clients a V5. When updating all libraries should be migrated*

## Client structure
The client structure has been changed to make clients more consistent across different implementations. Clients using V4 either had `client.Method()`, `client.[Api].Method()` or `client.[Api].[Topic].Method()`.  

This has been unified to be `client.[Api]Api.[Topic].Method()`:  
`bittrexClient.GetTickersAsync()` -> `bittrexClient.SpotApi.ExchangeData.GetTickersAsync()`  
`kucoinClient.Spot.GetTickersAsync()` -> `kucoinClient.SpotApi.ExchangeData.GetTickersAsync()`  
`binanceClient.Spot.Market.GetTickersAsync()` -> `binanceClient.SpotApi.ExchangeData.GetTickersAsync()`  

Socket clients are restructured as `client.[Api]Streams.Method()`:
`bittrexClient.SpotStreams.SubscribeToTickerUpdatesAsync()`  
`kucoinClient.SpotStreams.SubscribeToTickerUpdatesAsync()`  
`binanceClient.SpotStreams.SubscribeToAllTickerUpdatesAsync()`  


## Options structure
The options have been changed in 2 categories, options for the whole client, and options only for a specific sub Api. Some options might no longer be available on the base level and should be set on the Api options instead, for example the `BaseAddress`. 
The following example sets some basic options, and specifically overwrites the USD futures Api options to use the test net address and different Api credentials:  
*V4*
```csharp
var binanceClient = new BinanceClient(new BinanceApiClientOptions{
	LogLevel = LogLevel.Trace,
	RequestTimeout = TimeSpan.FromSeconds(60),
	ApiCredentials = new ApiCredentials("API KEY", "API SECRET"),
	BaseAddressUsdtFutures = new ApiCredentials("OTHER API KEY ONLY FOR USD FUTURES", "OTHER API SECRET ONLY FOR USD FUTURES")
	// No way to set separate credentials for the futures API
});
```

*V5*
```csharp
var binanceClient = new BinanceClient(new BinanceClientOptions()
{
	// Client options
	LogLevel = LogLevel.Trace,
	RequestTimeout = TimeSpan.FromSeconds(60),
	ApiCredentials = new ApiCredentials("API KEY", "API SECRET"),
	
	// Set options specifically for the USD futures API
	UsdFuturesApiOptions = new BinanceApiClientOptions
	{
		BaseAddress = BinanceApiAddresses.TestNet.UsdFuturesRestClientAddress,
		ApiCredentials = new ApiCredentials("OTHER API KEY ONLY FOR USD FUTURES", "OTHER API SECRET ONLY FOR USD FUTURES")
	}
});
```
See [Client options](https://github.com/JKorf/CryptoExchange.Net/wiki/Options) for more details on the specific options.

## IExchangeClient
The `IExchangeClient` has been replaced by the `ISpotClient` and `IFuturesClient`. Where previously the `IExchangeClient` was implemented on the base client level, the `ISpotClient`/`IFuturesClient` have been implemented on the sub-Api level.
This, in combination with the client restructuring, allows for more logically implemented interfaces, see this example:  
*V4*
```csharp
var spotClients = new [] {
	(IExhangeClient)binanceClient,
	(IExchangeClient)bittrexClient,
	(IExchangeClient)kucoinClient.Spot
};

// There was no common implementation for futures client
```

*V5*
```csharp
var spotClients = new [] {
	binanceClient.SpotApi.CommonSpotClient,
	bittrexClient.SpotApi.CommonSpotClient,
	kucoinClient.SpotApi.CommonSpotClient
};

var futuresClients = new [] {
	binanceClient.UsdFuturesApi.CommonFuturesClient,
	kucoinClient.FuturesApi.CommonFuturesClient
};
```

Where the IExchangeClient was returning interfaces which were implemented by models from the exchange, the `ISpotClient`/`IFuturesClient` returns actual objects defined in the `CryptoExchange.Net` library. This shifts the responsibility of parsing 
the library model to a shared model from the model class to the client class, which makes more sense and removes the need for separate library models to implement the same mapping logic. It also removes the need for the `Common` prefix on properties:  
*V4*
```csharp
var kline = await ((IExhangeClient)binanceClient).GetKlinesAysnc(/*params*/);
var closePrice = kline.CommonClose;
```

*V5*
```csharp
var kline = await binanceClient.SpotApi.ComonSpotClient.GetKlinesAysnc(/*params*/);
var closePrice = kline.ClosePrice;
```

For more details on the interfaces see [Common interfaces](interfaces.html)