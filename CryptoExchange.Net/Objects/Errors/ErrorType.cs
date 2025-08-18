using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Objects.Errors
{
    /// <summary>
    /// Type of error
    /// </summary>
    public enum ErrorType
    {
        #region Library errors

        /// <summary>
        /// Failed to connect to server
        /// </summary>
        UnableToConnect,
        /// <summary>
        /// Failed to complete the request to the server
        /// </summary>
        NetworkError,
        /// <summary>
        /// No API credentials have been specified
        /// </summary>
        MissingCredentials,
        /// <summary>
        /// Invalid parameter value
        /// </summary>
        InvalidParameter,
        /// <summary>
        /// Missing parameter value
        /// </summary>
        MissingParameter,
        /// <summary>
        /// Cancellation requested by user
        /// </summary>
        CancellationRequested,
        /// <summary>
        /// Invalid operation requested
        /// </summary>
        InvalidOperation,
        /// <summary>
        /// Failed to deserialize data
        /// </summary>
        DeserializationFailed,
        /// <summary>
        /// Websocket is temporarily paused
        /// </summary>
        WebsocketPaused,
        /// <summary>
        /// Timeout while waiting for data from the order book subscription
        /// </summary>
        OrderBookTimeout,
        /// <summary>
        /// All orders failed for a multi-order operation
        /// </summary>
        AllOrdersFailed,
        /// <summary>
        /// Request timeout
        /// </summary>
        Timeout,

        #endregion

        #region Server errors

        /// <summary>
        /// Unknown error
        /// </summary>
        Unknown,
        /// <summary>
        /// Not authorized or insufficient permissions
        /// </summary>
        Unauthorized,
        /// <summary>
        /// Request rate limit error, too many requests 
        /// </summary>
        RateLimitRequest,
        /// <summary>
        /// Connection rate limit error, too many connections
        /// </summary>
        RateLimitConnection,
        /// <summary>
        /// Subscription rate limit error, too many subscriptions
        /// </summary>
        RateLimitSubscription,
        /// <summary>
        /// Order rate limit error, too many orders
        /// </summary>
        RateLimitOrder,
        /// <summary>
        /// Request timestamp invalid
        /// </summary>
        InvalidTimestamp,
        /// <summary>
        /// Unknown symbol
        /// </summary>
        UnknownSymbol,
        /// <summary>
        /// Unknown asset
        /// </summary>
        UnknownAsset,
        /// <summary>
        /// Unknown order
        /// </summary>
        UnknownOrder,
        /// <summary>
        /// Duplicate subscription
        /// </summary>
        DuplicateSubscription,
        /// <summary>
        /// Invalid quantity
        /// </summary>
        InvalidQuantity,
        /// <summary>
        /// Invalid price
        /// </summary>
        InvalidPrice,
        /// <summary>
        /// Parameter(s) for stop or tp/sl order invalid
        /// </summary>
        InvalidStopParameters,
        /// <summary>
        /// Not enough balance to execute request
        /// </summary>
        InsufficientBalance,
        /// <summary>
        /// Client order id already in use
        /// </summary>
        DuplicateClientOrderId,
        /// <summary>
        /// Symbol is not currently trading
        /// </summary>
        UnavailableSymbol,
        /// <summary>
        /// Order rejected due to order configuration such as order type or time in force restrictions
        /// </summary>
        RejectedOrderConfiguration,
        /// <summary>
        /// There is no open position
        /// </summary>
        NoPosition,
        /// <summary>
        /// Max position reached
        /// </summary>
        MaxPosition,
        /// <summary>
        /// Error in the internal system
        /// </summary>
        SystemError,
        /// <summary>
        /// The target object is not in the correct state for an operation
        /// </summary>
        IncorrectState,
        /// <summary>
        /// Risk management error
        /// </summary>
        RiskError

        #endregion
    }
}
