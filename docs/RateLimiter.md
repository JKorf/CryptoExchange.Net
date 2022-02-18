---
title: Rate limiting
nav_order: 7
---

## Rate limiting
The library has build in rate limiting. These rate limits can be configured per client. Some client implementations where the exchange has clear rate limits will also have a default rate limiter already set up.
Rate limiting is configured in the client options, and can be set on a specific client or for all clients by either providing it in the constructor for a client, or by using the `SetDefaultOptions` on a client.

What to do when a limit is reached can be configured with the `RateLimitingBehaviour` client options, which has 2 possible options. Setting it to `Fail` will cause a request to fail without sending it. Setting it to `Wait` will cause the request to wait until the request can be send in accordance to the rate limiter.

A rate limiter can be configured in the options like so:
```csharp
new ClientOptions
{
	RateLimitingBehaviour = RateLimitingBehaviour.Wait,
	RateLimiters = new List<IRateLimiter>
	{
		new RateLimiter()
			.AddTotalRateLimit(50, TimeSpan.FromSeconds(10))
	}
}
```

This will add a rate limiter for 50 requests per 10 seconds.
A rate limiter can have multiple limits:
```csharp
new RateLimiter()
			.AddTotalRateLimit(50, TimeSpan.FromSeconds(10))
			.AddEndpointLimit("/api/order", 10, TimeSpan.FromSeconds(2))
```
This adds another limit of 10 requests per 2 seconds in addition to the 50 requests per 10 seconds limit.
These are the available rate limit configurations:

### AddTotalRateLimit
|Parameter|Description|
|---------|-----------|
|limit|The request weight limit per time period. Note that requests can have a weight specified. Default requests will have a weight of 1|
|perTimePeriod|The time period over which the limit is enforced|

A rate limit for the total amount of requests for all requests send from the client. 

### AddEndpointLimit
|Parameter|Description|
|---------|-----------|
|endpoint|The endpoint this limit is for|
|limit|The request weight limit per time period. Note that requests can have a weight specified. Default requests will have a weight of 1|
|perTimePeriod|The time period over which the limit is enforced|
|method|The HTTP method this limit is for. Defaults to all methods|
|excludeFromOtherRateLimits|When set to true requests to this endpoint won't be counted for other configured rate limits|

A rate limit for all requests send to a specific endpoint. Requests that do not fully match the endpoint will not be counted to this limit.

### AddPartialEndpointLimit
|Parameter|Description|
|---------|-----------|
|endpoint|The partial endpoint this limit is for. Partial means that a request will match this limiter when a part of the request URI path matches this endpoint|
|limit|The request weight limit per time period. Note that requests can have a weight specified. Default requests will have a weight of 1|
|perTimePeriod|The time period over which the limit is enforced|
|method|The HTTP method this limit is for. Defaults to all methods|
|countPerEndpoint|Whether all requests matching the endpoint pattern should be combined for this limit or each endpoint has its own limit|
|ignoreOtherRateLimits|When set to true requests to this endpoint won't be counted for other configured rate limits|

A rate limit for a partial endpoint. Requests will be counted towards this limit if the request path contains the endpoint. For example request `/api/v2/test` will match when the partial endpoint limit is set for `/api/v2`.

### AddApiKeyLimit
|Parameter|Description|
|---------|-----------|
|limit|The request weight limit per time period. Note that requests can have a weight specified. Default requests will have a weight of 1|
|perTimePeriod|The time period over which the limit is enforced|
|onlyForSignedRequests|Whether this rate limit should only be counter for signed/authenticated requests|
|excludeFromTotalRateLimit|Whether requests counted for this rate limited should not be counted towards the total rate limit|

A rate limit for an API key. Requests with the same API key will be grouped and limited.