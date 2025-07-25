using System;
using System.Collections.Generic;
using System.Diagnostics;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.RateLimiting;
using CryptoExchange.Net.RateLimiting.Interfaces;

namespace CryptoExchange.Net.OpenTelemetry;

public partial class Telemetry
{
    public void RecordRateLimitFailed(RequestDefinition definition, IRateLimitGuard guard, string host, RateLimitItemType itemType)
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

    public void RecordRateLimitDelayingRequest(RequestDefinition definition, IRateLimitGuard guard, string host, RateLimitItemType itemType, TimeSpan delay)
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

    public void RecordRateLimitDelayingConnection(RequestDefinition definition, IRateLimitGuard guard, string host, RateLimitItemType itemType, TimeSpan delay)
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