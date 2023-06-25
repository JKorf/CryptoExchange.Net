---
title: Logging
nav_order: 5
---

## Configuring logging
The library offers extensive logging, which depends on the dotnet `Microsoft.Extensions.Logging.ILogger` interface. This should provide ease of use when connecting the library logging to your existing logging implementation.

*Configure logging to write to the console*
```csharp
IServiceCollection services = new ServiceCollection();
services
	.AddBinance()
    .AddLogging(options =>
    {
        options.SetMinimumLevel(LogLevel.Trace);
        options.AddConsole();
    });
```

The library provides a TraceLogger ILogger implementation which writes log messages using `Trace.WriteLine`, but any other logging library can be used.

*Configure logging to use trace logging*
```csharp
IServiceCollection serviceCollection = new ServiceCollection();
serviceCollection.AddBinance()
    .AddLogging(options =>
    {
        options.SetMinimumLevel(LogLevel.Trace);
        options.AddProvider(new TraceLoggerProvider());
    });
```

### Using an external logging library and dotnet DI

With for example an ASP.Net Core or Blazor project the logging can be configured by the dependency container, which can then automatically be used be the clients.
The next example shows how to use Serilog. This assumes the `Serilog.AspNetCore` package (https://github.com/serilog/serilog-aspnetcore) is installed.

*Using serilog:*
```csharp
using Binance.Net;
using Serilog;

Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.Console()
        .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddBinance();
builder.Host.UseSerilog();
var app = builder.Build();

// startup

app.Run();
```

### Logging without dotnet DI
If you don't have a dependency injection service available because you are for example working on a simple console application you have 2 options for logging.

#### Create a ServiceCollection manually and get the client from the service provider

```csharp
IServiceCollection serviceCollection = new ServiceCollection();
serviceCollection.AddBinance();
serviceCollection.AddLogging(options =>
{
	options.SetMinimumLevel(LogLevel.Trace);
	options.AddConsole();
}).BuildServiceProvider();

var client = serviceCollection.GetRequiredService<IBinanceRestClient>();

```

#### Create a LoggerFactory manually

```csharp
var logFactory = new LoggerFactory();
logFactory.AddProvider(new ConsoleLoggerProvider());
var binanceClient = new BinanceRestClient(new HttpClient(), logFactory, options => { });
```

## Providing logging for issues
A big debugging tool when opening an issue on Github is providing logging of what data caused the issue. This can be provided two ways, via the `OriginalData` property of the call result or data event, or collecting the Trace logging.
### OriginalData
This is only useful when there is an issue in deserialization. So either a call result is giving a Deserialization error, or the result has a value that is unexpected. If that is the issue, please provide the original data that is received so the deserialization issue can be resolved based on the received data.
By default the `OriginalData` property in the `WebCallResult`/`DataEvent` object is not filled as saving the original data has a (very small) performance penalty. To save the original data in the `OriginalData` property the `OutputOriginalData` option should be set to `true` in the client options.  
*Enabled output data*
```csharp
var client = new BinanceClient(options =>
{
	options.OutputOriginalData = true
});
``` 

*Accessing original data*
```csharp
// Rest request
var tickerResult = await client.SpotApi.ExchangeData.GetTickersAsync();
var originallyReceivedData = tickerResult.OriginalData;

// Socket update
await client.SpotStreams.SubscribeToAllTickerUpdatesAsync(update => {
	var originallyRecievedData = update.OriginalData;
});
```

### Trace logging
Trace logging, which is the most verbose log level, will show everything the library does and includes the data that was send and received.  
Output data will look something like this:
```
2021-12-17 10:40:42:296 | Debug | Binance    | Client configuration: LogLevel: Trace, Writers: 1, OutputOriginalData: False, Proxy: -, AutoReconnect: True, ReconnectInterval: 00:00:05, MaxReconnectTries: , MaxResubscribeTries: 5, MaxConcurrentResubscriptionsPerSocket: 5, SocketResponseTimeout: 00:00:10, SocketNoDataTimeout: 00:00:00, SocketSubscriptionsCombineTarget: , CryptoExchange.Net: v5.0.0.0, Binance.Net: v8.0.0.0
2021-12-17 10:40:42:410 | Debug | Binance    | [15] Creating request for https://api.binance.com/api/v3/ticker/24hr
2021-12-17 10:40:42:439 | Debug | Binance    | [15] Sending GET request to https://api.binance.com/api/v3/ticker/24hr?symbol=BTCUSDT with headers Accept=[application/json], X-MBX-APIKEY=[XXX]
2021-12-17 10:40:43:024 | Debug | Binance    | [15] Response received in 571ms: {"symbol":"BTCUSDT","priceChange":"-1726.47000000","priceChangePercent":"-3.531","weightedAvgPrice":"48061.51544204","prevClosePrice":"48901.44000000","lastPrice":"47174.97000000","lastQty":"0.00352000","bidPrice":"47174.96000000","bidQty":"0.65849000","askPrice":"47174.97000000","askQty":"0.13802000","openPrice":"48901.44000000","highPrice":"49436.43000000","lowPrice":"46749.55000000","volume":"33136.69765000","quoteVolume":"1592599905.80360790","openTime":1639647642763,"closeTime":1639734042763,"firstId":1191596486,"lastId":1192649611,"count":1053126}
```
When opening an issue, please provide this logging when available.

### Example of serilog config and minimal API's

```csharp
using Binance.Net;
using Binance.Net.Interfaces.Clients;
using Serilog;

Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.Console()
        .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddBinance();
builder.Host.UseSerilog();
var app = builder.Build();

// startup

app.Urls.Add("http://localhost:3000");

app.MapGet("/price/{symbol}", async (string symbol) =>
{
    var client = app.Services.GetRequiredService<IBinanceRestClient>();
    var result = await client.SpotApi.ExchangeData.GetPriceAsync(symbol);
    return result.Data.Price;
});

app.Run();
```