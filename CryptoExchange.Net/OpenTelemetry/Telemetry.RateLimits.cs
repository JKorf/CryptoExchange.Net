using System;
using System.Collections.Generic;
using System.Diagnostics;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.RateLimiting;
using CryptoExchange.Net.RateLimiting.Interfaces;

namespace CryptoExchange.Net.OpenTelemetry;

public partial class Telemetry
{
    /// <summary>
    /// Records a failed rate limit event for the specified request definition and rate limit guard.
    /// </summary>
    /// <remarks>This method increments a telemetry counter to track rate limit failures and logs an activity
    /// event to provide additional context about the failure. The specific event logged depends on the type of item
    /// that triggered the failure.</remarks>
    /// <param name="definition">The request definition associated with the rate limit failure.</param>
    /// <param name="guard">The rate limit guard that detected the failure.</param>
    /// <param name="host">The host where the rate limit failure occurred.</param>
    /// <param name="itemType">The type of item (e.g., connection or request) that triggered the rate limit failure.</param>
    internal void RecordRateLimitFailed(RequestDefinition definition, IRateLimitGuard guard, string host, RateLimitItemType itemType)
    {
        CryptoExchangeTelemetry.RequestRateLimitCounter.Add(1, [
            .._baseTags,
            ..GetRateLimitTags(definition, guard, host, itemType, RateLimitingBehaviour.Fail)
        ]);

        var eventName = itemType == RateLimitItemType.Connection
            ? CryptoExchangeTelemetry.ActivityEvents.RateLimitFailConnection
            : CryptoExchangeTelemetry.ActivityEvents.RateLimitFailRequest;
        Activity.Current?.AddEvent(new ActivityEvent(eventName, tags: [
            ..GetRateLimitTags(definition, guard, host, itemType, RateLimitingBehaviour.Fail)
        ]));
    }

    /// <summary>
    /// Records telemetry and tracing information for a request that is delayed due to rate limiting.
    /// </summary>
    /// <remarks>This method captures metrics and tracing data related to rate-limited requests, including the
    /// delay duration and associated metadata. It emits telemetry counters for the number of rate-limited requests and
    /// the total delay time in milliseconds. Additionally, it creates tracing events and spans to provide detailed
    /// observability into rate-limiting behavior.</remarks>
    /// <param name="definition">The definition of the request being delayed, which provides context about the request's purpose and
    /// configuration.</param>
    /// <param name="guard">The rate limit guard responsible for managing rate limiting behavior for the request.</param>
    /// <param name="host">The host associated with the request, typically representing the target endpoint or service.</param>
    /// <param name="itemType">The type of item being rate-limited, such as an API endpoint or resource category.</param>
    /// <param name="delay">The duration of the delay caused by rate limiting, represented as a <see cref="TimeSpan"/>.</param>
    internal void RecordRateLimitDelayingRequest(RequestDefinition definition, IRateLimitGuard guard, string host, RateLimitItemType itemType, TimeSpan delay)
    {
        CryptoExchangeTelemetry.RequestRateLimitCounter.Add(1, [
            .._baseTags,
            ..GetRateLimitTags(definition, guard, host, itemType, RateLimitingBehaviour.Wait)
        ]);

        CryptoExchangeTelemetry.RequestRateLimitDelayCounter.Add((long)delay.TotalMilliseconds, [
            .._baseTags,
            ..GetRateLimitTags(definition, guard, host, itemType, RateLimitingBehaviour.Wait)
        ]);

        // TotalNanoseconds is not available in .netstandard2.0
        Activity.Current?.AddEvent(new ActivityEvent(CryptoExchangeTelemetry.ActivityEvents.RateLimitDelayRequest, tags: [
            ..GetRateLimitTags(definition, guard, host, itemType, RateLimitingBehaviour.Wait),
            new(CryptoExchangeTelemetry.Tags.RateLimitDelay, (long)delay.TotalMilliseconds),
        ]));

        // TODO: Add a config for whether to emit this Span or not.
        using var delaySpan = StartActivity(CryptoExchangeTelemetry.Activities.RateLimitDelayingRequest, ActivityKind.Internal, tags: [
            ..GetRateLimitTags(definition, guard, host, itemType, RateLimitingBehaviour.Wait),
        ]);
        delaySpan?.SetEndTime(DateTime.UtcNow + delay);
    }

    /// <summary>
    /// Records telemetry data related to a rate limit delay for a connection.
    /// </summary>
    /// <remarks>This method logs metrics and telemetry events to track rate limit delays for connections. It
    /// emits counters for the number of rate-limited requests and the total delay duration, as well as activity events
    /// for tracing purposes. The method also creates a span to represent the delay period, which can be used for
    /// distributed tracing.</remarks>
    /// <param name="definition">The request definition associated with the rate-limited operation.</param>
    /// <param name="guard">The rate limit guard that triggered the delay.</param>
    /// <param name="host">The host for which the rate limit delay occurred.</param>
    /// <param name="itemType">The type of item being rate-limited (e.g., request, connection).</param>
    /// <param name="delay">The duration of the rate limit delay.</param>
    internal void RecordRateLimitDelayingConnection(RequestDefinition definition, IRateLimitGuard guard, string host, RateLimitItemType itemType, TimeSpan delay)
    {
        CryptoExchangeTelemetry.RequestRateLimitCounter.Add(1, [
            .._baseTags,
            ..GetRateLimitTags(definition, guard, host, itemType, RateLimitingBehaviour.Wait),
        ]);

        CryptoExchangeTelemetry.ConnectionsRateLimitDelayCounter.Add((long)delay.TotalMilliseconds, [
            .._baseTags,
            ..GetRateLimitTags(definition, guard, host, itemType, RateLimitingBehaviour.Wait)
        ]);

        Activity.Current?.AddEvent(new ActivityEvent(CryptoExchangeTelemetry.ActivityEvents.RateLimitDelayConnection, tags: [
            ..GetRateLimitTags(definition, guard, host, itemType, RateLimitingBehaviour.Wait),
            new(CryptoExchangeTelemetry.Tags.RateLimitDelay, (long)delay.TotalMilliseconds),
        ]));

        // TODO: Add a config for whether to emit this Span or not.
        using var delaySpan = StartActivity(CryptoExchangeTelemetry.Activities.RateLimitDelayingConnection, ActivityKind.Internal, tags: [
            ..GetRateLimitTags(definition, guard, host, itemType, RateLimitingBehaviour.Wait),
        ]);
        delaySpan?.SetEndTime(DateTime.UtcNow + delay);
    }

    /// <summary>
    /// Generates a collection of key-value pairs representing telemetry tags for rate-limiting operations.
    /// </summary>
    /// <param name="definition">The request definition containing details such as the HTTP method and URL path. This parameter can be <see
    /// langword="null"/>.</param>
    /// <param name="guard">The rate limit guard providing information about the rate-limiting context. Must not be <see langword="null"/>.</param>
    /// <param name="host">The server address associated with the request. Must not be <see langword="null"/> or empty.</param>
    /// <param name="itemType">The type of rate-limited item, such as a request or resource.</param>
    /// <param name="behavior">The rate-limiting behavior applied, such as throttling or rejecting requests.</param>
    /// <returns>An array of key-value pairs where each pair represents a telemetry tag and its associated value.</returns>
    private static KeyValuePair<string, object?>[] GetRateLimitTags(RequestDefinition definition, IRateLimitGuard guard, string host,
        RateLimitItemType itemType, RateLimitingBehaviour behavior)
    {
        return [
            new(CryptoExchangeTelemetry.Tags.HttpRequestMethod, definition?.Method),
            new(CryptoExchangeTelemetry.Tags.HttpRequestUrlPath, definition?.Path),
            new(CryptoExchangeTelemetry.Tags.ServerAddress, host),
            new(CryptoExchangeTelemetry.Tags.RateLimitGuardName, guard.Name),
            new(CryptoExchangeTelemetry.Tags.RateLimitItemType, itemType),
            new(CryptoExchangeTelemetry.Tags.RateLimitBehavior, behavior)
        ];
    }
}