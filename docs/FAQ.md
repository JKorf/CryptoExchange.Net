---
title: FAQ
nav_order: 11
---

## Frequently asked questions

### I occasionally get a NullReferenceException, what's wrong?
You probably don't check the result status of a call and just assume the data is always there. `NullReferenceExecption`s will happen when you have code like this `var symbol = client.GetTickersAync().Result.Data.Symbol` because the `Data` property is null when the call fails. Instead check if the call is successful like this:
```csharp
var tickerResult = await client.GetTickersAync();
if(!tickerResult.Success)
{
  // Handle error
}
else
{
  // Handle result, it is now safe to access the Data property
  var symbol = tickerResult.Data.Symbol;
}
```

### The socket client stops sending updates after a little while
You probably didn't keep a reference to the socket client and it got disposed.
Instead of subscribing like this:
```csharp
private void SomeMethod()
{
  var socketClient = new BinanceSocketClient();
  socketClient.Spot.SubscribeToOrderBookUpdates("BTCUSDT", data => {
	// Handle data
  });
}
```
Subscribe like this:
```csharp
private BinanceSocketClient _socketClient = new BinanceSocketClient();

// .. rest of the class

private void SomeMethod()
{
  _socketClient.Spot.SubscribeToOrderBookUpdates("BTCUSDT", data => {
	// Handle data
  });
}

```

### Can I use the TestNet/US/other API with this library
Yes, generally these are all supported and can be configured by setting the BaseAddress in the client options. Some known API addresses should be available in the [Exchange]ApiAddresses class. For example:
```csharp
var client = new BinanceClient(new BinanceClientOptions
{
	SpotApiOptions = new BinanceApiClientOptions
	{
		BaseAddress = BinanceApiAddresses.TestNet.RestClientAddress
	},
	UsdFuturesApiOptions = new BinanceApiClientOptions
	{
		BaseAddress = BinanceApiAddresses.TestNet.UsdFuturesRestClientAddress
	}
});
```