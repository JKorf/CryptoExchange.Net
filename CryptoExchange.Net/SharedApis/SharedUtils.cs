using CryptoExchange.Net.Objects;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Shared interfaces utilities
    /// </summary>
    public static class SharedUtils
    {
        /// <summary>
        /// Get client information including supported features
        /// </summary>
        public static SharedClientInfo GetClientInfo(PlatformInfo platformInfo, ISharedApi client)
        {
            return new SharedClientInfo
            {
                Exchange = client.Exchange,
                TypeName = client.GetType().Name,
                SupportedEnvironments = platformInfo.SupportedEnvironments,
                SupportedTradingModes = client.SupportedTradingModes,
                CentralizationType = platformInfo.CentralizationType,
                Transport = client.Transport,
                Authenticated = client.Authenticated,
                Capabilities = client.Capabilities.Where(x => x.Supported).ToArray()
            };
        }

        /// <summary>
        /// Apply symbols request filter for asset type and trading mode
        /// </summary>
        public static T[] ApplySymbolFilter<T>(T[] symbols, GetSymbolsRequest request) where T : SharedSpotSymbol
        {
            IEnumerable<T> resultData = symbols;
            if (request.TradingMode != null)
                resultData = resultData.Where(x => x.TradingMode == request.TradingMode);
            if (request.BaseAssetType != null)
                resultData = resultData.Where(x => x.BaseAssetType == request.BaseAssetType);
            if (request.QuoteAssetType != null)
                resultData = resultData.Where(x => x.QuoteAssetType == request.QuoteAssetType);
            if (request.BaseAssetSubType != null)
                resultData = resultData.Where(x => x.BaseAssetSubType == request.BaseAssetSubType);
            if (request.QuoteAssetSubType != null)
                resultData = resultData.Where(x => x.QuoteAssetSubType == request.QuoteAssetSubType);
            return resultData.ToArray();
        }

        /// <summary>
        /// Register Shared API client in DI container
        /// </summary>
        public static IServiceCollection RegisterSharedApiClient<
            TSharedApiClient,
#if NET5_0_OR_GREATER
    [DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
        TImplementation
            >(this IServiceCollection services, Action<SharedApiClientRegistrationBuilder<TSharedApiClient>> configure)
                where TImplementation : class, TSharedApiClient
                where TSharedApiClient : class, ISharedApiClientBase
        {
            services.AddTransient<TSharedApiClient, TImplementation>();
            services.AddTransient<ISharedApiClientBase>(
                serviceProvider =>
                    serviceProvider.GetRequiredService<TSharedApiClient>());

            var builder =
                new SharedApiClientRegistrationBuilder<TSharedApiClient>(services);

            configure(builder);
            builder.RegisterTransportAgnosticCapabilities();

            return services;
        }
    }
}
