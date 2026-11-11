using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.Sessions;

public abstract partial class TradingSessionUserApi
{
    /// <inheritdoc cref="ICancelAllSpotOrdersOperation.CancelAllSpotOrdersOptions" />
    protected CancelAllSpotOrdersOptions? CancelAllSpotOrdersOptions
        => GetPublicCapability(SharedCapabilities.Orders.Spot.CancelAllOrders, TradingMode.Spot)
            ?.Capability.CancelAllSpotOrdersOptions;

    /// <inheritdoc cref="ICancelAllSpotOrdersOperation.CancelAllSpotOrdersAsync" />
    protected Task<IExchangeCallResult> CancelAllSpotOrdersAsync(
        CancelAllOrdersRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<ICancelAllSpotOrders, IExchangeCallResult>(
            SharedCapabilities.Orders.Spot.CancelAllOrders,
            request,
            GetTradingMode(request),
            (api, token) => api.CancelAllSpotOrdersAsync(request, token),
            error => HttpResult.Fail(_provider.ExchangeName, error),
            ct);

    /// <inheritdoc cref="ICancelAllSpotSymbolOrdersOperation.CancelAllSpotSymbolOrdersOptions" />
    protected CancelAllSpotSymbolOrdersOptions? CancelAllSpotSymbolOrdersOptions
        => GetPublicCapability(SharedCapabilities.Orders.Spot.CancelAllSymbolOrders, TradingMode.Spot)
            ?.Capability.CancelAllSpotSymbolOrdersOptions;

    /// <inheritdoc cref="ICancelAllSpotSymbolOrdersOperation.CancelAllSpotSymbolOrdersAsync" />
    protected Task<IExchangeCallResult> CancelAllSpotSymbolOrdersAsync(
        CancelAllSymbolOrdersRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<ICancelAllSpotSymbolOrders, IExchangeCallResult>(
            SharedCapabilities.Orders.Spot.CancelAllSymbolOrders,
            request,
            GetTradingMode(request),
            (api, token) => api.CancelAllSpotSymbolOrdersAsync(request, token),
            error => HttpResult.Fail(_provider.ExchangeName, error),
            ct);

    /// <inheritdoc cref="ICancelSpotOrderOperation.CancelSpotOrderOptions" />
    protected CancelSpotOrderOptions? CancelSpotOrderOptions
        => GetPublicCapability(SharedCapabilities.Orders.Spot.CancelOrder, TradingMode.Spot)
            ?.Capability.CancelSpotOrderOptions;

    /// <inheritdoc cref="ICancelSpotOrderOperation.CancelSpotOrderAsync" />
    protected Task<IExchangeCallResult<SharedId>> CancelSpotOrderAsync(
        CancelOrderRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<ICancelSpotOrder, IExchangeCallResult<SharedId>>(
            SharedCapabilities.Orders.Spot.CancelOrder,
            request,
            GetTradingMode(request),
            (api, token) => api.CancelSpotOrderAsync(request, token),
            error => HttpResult.Fail<SharedId>(_provider.ExchangeName, error),
            ct);

    /// <inheritdoc cref="ICancelSpotOrderByClientOrderIdOperation.CancelSpotOrderByClientOrderIdOptions" />
    protected CancelSpotOrderByClientOrderIdOptions? CancelSpotOrderByClientOrderIdOptions
        => GetPublicCapability(SharedCapabilities.Orders.Spot.CancelOrderByClientOrderId, TradingMode.Spot)
            ?.Capability.CancelSpotOrderByClientOrderIdOptions;

    /// <inheritdoc cref="ICancelSpotOrderByClientOrderIdOperation.CancelSpotOrderByClientOrderIdAsync" />
    protected Task<IExchangeCallResult<SharedId>> CancelSpotOrderByClientOrderIdAsync(
        CancelOrderRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<ICancelSpotOrderByClientOrderId, IExchangeCallResult<SharedId>>(
            SharedCapabilities.Orders.Spot.CancelOrderByClientOrderId,
            request,
            GetTradingMode(request),
            (api, token) => api.CancelSpotOrderByClientOrderIdAsync(request, token),
            error => HttpResult.Fail<SharedId>(_provider.ExchangeName, error),
            ct);

    /// <inheritdoc cref="IEditSpotOrderOperation.EditSpotOrderOptions" />
    protected EditSpotOrderOptions? EditSpotOrderOptions
        => GetPublicCapability(SharedCapabilities.Orders.Spot.EditOrder, TradingMode.Spot)
            ?.Capability.EditSpotOrderOptions;

    /// <inheritdoc cref="IEditSpotOrderOperation.EditSpotOrderAsync" />
    protected Task<IExchangeCallResult<SharedId>> EditSpotOrderAsync(
        EditOrderRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<IEditSpotOrder, IExchangeCallResult<SharedId>>(
            SharedCapabilities.Orders.Spot.EditOrder,
            request,
            GetTradingMode(request),
            (api, token) => api.EditSpotOrderAsync(request, token),
            error => HttpResult.Fail<SharedId>(_provider.ExchangeName, error),
            ct);

    /// <inheritdoc cref="IEditSpotOrderByClientOrderIdOperation.EditSpotOrderByClientOrderIdOptions" />
    protected EditSpotOrderByClientOrderIdOptions? EditSpotOrderByClientOrderIdOptions
        => GetPublicCapability(SharedCapabilities.Orders.Spot.EditOrderByClientOrderId, TradingMode.Spot)
            ?.Capability.EditSpotOrderByClientOrderIdOptions;

    /// <inheritdoc cref="IEditSpotOrderByClientOrderIdOperation.EditSpotOrderByClientOrderIdAsync" />
    protected Task<IExchangeCallResult<SharedId>> EditSpotOrderByClientOrderIdAsync(
        EditOrderRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<IEditSpotOrderByClientOrderId, IExchangeCallResult<SharedId>>(
            SharedCapabilities.Orders.Spot.EditOrderByClientOrderId,
            request,
            GetTradingMode(request),
            (api, token) => api.EditSpotOrderByClientOrderIdAsync(request, token),
            error => HttpResult.Fail<SharedId>(_provider.ExchangeName, error),
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
        => ExecuteAsync<IGetClosedSpotOrders, IExchangeCallResult<SharedSpotOrder[]>>(
            SharedCapabilities.Orders.Spot.GetClosedOrders,
            request,
            GetTradingMode(request),
            (api, token) => api.GetClosedSpotOrdersAsync(request, nextPageToken, token),
            error => HttpResult.Fail<SharedSpotOrder[]>(_provider.ExchangeName, error),
            ct);

    /// <inheritdoc cref="IGetOpenSpotOrdersOperation.GetOpenSpotOrdersOptions" />
    protected GetOpenSpotOrdersOptions? GetOpenSpotOrdersOptions
        => GetPublicCapability(SharedCapabilities.Orders.Spot.GetOpenOrders, TradingMode.Spot)
            ?.Capability.GetOpenSpotOrdersOptions;

    /// <inheritdoc cref="IGetOpenSpotOrdersOperation.GetOpenSpotOrdersAsync" />
    protected Task<IExchangeCallResult<SharedSpotOrder[]>> GetOpenSpotOrdersAsync(
        GetOpenOrdersRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<IGetOpenSpotOrders, IExchangeCallResult<SharedSpotOrder[]>>(
            SharedCapabilities.Orders.Spot.GetOpenOrders,
            request,
            GetTradingMode(request),
            (api, token) => api.GetOpenSpotOrdersAsync(request, token),
            error => HttpResult.Fail<SharedSpotOrder[]>(_provider.ExchangeName, error),
            ct);

    /// <inheritdoc cref="IGetSpotOrderOperation.GetSpotOrderOptions" />
    protected GetSpotOrderOptions? GetSpotOrderOptions
        => GetPublicCapability(SharedCapabilities.Orders.Spot.GetOrder, TradingMode.Spot)
            ?.Capability.GetSpotOrderOptions;

    /// <inheritdoc cref="IGetSpotOrderOperation.GetSpotOrderAsync" />
    protected Task<IExchangeCallResult<SharedSpotOrder>> GetSpotOrderAsync(
        GetOrderRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<IGetSpotOrder, IExchangeCallResult<SharedSpotOrder>>(
            SharedCapabilities.Orders.Spot.GetOrder,
            request,
            GetTradingMode(request),
            (api, token) => api.GetSpotOrderAsync(request, token),
            error => HttpResult.Fail<SharedSpotOrder>(_provider.ExchangeName, error),
            ct);

    /// <inheritdoc cref="IGetSpotOrderByClientOrderIdOperation.GetSpotOrderByClientOrderIdOptions" />
    protected GetSpotOrderByClientOrderIdOptions? GetSpotOrderByClientOrderIdOptions
        => GetPublicCapability(SharedCapabilities.Orders.Spot.GetOrderByClientOrderId, TradingMode.Spot)
            ?.Capability.GetSpotOrderByClientOrderIdOptions;

    /// <inheritdoc cref="IGetSpotOrderByClientOrderIdOperation.GetSpotOrderByClientOrderIdAsync" />
    protected Task<IExchangeCallResult<SharedSpotOrder>> GetSpotOrderByClientOrderIdAsync(
        GetOrderRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<IGetSpotOrderByClientOrderId, IExchangeCallResult<SharedSpotOrder>>(
            SharedCapabilities.Orders.Spot.GetOrderByClientOrderId,
            request,
            GetTradingMode(request),
            (api, token) => api.GetSpotOrderByClientOrderIdAsync(request, token),
            error => HttpResult.Fail<SharedSpotOrder>(_provider.ExchangeName, error),
            ct);

    /// <inheritdoc cref="IPlaceMultipleSpotOrdersOperation.PlaceMultipleSpotOrdersOptions" />
    protected PlaceMultipleSpotOrdersOptions? PlaceMultipleSpotOrdersOptions
        => GetPublicCapability(SharedCapabilities.Orders.Spot.PlaceMultipleOrders, TradingMode.Spot)
            ?.Capability.PlaceMultipleSpotOrdersOptions;

    /// <inheritdoc cref="IPlaceMultipleSpotOrdersOperation.PlaceMultipleSpotOrdersAsync" />
    protected Task<IExchangeCallResult<CallResult<SharedId>[]>> PlaceMultipleSpotOrdersAsync(
        PlaceMultipleSpotOrdersRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<IPlaceMultipleSpotOrders, IExchangeCallResult<CallResult<SharedId>[]>>(
            SharedCapabilities.Orders.Spot.PlaceMultipleOrders,
            request,
            GetTradingMode(request),
            (api, token) => api.PlaceMultipleSpotOrdersAsync(request, token),
            error => HttpResult.Fail<CallResult<SharedId>[]>(_provider.ExchangeName, error),
            ct);

    /// <inheritdoc cref="IPlaceSpotOrderOperation.PlaceSpotOrderOptions" />
    protected PlaceSpotOrderOptions? PlaceSpotOrderOptions
        => GetPublicCapability(SharedCapabilities.Orders.Spot.PlaceOrder, TradingMode.Spot)
            ?.Capability.PlaceSpotOrderOptions;

    /// <inheritdoc cref="IPlaceSpotOrderOperation.PlaceSpotOrderAsync" />
    protected Task<IExchangeCallResult<SharedId>> PlaceSpotOrderAsync(
        PlaceSpotOrderRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<IPlaceSpotOrder, IExchangeCallResult<SharedId>>(
            SharedCapabilities.Orders.Spot.PlaceOrder,
            request,
            GetTradingMode(request),
            (api, token) => api.PlaceSpotOrderAsync(request, token),
            error => HttpResult.Fail<SharedId>(_provider.ExchangeName, error),
            ct);
}
