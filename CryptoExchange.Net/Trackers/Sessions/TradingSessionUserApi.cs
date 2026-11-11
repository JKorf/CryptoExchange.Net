using CryptoExchange.Net.Interfaces.Clients;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.Sessions.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.Sessions;

/// <summary>
/// Shared execution for user-bound operations within a specific API surface
/// </summary>
public abstract partial class TradingSessionUserApi : ITradingUserApi
{
    private readonly ISharedUserClientProvider _provider;
    private readonly Func<ISharedApiClientBase, ISharedApi[]> _selectApis;

    /// <inheritdoc />
    public string UserIdentifier { get; }

    /// <inheritdoc />
    public TradeEnvironment? Environment { get; }

    protected TradingSessionUserApi(
        string userIdentifier,
        TradeEnvironment? environment,
        ISharedUserClientProvider provider,
        Func<ISharedApiClientBase, ISharedApi[]> selectApis)
    {
        UserIdentifier = userIdentifier;
        Environment = environment;
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _selectApis = selectApis ?? throw new ArgumentNullException(nameof(selectApis));
    }

    /// <inheritdoc />
    public bool Supports<TCapability>(
        SharedCapabilityReference<TCapability> capability,
        TradingMode? tradingMode = null)
        where TCapability : ISharedApiCapability
        => GetOptions(capability, tradingMode) != null;

    /// <inheritdoc />
    public CapabilityOptions? GetOptions<TCapability>(
        SharedCapabilityReference<TCapability> capability,
        TradingMode? tradingMode = null)
        where TCapability : ISharedApiCapability
        => GetPublicCapability(capability, tradingMode)?.Options;

    protected SharedCapabilityResolution<TCapability>? GetPublicCapability<TCapability>(
        SharedCapabilityReference<TCapability> capability,
        TradingMode? tradingMode = null)
        where TCapability : ISharedApiCapability
        => ResolveCapability(_provider.GetPublicSharedApiClient(Environment), capability, tradingMode);

    protected TCapability GetRequiredPublicCapability<TCapability>(
        SharedCapabilityReference<TCapability> capability,
        TradingMode? tradingMode = null)
        where TCapability : ISharedApiCapability
    {
        var resolved = GetPublicCapability(capability, tradingMode)
            ?? throw new NotSupportedException(GetUnsupportedMessage(capability, tradingMode));
        return resolved.Capability;
    }

    private SharedCapabilityResolution<TCapability>? ResolveCapability<TCapability>(
        ISharedApiClientBase client,
        SharedCapabilityReference<TCapability> capability,
        TradingMode? tradingMode)
        where TCapability : ISharedApiCapability
    {
        if (capability == null)
            throw new ArgumentNullException(nameof(capability));

        var apis = _selectApis(client);
        if (apis == null || apis.Length == 0)
            throw new InvalidOperationException("The API selector must return at least one API.");

        var scoped = new ScopedSharedApiClient(client.PreferredTransport, apis);
        return tradingMode.HasValue
            ? scoped.GetCapability(capability, tradingMode.Value)
            : scoped.GetCapability(capability);
    }

    protected Task<IExchangeCallResult<TData>> ExecuteAsync<TCapability, TData>(
        SharedCapabilityReference<TCapability> capability,
        SharedRequest request,
        Func<TCapability, CancellationToken, Task<IExchangeCallResult<TData>>> execute,
        CancellationToken ct = default)
        where TCapability : ISharedApiCapability
        => ExecuteAsync(
            capability,
            request,
            GetTradingMode(request),
            execute,
            error => HttpResult.Fail<TData>(_provider.ExchangeName, error),
            ct);

    protected Task<IExchangeCallResult> ExecuteAsync<TCapability>(
        SharedCapabilityReference<TCapability> capability,
        SharedRequest request,
        Func<TCapability, CancellationToken, Task<IExchangeCallResult>> execute,
        CancellationToken ct = default)
        where TCapability : ISharedApiCapability
        => ExecuteAsync(
            capability,
            request,
            GetTradingMode(request),
            execute,
            error => HttpResult.Fail(_provider.ExchangeName, error),
            ct);

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
        if (GetPublicCapability(capability, tradingMode) == null)
            return fail(new InvalidOperationError(GetUnsupportedMessage(capability, tradingMode)));

        var client = await _provider.TryGetSharedApiClientAsync(
            UserIdentifier, Environment, ct).ConfigureAwait(false);
        if (!client.Success)
            return fail(client.Error);

        var resolved = ResolveCapability(client.Data, capability, tradingMode);
        if (resolved == null)
            return fail(new InvalidOperationError(GetUnsupportedMessage(capability, tradingMode)));

        return await execute(resolved.Capability, ct).ConfigureAwait(false);
    }

    private string GetUnsupportedMessage<TCapability>(
        SharedCapabilityReference<TCapability> capability,
        TradingMode? tradingMode)
        where TCapability : ISharedApiCapability
        => $"{_provider.ExchangeName} does not support {capability} on this API"
            + (tradingMode.HasValue ? $" for {tradingMode}." : ".");

    private static TradingMode? GetTradingMode(SharedRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        return request.TradingMode;
    }

    private sealed class ScopedSharedApiClient : SharedApiClientBase
    {
        internal ScopedSharedApiClient(SharedTransport preferredTransport, ISharedApi[] apis)
            : base(preferredTransport, apis)
        {
        }
    }
}
