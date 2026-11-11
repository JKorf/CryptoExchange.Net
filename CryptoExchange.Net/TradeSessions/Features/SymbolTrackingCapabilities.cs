using CryptoExchange.Net.Interfaces.Clients;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using System;

namespace CryptoExchange.Net.TradeSessions.Features;

internal static class SymbolTrackingCapabilities
{
    internal static TCapability Resolve<TCapability>(
        ISharedUserClientProvider provider,
        SharedSymbol symbol,
        TradeEnvironment? environment,
        SharedCapabilityReference<TCapability> capability,
        string feature)
        where TCapability : ISharedApiCapability
    {
        var client = provider.GetPublicSharedApiClient(environment);
        var resolved = client.GetCapability(capability, symbol.TradingMode)
            ?? throw new NotSupportedException(
                $"{client.Exchange} does not support {feature} for {symbol.TradingMode}.");
        return resolved.Capability;
    }
}
