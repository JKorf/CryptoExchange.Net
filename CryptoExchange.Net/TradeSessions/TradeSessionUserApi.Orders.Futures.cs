using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.Sessions;

public abstract partial class TradeSessionUserApi
{
    /// <inheritdoc cref="ICancelAllFuturesOrdersOperation.CancelAllFuturesOrdersOptions" />
    protected CancelAllFuturesOrdersOptions? CancelAllFuturesOrdersOptions
        => GetPublicCapability(SharedCapabilities.Orders.Futures.CancelAllOrders)
            ?.Capability.CancelAllFuturesOrdersOptions;

    /// <inheritdoc cref="ICancelAllFuturesOrdersOperation.CancelAllFuturesOrdersAsync" />
    protected Task<IExchangeCallResult> CancelAllFuturesOrdersAsync(
        CancelAllOrdersRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<ICancelAllFuturesOrders>(
            SharedCapabilities.Orders.Futures.CancelAllOrders,
            request,
            (api, token) => api.CancelAllFuturesOrdersAsync(request, token),
            ct);

    /// <inheritdoc cref="ICancelAllFuturesSymbolOrdersOperation.CancelAllFuturesSymbolOrdersOptions" />
    protected CancelAllFuturesSymbolOrdersOptions? CancelAllFuturesSymbolOrdersOptions
        => GetPublicCapability(SharedCapabilities.Orders.Futures.CancelAllSymbolOrders)
            ?.Capability.CancelAllFuturesSymbolOrdersOptions;

    /// <inheritdoc cref="ICancelAllFuturesSymbolOrdersOperation.CancelAllFuturesSymbolOrdersAsync" />
    protected Task<IExchangeCallResult> CancelAllFuturesSymbolOrdersAsync(
        CancelAllSymbolOrdersRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<ICancelAllFuturesSymbolOrders>(
            SharedCapabilities.Orders.Futures.CancelAllSymbolOrders,
            request,
            (api, token) => api.CancelAllFuturesSymbolOrdersAsync(request, token),
            ct);

    /// <inheritdoc cref="ICancelFuturesOrderOperation.CancelFuturesOrderOptions" />
    protected CancelFuturesOrderOptions? CancelFuturesOrderOptions
        => GetPublicCapability(SharedCapabilities.Orders.Futures.CancelOrder)
            ?.Capability.CancelFuturesOrderOptions;

    /// <inheritdoc cref="ICancelFuturesOrderOperation.CancelFuturesOrderAsync" />
    protected Task<IExchangeCallResult<SharedId>> CancelFuturesOrderAsync(
        CancelOrderRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<ICancelFuturesOrder, SharedId>(
            SharedCapabilities.Orders.Futures.CancelOrder,
            request,
            (api, token) => api.CancelFuturesOrderAsync(request, token),
            ct);

    /// <inheritdoc cref="ICancelFuturesOrderByClientOrderIdOperation.CancelFuturesOrderByClientOrderIdOptions" />
    protected CancelFuturesOrderByClientOrderIdOptions? CancelFuturesOrderByClientOrderIdOptions
        => GetPublicCapability(SharedCapabilities.Orders.Futures.CancelOrderByClientOrderId)
            ?.Capability.CancelFuturesOrderByClientOrderIdOptions;

    /// <inheritdoc cref="ICancelFuturesOrderByClientOrderIdOperation.CancelFuturesOrderByClientOrderIdAsync" />
    protected Task<IExchangeCallResult<SharedId>> CancelFuturesOrderByClientOrderIdAsync(
        CancelOrderRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<ICancelFuturesOrderByClientOrderId, SharedId>(
            SharedCapabilities.Orders.Futures.CancelOrderByClientOrderId,
            request,
            (api, token) => api.CancelFuturesOrderByClientOrderIdAsync(request, token),
            ct);

    /// <inheritdoc cref="IEditFuturesOrderOperation.EditFuturesOrderOptions" />
    protected EditFuturesOrderOptions? EditFuturesOrderOptions
        => GetPublicCapability(SharedCapabilities.Orders.Futures.EditOrder)
            ?.Capability.EditFuturesOrderOptions;

    /// <inheritdoc cref="IEditFuturesOrderOperation.EditFuturesOrderAsync" />
    protected Task<IExchangeCallResult<SharedId>> EditFuturesOrderAsync(
        EditOrderRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<IEditFuturesOrder, SharedId>(
            SharedCapabilities.Orders.Futures.EditOrder,
            request,
            (api, token) => api.EditFuturesOrderAsync(request, token),
            ct);

    /// <inheritdoc cref="IEditFuturesOrderByClientOrderIdOperation.EditFuturesOrderByClientOrderIdOptions" />
    protected EditFuturesOrderByClientOrderIdOptions? EditFuturesOrderByClientOrderIdOptions
        => GetPublicCapability(SharedCapabilities.Orders.Futures.EditOrderByClientOrderId)
            ?.Capability.EditFuturesOrderByClientOrderIdOptions;

    /// <inheritdoc cref="IEditFuturesOrderByClientOrderIdOperation.EditFuturesOrderByClientOrderIdAsync" />
    protected Task<IExchangeCallResult<SharedId>> EditFuturesOrderByClientOrderIdAsync(
        EditOrderRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<IEditFuturesOrderByClientOrderId, SharedId>(
            SharedCapabilities.Orders.Futures.EditOrderByClientOrderId,
            request,
            (api, token) => api.EditFuturesOrderByClientOrderIdAsync(request, token),
            ct);

    /// <inheritdoc cref="IGetClosedFuturesOrdersOperation.GetClosedFuturesOrdersOptions" />
    protected GetFuturesClosedOrdersOptions? GetClosedFuturesOrdersOptions
        => GetPublicCapability(SharedCapabilities.Orders.Futures.GetClosedOrders)
            ?.Capability.GetClosedFuturesOrdersOptions;

    /// <inheritdoc cref="IGetClosedFuturesOrdersOperation.GetClosedFuturesOrdersAsync" />
    protected Task<IExchangeCallResult<SharedFuturesOrder[]>> GetClosedFuturesOrdersAsync(
        GetClosedOrdersRequest request,
        PageRequest? nextPageToken = null,
        CancellationToken ct = default)
        => ExecuteAsync<IGetClosedFuturesOrders, SharedFuturesOrder[]>(
            SharedCapabilities.Orders.Futures.GetClosedOrders,
            request,
            (api, token) => api.GetClosedFuturesOrdersAsync(request, nextPageToken, token),
            ct);

    /// <inheritdoc cref="IGetFuturesOrderOperation.GetFuturesOrderOptions" />
    protected GetFuturesOrderOptions? GetFuturesOrderOptions
        => GetPublicCapability(SharedCapabilities.Orders.Futures.GetOrder)
            ?.Capability.GetFuturesOrderOptions;

    /// <inheritdoc cref="IGetFuturesOrderOperation.GetFuturesOrderAsync" />
    protected Task<IExchangeCallResult<SharedFuturesOrder>> GetFuturesOrderAsync(
        GetOrderRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<IGetFuturesOrder, SharedFuturesOrder>(
            SharedCapabilities.Orders.Futures.GetOrder,
            request,
            (api, token) => api.GetFuturesOrderAsync(request, token),
            ct);

    /// <inheritdoc cref="IGetFuturesOrderByClientOrderIdOperation.GetFuturesOrderByClientOrderIdOptions" />
    protected GetFuturesOrderByClientOrderIdOptions? GetFuturesOrderByClientOrderIdOptions
        => GetPublicCapability(SharedCapabilities.Orders.Futures.GetOrderByClientOrderId)
            ?.Capability.GetFuturesOrderByClientOrderIdOptions;

    /// <inheritdoc cref="IGetFuturesOrderByClientOrderIdOperation.GetFuturesOrderByClientOrderIdAsync" />
    protected Task<IExchangeCallResult<SharedFuturesOrder>> GetFuturesOrderByClientOrderIdAsync(
        GetOrderRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<IGetFuturesOrderByClientOrderId, SharedFuturesOrder>(
            SharedCapabilities.Orders.Futures.GetOrderByClientOrderId,
            request,
            (api, token) => api.GetFuturesOrderByClientOrderIdAsync(request, token),
            ct);

    /// <inheritdoc cref="IGetOpenFuturesOrdersOperation.GetOpenFuturesOrdersOptions" />
    protected GetOpenFuturesOrdersOptions? GetOpenFuturesOrdersOptions
        => GetPublicCapability(SharedCapabilities.Orders.Futures.GetOpenOrders)
            ?.Capability.GetOpenFuturesOrdersOptions;

    /// <inheritdoc cref="IGetOpenFuturesOrdersOperation.GetOpenFuturesOrdersAsync" />
    protected Task<IExchangeCallResult<SharedFuturesOrder[]>> GetOpenFuturesOrdersAsync(
        GetOpenOrdersRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<IGetOpenFuturesOrders, SharedFuturesOrder[]>(
            SharedCapabilities.Orders.Futures.GetOpenOrders,
            request,
            (api, token) => api.GetOpenFuturesOrdersAsync(request, token),
            ct);

    /// <inheritdoc cref="IPlaceFuturesOrderOperation.PlaceFuturesOrderOptions" />
    protected PlaceFuturesOrderOptions? PlaceFuturesOrderOptions
        => GetPublicCapability(SharedCapabilities.Orders.Futures.PlaceOrder)
            ?.Capability.PlaceFuturesOrderOptions;

    /// <inheritdoc cref="IPlaceFuturesOrderOperation.PlaceFuturesOrderAsync" />
    protected Task<IExchangeCallResult<SharedId>> PlaceFuturesOrderAsync(
        PlaceFuturesOrderRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<IPlaceFuturesOrder, SharedId>(
            SharedCapabilities.Orders.Futures.PlaceOrder,
            request,
            (api, token) => api.PlaceFuturesOrderAsync(request, token),
            ct);

    /// <inheritdoc cref="IPlaceMultipleFuturesOrdersOperation.PlaceMultipleFuturesOrdersOptions" />
    protected PlaceMultipleFuturesOrdersOptions? PlaceMultipleFuturesOrdersOptions
        => GetPublicCapability(SharedCapabilities.Orders.Futures.PlaceMultipleOrders)
            ?.Capability.PlaceMultipleFuturesOrdersOptions;

    /// <inheritdoc cref="IPlaceMultipleFuturesOrdersOperation.PlaceMultipleFuturesOrdersAsync" />
    protected Task<IExchangeCallResult<CallResult<SharedId>[]>> PlaceMultipleFuturesOrdersAsync(
        PlaceMultipleFuturesOrdersRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<IPlaceMultipleFuturesOrders, CallResult<SharedId>[]>(
            SharedCapabilities.Orders.Futures.PlaceMultipleOrders,
            request,
            (api, token) => api.PlaceMultipleFuturesOrdersAsync(request, token),
            ct);
}
