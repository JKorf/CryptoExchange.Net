---
title: Order books
nav_order: 6
---

## Locally synced order book
Each implementation provides an order book implementation. These implementations will provide a client side order book and will take care of synchronization with the server, and will handle reconnecting and resynchronizing in case of a dropped connection.
Order book implementations are named as `[ExchangeName][Type]SymbolOrderBook`, for example `BinanceSpotSymbolOrderBook`. 

## Usage
Start the book synchronization by calling the `StartAsync` method. This returns a success state whether the book is successfully synchronized and started. You can listen to the `OnStatusChange` event to be notified of when the status of a book changes. Note that the order book is only synchronized with the server when the state is `Synced`.

*Start an order book and print the top 3 rows*
```csharp

var book = new BinanceSpotSymbolOrderBook("BTCUSDT");
book.OnStatusChange += (oldState, newState) => Console.WriteLine($"State changed from {oldState} to {newState}");
var startResult = await book.StartAsync();
if (!startResult.Success)
{
	Console.WriteLine("Failed to start order book: " + startResult.Error);
	return;
}

while(true)
{
	Console.WriteLine(book.ToString(3);
	await Task.Delay(500);
}

```

### Accessing bids/asks
You can access the current Bid/Ask lists using the responding properties:  
`var currentBidList = book.Bids;`  
`var currentAskList = book.Asks;`  

Note that these will return copies of the internally synced lists when accessing the properties, and when accessing them in sequence like above does mean that the lists may not be in sync with eachother since they're accessed at different points in time.
When you need both lists in sync you should access the `Book` property.  
`var (currentBidList, currentAskList) = book.Book;`  

Because copies of the lists are made when accessing the bids/asks properties the performance impact should be considered. When only the current best ask/bid info is needed you can access the `BestOffers` property.  
`var (bestBid, bestAsk) = book.BestOffers;`  

### Events
The following events are available on the symbol order book:  
`book.OnStatusChange`: The book has changed state. This happens during connecting, the connection was lost or the order book was detected to be out of sync. The asks/bids are only the actual with the server when state is `Synced`.  
`book.OnOrderBookUpdate`: The book has changed, the arguments contain the changed entries.  
`book.OnBestOffersChanged`: The best offer (best bid, best ask) has changed.

```csharp

book.OnStatusChange += (oldStatus, newStatus) => { Console.WriteLine($"State changed from {oldStatus} to {newStatus}"); };
book.OnOrderBookUpdate += (bidsAsks) => { Console.WriteLine($"Order book changed: {bidsAsks.Asks.Count()} asks, {bidsAsks.Bids.Count()} bids"); };
book.OnBestOffersChanged += (bestOffer) => { Console.WriteLine($"Best offer changed, best bid: {bestOffer.BestBid.Price}, best ask: {bestOffer.BestAsk.Price}"); };

```