using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.SharedApis;
using NUnit.Framework;
using System;
using System.Text.Json;

namespace CryptoExchange.Net.UnitTests
{
    [TestFixture()]
    public class SharedModelConversionTests
    {
        [TestCase(TradingMode.Spot, "ETH", "USDT", null)]
        [TestCase(TradingMode.PerpetualLinear, "ETH", "USDT", null)]
        [TestCase(TradingMode.DeliveryLinear, "ETH", "USDT", 1748432430)]
        public void TestSharedSymbolConversion(TradingMode tradingMode, string baseAsset, string quoteAsset, int? deliverTime)
        {
            DateTime? time = deliverTime == null ? null : DateTimeConverter.ParseFromDouble(deliverTime.Value);
            var symbol = new SharedSymbol(tradingMode, baseAsset, quoteAsset, time);

            var serialized = JsonSerializer.Serialize(symbol);
            var restored = JsonSerializer.Deserialize<SharedSymbol>(serialized);

            Assert.That(restored!.TradingMode, Is.EqualTo(symbol.TradingMode));
            Assert.That(restored.BaseAsset, Is.EqualTo(symbol.BaseAsset));
            Assert.That(restored.QuoteAsset, Is.EqualTo(symbol.QuoteAsset));
            Assert.That(restored.DeliverTime, Is.EqualTo(symbol.DeliverTime));
        }

        [TestCase(0.1, null, null)]
        [TestCase(0.1, 0.1, null)]
        [TestCase(0.1, 0.1, 0.1)]
        [TestCase(null, 0.1, null)]
        [TestCase(null, 0.1, 0.1)]
        public void TestSharedQuantityConversion(double? baseQuantity, double? quoteQuantity, double? contractQuantity)
        {
            var symbol = new SharedOrderQuantity((decimal?)baseQuantity, (decimal?)quoteQuantity, (decimal?)contractQuantity);

            var serialized = JsonSerializer.Serialize(symbol);
            var restored = JsonSerializer.Deserialize<SharedOrderQuantity>(serialized);

            Assert.That(restored!.QuantityInBaseAsset, Is.EqualTo(symbol.QuantityInBaseAsset));
            Assert.That(restored.QuantityInQuoteAsset, Is.EqualTo(symbol.QuantityInQuoteAsset));
            Assert.That(restored.QuantityInContracts, Is.EqualTo(symbol.QuantityInContracts));
        }
    }
}
