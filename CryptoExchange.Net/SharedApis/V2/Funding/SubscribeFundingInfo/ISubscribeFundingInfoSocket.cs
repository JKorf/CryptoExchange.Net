using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for subscribing to funding info updates
    /// </summary>
    public interface ISubscribeFundingInfoOperation
    {
        /// <summary>
        /// Funding info subscription options
        /// </summary>
        SubscribeFundingInfoOptions SubscribeFundingInfoOptions { get; }

        /// <summary>
        /// Subscribe to funding info updates
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="handler">Update handler</param>
        /// <param name="ct">Cancellation token, can be used to stop the updates</param>
        /// <returns></returns>
        Task<WebSocketResult<UpdateSubscription>> SubscribeToFundingInfoUpdatesAsync(
            SubscribeFundingInfoRequest request,
            Action<DataEvent<SharedFundingInfo>> handler,
            CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for subscribing to funding info updates
    /// </summary>
    /// <see cref="ISubscribeFundingInfoOperation"/>
    public interface ISubscribeFundingInfoSocket : ISubscribeFundingInfoOperation, ISharedSubscription
    {
    }
}
