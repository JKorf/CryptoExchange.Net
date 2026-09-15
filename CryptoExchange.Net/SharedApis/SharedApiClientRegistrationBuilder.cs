using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Shared API dependency injection registration builder
    /// </summary>
    /// <typeparam name="TSharedApiClient"></typeparam>
    public class SharedApiClientRegistrationBuilder<TSharedApiClient>
        where TSharedApiClient : class, ISharedApiClientBase
    {
        private readonly IServiceCollection _services;
        private readonly HashSet<Type> _baseCapabilityTypes = new();

        internal SharedApiClientRegistrationBuilder(
            IServiceCollection services)
        {
            _services = services;
        }
        
        /// <summary>
        /// Adds a shared API
        /// </summary>
        public SharedApiClientRegistrationBuilder<TSharedApiClient> Add<
#if NET5_0_OR_GREATER
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.Interfaces)]
#endif
            TSharedApi>(
            Func<TSharedApiClient, TSharedApi> selector)
            where TSharedApi : class, ISharedApi
        {
            if (selector is null)
                throw new ArgumentNullException(nameof(selector));

            _services.AddTransient<TSharedApi>(
                serviceProvider =>
                {
                    var client = serviceProvider
                        .GetRequiredService<TSharedApiClient>();

                    return selector(client);
                });

            var interfaces = typeof(TSharedApi).GetInterfaces();

            var transportCapabilities = interfaces
                .Where(type =>
                    typeof(ISharedApiCapability).IsAssignableFrom(type)
                    && (typeof(ISharedRest).IsAssignableFrom(type)
                        || typeof(ISharedSocket).IsAssignableFrom(type))
                    && type != typeof(ISharedRest)
                    && type != typeof(ISharedSocket)
                    && type != typeof(ISharedSubscription))
                .Distinct();

            foreach (var capabilityType in transportCapabilities)
            {
                _services.AddTransient(
                    capabilityType,
                    serviceProvider =>
                    {
                        var client = serviceProvider
                            .GetRequiredService<TSharedApiClient>();

                        return selector(client);
                    });
            }

            var baseCapabilities = interfaces
                .Where(type =>
                    type != typeof(ISharedApiCapability)
                    && typeof(ISharedApiCapability).IsAssignableFrom(type)
                    && !typeof(ISharedRest).IsAssignableFrom(type)
                    && !typeof(ISharedSocket).IsAssignableFrom(type));

            foreach (var capabilityType in baseCapabilities)
                _baseCapabilityTypes.Add(capabilityType);

            return this;
        }

        internal void RegisterTransportAgnosticCapabilities()
        {
            foreach (var capabilityType in _baseCapabilityTypes)
            {
                _services.AddTransient(
                    capabilityType,
                    serviceProvider =>
                    {
                        var client = serviceProvider
                            .GetRequiredService<TSharedApiClient>();

                        var resolver = (ISharedApiClientResolver)client;

                        return resolver.GetCapability(capabilityType)
                            ?? throw new InvalidOperationException(
                                $"No implementation of " +
                                $"{capabilityType.Name} is available on " +
                                $"{typeof(TSharedApiClient).Name}");
                    });
            }
        }
    }
}
