using CryptoExchange.Net.SharedApis;
using NUnit.Framework;
using System;

namespace CryptoExchange.Net.UnitTests
{
    [TestFixture()]
    public class SharedSymbolTests
    {
        [Test]
        public void SharedSymbol_Constructor_Should_SetAllProperties()
        {
            // arrange
            var tradingMode = TradingMode.Spot;
            var baseAsset = "BTC";
            var quoteAsset = "USDT";

            // act
            var symbol = new SharedSymbol(tradingMode, baseAsset, quoteAsset);

            // assert
            Assert.That(symbol.TradingMode, Is.EqualTo(tradingMode));
            Assert.That(symbol.BaseAsset, Is.EqualTo(baseAsset));
            Assert.That(symbol.QuoteAsset, Is.EqualTo(quoteAsset));
            Assert.That(symbol.DeliverTime, Is.Null);
            Assert.That(symbol.SymbolName, Is.Null);
        }

        [Test]
        public void SharedSymbol_Constructor_WithDeliveryTime_Should_SetDeliveryTime()
        {
            // arrange
            var tradingMode = TradingMode.DeliveryLinear;
            var baseAsset = "BTC";
            var quoteAsset = "USDT";
            var deliveryTime = new DateTime(2026, 6, 25, 0, 0, 0, DateTimeKind.Utc);

            // act
            var symbol = new SharedSymbol(tradingMode, baseAsset, quoteAsset, deliveryTime);

            // assert
            Assert.That(symbol.TradingMode, Is.EqualTo(tradingMode));
            Assert.That(symbol.BaseAsset, Is.EqualTo(baseAsset));
            Assert.That(symbol.QuoteAsset, Is.EqualTo(quoteAsset));
            Assert.That(symbol.DeliverTime, Is.EqualTo(deliveryTime));
            Assert.That(symbol.SymbolName, Is.Null);
        }

        [Test]
        public void SharedSymbol_Constructor_WithNullDeliveryTime_Should_SetToNull()
        {
            // arrange
            var tradingMode = TradingMode.Spot;
            var baseAsset = "ETH";
            var quoteAsset = "BTC";

            // act
            var symbol = new SharedSymbol(tradingMode, baseAsset, quoteAsset, deliverTime: null);

            // assert
            Assert.That(symbol.DeliverTime, Is.Null);
        }

        [TestCase(TradingMode.Spot)]
        [TestCase(TradingMode.PerpetualLinear)]
        [TestCase(TradingMode.PerpetualInverse)]
        [TestCase(TradingMode.DeliveryLinear)]
        [TestCase(TradingMode.DeliveryInverse)]
        public void SharedSymbol_Constructor_WithDifferentTradingModes_Should_SetCorrectly(TradingMode tradingMode)
        {
            // arrange
            var baseAsset = "BTC";
            var quoteAsset = "USDT";

            // act
            var symbol = new SharedSymbol(tradingMode, baseAsset, quoteAsset);

            // assert
            Assert.That(symbol.TradingMode, Is.EqualTo(tradingMode));
        }

        [Test]
        public void SharedSymbol_ConstructorWithSymbolName_Should_SetSymbolName()
        {
            // arrange
            var tradingMode = TradingMode.Spot;
            var baseAsset = "BTC";
            var quoteAsset = "USDT";
            var symbolName = "BTC-USDT";

            // act
            var symbol = new SharedSymbol(tradingMode, baseAsset, quoteAsset, symbolName);

            // assert
            Assert.That(symbol.TradingMode, Is.EqualTo(tradingMode));
            Assert.That(symbol.BaseAsset, Is.EqualTo(baseAsset));
            Assert.That(symbol.QuoteAsset, Is.EqualTo(quoteAsset));
            Assert.That(symbol.SymbolName, Is.EqualTo(symbolName));
            Assert.That(symbol.DeliverTime, Is.Null);
        }

        [Test]
        public void SharedSymbol_ConstructorWithSymbolName_WithCustomFormat_Should_SetCorrectly()
        {
            // arrange
            var tradingMode = TradingMode.PerpetualLinear;
            var baseAsset = "ETH";
            var quoteAsset = "USDT";
            var symbolName = "ETHUSDT-PERP";

            // act
            var symbol = new SharedSymbol(tradingMode, baseAsset, quoteAsset, symbolName);

            // assert
            Assert.That(symbol.SymbolName, Is.EqualTo(symbolName));
        }

        [Test]
        public void SharedSymbol_ConstructorWithSymbolName_WithEmptyString_Should_SetEmptyString()
        {
            // arrange
            var tradingMode = TradingMode.Spot;
            var baseAsset = "BTC";
            var quoteAsset = "USDT";
            var symbolName = "";

            // act
            var symbol = new SharedSymbol(tradingMode, baseAsset, quoteAsset, symbolName);

            // assert
            Assert.That(symbol.SymbolName, Is.EqualTo(""));
        }

        [Test]
        public void GetSymbol_WithSymbolNameSet_Should_ReturnSymbolName()
        {
            // arrange
            var symbol = new SharedSymbol(TradingMode.Spot, "BTC", "USDT", "CUSTOM-BTC-USDT");
            var formatFunc = new Func<string, string, TradingMode, DateTime?, string>(
                (b, q, t, d) => $"{b}{q}");

            // act
            var result = symbol.GetSymbol(formatFunc);

            // assert
            Assert.That(result, Is.EqualTo("CUSTOM-BTC-USDT"));
        }

        [Test]
        public void GetSymbol_WithSymbolNameNull_Should_UseFormatFunction()
        {
            // arrange
            var symbol = new SharedSymbol(TradingMode.Spot, "BTC", "USDT");
            var formatFunc = new Func<string, string, TradingMode, DateTime?, string>(
                (b, q, t, d) => $"{b}/{q}");

            // act
            var result = symbol.GetSymbol(formatFunc);

            // assert
            Assert.That(result, Is.EqualTo("BTC/USDT"));
        }

        [Test]
        public void GetSymbol_WithComplexFormatFunction_Should_ApplyCorrectly()
        {
            // arrange
            var symbol = new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT");
            var formatFunc = new Func<string, string, TradingMode, DateTime?, string>(
                (b, q, t, d) => t == TradingMode.PerpetualLinear ? $"{b}{q}-PERP" : $"{b}{q}");

            // act
            var result = symbol.GetSymbol(formatFunc);

            // assert
            Assert.That(result, Is.EqualTo("ETHUSDT-PERP"));
        }

        [Test]
        public void GetSymbol_WithDeliveryTime_Should_PassDeliveryTimeToFormatter()
        {
            // arrange
            var deliveryTime = new DateTime(2026, 6, 25);
            var symbol = new SharedSymbol(TradingMode.DeliveryLinear, "BTC", "USDT", deliveryTime);
            var formatFunc = new Func<string, string, TradingMode, DateTime?, string>(
                (b, q, t, d) => d.HasValue ? $"{b}{q}_{d.Value:yyyyMMdd}" : $"{b}{q}");

            // act
            var result = symbol.GetSymbol(formatFunc);

            // assert
            Assert.That(result, Is.EqualTo("BTCUSDT_20260625"));
        }

        [Test]
        public void GetSymbol_WithTradingMode_Should_PassTradingModeToFormatter()
        {
            // arrange
            var symbol = new SharedSymbol(TradingMode.PerpetualInverse, "BTC", "USD");
            var formatFunc = new Func<string, string, TradingMode, DateTime?, string>(
                (b, q, t, d) =>
                {
                    return t switch
                    {
                        TradingMode.Spot => $"{b}{q}",
                        TradingMode.PerpetualLinear => $"{b}{q}-PERP",
                        TradingMode.PerpetualInverse => $"{b}{q}I-PERP",
                        _ => $"{b}{q}"
                    };
                });

            // act
            var result = symbol.GetSymbol(formatFunc);

            // assert
            Assert.That(result, Is.EqualTo("BTCUSDI-PERP"));
        }

        [Test]
        public void GetSymbol_WithEmptySymbolName_Should_UseFormatFunction()
        {
            // arrange
            var symbol = new SharedSymbol(TradingMode.Spot, "BTC", "USDT", "");
            var formatFunc = new Func<string, string, TradingMode, DateTime?, string>(
                (b, q, t, d) => $"{b}-{q}");

            // act
            var result = symbol.GetSymbol(formatFunc);

            // assert
            Assert.That(result, Is.EqualTo("BTC-USDT"));
        }

        [Test]
        public void GetSymbol_WithWhitespaceSymbolName_Should_ReturnWhitespace()
        {
            // arrange
            var symbol = new SharedSymbol(TradingMode.Spot, "BTC", "USDT", "   ");
            var formatFunc = new Func<string, string, TradingMode, DateTime?, string>(
                (b, q, t, d) => $"{b}-{q}");

            // act
            var result = symbol.GetSymbol(formatFunc);

            // assert
            Assert.That(result, Is.EqualTo("   "));
        }

        [Test]
        public void SharedSymbol_RecordEquality_SameValues_Should_BeEqual()
        {
            // arrange
            var symbol1 = new SharedSymbol(TradingMode.Spot, "BTC", "USDT");
            var symbol2 = new SharedSymbol(TradingMode.Spot, "BTC", "USDT");

            // act & assert
            Assert.That(symbol1, Is.EqualTo(symbol2));
        }

        [Test]
        public void SharedSymbol_RecordEquality_DifferentBaseAsset_Should_NotBeEqual()
        {
            // arrange
            var symbol1 = new SharedSymbol(TradingMode.Spot, "BTC", "USDT");
            var symbol2 = new SharedSymbol(TradingMode.Spot, "ETH", "USDT");

            // act & assert
            Assert.That(symbol1, Is.Not.EqualTo(symbol2));
        }

        [Test]
        public void SharedSymbol_RecordEquality_DifferentQuoteAsset_Should_NotBeEqual()
        {
            // arrange
            var symbol1 = new SharedSymbol(TradingMode.Spot, "BTC", "USDT");
            var symbol2 = new SharedSymbol(TradingMode.Spot, "BTC", "EUR");

            // act & assert
            Assert.That(symbol1, Is.Not.EqualTo(symbol2));
        }

        [Test]
        public void SharedSymbol_RecordEquality_DifferentTradingMode_Should_NotBeEqual()
        {
            // arrange
            var symbol1 = new SharedSymbol(TradingMode.Spot, "BTC", "USDT");
            var symbol2 = new SharedSymbol(TradingMode.PerpetualLinear, "BTC", "USDT");

            // act & assert
            Assert.That(symbol1, Is.Not.EqualTo(symbol2));
        }

        [Test]
        public void SharedSymbol_RecordEquality_DifferentDeliveryTime_Should_NotBeEqual()
        {
            // arrange
            var deliveryTime1 = new DateTime(2026, 6, 25);
            var deliveryTime2 = new DateTime(2026, 9, 25);
            var symbol1 = new SharedSymbol(TradingMode.DeliveryLinear, "BTC", "USDT", deliveryTime1);
            var symbol2 = new SharedSymbol(TradingMode.DeliveryLinear, "BTC", "USDT", deliveryTime2);

            // act & assert
            Assert.That(symbol1, Is.Not.EqualTo(symbol2));
        }

        [Test]
        public void SharedSymbol_RecordEquality_DifferentSymbolName_Should_NotBeEqual()
        {
            // arrange
            var symbol1 = new SharedSymbol(TradingMode.Spot, "BTC", "USDT", "BTCUSDT");
            var symbol2 = new SharedSymbol(TradingMode.Spot, "BTC", "USDT", "BTC-USDT");

            // act & assert
            Assert.That(symbol1, Is.Not.EqualTo(symbol2));
        }

        [Test]
        public void SharedSymbol_RecordEquality_OneWithSymbolNameOneWithout_Should_NotBeEqual()
        {
            // NOTE; although this should probably be equal it's considered not because the SymbolName property isn't equal
            // Overridding equality to ignore SymbolName would be possible but would break the default record equality behavior and cause confusion

            // arrange
            var symbol1 = new SharedSymbol(TradingMode.Spot, "BTC", "USDT", "BTCUSDT");
            var symbol2 = new SharedSymbol(TradingMode.Spot, "BTC", "USDT");

            // act & assert
            Assert.That(symbol1, Is.Not.EqualTo(symbol2));
        }

        [Test]
        public void SharedSymbol_RecordEquality_WithAllPropertiesSet_Should_BeEqual()
        {
            // arrange
            var deliveryTime = new DateTime(2026, 6, 25);
            var symbol1 = new SharedSymbol(TradingMode.DeliveryLinear, "BTC", "USDT", deliveryTime)
            {
                SymbolName = "BTCUSDT-0625"
            };
            var symbol2 = new SharedSymbol(TradingMode.DeliveryLinear, "BTC", "USDT", deliveryTime)
            {
                SymbolName = "BTCUSDT-0625"
            };

            // act & assert
            Assert.That(symbol1, Is.EqualTo(symbol2));
        }

        [Test]
        public void SharedSymbol_Properties_Should_BeSettable()
        {
            // arrange
            var symbol = new SharedSymbol(TradingMode.Spot, "BTC", "USDT");

            // act
            symbol.BaseAsset = "ETH";
            symbol.QuoteAsset = "EUR";
            symbol.TradingMode = TradingMode.PerpetualLinear;
            symbol.SymbolName = "CUSTOM";
            symbol.DeliverTime = DateTime.UtcNow;

            // assert
            Assert.That(symbol.BaseAsset, Is.EqualTo("ETH"));
            Assert.That(symbol.QuoteAsset, Is.EqualTo("EUR"));
            Assert.That(symbol.TradingMode, Is.EqualTo(TradingMode.PerpetualLinear));
            Assert.That(symbol.SymbolName, Is.EqualTo("CUSTOM"));
            Assert.That(symbol.DeliverTime, Is.Not.Null);
        }

        [Test]
        public void SharedSymbol_WithSpecialCharactersInAssets_Should_HandleCorrectly()
        {
            // arrange
            var baseAsset = "BTC-123";
            var quoteAsset = "USDT_2.0";

            // act
            var symbol = new SharedSymbol(TradingMode.Spot, baseAsset, quoteAsset);

            // assert
            Assert.That(symbol.BaseAsset, Is.EqualTo(baseAsset));
            Assert.That(symbol.QuoteAsset, Is.EqualTo(quoteAsset));
        }

        [Test]
        public void SharedSymbol_WithLongAssetNames_Should_HandleCorrectly()
        {
            // arrange
            var baseAsset = "VERYLONGASSETNAMEFORTESTING";
            var quoteAsset = "ANOTHERVERYLONGASSETNAME";

            // act
            var symbol = new SharedSymbol(TradingMode.Spot, baseAsset, quoteAsset);

            // assert
            Assert.That(symbol.BaseAsset, Is.EqualTo(baseAsset));
            Assert.That(symbol.QuoteAsset, Is.EqualTo(quoteAsset));
        }
    }
}