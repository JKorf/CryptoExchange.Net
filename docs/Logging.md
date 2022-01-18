---
title: Log config
nav_order: 4
---

## Configuring logging
The library offers extensive logging, for which you can supply your own logging implementation. The logging can be configured via the client options (see [Client options](https://github.com/JKorf/CryptoExchange.Net/wiki/Options)). The examples here are using the `BinanceClient` but they should be the same for each implementation.

Logging is based on the `Microsoft.Extensions.Logging.ILogger` interface. This should provide ease of use when connecting the library logging to your existing logging implementation.

## Serilog
To make the CryptoExchange.Net logging write to the Serilog logger you can use the following methods, depending on the type of project you're using. The following examples assume that the `Serilog.Sinks.Console` package is already installed.

### Dotnet hosting

With for example an ASP.Net Core or Blazor project the logging can be added to the dependency container, which you can then use to inject it into the client. Make sure to install the `Serilog.AspNetCore` package (https://github.com/serilog/serilog-aspnetcore).

<Details>
<Summary>
Using ILogger injection

</Summary>
<BlockQuote>
Adding `UseSerilog()` in the `CreateHostBuilder` will add the Serilog logging implementation as an ILogger which you can inject into implementations.

*Configuring Serilog as ILogger:*
```csharp

public static void Main(string[] args)
{
	Log.Logger = new LoggerConfiguration()
		.MinimumLevel.Debug()
		.WriteTo.Console()
		.CreateLogger();

	CreateHostBuilder(args).Build().Run();
}

public static IHostBuilder CreateHostBuilder(string[] args) =>
	Host.CreateDefaultBuilder(args)
		.UseSerilog()
		.ConfigureWebHostDefaults(webBuilder =>
		{
			webBuilder.UseStartup<Startup>();
		});
				
```


*Injecting ILogger:*
```csharp

public class BinanceDataProvider
{
	BinanceClient _client;

	public BinanceDataProvider(ILogger<BinanceDataProvider> logger)
	{
		_client = new BinanceClient(new BinanceClientOptions
		{
			LogLevel = LogLevel.Trace,
			LogWriters = new List<ILogger> { logger }
		});

	}
}

```

</BlockQuote>
</Details>

<Details>
<Summary>
Using Add[Library] extension method

</Summary>
<BlockQuote>
When using the `Add[Library]` extension method, for instance `AddBinance()`, there is a small issue that there is no available `ILogger<>` yet when adding the library. This can be solved as follows:

*Configuring Serilog as ILogger:*
```csharp

public static void Main(string[] args)
{
	Log.Logger = new LoggerConfiguration()
		.MinimumLevel.Debug()
		.WriteTo.Console()
		.CreateLogger();

	CreateHostBuilder(args).Build().Run();
}

public static IHostBuilder CreateHostBuilder(string[] args) =>
	Host.CreateDefaultBuilder(args)
		.ConfigureWebHostDefaults(webBuilder =>
		{
			webBuilder.UseStartup(
				context => new Startup(context.Configuration, LoggerFactory.Create(config => config.AddSerilog()) )); // <- this allows us to use ILoggerFactory in the Startup.cs
		});
				
```


*Injecting ILogger:*
```csharp

public class Startup
{
	private ILoggerFactory _loggerFactory;

	public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
	{
		Configuration = configuration;
		_loggerFactory = loggerFactory;
	}
	
	/* .. rest of class .. */
	
	public void ConfigureServices(IServiceCollection services)
	{
		services.AddBinance((restClientOptions, socketClientOptions) => {
			// Point the logging to use the ILogger configuration
			restClientOptions.LogWriters = new List<ILogger> { _loggerFactory.CreateLogger<IBinanceClient>() };
		});
		
		// Rest of service registrations
	}
}

```

</BlockQuote>
</Details>

### Console application
If you don't have a dependency injection service available because you are for example working on a simple console application you can use a slightly different approach.  

*Configuring Serilog as ILogger:*
```csharp
var serilogLogger = new LoggerConfiguration()
	.MinimumLevel.Debug()
	.WriteTo.Console()
	.CreateLogger();

var loggerFactory = (ILoggerFactory)new LoggerFactory();
loggerFactory.AddSerilog(serilogLogger);

```

*Injecting ILogger:*
```csharp

var client = new BinanceClient(new BinanceClientOptions
{
	LogLevel = LogLevel.Trace,
	LogWriters = new List<ILogger> { loggerFactory.CreateLogger("") }
});
```

The `BinanceClient` will now write the logging it produces to the Serilog logger.

## Log4Net

To make the CryptoExchange.Net logging write to the Log4Net logge with for example an ASP.Net Core or Blazor project the logging can be added to the dependency container, which you can then use to inject it into the client you're using. Make sure to install the `Microsoft.Extensions.Logging.Log4Net.AspNetCore` package (https://github.com/huorswords/Microsoft.Extensions.Logging.Log4Net.AspNetCore). 
Adding `AddLog4Net()` in the `ConfigureLogging` call will add the Log4Net implementation as an ILogger which you can inject into implementations. Make sure you have a log4net.config configuration file in your project.

*Configuring Log4Net as ILogger:*
```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
	Host.CreateDefaultBuilder(args)
		.ConfigureWebHostDefaults(webBuilder =>
		{
			webBuilder.ConfigureLogging(logging =>
			{
				logging.AddLog4Net();
				logging.SetMinimumLevel(LogLevel.Trace);
			});
			webBuilder.UseStartup<Startup>();
		});
```

*Injecting ILogger:*
```csharp

public class BinanceDataProvider
{
	BinanceClient _client;

	public BinanceDataProvider(ILogger<BinanceDataProvider> logger)
	{
		_client = new BinanceClient(new BinanceClientOptions
		{
			LogLevel = LogLevel.Trace,
			LogWriters = new List<ILogger> { logger }
		});

	}
}

```

If you don't have the Dotnet dependency container available you'll need to provide your own ILogger implementation. See [Custom logger](#custom-logger).

## NLog
To make the CryptoExchange.Net logging write to the NLog logger you can use the following ways, depending on the type of project you're using.

### Dotnet hosting

With for example an ASP.Net Core or Blazor project the logging can be added to the dependency container, which you can then use to inject it into the client you're using. Make sure to install the `NLog.Web.AspNetCore` package (https://github.com/NLog/NLog/wiki/Getting-started-with-ASP.NET-Core-5). 
Adding `UseNLog()` to the `CreateHostBuilder()` method will add the NLog implementation as an ILogger which you can inject into implementations. Make sure you have a nlog.config configuration file in your project.

*Configuring NLog as ILogger:*
```csharp
 public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        })
         .ConfigureLogging(logging =>
         {
             logging.ClearProviders();
             logging.SetMinimumLevel(LogLevel.Trace);
         })
        .UseNLog();
```

*Injecting ILogger:*
```csharp

public class BinanceDataProvider
{
	BinanceClient _client;

	public BinanceDataProvider(ILogger<BinanceDataProvider> logger)
	{
		_client = new BinanceClient(new BinanceClientOptions
		{
			LogLevel = LogLevel.Trace,
			LogWriters = new List<ILogger> { logger }
		});

	}
}

```

If you don't have the Dotnet dependency container available you'll need to provide your own ILogger implementation. See [Custom logger](#custom-logger).

## Custom logger
If you're using a different framework or for some other reason these methods don't work for you you can create a custom ILogger implementation to receive the logging. All you need to do is create an implementation of the ILogger interface and provide that to the client.

*A simple console logging implementation (note that the ConsoleLogger is already available in the CryptoExchange.Net library)*:
```csharp

public class ConsoleLogger : ILogger
{
	public IDisposable BeginScope<TState>(TState state) => null;

	public bool IsEnabled(LogLevel logLevel) => true;

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
	{
		var logMessage = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} | {logLevel} | {formatter(state, exception)}";
		Console.WriteLine(logMessage);
	}
}

```

*Injecting the console logging implementation:*
```csharp

var client = new BinanceClient(new BinanceClientOptions
{
	LogLevel = LogLevel.Trace,
	LogWriters = new List<ILogger> { new ConsoleLogger() }
});

```

## Provide logging for issues
A big debugging tool when opening an issue on Github is providing logging of what data caused the issue. This can be provided two ways, via the `OriginalData` property of the call result or data event, or collecting the Trace logging.
### OriginalData
This is only useful when there is an issue in deserialization. So either a call result is giving a Deserialization error, or the result has a value that is unexpected. If that is the issue, please provide the original data that is received so the deserialization issue can be resolved based on the received data.
By default the `OriginalData` property in the `WebCallResult`/`DataEvent` object is not filled as saving the original data has a (very small) performance penalty. To save the original data in the `OriginalData` property the `OutputOriginalData` option should be set to `true` in the client options.  
*Enabled output data*
```csharp
var client = new BinanceClient(new BinanceClientOptions
{
	OutputOriginalData = true
});
``` 

*Accessing original data*
```csharp
// Rest request
var tickerResult = client.SpotApi.ExchangeData.GetTickersAsync();
var originallyRecievedData = tickerResult.OriginalData;

// Socket update
client.SpotStreams.SubscribeToAllTickerUpdatesAsync(update => {
	var originallyRecievedData = update.OriginalData;
});
```

### Trace logging
Trace logging, which is the most verbose log level, can be enabled in the client options.  
*Enabled output data*
```csharp
var client = new BinanceClient(new BinanceClientOptions
{
	LogLevel = LogLevel.Trace
});
``` 
After enabling trace logging all data send to/received from the server is written to the log writers. By default this is written to the output window in Visual Studio via Debug.WriteLine, though this might be different depending on how you configured your logging.
Output data will look something like this:
```
2021-12-17 10:40:42:296 | Debug | Binance    | Client configuration: LogLevel: Trace, Writers: 1, OutputOriginalData: False, Proxy: -, AutoReconnect: True, ReconnectInterval: 00:00:05, MaxReconnectTries: , MaxResubscribeTries: 5, MaxConcurrentResubscriptionsPerSocket: 5, SocketResponseTimeout: 00:00:10, SocketNoDataTimeout: 00:00:00, SocketSubscriptionsCombineTarget: , CryptoExchange.Net: v5.0.0.0, Binance.Net: v8.0.0.0
2021-12-17 10:40:42:410 | Debug | Binance    | [15] Creating request for https://api.binance.com/api/v3/ticker/24hr
2021-12-17 10:40:42:439 | Debug | Binance    | [15] Sending GET request to https://api.binance.com/api/v3/ticker/24hr?symbol=BTCUSDT with headers Accept=[application/json], X-MBX-APIKEY=[XXX]
2021-12-17 10:40:43:024 | Debug | Binance    | [15] Response received in 571ms: {"symbol":"BTCUSDT","priceChange":"-1726.47000000","priceChangePercent":"-3.531","weightedAvgPrice":"48061.51544204","prevClosePrice":"48901.44000000","lastPrice":"47174.97000000","lastQty":"0.00352000","bidPrice":"47174.96000000","bidQty":"0.65849000","askPrice":"47174.97000000","askQty":"0.13802000","openPrice":"48901.44000000","highPrice":"49436.43000000","lowPrice":"46749.55000000","volume":"33136.69765000","quoteVolume":"1592599905.80360790","openTime":1639647642763,"closeTime":1639734042763,"firstId":1191596486,"lastId":1192649611,"count":1053126}
```
When opening an issue, please provide this logging when available.
