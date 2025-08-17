using System;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace CryptoExchange.Net.OpenTelemetry;

/*
 * Adding these extension requires referencing the package: OpenTelemetry.Api
 * I really hate this, but it follows the pattern of other OpenTelemetry extensions.
 * Alternative we can add documentation for consumers to call AddSource/AddMeter directly and
 * provide the names of the ActivitySource and Meter in a `public` class.
 */

public static class OpenTelemetryExtensions
{
    /// <summary>
    /// Enables tracing for CryptoExchange.Net
    /// </summary>
    public static TracerProviderBuilder AddCryptoExchangeNetInstrumentation(this TracerProviderBuilder builder)
    {
        if (builder is null) throw new ArgumentNullException(nameof(builder));
        return builder.AddSource(CryptoExchangeTelemetry.ActivitySourceName);
    }

    /// <summary>
    /// Enables metrics for CryptoExchange.Net
    /// </summary>
    public static MeterProviderBuilder AddCryptoExchangeNetInstrumentation(this MeterProviderBuilder builder)
    {
        if (builder is null) throw new ArgumentNullException(nameof(builder));
        return builder.AddMeter(CryptoExchangeTelemetry.MeterName);
    }
}