using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.Sessions.Interfaces;

/// <summary>
/// User-bound access to <see cref="ICancelAllSpotOrders"/>.
/// </summary>
public interface ITradingUserCancelAllSpotOrders
{
    /// <inheritdoc cref="ICancelAllSpotOrders.CancelAllSpotOrdersOptions" />
    CancelAllSpotOrdersOptions CancelAllSpotOrdersOptions { get; }

    /// <inheritdoc cref="ICancelAllSpotOrders.CancelAllSpotOrdersAsync" />
    Task<IExchangeCallResult> CancelAllSpotOrdersAsync(
        CancelAllOrdersRequest request,
        CancellationToken ct = default);
}

/// <summary>
/// User-bound access to <see cref="ICancelAllSpotSymbolOrders"/>.
/// </summary>
public interface ITradingUserCancelAllSpotSymbolOrders
{
    /// <inheritdoc cref="ICancelAllSpotSymbolOrders.CancelAllSpotSymbolOrdersOptions" />
    CancelAllSpotSymbolOrdersOptions CancelAllSpotSymbolOrdersOptions { get; }

    /// <inheritdoc cref="ICancelAllSpotSymbolOrders.CancelAllSpotSymbolOrdersAsync" />
    Task<IExchangeCallResult> CancelAllSpotSymbolOrdersAsync(
        CancelAllSymbolOrdersRequest request,
        CancellationToken ct = default);
}

/// <summary>
/// User-bound access to <see cref="ICancelSpotOrder"/>.
/// </summary>
public interface ITradingUserCancelSpotOrder
{
    /// <inheritdoc cref="ICancelSpotOrder.CancelSpotOrderOptions" />
    CancelSpotOrderOptions CancelSpotOrderOptions { get; }

    /// <inheritdoc cref="ICancelSpotOrder.CancelSpotOrderAsync" />
    Task<IExchangeCallResult<SharedId>> CancelSpotOrderAsync(
        CancelOrderRequest request,
        CancellationToken ct = default);
}

/// <summary>
/// User-bound access to <see cref="ICancelSpotOrderByClientOrderId"/>.
/// </summary>
public interface ITradingUserCancelSpotOrderByClientOrderId
{
    /// <inheritdoc cref="ICancelSpotOrderByClientOrderId.CancelSpotOrderByClientOrderIdOptions" />
    CancelSpotOrderByClientOrderIdOptions CancelSpotOrderByClientOrderIdOptions { get; }

    /// <inheritdoc cref="ICancelSpotOrderByClientOrderId.CancelSpotOrderByClientOrderIdAsync" />
    Task<IExchangeCallResult<SharedId>> CancelSpotOrderByClientOrderIdAsync(
        CancelOrderRequest request,
        CancellationToken ct = default);
}

/// <summary>
/// User-bound access to <see cref="IEditSpotOrder"/>.
/// </summary>
public interface ITradingUserEditSpotOrder
{
    /// <inheritdoc cref="IEditSpotOrder.EditSpotOrderOptions" />
    EditSpotOrderOptions EditSpotOrderOptions { get; }

    /// <inheritdoc cref="IEditSpotOrder.EditSpotOrderAsync" />
    Task<IExchangeCallResult<SharedId>> EditSpotOrderAsync(
        EditOrderRequest request,
        CancellationToken ct = default);
}

/// <summary>
/// User-bound access to <see cref="IEditSpotOrderByClientOrderId"/>.
/// </summary>
public interface ITradingUserEditSpotOrderByClientOrderId
{
    /// <inheritdoc cref="IEditSpotOrderByClientOrderId.EditSpotOrderByClientOrderIdOptions" />
    EditSpotOrderByClientOrderIdOptions EditSpotOrderByClientOrderIdOptions { get; }

    /// <inheritdoc cref="IEditSpotOrderByClientOrderId.EditSpotOrderByClientOrderIdAsync" />
    Task<IExchangeCallResult<SharedId>> EditSpotOrderByClientOrderIdAsync(
        EditOrderRequest request,
        CancellationToken ct = default);
}

/// <summary>
/// User-bound access to <see cref="IGetClosedSpotOrders"/>.
/// </summary>
public interface ITradingUserGetClosedSpotOrders
{
    /// <inheritdoc cref="IGetClosedSpotOrders.GetClosedSpotOrdersOptions" />
    GetSpotClosedOrdersOptions GetClosedSpotOrdersOptions { get; }

    /// <inheritdoc cref="IGetClosedSpotOrders.GetClosedSpotOrdersAsync" />
    Task<IExchangeCallResult<SharedSpotOrder[]>> GetClosedSpotOrdersAsync(
        GetClosedOrdersRequest request,
        PageRequest? nextPageToken = null,
        CancellationToken ct = default);
}

/// <summary>
/// User-bound access to <see cref="IGetOpenSpotOrders"/>.
/// </summary>
public interface ITradingUserGetOpenSpotOrders
{
    /// <inheritdoc cref="IGetOpenSpotOrders.GetOpenSpotOrdersOptions" />
    GetOpenSpotOrdersOptions GetOpenSpotOrdersOptions { get; }

    /// <inheritdoc cref="IGetOpenSpotOrders.GetOpenSpotOrdersAsync" />
    Task<IExchangeCallResult<SharedSpotOrder[]>> GetOpenSpotOrdersAsync(
        GetOpenOrdersRequest request,
        CancellationToken ct = default);
}

/// <summary>
/// User-bound access to <see cref="IGetSpotOrder"/>.
/// </summary>
public interface ITradingUserGetSpotOrder
{
    /// <inheritdoc cref="IGetSpotOrder.GetSpotOrderOptions" />
    GetSpotOrderOptions GetSpotOrderOptions { get; }

    /// <inheritdoc cref="IGetSpotOrder.GetSpotOrderAsync" />
    Task<IExchangeCallResult<SharedSpotOrder>> GetSpotOrderAsync(
        GetOrderRequest request,
        CancellationToken ct = default);
}

/// <summary>
/// User-bound access to <see cref="IGetSpotOrderByClientOrderId"/>.
/// </summary>
public interface ITradingUserGetSpotOrderByClientOrderId
{
    /// <inheritdoc cref="IGetSpotOrderByClientOrderId.GetSpotOrderByClientOrderIdOptions" />
    GetSpotOrderByClientOrderIdOptions GetSpotOrderByClientOrderIdOptions { get; }

    /// <inheritdoc cref="IGetSpotOrderByClientOrderId.GetSpotOrderByClientOrderIdAsync" />
    Task<IExchangeCallResult<SharedSpotOrder>> GetSpotOrderByClientOrderIdAsync(
        GetOrderRequest request,
        CancellationToken ct = default);
}

/// <summary>
/// User-bound access to <see cref="IPlaceMultipleSpotOrders"/>.
/// </summary>
public interface ITradingUserPlaceMultipleSpotOrders
{
    /// <inheritdoc cref="IPlaceMultipleSpotOrders.PlaceMultipleSpotOrdersOptions" />
    PlaceMultipleSpotOrdersOptions PlaceMultipleSpotOrdersOptions { get; }

    /// <inheritdoc cref="IPlaceMultipleSpotOrders.PlaceMultipleSpotOrdersAsync" />
    Task<IExchangeCallResult<CallResult<SharedId>[]>> PlaceMultipleSpotOrdersAsync(
        PlaceMultipleSpotOrdersRequest request,
        CancellationToken ct = default);
}

/// <summary>
/// User-bound access to <see cref="IPlaceSpotOrder"/>.
/// </summary>
public interface ITradingUserPlaceSpotOrder
{
    /// <inheritdoc cref="IPlaceSpotOrder.PlaceSpotOrderOptions" />
    PlaceSpotOrderOptions PlaceSpotOrderOptions { get; }

    /// <inheritdoc cref="IPlaceSpotOrder.PlaceSpotOrderAsync" />
    Task<IExchangeCallResult<SharedId>> PlaceSpotOrderAsync(
        PlaceSpotOrderRequest request,
        CancellationToken ct = default);
}
