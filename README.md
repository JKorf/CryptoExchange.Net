# ![.CryptoExchange.Net](https://github.com/JKorf/CryptoExchange.Net/blob/ffcb7db8ff597c2f14982d68464015a748815580/CryptoExchange.Net/Icon/icon.png) CryptoExchange.Net  

[![.NET](https://img.shields.io/github/actions/workflow/status/JKorf/CryptoExchange.Net/dotnet.yml?style=for-the-badge)](https://github.com/JKorf/CryptoExchange.Net/actions/workflows/dotnet.yml) [![Nuget downloads](https://img.shields.io/nuget/dt/CryptoExchange.Net.svg?style=for-the-badge)](https://www.nuget.org/packages/CryptoExchange.Net) ![License](https://img.shields.io/github/license/JKorf/CryptoExchange.Net?style=for-the-badge)
![Since](https://img.shields.io/badge/since-2018-brightgreen?style=for-the-badge)

CryptoExchange.Net is a base library which is used to implement different cryptocurrency (exchange) API's. It provides a standardized way of implementing different API's, which results in a very similar experience for users of the API implementations. 
Note that the CryptoExchange.Net package itself can not be used directly for accessing API's. Either install a client library from the list below or use [CryptoClients.Net](https://github.com/jkorf/CryptoClients.Net) which includes access to all exchange API's.

For more information on what CryptoExchange.Net and it's client libraries offers see the [Documentation](https://cryptoexchange.jkorf.dev/).

### For AI Coding Assistants

This library and the entire CryptoExchange.Net ecosystem provide first-class support for AI coding assistants. The relevant skill files are in this repository:

- **Agents**: `AGENTS.md` (auto-detected at repo root)
- **Cursor**: `.cursor/rules/cryptoexchange-net.mdc`
- **GitHub Copilot**: `.github/copilot-instructions.md`
- **Other tools** (Windsurf, Codex, Continue, Aider, etc.): `llms.txt` at repo root
- **Compilable examples**: `Examples/ai-friendly/`

For single-exchange code, see also the AI files in each exchange's repository (Binance.Net, Bybit.Net, OKX.Net, ...) — they cover exchange-specific patterns.

See [cryptoexchange-skills-hub](https://github.com/JKorf/cryptoexchange-skills-hub) for installable skills.

**Quick prompt to verify your assistant is using these:**
> "Show me how to fetch BTC/USDT spot tickers from Binance and OKX concurrently in C# using the SharedApis pattern."

The expected output should use `.SharedClient` properties, `SharedSymbol`, `ISpotTickerRestClient`, and `Task.WhenAll`.

### CryptoExchange.Net Ecosystem
Full list of all libraries part of the CryptoExchange.Net ecosystem. Consider using a referral link to support development, as well as potentially get some trading fee discount!

||API|Type|Repository|Nuget|Referral Link|Referral Fee Discount|
|--|--|--|--|--|--|--|
|![Aster](https://raw.githubusercontent.com/JKorf/Aster.Net/refs/heads/main/Aster.Net/Icon/icon.png)|Aster|DEX|[JKorf/Aster.Net](https://github.com/JKorf/Aster.Net)|[![Nuget version](https://img.shields.io/nuget/v/JKorf.Aster.net.svg?style=flat-square)](https://www.nuget.org/packages/JKorf.Aster.Net)|[Link](https://www.asterdex.com/en/referral/FD2E11)|4%|
|![Binance](https://raw.githubusercontent.com/JKorf/Binance.Net/refs/heads/master/Binance.Net/Icon/icon.png)|Binance|CEX|[JKorf/Binance.Net](https://github.com/JKorf/Binance.Net)|[![Nuget version](https://img.shields.io/nuget/v/Binance.net.svg?style=flat-square)](https://www.nuget.org/packages/Binance.Net)|[Link](https://accounts.binance.com/register?ref=X5K3F2ZG)|20%|
|![BingX](https://raw.githubusercontent.com/JKorf/BingX.Net/refs/heads/main/BingX.Net/Icon/BingX.png)|BingX|CEX|[JKorf/BingX.Net](https://github.com/JKorf/BingX.Net)|[![Nuget version](https://img.shields.io/nuget/v/JK.BingX.net.svg?style=flat-square)](https://www.nuget.org/packages/JK.BingX.Net)|[Link](https://bingx.com/invite/FFHRJKWG/)|20%|
|![Bitfinex](https://raw.githubusercontent.com/JKorf/Bitfinex.Net/refs/heads/master/Bitfinex.Net/Icon/icon.png)|Bitfinex|CEX|[JKorf/Bitfinex.Net](https://github.com/JKorf/Bitfinex.Net)|[![Nuget version](https://img.shields.io/nuget/v/Bitfinex.net.svg?style=flat-square)](https://www.nuget.org/packages/Bitfinex.Net)|-|-|
|![Bitget](https://raw.githubusercontent.com/JKorf/Bitget.Net/refs/heads/main/Bitget.Net/Icon/icon.png)|Bitget|CEX|[JKorf/Bitget.Net](https://github.com/JKorf/Bitget.Net)|[![Nuget version](https://img.shields.io/nuget/v/JK.Bitget.net.svg?style=flat-square)](https://www.nuget.org/packages/JK.Bitget.Net)|[Link](https://partner.bitget.com/bg/1qlf6pj1)|20%|
|![BitMart](https://raw.githubusercontent.com/JKorf/BitMart.Net/refs/heads/main/BitMart.Net/Icon/icon.png)|BitMart|CEX|[JKorf/BitMart.Net](https://github.com/JKorf/BitMart.Net)|[![Nuget version](https://img.shields.io/nuget/v/BitMart.net.svg?style=flat-square)](https://www.nuget.org/packages/BitMart.Net)|[Link](https://www.bitmart.com/invite/JKorfAPI/en-US)|30%|
|![BitMEX](https://raw.githubusercontent.com/JKorf/BitMEX.Net/refs/heads/main/BitMEX.Net/Icon/icon.png)|BitMEX|CEX|[JKorf/BitMEX.Net](https://github.com/JKorf/BitMEX.Net)|[![Nuget version](https://img.shields.io/nuget/v/JKorf.BitMEX.net.svg?style=flat-square)](https://www.nuget.org/packages/JKorf.BitMEX.Net)|[Link](https://www.bitmex.com/app/register/94f98e)|30%|
|![Bitstamp](https://raw.githubusercontent.com/JKorf/Bitstamp.Net/refs/heads/main/Bitstamp.Net/Icon/icon.png)|Bitstamp|CEX|[JKorf/Bitstamp.Net](https://github.com/JKorf/Bitstamp.Net)|[![Nuget version](https://img.shields.io/nuget/v/Bitstamp.net.svg?style=flat-square)](https://www.nuget.org/packages/Bitstamp.Net)|-|-|
|![BloFin](https://raw.githubusercontent.com/JKorf/BloFin.Net/refs/heads/main/BloFin.Net/Icon/icon.png)|BloFin|CEX|[JKorf/BloFin.Net](https://github.com/JKorf/BloFin.Net)|[![Nuget version](https://img.shields.io/nuget/v/BloFin.net.svg?style=flat-square)](https://www.nuget.org/packages/BloFin.Net)|-|-|
|![Bybit](https://raw.githubusercontent.com/JKorf/Bybit.Net/refs/heads/main/ByBit.Net/Icon/icon.png)|Bybit|CEX|[JKorf/Bybit.Net](https://github.com/JKorf/Bybit.Net)|[![Nuget version](https://img.shields.io/nuget/v/Bybit.net.svg?style=flat-square)](https://www.nuget.org/packages/Bybit.Net)|[Link](https://partner.bybit.com/b/jkorf)|-|
|![Coinbase](https://raw.githubusercontent.com/JKorf/Coinbase.Net/refs/heads/main/Coinbase.Net/Icon/icon.png)|Coinbase|CEX|[JKorf/Coinbase.Net](https://github.com/JKorf/Coinbase.Net)|[![Nuget version](https://img.shields.io/nuget/v/JKorf.Coinbase.Net.svg?style=flat-square)](https://www.nuget.org/packages/JKorf.Coinbase.Net)|[Link](https://advanced.coinbase.com/join/T6H54H8)|-|
|![CoinEx](https://raw.githubusercontent.com/JKorf/CoinEx.Net/refs/heads/master/CoinEx.Net/Icon/icon.png)|CoinEx|CEX|[JKorf/CoinEx.Net](https://github.com/JKorf/CoinEx.Net)|[![Nuget version](https://img.shields.io/nuget/v/CoinEx.net.svg?style=flat-square)](https://www.nuget.org/packages/CoinEx.Net)|[Link](https://www.coinex.com/register?rc=rbtnp)|20%|
|![CoinW](https://raw.githubusercontent.com/JKorf/CoinW.Net/refs/heads/main/CoinW.Net/Icon/icon.png)|CoinW|CEX|[JKorf/CoinW.Net](https://github.com/JKorf/CoinW.Net)|[![Nuget version](https://img.shields.io/nuget/v/CoinW.net.svg?style=flat-square)](https://www.nuget.org/packages/CoinW.Net)|[Link](https://www.coinw.com/register?rc=rbtnp)|-|
|![CoinGecko](https://raw.githubusercontent.com/JKorf/CoinGecko.Net/refs/heads/main/CoinGecko.Net/Icon/icon.png)|CoinGecko|-|[JKorf/CoinGecko.Net](https://github.com/JKorf/CoinGecko.Net)|[![Nuget version](https://img.shields.io/nuget/v/CoinGecko.net.svg?style=flat-square)](https://www.nuget.org/packages/CoinGecko.Net)|-|-|
|![Crypto.com](https://raw.githubusercontent.com/JKorf/CryptoCom.Net/refs/heads/main/CryptoCom.Net/Icon/icon.png)|Crypto.com|CEX|[JKorf/CryptoCom.Net](https://github.com/JKorf/CryptoCom.Net)|[![Nuget version](https://img.shields.io/nuget/v/CryptoCom.net.svg?style=flat-square)](https://www.nuget.org/packages/CryptoCom.Net)|[Link](https://crypto.com/exch/26ge92xbkn)|-|
|![DeepCoin](https://raw.githubusercontent.com/JKorf/DeepCoin.Net/refs/heads/main/DeepCoin.Net/Icon/icon.png)|DeepCoin|CEX|[JKorf/DeepCoin.Net](https://github.com/JKorf/DeepCoin.Net)|[![Nuget version](https://img.shields.io/nuget/v/DeepCoin.net.svg?style=flat-square)](https://www.nuget.org/packages/DeepCoin.Net)|[Link](https://s.deepcoin.com/jddhfca)|-|
|![Gate.io](https://raw.githubusercontent.com/JKorf/GateIo.Net/refs/heads/main/GateIo.Net/Icon/icon.png)|Gate.io|CEX|[JKorf/GateIo.Net](https://github.com/JKorf/GateIo.Net)|[![Nuget version](https://img.shields.io/nuget/v/GateIo.net.svg?style=flat-square)](https://www.nuget.org/packages/GateIo.Net)|[Link](https://www.gate.io/share/JKorf)|20%|
|![HTX](https://raw.githubusercontent.com/JKorf/HTX.Net/refs/heads/master/HTX.Net/Icon/icon.png)|HTX|CEX|[JKorf/HTX.Net](https://github.com/JKorf/HTX.Net)|[![Nuget version](https://img.shields.io/nuget/v/JKorf.HTX.net.svg?style=flat-square)](https://www.nuget.org/packages/JKorf.HTX.Net)|[Link](https://www.htx.com/invite/en-us/1f?invite_code=ekek5223)|30%|
|![HyperLiquid](https://raw.githubusercontent.com/JKorf/HyperLiquid.Net/refs/heads/main/HyperLiquid.Net/Icon/icon.png)|HyperLiquid|DEX|[JKorf/HyperLiquid.Net](https://github.com/JKorf/HyperLiquid.Net)|[![Nuget version](https://img.shields.io/nuget/v/HyperLiquid.Net.svg?style=flat-square)](https://www.nuget.org/packages/HyperLiquid.Net)|[Link](https://app.hyperliquid.xyz/join/JKORF)|4%|
|![Kraken](https://raw.githubusercontent.com/JKorf/Kraken.Net/refs/heads/master/Kraken.Net/Icon/icon.png)|Kraken|CEX|[JKorf/Kraken.Net](https://github.com/JKorf/Kraken.Net)|[![Nuget version](https://img.shields.io/nuget/v/KrakenExchange.net.svg?style=flat-square)](https://www.nuget.org/packages/KrakenExchange.Net)|-|-|
|![Kucoin](https://raw.githubusercontent.com/JKorf/Kucoin.Net/refs/heads/master/Kucoin.Net/Icon/icon.png)|Kucoin|CEX|[JKorf/Kucoin.Net](https://github.com/JKorf/Kucoin.Net)|[![Nuget version](https://img.shields.io/nuget/v/Kucoin.net.svg?style=flat-square)](https://www.nuget.org/packages/Kucoin.Net)|[Link](https://www.kucoin.com/r/rf/QBS4FPED)|-|
|![LBank](https://raw.githubusercontent.com/JKorf/LBank.Net/refs/heads/main/LBank.Net/Icon/icon.png)|LBank|CEX|[JKorf/LBank.Net](https://github.com/JKorf/LBank.Net)|[![Nuget version](https://img.shields.io/nuget/v/LBank.net.svg?style=flat-square)](https://www.nuget.org/packages/LBank.Net)|[Link](https://www.lbank.com/ref/60SLT)|-|
|![Lighter](https://raw.githubusercontent.com/JKorf/Lighter.Net/refs/heads/main/Lighter.Net/Icon/icon.png)|Lighter|DEX|[JKorf/Lighter.Net](https://github.com/JKorf/Lighter.Net)|[![Nuget version](https://img.shields.io/nuget/v/JKorf.Lighter.net.svg?style=flat-square)](https://www.nuget.org/packages/JKorf.Lighter.Net)|-|-|
|![Mexc](https://raw.githubusercontent.com/JKorf/Mexc.Net/refs/heads/main/Mexc.Net/Icon/icon.png)|Mexc|CEX|[JKorf/Mexc.Net](https://github.com/JKorf/Mexc.Net)|[![Nuget version](https://img.shields.io/nuget/v/JK.Mexc.net.svg?style=flat-square)](https://www.nuget.org/packages/JK.Mexc.Net)|-|-|
|![OKX](https://raw.githubusercontent.com/JKorf/OKX.Net/refs/heads/main/OKX.Net/Icon/icon.png)|OKX|CEX|[JKorf/OKX.Net](https://github.com/JKorf/OKX.Net)|[![Nuget version](https://img.shields.io/nuget/v/JK.OKX.net.svg?style=flat-square)](https://www.nuget.org/packages/JK.OKX.Net)|[Link](https://www.okx.com/join/14592495)|20%|
|![Pionex](https://raw.githubusercontent.com/JKorf/Pionex.Net/refs/heads/main/Pionex.Net/Icon/icon.png)|Pionex|CEX|[JKorf/Pionex.Net](https://github.com/JKorf/Pionex.Net)|[![Nuget version](https://img.shields.io/nuget/v/Pionex.net.svg?style=flat-square)](https://www.nuget.org/packages/Pionex.Net)|-|-|
|![Polymarket](https://raw.githubusercontent.com/JKorf/Polymarket.Net/main/Polymarket.Net/Icon/icon.png)|Polymarket|DEX|[JKorf/Polymarket.Net](https://github.com/JKorf/Polymarket.Net)|[![Nuget version](https://img.shields.io/nuget/v/Polymarket.net.svg?style=flat-square)](https://www.nuget.org/packages/Polymarket.Net)|-|-|
|![Toobit](https://raw.githubusercontent.com/JKorf/Toobit.Net/refs/heads/main/Toobit.Net/Icon/icon.png)|Toobit|CEX|[JKorf/Toobit.Net](https://github.com/JKorf/Toobit.Net)|[![Nuget version](https://img.shields.io/nuget/v/Toobit.net.svg?style=flat-square)](https://www.nuget.org/packages/Toobit.Net)|[Link](https://www.toobit.com/en-US/register?invite_code=zsV19h)|-|
|![Upbit](https://raw.githubusercontent.com/JKorf/Upbit.Net/refs/heads/main/Upbit.Net/Icon/icon.png)|Upbit|CEX|[JKorf/Upbit.Net](https://github.com/JKorf/Upbit.Net)|[![Nuget version](https://img.shields.io/nuget/v/JKorf.Upbit.net.svg?style=flat-square)](https://www.nuget.org/packages/JKorf.Upbit.Net)|-|-|
|![Weex](https://raw.githubusercontent.com/JKorf/Weex.Net/refs/heads/main/Weex.Net/Icon/icon.png)|Weex|CEX|[JKorf/Weex.Net](https://github.com/JKorf/Weex.Net)|[![Nuget version](https://img.shields.io/nuget/v/Weex.net.svg?style=flat-square)](https://www.nuget.org/packages/Weex.Net)|-|-|
|![WhiteBit](https://raw.githubusercontent.com/JKorf/WhiteBit.Net/refs/heads/main/WhiteBit.Net/Icon/icon.png)|WhiteBit|CEX|[JKorf/WhiteBit.Net](https://github.com/JKorf/WhiteBit.Net)|[![Nuget version](https://img.shields.io/nuget/v/WhiteBit.net.svg?style=flat-square)](https://www.nuget.org/packages/WhiteBit.Net)|[Link](https://whitebit.com/referral/a8e59b59-186c-4662-824c-3095248e0edf)|-|
|![XT](https://raw.githubusercontent.com/JKorf/XT.Net/refs/heads/main/XT.Net/Icon/icon.png)|XT|CEX|[JKorf/XT.Net](https://github.com/JKorf/XT.Net)|[![Nuget version](https://img.shields.io/nuget/v/XT.net.svg?style=flat-square)](https://www.nuget.org/packages/XT.Net)|[Link](https://www.xt.com/ru/accounts/register?ref=CZG39C)|25%|

Any of these can be installed independently or install [CryptoClients.Net](https://github.com/jkorf/CryptoClients.Net) which includes all exchange API's.

### Full demo application
A full demo application is available using the [CryptoClients.Net](https://github.com/jkorf/CryptoClients.Net) library:  
https://github.com/JKorf/CryptoManager.Net

## Discord
[![Nuget version](https://img.shields.io/discord/847020490588422145?style=for-the-badge)](https://discord.gg/MSpeEtSY8t)  
A Discord server is available [here](https://discord.gg/MSpeEtSY8t). Feel free to join for discussion and/or questions around the CryptoExchange.Net and implementation libraries.

## Support the project
Any support is greatly appreciated.

### Referral
When creating an account on new exchanges please consider using a referral link from above.

### Donate
Make a one time donation in a crypto currency of your choice. If you prefer to donate in a different currency or network send me a message.
   
**USDT (TRX)**  TKigKeJPXZYyMVDgMyXxMf17MWYia92Rjd 

### Sponsor
Alternatively, sponsor me on Github using [Github Sponsors](https://github.com/sponsors/JKorf). 

## Update notes from version 11.x to 12.x for client implementations
* Result types:
  * (Web)CallResult types are replaced by HttpResult, WebSocketResult and QueryResult
  * Use (Http/WebSocket/Query)Result.Ok(..) and .Fail(..) for creation
  * Result objects no longer override implicit conversion to bool, use Success property instead
  * CallResult.SuccessResult has been replaced with CallResult.Ok()

* Parameters & serialization:
  * ParameterCollection type is replaced by Parameters, most AddXX() methods can be replaced by Add()
  * Parameter serialization behavior can be controlled in the Add method as third parameter, or in the ParameterSerializationSettings
  * ArraySerialization has been move into the new Parameters object
  * RestRequestConfiguration in AuthenticationProvider.ProcessRequest now contains the full RequestDefinition instead of copied fields. This changes for example `request.Authenticated` to `request.RequestDefinition.Authenticated`

WebSocket routing:
  * MessageRouting has been split into event and query routing; use CreateForEvent for subscriptions and CreateForQuery for queries.
  * Queries returning a mapped type can specify a second type parameter in CreateForQuery for the result type
  * MessageRouter.CreateWithoutHandler has been replaced with CreateVoid

Shared APIs:
  * Option defintions now always require the exchange name as first parameter
  * Every request/subscription now has a dedicated options type
  * ExchangeSymbolCache now requires EnvironmentName as parameter for operations
  * Validation has been unified via `SharedClient.[Request]Options.ValidateRequest(request, this);`
  * Validation now includes auth check and klines support internally, no need for explicit checks
  * AsExchangeResult/ExchangeWebResult has been removed, use normal HttpResults instead
  * ExchangeResult has been replaced by ExchangeCallResult
  * TradingMode has been removed from the response model, only maintained on models where it makes sense
  * IListenKey support has been removed, listen keys should be managed internally with TokenManager

Various:
  * ApiClients now require an exchange name in the constructor
  * ApiClients now required ILoggerFactory parameter instead of ILogger instance
  * RestApiClient SendAsync without type parameter removed, use SendAsync<Unit> instead
  * Address parameter removed from SendAsync RestApiClient, should be specified on the request definition instead
  * SymbolOrderBook DoResyncAsync now returns CallResult instead of CallResult<bool> which was redundant
  * PlatformInfo now required support environment names in the constructor

## Release notes
* Version 12.5.0 - 21 Aug 2026
    * Shared APIs
      * Added ISpotOrderManagementSocketClient for placing/canceling Spot orders via Shared websocket implementation
      * Added IFuturesOrderManagementSocketClient for placing/canceling Futures orders via Shared websocket implementation
      * Added UpperFundingCap, LowerFundingCap to SharedFuturesSymbol model
      * Added UpperPriceLimitPerecentage, LowerPriceLimitPercentage, MakerFeePercentage and TakerFeePercentage to SharedSpotSymbol model
      * Added WithCalculatedQuantities(price, contractSize) to SharedQuantity to retrieve a copy with derived quantities
      * Added Description property to Shared EndpointOptions classes
      * Added additional check for symbol type to Shared Spot endpoints
      * Added QuantityType property to SharedOrderBook
      * Added auto calculated quote quantity where for SharedOrderQuantity where it makes sense
      * Updated SharedId value to be nullable
      * Updated quantity/volumes to SharedOrderQuantity model for SharedBookTicker, SharedOpenInterest, SharedPosition, SharedPositionHistory and SharedUserTrade
      * Updated Shared client info string representation
    * Added IQueryResult interface to QueryResult, moved OriginalData to the base class
    * Added UsePublicConnectionForAuth property to SocketApiClient
    * Added support for order polling where Shared order implementation doesn't support time filtering to UserDataTracker implementations
    * Added ManualUpdateSubscription and UpdateSubscription additional constructor to allow producing websocket events without actual connection
    * Updated logging unhandled websocket message
    * Split UserClientProvider into base class with RestClient and derived class also containing SocketClient
    * Removed unnecessary log from token manager
    * Fixed incorrect check test output

* Version 12.4.0 - 28 Jul 2026
    * Added AveragePrice property to SharedQuantity model
    * Added DebuggerDisplay attributes to Result objects
    * Updated SharedFuturesTicker, SharedSpotTicker, SharedTrade and SharedKline to use SharedOrderQuantity for volumes/quantities
    * Updated REST json deserialization error for empty response

* Version 12.3.0 - 23 Jul 2026
    * Added calculation of AveragePrice on Shared order models if data is available and AveragePrice is not set
    * Extracted ConnectionCanBeUsedFor method in SocketApiClient for easier custom logic implementation
    * Updated some Shared APIs error messages
    * Remove duplicate warnings from testing output

* Version 12.2.0 - 20 Jul 2026
    * Added SpotSymbolCatalog to Shared ISpotSymbolRestClient interface
    * Added FuturesSymbolCatalog to Shared IFuturesSymbolRestClient interface
    * Added BaseAssetType, BaseAssetSubType, QuoteAssetType and QuoteAssetSubType to GetSymbolsRequest model
    * Added DisplayName to SharedSpotSymbol and SharedFuturesSymbol models
    * Added BaseAssetType, BaseAssetSubType, QuoteAssetType and QuoteAssetSubType to SharedSpotSymbol and SharedFuturesSymbol models
    * Added IsStableCoin, IsCommodity and IsEquity helper methods to LibraryHelpers
    * Added DebuggerDisplay attributes to Shared models
    * Fixed socket connection combine calculations

* Version 12.1.1 - 11 Jul 2026
    * Added timestamp deserialization support for yyyy-MM-dd HH:mm:ss.ffffff+00:00:00

* Version 12.1.0 - 09 Jul 2026
    * Added ExchangeParameters parameter to KlineTracker, TradeTracker and ITrackerFactory methods
    * Updated some testing logic
    * Fixed nullability operator on Parameters.AddCommaSeperated

* Version 12.0.2 - 01 Jul 2026
    * Updated test validation to output a list of issues instead of throwing on the first

* Version 12.0.1 - 29 Jun 2026
    * Fixed bug in bool converter

* Version 12.0.0 - 29 Jun 2026
    * Result types:
      * (Web)CallResult types are replaced by HttpResult, WebSocketResult and QueryResult with the same logic
      * Updated result types to record type
      * Result creation can be done with (Http/WebSocket/Query)Result.Ok(..) and .Fail(..)
      * Removed implicit result type conversion to bool, `if (result)` no longer works, instead use `if (result.Success)`
      * Replaced CallResult.SuccessResult with CallResult.Ok()
      * Fixed result object nullability hinting, for example Data might be null if Success isn't checked for true
    * Parameters & serialization:
      * Added support for `enabled` and `disabled` strings to bool converter
      * Removed ParameterCollection type, has been replaced by Parameters type
      * Removed ArraySerialization, OrderParameters and ParameterOrderComparer properties from RestApiClient, moved to ParameterSerializationsSettings
      * Updated RestRequestConfiguration in AuthenticationProvider.ProcessRequest to contain the full RequestDefinition instead of copied fields	
    * Clients:
      * Updated Api client constructor logging parameter from ILogger to ILoggerFactory? 
      * Added Api client constructor exchange name parameter
      * Added ToString overrides on base API types
      * Added Exchange property on BaseApiClient
      * Added ApiCredentials property on IRestApiClient and ISocketApiClient interfaces
      * Updated ILogger source from client name to topic specific client name
      * Removed logging from client creation
      * Fixed BaseRestClient SetApiCredentials not marked as virtual
    * Rest:
      * Added BaseAddress to RequestDefinition object
      * Updated RestApiClient AuthenticationProvider logic from private to protected and virtual
      * Removed RestApiClient.SendAsync baseAddress parameter removed
      * Removed RestApiClient.SendAsync without type parameter
    * WebSocket:
      * Updated MessageRouting definition into CreateForEvent for subscriptions and CreateForQuery for queries
      * Improved Query type safety with CeateForQuery which allows second parameter for specifying the result type
      * Renamed MessageRouter.CreateWithoutHandler to CreateVoid
      * Updated SocketApiClient.GetSocketConnection to check connection uri instead of Tag for finding compatible connections
      * Removed unused UnhandledMessageExpected property SocketApiClient
      * Fixed issue in SocketApiClient.GetSocketConnection causing requests to always wait the full max 10 seconds when there was a reconnecting socket
    	
    * Shared APIs:
      * Updated Option definitions to always require the exchange name as first parameter
      * Added missing dedicated option types
      * Added Discover method on ISharedClient interface, returning info on supported capabilities and operations
      * Added SharedRequest GetParamValue helper method accepting multiple parameter names
      * Added ResetStaticExchangeParameters method on ExchangeParameters
      * Added Status property to SharedWithdrawal model
      * Added TradingModes property to SharedBalance model
      * Updated ExchangeSymbolCache to support multiple environments and additional key separation
      * Updated Shared ExchangeParameters parameter names to be case insensitive
      * Updated code comments
      * Replaced ExchangeResult with ExchangeCallResult type
      * Removed AsExchangeResult/ExchangeWebResult
      * Removed TradingMode from the response model, only maintained on models where it makes sense
      * Removed IListenKey support, listen keys now rely on internal management with TokenManager
    * Rate limiting:
      * Fixed websocket connection attempts counting towards rate limit even when server could not be reached
      * Removed host from rate limit methods, now part of the already provided RequestDefinition
      * Added amount parameter to RateLimit Reset method to allow partially resetting the limit
    * Added TokenManager implementation for automatic listenkey/token management
    * Added UserClientProvider base class
    * Added async streaming on UserDataTracker items with StreamUpdatesAsync
    * Added cancellation token support to UserDataTracker starting
    * Added Unit type for non-result types
    * Added ServerError constructor taking ErrorType and message to make it easier to create
    * Added SupportedEnvironments property to PlatformInfo
    * Updated SymbolOrderBook DoResyncAsync to return CallResult instead of CallResult<bool> which was redundant
    * Various small performance improvements
