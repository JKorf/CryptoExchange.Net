# CryptoExchange.Net
![Build status](https://travis-ci.com/JKorf/CryptoExchange.Net.svg?branch=master) ![Nuget version](https://img.shields.io/nuget/v/CryptoExchange.Net.svg)  ![Nuget downloads](https://img.shields.io/nuget/dt/CryptoExchange.Net.svg)

CryptoExchange.Net is a base package which can be used to easily implement crypto currency exchange API's in C#. This library offers base classes for creating rest and websocket clients, and includes additional features like an automatically synchronizing order book implementation, error handling and automatic reconnects on websocket connections.

[Documentation](https://jkorf.github.io/CryptoExchange.Net/)

## Discord
A Discord server is available [here](https://discord.gg/MSpeEtSY8t). Feel free to join for discussion and/or questions around the CryptoExchange.Net and implementation libraries.

## Donate / Sponsor
I develop and maintain this package on my own for free in my spare time. Donations are greatly appreciated. If you prefer to donate any other currency please contact me.

**Btc**:  12KwZk3r2Y3JZ2uMULcjqqBvXmpDwjhhQS  
**Eth**:  0x069176ca1a4b1d6e0b7901a6bc0dbf3bb0bf5cc2  
**Nano**: xrb_1ocs3hbp561ef76eoctjwg85w5ugr8wgimkj8mfhoyqbx4s1pbc74zggw7gs  

Alternatively, sponsor me on Github using [Github Sponsors](https://github.com/sponsors/JKorf)  

## Release notes
* Version 5.0.0
	* Added Github.io page for documentation: https://jkorf.github.io/CryptoExchange.Net/
	* Added single DateTimeConverter replacing the different timestamp converters 
	* Added additional request related properties to WebCallResult
	* Added CancelationToken support for websockets
	* Added CancelationToken support for SymbolOrderBook starting
	* Added TimeSync support
	* Refactored base client classes into BaseClient and ApiClient to provide a more defined client structure
	* Refactored client options to have better control over each different ApiClient
	* Refactored authentication provider to be more flexible
	* Refactored rate limiter implementation
	* Refactored IExchangeClient interface to ISpotClient and IFuturesClient
	* Refactored socket reconnection to immediately try to reconnect before waiting the ReconnectTimeout
	* Improved SymbolOrderBook stability
	* Updated code docs

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