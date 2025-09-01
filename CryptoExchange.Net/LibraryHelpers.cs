using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace CryptoExchange.Net
{
    /// <summary>
    /// Helpers for client libraries
    /// </summary>
    public static class LibraryHelpers
    {
        /// <summary>
        /// Client order id separator
        /// </summary>
        public const string ClientOrderIdSeparator = "JK";

        /// <summary>
        /// Apply broker id to a client order id
        /// </summary>
        /// <param name="clientOrderId"></param>
        /// <param name="brokerId"></param>
        /// <param name="maxLength"></param>
        /// <param name="allowValueAdjustment"></param>
        /// <returns></returns>
        public static string ApplyBrokerId(string? clientOrderId, string brokerId, int maxLength, bool allowValueAdjustment)
        {
            var reservedLength = brokerId.Length + ClientOrderIdSeparator.Length;

            if ((clientOrderId?.Length + reservedLength) > maxLength)
                return clientOrderId!;

            if (!string.IsNullOrEmpty(clientOrderId))
            {
                if (allowValueAdjustment)
                    clientOrderId = brokerId + ClientOrderIdSeparator + clientOrderId;

                return clientOrderId!;
            }
            else
            {
                clientOrderId = ExchangeHelpers.AppendRandomString(brokerId + ClientOrderIdSeparator, maxLength);
            }

            return clientOrderId;
        }

        /// <summary>
        /// Create a new HttpMessageHandler instance
        /// </summary>  
        public static HttpMessageHandler CreateHttpClientMessageHandler(ApiProxy? proxy, TimeSpan? keepAliveInterval)
        {
#if NET5_0_OR_GREATER
            var socketHandler = new SocketsHttpHandler();
            try
            {
                if (keepAliveInterval != null && keepAliveInterval != TimeSpan.Zero)
                {
                    socketHandler.KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always;
                    socketHandler.KeepAlivePingDelay = keepAliveInterval.Value;
                    socketHandler.KeepAlivePingTimeout = TimeSpan.FromSeconds(10);
                }

                socketHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                socketHandler.DefaultProxyCredentials = CredentialCache.DefaultCredentials;
            }
            catch (PlatformNotSupportedException) { }
            catch (NotImplementedException) { } // Mono runtime throws NotImplementedException

            if (proxy != null)
            {
                socketHandler.Proxy = new WebProxy
                {
                    Address = new Uri($"{proxy.Host}:{proxy.Port}"),
                    Credentials = proxy.Password == null ? null : new NetworkCredential(proxy.Login, proxy.Password)
                };
            }
            return socketHandler;
#else
            var httpHandler = new HttpClientHandler();
            try
            {
                httpHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                httpHandler.DefaultProxyCredentials = CredentialCache.DefaultCredentials;
            }
            catch (PlatformNotSupportedException) { }
            catch (NotImplementedException) { } // Mono runtime throws NotImplementedException

            if (proxy != null)
            {
                httpHandler.Proxy = new WebProxy
                {
                    Address = new Uri($"{proxy.Host}:{proxy.Port}"),
                    Credentials = proxy.Password == null ? null : new NetworkCredential(proxy.Login, proxy.Password)
                };
            }
            return httpHandler;
#endif
        }
    }
}
