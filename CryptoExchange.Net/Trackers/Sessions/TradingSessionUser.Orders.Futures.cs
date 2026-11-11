using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.Sessions;

public abstract partial class TradingSessionUser
{
    /// <inheritdoc cref="ICancelAllFuturesOrders.CancelAllFuturesOrdersOptions" />
    protected CancelAllFuturesOrdersOptions? GetCancelAllFuturesOrdersOptions(TradingMode tradingMode)
        => GetPublicCapability(SharedCapabilities.Orders.Futures.CancelAllOrders, tradingMode)
            ?.Capability.CancelAllFuturesOrdersOptions;

    /// <inheritdoc cref="ICancelAllFuturesOrders.CancelAllFuturesOrdersAsync" />
    protected Task<IExchangeCallResult> CancelAllFuturesOrdersAsync(
        CancelAllOrdersRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<ICancelAllFuturesOrders, IExchangeCallResult>(
            SharedCapabilities.Orders.Futures.CancelAllOrders,
            request,
            GetTradingMode(request),
            (api, token) => api.CancelAllFuturesOrdersAsync(request, token),
            error => HttpResult.Fail(_provider.ExchangeName, error),
            ct);

    /// <inheritdoc cref="ICancelAllFuturesSymbolOrders.CancelAllFuturesSymbolOrdersOptions" />
    protected CancelAllFuturesSymbolOrdersOptions? GetCancelAllFuturesSymbolOrdersOptions(TradingMode tradingMode)
        => GetPublicCapability(SharedCapabilities.Orders.Futures.CancelAllSymbolOrders, tradingMode)
            ?.Capability.CancelAllFuturesSymbolOrdersOptions;

    /// <inheritdoc cref="ICancelAllFuturesSymbolOrders.CancelAllFuturesSymbolOrdersAsync" />
    protected Task<IExchangeCallResult> CancelAllFuturesSymbolOrdersAsync(
        CancelAllSymbolOrdersRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<ICancelAllFuturesSymbolOrders, IExchangeCallResult>(
            SharedCapabilities.Orders.Futures.CancelAllSymbolOrders,
            request,
            GetTradingMode(request),
            (api, token) => api.CancelAllFuturesSymbolOrdersAsync(request, token),
            error => HttpResult.Fail(_provider.ExchangeName, error),
            ct);

    /// <inheritdoc cref="ICancelFuturesOrder.CancelFuturesOrderOptions" />
    protected CancelFuturesOrderOptions? GetCancelFuturesOrderOptions(TradingMode tradingMode)
        => GetPublicCapability(SharedCapabilities.Orders.Futures.CancelOrder, tradingMode)
            ?.Capability.CancelFuturesOrderOptions;

    /// <inheritdoc cref="ICancelFuturesOrder.CancelFuturesOrderAsync" />
    protected Task<IExchangeCallResult<SharedId>> CancelFuturesOrderAsync(
        CancelOrderRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<ICancelFuturesOrder, IExchangeCallResult<SharedId>>(
            SharedCapabilities.Orders.Futures.CancelOrder,
            request,
            GetTradingMode(request),
            (api, token) => api.CancelFuturesOrderAsync(request, token),
            error => HttpResult.Fail<SharedId>(_provider.ExchangeName, error),
            ct);

    /// <inheritdoc cref="ICancelFuturesOrderByClientOrderId.CancelFuturesOrderByClientOrderIdOptions" />
    protected CancelFuturesOrderByClientOrderIdOptions? GetCancelFuturesOrderByClientOrderIdOptions(
        TradingMode tradingMode)
        => GetPublicCapability(SharedCapabilities.Orders.Futures.CancelOrderByClientOrderId, tradingMode)
            ?.Capability.CancelFuturesOrderByClientOrderIdOptions;

    /// <inheritdoc cref="ICancelFuturesOrderByClientOrderId.CancelFuturesOrderByClientOrderIdAsync" />
    protected Task<IExchangeCallResult<SharedId>> CancelFuturesOrderByClientOrderIdAsync(
        CancelOrderRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<ICancelFuturesOrderByClientOrderId, IExchangeCallResult<SharedId>>(
            SharedCapabilities.Orders.Futures.CancelOrderByClientOrderId,
            request,
            GetTradingMode(request),
            (api, token) => api.CancelFuturesOrderByClientOrderIdAsync(request, token),
            error => HttpResult.Fail<SharedId>(_provider.ExchangeName, error),
            ct);

    /// <inheritdoc cref="IEditFuturesOrder.EditFuturesOrderOptions" />
    protected EditFuturesOrderOptions? GetEditFuturesOrderOptions(TradingMode tradingMode)
        => GetPublicCapability(SharedCapabilities.Orders.Futures.EditOrder, tradingMode)
            ?.Capability.EditFuturesOrderOptions;

    /// <inheritdoc cref="IEditFuturesOrder.EditFuturesOrderAsync" />
    protected Task<IExchangeCallResult<SharedId>> EditFuturesOrderAsync(
        EditOrderRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<IEditFuturesOrder, IExchangeCallResult<SharedId>>(
            SharedCapabilities.Orders.Futures.EditOrder,
            request,
            GetTradingMode(request),
            (api, token) => api.EditFuturesOrderAsync(request, token),
            error => HttpResult.Fail<SharedId>(_provider.ExchangeName, error),
            ct);

    /// <inheritdoc cref="IEditFuturesOrderByClientOrderId.EditFuturesOrderByClientOrderIdOptions" />
    protected EditFuturesOrderByClientOrderIdOptions? GetEditFuturesOrderByClientOrderIdOptions(TradingMode tradingMode)
        => GetPublicCapability(SharedCapabilities.Orders.Futures.EditOrderByClientOrderId, tradingMode)
            ?.Capability.EditFuturesOrderByClientOrderIdOptions;

    /// <inheritdoc cref="IEditFuturesOrderByClientOrderId.EditFuturesOrderByClientOrderIdAsync" />
    protected Task<IExchangeCallResult<SharedId>> EditFuturesOrderByClientOrderIdAsync(
        EditOrderRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<IEditFuturesOrderByClientOrderId, IExchangeCallResult<SharedId>>(
            SharedCapabilities.Orders.Futures.EditOrderByClientOrderId,
            request,
            GetTradingMode(request),
            (api, token) => api.EditFuturesOrderByClientOrderIdAsync(request, token),
            error => HttpResult.Fail<SharedId>(_provider.ExchangeName, error),
            ct);

    /// <inheritdoc cref="IGetClosedFuturesOrders.GetClosedFuturesOrdersOptions" />
    protected GetFuturesClosedOrdersOptions? GetClosedFuturesOrdersOptions(TradingMode tradingMode)
        => GetPublicCapability(SharedCapabilities.Orders.Futures.GetClosedOrders, tradingMode)
            ?.Capability.GetClosedFuturesOrdersOptions;

    /// <inheritdoc cref="IGetClosedFuturesOrders.GetClosedFuturesOrdersAsync" />
    protected Task<IExchangeCallResult<SharedFuturesOrder[]>> GetClosedFuturesOrdersAsync(
        GetClosedOrdersRequest request,
        PageRequest? nextPageToken = null,
        CancellationToken ct = default)
        => ExecuteAsync<IGetClosedFuturesOrders, IExchangeCallResult<SharedFuturesOrder[]>>(
            SharedCapabilities.Orders.Futures.GetClosedOrders,
            request,
            GetTradingMode(request),
            (api, token) => api.GetClosedFuturesOrdersAsync(request, nextPageToken, token),
            error => HttpResult.Fail<SharedFuturesOrder[]>(_provider.ExchangeName, error),
            ct);

    /// <inheritdoc cref="IGetFuturesOrder.GetFuturesOrderOptions" />
    protected GetFuturesOrderOptions? GetFuturesOrderOptions(TradingMode tradingMode)
        => GetPublicCapability(SharedCapabilities.Orders.Futures.GetOrder, tradingMode)
            ?.Capability.GetFuturesOrderOptions;

    /// <inheritdoc cref="IGetFuturesOrder.GetFuturesOrderAsync" />
    protected Task<IExchangeCallResult<SharedFuturesOrder>> GetFuturesOrderAsync(
        GetOrderRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<IGetFuturesOrder, IExchangeCallResult<SharedFuturesOrder>>(
            SharedCapabilities.Orders.Futures.GetOrder,
            request,
            GetTradingMode(request),
            (api, token) => api.GetFuturesOrderAsync(request, token),
            error => HttpResult.Fail<SharedFuturesOrder>(_provider.ExchangeName, error),
            ct);

    /// <inheritdoc cref="IGetFuturesOrderByClientOrderId.GetFuturesOrderByClientOrderIdOptions" />
    protected GetFuturesOrderByClientOrderIdOptions? GetFuturesOrderByClientOrderIdOptions(TradingMode tradingMode)
        => GetPublicCapability(SharedCapabilities.Orders.Futures.GetOrderByClientOrderId, tradingMode)
            ?.Capability.GetFuturesOrderByClientOrderIdOptions;

    /// <inheritdoc cref="IGetFuturesOrderByClientOrderId.GetFuturesOrderByClientOrderIdAsync" />
    protected Task<IExchangeCallResult<SharedFuturesOrder>> GetFuturesOrderByClientOrderIdAsync(
        GetOrderRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<IGetFuturesOrderByClientOrderId, IExchangeCallResult<SharedFuturesOrder>>(
            SharedCapabilities.Orders.Futures.GetOrderByClientOrderId,
            request,
            GetTradingMode(request),
            (api, token) => api.GetFuturesOrderByClientOrderIdAsync(request, token),
            error => HttpResult.Fail<SharedFuturesOrder>(_provider.ExchangeName, error),
            ct);

    /// <inheritdoc cref="IGetOpenFuturesOrders.GetOpenFuturesOrdersOptions" />
    protected GetOpenFuturesOrdersOptions? GetOpenFuturesOrdersOptions(TradingMode tradingMode)
        => GetPublicCapability(SharedCapabilities.Orders.Futures.GetOpenOrders, tradingMode)
            ?.Capability.GetOpenFuturesOrdersOptions;

    /// <inheritdoc cref="IGetOpenFuturesOrders.GetOpenFuturesOrdersAsync" />
    protected Task<IExchangeCallResult<SharedFuturesOrder[]>> GetOpenFuturesOrdersAsync(
        GetOpenOrdersRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<IGetOpenFuturesOrders, IExchangeCallResult<SharedFuturesOrder[]>>(
            SharedCapabilities.Orders.Futures.GetOpenOrders,
            request,
            GetTradingMode(request),
            (api, token) => api.GetOpenFuturesOrdersAsync(request, token),
            error => HttpResult.Fail<SharedFuturesOrder[]>(_provider.ExchangeName, error),
            ct);

    /// <inheritdoc cref="IPlaceFuturesOrder.PlaceFuturesOrderOptions" />
    protected PlaceFuturesOrderOptions? GetPlaceFuturesOrderOptions(TradingMode tradingMode)
        => GetPublicCapability(SharedCapabilities.Orders.Futures.PlaceOrder, tradingMode)
            ?.Capability.PlaceFuturesOrderOptions;

    /// <inheritdoc cref="IPlaceFuturesOrder.PlaceFuturesOrderAsync" />
    protected Task<IExchangeCallResult<SharedId>> PlaceFuturesOrderAsync(
        PlaceFuturesOrderRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<IPlaceFuturesOrder, IExchangeCallResult<SharedId>>(
            SharedCapabilities.Orders.Futures.PlaceOrder,
            request,
            GetTradingMode(request),
            (api, token) => api.PlaceFuturesOrderAsync(request, token),
            error => HttpResult.Fail<SharedId>(_provider.ExchangeName, error),
            ct);

    /// <inheritdoc cref="IPlaceMultipleFuturesOrders.PlaceMultipleFuturesOrdersOptions" />
    protected PlaceMultipleFuturesOrdersOptions? GetPlaceMultipleFuturesOrdersOptions(TradingMode tradingMode)
        => GetPublicCapability(SharedCapabilities.Orders.Futures.PlaceMultipleOrders, tradingMode)
            ?.Capability.PlaceMultipleFuturesOrdersOptions;

    /// <inheritdoc cref="IPlaceMultipleFuturesOrders.PlaceMultipleFuturesOrdersAsync" />
    protected Task<IExchangeCallResult<CallResult<SharedId>[]>> PlaceMultipleFuturesOrdersAsync(
        PlaceMultipleFuturesOrdersRequest request,
        CancellationToken ct = default)
        => ExecuteAsync<IPlaceMultipleFuturesOrders, IExchangeCallResult<CallResult<SharedId>[]>>(
            SharedCapabilities.Orders.Futures.PlaceMultipleOrders,
            request,
            GetTradingMode(request),
            (api, token) => api.PlaceMultipleFuturesOrdersAsync(request, token),
            error => HttpResult.Fail<CallResult<SharedId>[]>(_provider.ExchangeName, error),
            ct);
}
