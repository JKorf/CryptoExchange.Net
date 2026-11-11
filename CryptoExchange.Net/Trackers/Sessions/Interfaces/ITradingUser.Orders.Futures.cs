using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.Sessions.Interfaces;

/// <summary>
/// User-bound access to <see cref="ICancelAllFuturesOrders"/>.
/// </summary>
public interface ITradingUserCancelAllFuturesOrders
{
    /// <inheritdoc cref="ICancelAllFuturesOrders.CancelAllFuturesOrdersOptions" />
    CancelAllFuturesOrdersOptions GetCancelAllFuturesOrdersOptions(TradingMode tradingMode);

    /// <inheritdoc cref="ICancelAllFuturesOrders.CancelAllFuturesOrdersAsync" />
    Task<IExchangeCallResult> CancelAllFuturesOrdersAsync(
        CancelAllOrdersRequest request,
        CancellationToken ct = default);
}

/// <summary>
/// User-bound access to <see cref="ICancelAllFuturesSymbolOrders"/>.
/// </summary>
public interface ITradingUserCancelAllFuturesSymbolOrders
{
    /// <inheritdoc cref="ICancelAllFuturesSymbolOrders.CancelAllFuturesSymbolOrdersOptions" />
    CancelAllFuturesSymbolOrdersOptions GetCancelAllFuturesSymbolOrdersOptions(TradingMode tradingMode);

    /// <inheritdoc cref="ICancelAllFuturesSymbolOrders.CancelAllFuturesSymbolOrdersAsync" />
    Task<IExchangeCallResult> CancelAllFuturesSymbolOrdersAsync(
        CancelAllSymbolOrdersRequest request,
        CancellationToken ct = default);
}

/// <summary>
/// User-bound access to <see cref="ICancelFuturesOrder"/>.
/// </summary>
public interface ITradingUserCancelFuturesOrder
{
    /// <inheritdoc cref="ICancelFuturesOrder.CancelFuturesOrderOptions" />
    CancelFuturesOrderOptions GetCancelFuturesOrderOptions(TradingMode tradingMode);

    /// <inheritdoc cref="ICancelFuturesOrder.CancelFuturesOrderAsync" />
    Task<IExchangeCallResult<SharedId>> CancelFuturesOrderAsync(
        CancelOrderRequest request,
        CancellationToken ct = default);
}

/// <summary>
/// User-bound access to <see cref="ICancelFuturesOrderByClientOrderId"/>.
/// </summary>
public interface ITradingUserCancelFuturesOrderByClientOrderId
{
    /// <inheritdoc cref="ICancelFuturesOrderByClientOrderId.CancelFuturesOrderByClientOrderIdOptions" />
    CancelFuturesOrderByClientOrderIdOptions GetCancelFuturesOrderByClientOrderIdOptions(TradingMode tradingMode);

    /// <inheritdoc cref="ICancelFuturesOrderByClientOrderId.CancelFuturesOrderByClientOrderIdAsync" />
    Task<IExchangeCallResult<SharedId>> CancelFuturesOrderByClientOrderIdAsync(
        CancelOrderRequest request,
        CancellationToken ct = default);
}

/// <summary>
/// User-bound access to <see cref="IEditFuturesOrder"/>.
/// </summary>
public interface ITradingUserEditFuturesOrder
{
    /// <inheritdoc cref="IEditFuturesOrder.EditFuturesOrderOptions" />
    EditFuturesOrderOptions GetEditFuturesOrderOptions(TradingMode tradingMode);

    /// <inheritdoc cref="IEditFuturesOrder.EditFuturesOrderAsync" />
    Task<IExchangeCallResult<SharedId>> EditFuturesOrderAsync(
        EditOrderRequest request,
        CancellationToken ct = default);
}

/// <summary>
/// User-bound access to <see cref="IEditFuturesOrderByClientOrderId"/>.
/// </summary>
public interface ITradingUserEditFuturesOrderByClientOrderId
{
    /// <inheritdoc cref="IEditFuturesOrderByClientOrderId.EditFuturesOrderByClientOrderIdOptions" />
    EditFuturesOrderByClientOrderIdOptions GetEditFuturesOrderByClientOrderIdOptions(TradingMode tradingMode);

    /// <inheritdoc cref="IEditFuturesOrderByClientOrderId.EditFuturesOrderByClientOrderIdAsync" />
    Task<IExchangeCallResult<SharedId>> EditFuturesOrderByClientOrderIdAsync(
        EditOrderRequest request,
        CancellationToken ct = default);
}

/// <summary>
/// User-bound access to <see cref="IGetClosedFuturesOrders"/>.
/// </summary>
public interface ITradingUserGetClosedFuturesOrders
{
    /// <inheritdoc cref="IGetClosedFuturesOrders.GetClosedFuturesOrdersOptions" />
    GetFuturesClosedOrdersOptions GetClosedFuturesOrdersOptions(TradingMode tradingMode);

    /// <inheritdoc cref="IGetClosedFuturesOrders.GetClosedFuturesOrdersAsync" />
    Task<IExchangeCallResult<SharedFuturesOrder[]>> GetClosedFuturesOrdersAsync(
        GetClosedOrdersRequest request,
        PageRequest? nextPageToken = null,
        CancellationToken ct = default);
}

/// <summary>
/// User-bound access to <see cref="IGetFuturesOrder"/>.
/// </summary>
public interface ITradingUserGetFuturesOrder
{
    /// <inheritdoc cref="IGetFuturesOrder.GetFuturesOrderOptions" />
    GetFuturesOrderOptions GetFuturesOrderOptions(TradingMode tradingMode);

    /// <inheritdoc cref="IGetFuturesOrder.GetFuturesOrderAsync" />
    Task<IExchangeCallResult<SharedFuturesOrder>> GetFuturesOrderAsync(
        GetOrderRequest request,
        CancellationToken ct = default);
}

/// <summary>
/// User-bound access to <see cref="IGetFuturesOrderByClientOrderId"/>.
/// </summary>
public interface ITradingUserGetFuturesOrderByClientOrderId
{
    /// <inheritdoc cref="IGetFuturesOrderByClientOrderId.GetFuturesOrderByClientOrderIdOptions" />
    GetFuturesOrderByClientOrderIdOptions GetFuturesOrderByClientOrderIdOptions(TradingMode tradingMode);

    /// <inheritdoc cref="IGetFuturesOrderByClientOrderId.GetFuturesOrderByClientOrderIdAsync" />
    Task<IExchangeCallResult<SharedFuturesOrder>> GetFuturesOrderByClientOrderIdAsync(
        GetOrderRequest request,
        CancellationToken ct = default);
}

/// <summary>
/// User-bound access to <see cref="IGetOpenFuturesOrders"/>.
/// </summary>
public interface ITradingUserGetOpenFuturesOrders
{
    /// <inheritdoc cref="IGetOpenFuturesOrders.GetOpenFuturesOrdersOptions" />
    GetOpenFuturesOrdersOptions GetOpenFuturesOrdersOptions(TradingMode tradingMode);

    /// <inheritdoc cref="IGetOpenFuturesOrders.GetOpenFuturesOrdersAsync" />
    Task<IExchangeCallResult<SharedFuturesOrder[]>> GetOpenFuturesOrdersAsync(
        GetOpenOrdersRequest request,
        CancellationToken ct = default);
}

/// <summary>
/// User-bound access to <see cref="IPlaceFuturesOrder"/>.
/// </summary>
public interface ITradingUserPlaceFuturesOrder
{
    /// <inheritdoc cref="IPlaceFuturesOrder.PlaceFuturesOrderOptions" />
    PlaceFuturesOrderOptions GetPlaceFuturesOrderOptions(TradingMode tradingMode);

    /// <inheritdoc cref="IPlaceFuturesOrder.PlaceFuturesOrderAsync" />
    Task<IExchangeCallResult<SharedId>> PlaceFuturesOrderAsync(
        PlaceFuturesOrderRequest request,
        CancellationToken ct = default);
}

/// <summary>
/// User-bound access to <see cref="IPlaceMultipleFuturesOrders"/>.
/// </summary>
public interface ITradingUserPlaceMultipleFuturesOrders
{
    /// <inheritdoc cref="IPlaceMultipleFuturesOrders.PlaceMultipleFuturesOrdersOptions" />
    PlaceMultipleFuturesOrdersOptions GetPlaceMultipleFuturesOrdersOptions(TradingMode tradingMode);

    /// <inheritdoc cref="IPlaceMultipleFuturesOrders.PlaceMultipleFuturesOrdersAsync" />
    Task<IExchangeCallResult<CallResult<SharedId>[]>> PlaceMultipleFuturesOrdersAsync(
        PlaceMultipleFuturesOrdersRequest request,
        CancellationToken ct = default);
}
