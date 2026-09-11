using CryptoExchange.Net.SharedApis;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Linq;

namespace CryptoExchange.Net.UnitTests
{
    [TestFixture]
    internal class SharedApiClientTests
    {
        [Test]
        public void GetCapability_ShouldUseCapabilityTradingModes()
        {
            var sharedApi = new TestSharedApi(
                [TradingMode.Spot, TradingMode.PerpetualLinear],
                [TradingMode.Spot]);
            var client = new TestSharedApiClient(sharedApi);

            Assert.Multiple(() =>
            {
                Assert.That(client.GetCapability<ITestCapability>(TradingMode.Spot)?.Capability, Is.SameAs(sharedApi));
                Assert.That(client.GetCapability<ITestCapability>(TradingMode.PerpetualLinear), Is.Null);
                Assert.That(client.GetCapabilities<ITestCapability>(TradingMode.Spot), Has.Count.EqualTo(1));
                Assert.That(client.GetCapabilities<ITestCapability>(TradingMode.PerpetualLinear), Is.Empty);
            });
        }

        [Test]
        public void CapabilityTradingModes_ShouldDefaultToSharedApiTradingModes()
        {
            var sharedApi = new TestSharedApi(
                [TradingMode.Spot, TradingMode.PerpetualLinear],
                null);
            var client = new TestSharedApiClient(sharedApi);

            Assert.Multiple(() =>
            {
                Assert.That(client.GetCapability<ITestCapability>(TradingMode.Spot)?.Capability, Is.SameAs(sharedApi));
                Assert.That(client.GetCapability<ITestCapability>(TradingMode.PerpetualLinear)?.Capability, Is.SameAs(sharedApi));
            });
        }

        [Test]
        public void GetCapabilities_WithReference_ShouldReturnMatchingCapabilities()
        {
            var sharedApi = new TestSharedApi(
                [TradingMode.Spot],
                [TradingMode.Spot]);
            var client = new TestSharedApiClient(sharedApi);

            var result = client.GetCapabilities(
                new SharedCapabilityReference<ITestCapability>(),
                TradingMode.Spot);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Capability, Is.SameAs(sharedApi));
            Assert.That(result[0].Options, Is.SameAs(sharedApi.CapabilityOptions));
        }

        [Test]
        public void Discover_ShouldReturnAllSharedApiInformation()
        {
            var restApi = new TestRestSharedApi();
            var socketApi = new TestSocketSharedApi();
            var client = new TestDiSharedApiClient(
                SharedTransport.Socket,
                restApi,
                socketApi);

            var result = client.Discover();

            Assert.Multiple(() =>
            {
                Assert.That(result.PreferredTransport, Is.EqualTo(SharedTransport.Socket));
                Assert.That(result.SharedApis, Has.Length.EqualTo(2));
                Assert.That(result.SharedApis[0].Transport, Is.EqualTo(SharedTransport.Rest));
                Assert.That(result.SharedApis[1].Transport, Is.EqualTo(SharedTransport.Socket));
                Assert.That(result.SharedApis[0].SupportedTradingModes, Is.EqualTo(new[] { TradingMode.Spot }));
                Assert.That(result.SharedApis[0].Capabilities, Has.Length.EqualTo(1));
                Assert.That(result.SharedApis[0].Authenticated, Is.False);
            });
        }

        [Test]
        public void RegisterSharedApiClientCapabilities_ShouldResolvePreferredTransport()
        {
            var restApi = new TestRestSharedApi();
            var socketApi = new TestSocketSharedApi();
            var services = new ServiceCollection();

            services.RegisterSharedApi<ITestRestSharedApi>(_ => restApi);
            services.RegisterSharedApi<ITestSocketSharedApi>(_ => socketApi);
            services.AddTransient<ITestSharedApiClient>(_ =>
                new TestDiSharedApiClient(SharedTransport.Socket, restApi, socketApi));
            services.RegisterSharedApiClientCapabilities<ITestSharedApiClient>();

            using var provider = services.BuildServiceProvider();

            Assert.Multiple(() =>
            {
                Assert.That(provider.GetRequiredService<ITestCapability>(), Is.SameAs(socketApi));
                Assert.That(provider.GetServices<ITestCapability>().ToArray(), Is.EqualTo(new[] { socketApi }));
                Assert.That(provider.GetRequiredService<ITestRestCapability>(), Is.SameAs(restApi));
                Assert.That(provider.GetRequiredService<ITestSocketCapability>(), Is.SameAs(socketApi));
            });
        }

        [Test]
        public void CapabilityTradingModes_ShouldIntersectApiApplicableAndOverrideModes()
        {
            var sharedApi = new TestSharedApi(
                [TradingMode.Spot, TradingMode.PerpetualLinear, TradingMode.DeliveryLinear],
                [TradingMode.Spot, TradingMode.PerpetualLinear],
                [TradingMode.PerpetualLinear, TradingMode.DeliveryLinear]);

            Assert.That(
                sharedApi.CapabilityOptions.SupportedTradingModes,
                Is.EqualTo(new[] { TradingMode.PerpetualLinear }));
        }

        [Test]
        public void CapabilityOptions_ShouldApplyIntrinsicTradingModes()
        {
            var apiModes = new[]
            {
                TradingMode.Spot,
                TradingMode.PerpetualLinear,
                TradingMode.DeliveryLinear,
                TradingMode.PerpetualInverse,
                TradingMode.DeliveryInverse
            };
            var spotOptions = new PlaceSpotOrderOptions("TestExchange");
            var futuresOptions = new GetPositionsOptions("TestExchange", false);
            var perpetualOptions = new GetFundingRateHistoryOptions(
                "TestExchange",
                true,
                true,
                true,
                100,
                false);
            var unrestrictedOptions = new GetTickerOptions("TestExchange");

            _ = new TestOptionsHost(apiModes, spotOptions);
            _ = new TestOptionsHost(apiModes, futuresOptions);
            _ = new TestOptionsHost(apiModes, perpetualOptions);
            _ = new TestOptionsHost(apiModes, unrestrictedOptions);

            Assert.Multiple(() =>
            {
                Assert.That(spotOptions.SupportedTradingModes, Is.EqualTo(new[] { TradingMode.Spot }));
                Assert.That(futuresOptions.SupportedTradingModes, Is.EqualTo(new[]
                {
                    TradingMode.PerpetualLinear,
                    TradingMode.DeliveryLinear,
                    TradingMode.PerpetualInverse,
                    TradingMode.DeliveryInverse
                }));
                Assert.That(perpetualOptions.SupportedTradingModes, Is.EqualTo(new[]
                {
                    TradingMode.PerpetualLinear,
                    TradingMode.PerpetualInverse
                }));
                Assert.That(unrestrictedOptions.SupportedTradingModes, Is.EqualTo(apiModes));
            });
        }

        private interface ITestCapability : ISharedApiCapability
        {
        }

        private interface ITestRestCapability : ITestCapability, ISharedRest
        {
        }

        private interface ITestSocketCapability : ITestCapability, ISharedSocket
        {
        }

        private interface ITestRestSharedApi : ITestRestCapability
        {
        }

        private interface ITestSocketSharedApi : ITestSocketCapability
        {
        }

        private interface ITestSharedApiClient : ISharedApiClientBase
        {
            ITestRestSharedApi Rest { get; }
            ITestSocketSharedApi Socket { get; }
        }

        private sealed class TestCapabilityOptions : CapabilityOptions<SharedRequest, ITestCapability>
        {
            public override string Description => "Test capability";

            public TestCapabilityOptions(TradingMode[]? applicableTradingModes)
                : base("TestExchange", false, "TestOperation", [], applicableTradingModes)
            {
            }
        }

        private sealed class TestSharedApi : SharedApiBase, ITestCapability
        {
            public TestCapabilityOptions CapabilityOptions { get; }

            public TestSharedApi(TradingMode[] apiTradingModes, TradingMode[]? capabilityTradingModes)
                : this(apiTradingModes, capabilityTradingModes, null)
            {
            }

            public TestSharedApi(
                TradingMode[] apiTradingModes,
                TradingMode[]? capabilityTradingModes,
                TradingMode[]? applicableTradingModes)
                : base(
                    SharedTransport.Rest,
                    "TestExchange",
                    apiTradingModes,
                    () => false,
                    (baseAsset, quoteAsset, tradingMode, deliverDate) => $"{baseAsset}{quoteAsset}")
            {
                var options = new TestCapabilityOptions(applicableTradingModes)
                {
                    SupportedTradingModeOverrides = capabilityTradingModes
                };

                SetCapabilities(options);
                CapabilityOptions = options;
            }

            public override SharedClientInfo Discover() => new();
        }

        private sealed class TestOptionsHost : SharedApiBase
        {
            public TestOptionsHost(TradingMode[] apiTradingModes, CapabilityOptions capabilityOptions)
                : base(
                    SharedTransport.Rest,
                    "TestExchange",
                    apiTradingModes,
                    () => false,
                    (baseAsset, quoteAsset, tradingMode, deliverDate) => $"{baseAsset}{quoteAsset}")
            {
                SetCapabilities(capabilityOptions);
            }

            public override SharedClientInfo Discover() => new();
        }

        private abstract class TestTransportSharedApi : SharedApiBase
        {
            protected TestTransportSharedApi(SharedTransport transport)
                : base(
                    transport,
                    "TestExchange",
                    [TradingMode.Spot],
                    () => false,
                    (baseAsset, quoteAsset, tradingMode, deliverDate) => $"{baseAsset}{quoteAsset}")
            {
                SetCapabilities(new TestCapabilityOptions([TradingMode.Spot]));
            }

            public override SharedClientInfo Discover() => new()
            {
                Exchange = this.Exchange,
                TypeName = GetType().Name,
                SupportedTradingModes = this.SupportedTradingModes,
                Transport = this.Transport,
                Authenticated = this.Authenticated,
                Capabilities = ((ISharedApi)this).Capabilities.ToArray()
            };
        }

        private sealed class TestRestSharedApi : TestTransportSharedApi, ITestRestSharedApi
        {
            public TestRestSharedApi() : base(SharedTransport.Rest)
            {
            }
        }

        private sealed class TestSocketSharedApi : TestTransportSharedApi, ITestSocketSharedApi
        {
            public TestSocketSharedApi() : base(SharedTransport.Socket)
            {
            }
        }

        private sealed class TestDiSharedApiClient : SharedApiClientBase, ITestSharedApiClient
        {
            public ITestRestSharedApi Rest { get; }
            public ITestSocketSharedApi Socket { get; }

            public TestDiSharedApiClient(
                SharedTransport preferredTransport,
                ITestRestSharedApi rest,
                ITestSocketSharedApi socket)
                : base(preferredTransport, rest, socket)
            {
                Rest = rest;
                Socket = socket;
            }
        }

        private sealed class TestSharedApiClient : SharedApiClientBase
        {
            public TestSharedApiClient(params ISharedApiCapability[] sharedApis)
                : base(SharedTransport.Rest, sharedApis)
            {
            }
        }
    }
}
