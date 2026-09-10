using CryptoExchange.Net.SharedApis;
using NUnit.Framework;

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
                Assert.That(client.GetCapability<ITestCapability>(TradingMode.Spot), Is.SameAs(sharedApi));
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
                Assert.That(client.GetCapability<ITestCapability>(TradingMode.Spot), Is.SameAs(sharedApi));
                Assert.That(client.GetCapability<ITestCapability>(TradingMode.PerpetualLinear), Is.SameAs(sharedApi));
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

        private sealed class TestSharedApiClient : SharedApiClientBase
        {
            public TestSharedApiClient(params ISharedApiCapability[] sharedApis)
                : base([SharedTransport.Rest, SharedTransport.Socket], sharedApis)
            {
            }
        }
    }
}
