---
title: Common interfaces
nav_order: 5
---

Clients have a common interface implementation to allow a shared code base to use the same functionality for different exchanges. The interface is implemented at the `API` level, for example:
```csharp
var binanceClient = new BinanceClient();
ISpotClient spotClient = binanceClient.SpotApi.CommonSpotClient;
IFuturesClient futuresClient = binanceClient.UsdFuturesApi.CommonFuturesClient;

```

## ISpotClient
TODO 

## IFuturesClient
TODO