using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.Sessions;

public abstract partial class TradeSessionUserApi
{
    /// <inheritdoc cref="ICancelAllSpotOrdersOperation.CancelAllSpotOrdersOptions" />
    protected CancelAllSpotOrdersOptions? CancelAllSpotOrdersOptions
        => GetPublicCapability(SharedCapabilities.Orders.Spot.CancelAllOrders, TradingMode.Spot)
            ?.Capability.CancelAllSpotOrdersOptions;

    /// <inheritdoc cref="ICancelAllSpotOrdersOperation.CancelAllSpotOrdersAsync" />
    protected Task<IExchangeCallResult> CancelAllSpotOrdersAsync(
        CancelAllOrdersRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<ICancelAllSpotOrders>(
            SharedCapabilities.Orders.Spot.CancelAllOrders,
            request,
            (api, token) => api.CancelAllSpotOrdersAsync(request, token),
            ct);

    /// <inheritdoc cref="ICancelAllSpotSymbolOrdersOperation.CancelAllSpotSymbolOrdersOptions" />
    protected CancelAllSpotSymbolOrdersOptions? CancelAllSpotSymbolOrdersOptions
        => GetPublicCapability(SharedCapabilities.Orders.Spot.CancelAllSymbolOrders, TradingMode.Spot)
            ?.Capability.CancelAllSpotSymbolOrdersOptions;

    /// <inheritdoc cref="ICancelAllSpotSymbolOrdersOperation.CancelAllSpotSymbolOrdersAsync" />
    protected Task<IExchangeCallResult> CancelAllSpotSymbolOrdersAsync(
        CancelAllSymbolOrdersRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<ICancelAllSpotSymbolOrders>(
            SharedCapabilities.Orders.Spot.CancelAllSymbolOrders,
            request,
            (api, token) => api.CancelAllSpotSymbolOrdersAsync(request, token),
            ct);

    /// <inheritdoc cref="ICancelSpotOrderOperation.CancelSpotOrderOptions" />
    protected CancelSpotOrderOptions? CancelSpotOrderOptions
        => GetPublicCapability(SharedCapabilities.Orders.Spot.CancelOrder, TradingMode.Spot)
            ?.Capability.CancelSpotOrderOptions;

    /// <inheritdoc cref="ICancelSpotOrderOperation.CancelSpotOrderAsync" />
    protected Task<IExchangeCallResult<SharedId>> CancelSpotOrderAsync(
        CancelOrderRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<ICancelSpotOrder, SharedId>(
            SharedCapabilities.Orders.Spot.CancelOrder,
            request,
            (api, token) => api.CancelSpotOrderAsync(request, token),
            ct);

    /// <inheritdoc cref="ICancelSpotOrderByClientOrderIdOperation.CancelSpotOrderByClientOrderIdOptions" />
    protected CancelSpotOrderByClientOrderIdOptions? CancelSpotOrderByClientOrderIdOptions
        => GetPublicCapability(SharedCapabilities.Orders.Spot.CancelOrderByClientOrderId, TradingMode.Spot)
            ?.Capability.CancelSpotOrderByClientOrderIdOptions;

    /// <inheritdoc cref="ICancelSpotOrderByClientOrderIdOperation.CancelSpotOrderByClientOrderIdAsync" />
    protected Task<IExchangeCallResult<SharedId>> CancelSpotOrderByClientOrderIdAsync(
        CancelOrderRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<ICancelSpotOrderByClientOrderId, SharedId>(
            SharedCapabilities.Orders.Spot.CancelOrderByClientOrderId,
            request,
            (api, token) => api.CancelSpotOrderByClientOrderIdAsync(request, token),
            ct);

    /// <inheritdoc cref="IEditSpotOrderOperation.EditSpotOrderOptions" />
    protected EditSpotOrderOptions? EditSpotOrderOptions
        => GetPublicCapability(SharedCapabilities.Orders.Spot.EditOrder, TradingMode.Spot)
            ?.Capability.EditSpotOrderOptions;

    /// <inheritdoc cref="IEditSpotOrderOperation.EditSpotOrderAsync" />
    protected Task<IExchangeCallResult<SharedId>> EditSpotOrderAsync(
        EditOrderRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<IEditSpotOrder, SharedId>(
            SharedCapabilities.Orders.Spot.EditOrder,
            request,
            (api, token) => api.EditSpotOrderAsync(request, token),
            ct);

    /// <inheritdoc cref="IEditSpotOrderByClientOrderIdOperation.EditSpotOrderByClientOrderIdOptions" />
    protected EditSpotOrderByClientOrderIdOptions? EditSpotOrderByClientOrderIdOptions
        => GetPublicCapability(SharedCapabilities.Orders.Spot.EditOrderByClientOrderId, TradingMode.Spot)
            ?.Capability.EditSpotOrderByClientOrderIdOptions;

    /// <inheritdoc cref="IEditSpotOrderByClientOrderIdOperation.EditSpotOrderByClientOrderIdAsync" />
    protected Task<IExchangeCallResult<SharedId>> EditSpotOrderByClientOrderIdAsync(
        EditOrderRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<IEditSpotOrderByClientOrderId, SharedId>(
            SharedCapabilities.Orders.Spot.EditOrderByClientOrderId,
            request,
            (api, token) => api.EditSpotOrderByClientOrderIdAsync(request, token),
            ct);

    /// <inheritdoc cref="IGetClosedSpotOrdersOperation.GetClosedSpotOrdersOptions" />
    protected GetSpotClosedOrdersOptions? GetClosedSpotOrdersOptions
        => GetPublicCapability(SharedCapabilities.Orders.Spot.GetClosedOrders, TradingMode.Spot)
            ?.Capability.GetClosedSpotOrdersOptions;

    /// <inheritdoc cref="IGetClosedSpotOrdersOperation.GetClosedSpotOrdersAsync" />
    protected Task<IExchangeCallResult<SharedSpotOrder[]>> GetClosedSpotOrdersAsync(
        GetClosedOrdersRequest request,
        PageRequest? nextPageToken = null,
        CancellationToken ct = default)
        => ExecuteAsync<IGetClosedSpotOrders, SharedSpotOrder[]>(
            SharedCapabilities.Orders.Spot.GetClosedOrders,
            request,
            (api, token) => api.GetClosedSpotOrdersAsync(request, nextPageToken, token),
            ct);

    /// <inheritdoc cref="IGetOpenSpotOrdersOperation.GetOpenSpotOrdersOptions" />
    protected GetOpenSpotOrdersOptions? GetOpenSpotOrdersOptions
        => GetPublicCapability(SharedCapabilities.Orders.Spot.GetOpenOrders, TradingMode.Spot)
            ?.Capability.GetOpenSpotOrdersOptions;

    /// <inheritdoc cref="IGetOpenSpotOrdersOperation.GetOpenSpotOrdersAsync" />
    protected Task<IExchangeCallResult<SharedSpotOrder[]>> GetOpenSpotOrdersAsync(
        GetOpenOrdersRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<IGetOpenSpotOrders, SharedSpotOrder[]>(
            SharedCapabilities.Orders.Spot.GetOpenOrders,
            request,
            (api, token) => api.GetOpenSpotOrdersAsync(request, token),
            ct);

    /// <inheritdoc cref="IGetSpotOrderOperation.GetSpotOrderOptions" />
    protected GetSpotOrderOptions? GetSpotOrderOptions
        => GetPublicCapability(SharedCapabilities.Orders.Spot.GetOrder, TradingMode.Spot)
            ?.Capability.GetSpotOrderOptions;

    /// <inheritdoc cref="IGetSpotOrderOperation.GetSpotOrderAsync" />
    protected Task<IExchangeCallResult<SharedSpotOrder>> GetSpotOrderAsync(
        GetOrderRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<IGetSpotOrder, SharedSpotOrder>(
            SharedCapabilities.Orders.Spot.GetOrder,
            request,
            (api, token) => api.GetSpotOrderAsync(request, token),
            ct);

    /// <inheritdoc cref="IGetSpotOrderByClientOrderIdOperation.GetSpotOrderByClientOrderIdOptions" />
    protected GetSpotOrderByClientOrderIdOptions? GetSpotOrderByClientOrderIdOptions
        => GetPublicCapability(SharedCapabilities.Orders.Spot.GetOrderByClientOrderId, TradingMode.Spot)
            ?.Capability.GetSpotOrderByClientOrderIdOptions;

    /// <inheritdoc cref="IGetSpotOrderByClientOrderIdOperation.GetSpotOrderByClientOrderIdAsync" />
    protected Task<IExchangeCallResult<SharedSpotOrder>> GetSpotOrderByClientOrderIdAsync(
        GetOrderRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<IGetSpotOrderByClientOrderId, SharedSpotOrder>(
            SharedCapabilities.Orders.Spot.GetOrderByClientOrderId,
            request,
            (api, token) => api.GetSpotOrderByClientOrderIdAsync(request, token),
            ct);

    /// <inheritdoc cref="IPlaceMultipleSpotOrdersOperation.PlaceMultipleSpotOrdersOptions" />
    protected PlaceMultipleSpotOrdersOptions? PlaceMultipleSpotOrdersOptions
        => GetPublicCapability(SharedCapabilities.Orders.Spot.PlaceMultipleOrders, TradingMode.Spot)
            ?.Capability.PlaceMultipleSpotOrdersOptions;

    /// <inheritdoc cref="IPlaceMultipleSpotOrdersOperation.PlaceMultipleSpotOrdersAsync" />
    protected Task<IExchangeCallResult<CallResult<SharedId>[]>> PlaceMultipleSpotOrdersAsync(
        PlaceMultipleSpotOrdersRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<IPlaceMultipleSpotOrders, CallResult<SharedId>[]>(
            SharedCapabilities.Orders.Spot.PlaceMultipleOrders,
            request,
            (api, token) => api.PlaceMultipleSpotOrdersAsync(request, token),
            ct);

    /// <inheritdoc cref="IPlaceSpotOrderOperation.PlaceSpotOrderOptions" />
    protected PlaceSpotOrderOptions? PlaceSpotOrderOptions
        => GetPublicCapability(SharedCapabilities.Orders.Spot.PlaceOrder, TradingMode.Spot)
            ?.Capability.PlaceSpotOrderOptions;

    /// <inheritdoc cref="IPlaceSpotOrderOperation.PlaceSpotOrderAsync" />
    protected Task<IExchangeCallResult<SharedId>> PlaceSpotOrderAsync(
        PlaceSpotOrderRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<IPlaceSpotOrder, SharedId>(
            SharedCapabilities.Orders.Spot.PlaceOrder,
            request,
            (api, token) => api.PlaceSpotOrderAsync(request, token),
            ct);
}
