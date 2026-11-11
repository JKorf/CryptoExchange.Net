using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.Sessions;

public abstract partial class TradingSessionUser
{
    public bool Supports<TCapability>(
        SharedCapabilityReference<TCapability> capability,
        TradingMode tradingMode)
        where TCapability : ISharedApiCapability
        => GetOptions(capability, tradingMode) != null;

    public CapabilityOptions? GetOptions<TCapability>(
        SharedCapabilityReference<TCapability> capability,
        TradingMode tradingMode)
        where TCapability : ISharedApiCapability
        => GetPublicCapability(capability, tradingMode)?.Options;

    protected SharedCapabilityResolution<TCapability>? GetPublicCapability<TCapability>(
        SharedCapabilityReference<TCapability> capability,
        TradingMode tradingMode)
        where TCapability : ISharedApiCapability
    {
        if (capability == null)
            throw new ArgumentNullException(nameof(capability));

        return _provider.GetPublicSharedApiClient(Environment).GetCapability(capability, tradingMode);
    }

    protected TCapability GetRequiredPublicCapability<TCapability>(
        SharedCapabilityReference<TCapability> capability,
        TradingMode tradingMode)
        where TCapability : ISharedApiCapability
    {
        var resolved = GetPublicCapability(capability, tradingMode)
            ?? throw new NotSupportedException(
                $"{_provider.ExchangeName} does not support {capability} for {tradingMode}.");
        return resolved.Capability;
    }

    private async Task<TResult> ExecuteAsync<TCapability, TResult>(
        SharedCapabilityReference<TCapability> capability,
        SharedRequest request,
        TradingMode? tradingMode,
        Func<TCapability, CancellationToken, Task<TResult>> execute,
        Func<Error, TResult> fail,
        CancellationToken ct)
        where TCapability : ISharedApiCapability
        where TResult : IExchangeCallResult
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (tradingMode == null)
            return fail(new InvalidOperationError("A trading mode is required for futures order operations."));

        var client = await _provider.TryGetSharedApiClientAsync(
            UserIdentifier, Environment, ct).ConfigureAwait(false);
        if (!client.Success)
            return fail(client.Error);

        var resolved = client.Data.GetCapability(capability, tradingMode.Value);
        if (resolved == null)
        {
            return fail(new InvalidOperationError(
                $"{_provider.ExchangeName} does not support {capability} for {tradingMode}."));
        }

        return await execute(resolved.Capability, ct).ConfigureAwait(false);
    }

    private static TradingMode? GetTradingMode(SharedRequest request, TradingMode? defaultMode = null)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        return request.TradingMode ?? defaultMode;
    }
}
