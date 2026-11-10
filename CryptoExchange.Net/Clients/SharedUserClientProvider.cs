using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces.Clients;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Options;
using CryptoExchange.Net.SharedApis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Clients;

public abstract class SharedUserClientProvider<
    TRestClient,
    TSocketClient,
    TSharedClient,
    TRestOptions,
    TSocketOptions,
    TCredentials,
    TEnvironment>
    : UserClientProvider<
        TRestClient,
        TSocketClient,
        TRestOptions,
        TSocketOptions,
        TCredentials,
        TEnvironment>,
      ISharedUserClientProvider<TSharedClient, TEnvironment>
    where TRestClient : class, IRestClient<TCredentials>
    where TSocketClient : class, ISocketClient<TCredentials>
    where TSharedClient : class, ISharedApiClientBase
    where TRestOptions :
        RestExchangeOptions<TEnvironment, TCredentials>, new()
    where TSocketOptions :
        SocketExchangeOptions<TEnvironment, TCredentials>, new()
    where TCredentials : ApiCredentials
    where TEnvironment : TradeEnvironment
{
    private readonly object _sharedClientsLock = new();

    private readonly Dictionary<
        (string? UserIdentifier, string EnvironmentKey),
        (TRestClient RestClient, TSocketClient SocketClient, TSharedClient SharedClient)>
        _sharedClients = new();

    protected SharedUserClientProvider(
        HttpClient? httpClient,
        ILoggerFactory? loggerFactory,
        IOptions<TRestOptions> restOptions,
        IOptions<TSocketOptions> socketOptions,
        CredentialResolver? credentialResolver)
        : base(
            httpClient,
            loggerFactory,
            restOptions,
            socketOptions,
            credentialResolver)
    {
    }

    protected abstract TSharedClient ConstructSharedApiClient(
        TRestClient restClient,
        TSocketClient socketClient);

    public TSharedClient GetPublicSharedApiClient(
        TEnvironment? environment = null)
    {
        var rest = GetPublicRestClient(environment);
        var socket = GetPublicSocketClient(environment);

        return GetOrCreateSharedClient(
            null, environment, rest, socket);
    }

    public async Task<CallResult<TSharedClient>>
        TryGetSharedApiClientAsync(
            string userIdentifier,
            TEnvironment? environment = null,
            CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userIdentifier))
        {
            throw new ArgumentException(
                "A user identifier is required.",
                nameof(userIdentifier));
        }

        ct.ThrowIfCancellationRequested();

        var rest = await TryGetRestClientAsync(
            userIdentifier, environment, ct).ConfigureAwait(false);

        if (!rest.Success)
        {
            return CallResult.Fail<TSharedClient>(
                rest.Error, rest.OriginalData);
        }

        var socket = await TryGetSocketClientAsync(
            userIdentifier, environment, ct).ConfigureAwait(false);

        if (!socket.Success)
        {
            return CallResult.Fail<TSharedClient>(
                socket.Error, socket.OriginalData);
        }

        ct.ThrowIfCancellationRequested();

        return CallResult.Ok(GetOrCreateSharedClient(
            userIdentifier,
            environment,
            rest.Data,
            socket.Data));
    }

    private TSharedClient GetOrCreateSharedClient(
        string? userIdentifier,
        TEnvironment? environment,
        TRestClient rest,
        TSocketClient socket)
    {
        var key = (
            userIdentifier,
            GetEnvironmentCacheKey(environment));

        lock (_sharedClientsLock)
        {
            if (_sharedClients.TryGetValue(key, out var existing)
                && ReferenceEquals(existing.RestClient, rest)
                && ReferenceEquals(existing.SocketClient, socket))
            {
                return existing.SharedClient;
            }

            var shared = ConstructSharedApiClient(rest, socket);
            _sharedClients[key] = (rest, socket, shared);
            return shared;
        }
    }

    public override void InitializeUserClient(
        string userIdentifier,
        TCredentials credentials,
        TEnvironment? environment = null)
    {
        base.InitializeUserClient(
            userIdentifier, credentials, environment);

        RemoveSharedClient(userIdentifier, environment);
    }

    public override void ClearUserClients(
        string userIdentifier,
        TEnvironment? environment = null)
    {
        base.ClearUserClients(userIdentifier, environment);
        RemoveSharedClient(userIdentifier, environment);
    }

    public override void Clear()
    {
        base.Clear();

        lock (_sharedClientsLock)
            _sharedClients.Clear();
    }

    private void RemoveSharedClient(
        string userIdentifier,
        TEnvironment? environment)
    {
        lock (_sharedClientsLock)
        {
            _sharedClients.Remove((
                userIdentifier,
                GetEnvironmentCacheKey(environment)));
        }
    }

    ISharedApiClientBase
        ISharedUserClientProvider.GetPublicSharedApiClient(
            TradeEnvironment? environment)
        => GetPublicSharedApiClient(
            GetTypedEnvironment(environment));

    async Task<CallResult<ISharedApiClientBase>>
        ISharedUserClientProvider.TryGetSharedApiClientAsync(
            string userIdentifier,
            TradeEnvironment? environment,
            CancellationToken ct)
    {
        var result = await TryGetSharedApiClientAsync(
            userIdentifier,
            GetTypedEnvironment(environment),
            ct).ConfigureAwait(false);

        return new CallResult<ISharedApiClientBase>
        {
            Data = result.Data,
            Error = result.Error,
            OriginalData = result.OriginalData
        };
    }

    private static TEnvironment? GetTypedEnvironment(
        TradeEnvironment? environment)
    {
        if (environment == null)
            return null;

        if (environment is TEnvironment typed)
            return typed;

        throw new ArgumentException(
            $"An environment of type {typeof(TEnvironment).Name} is required.",
            nameof(environment));
    }
}