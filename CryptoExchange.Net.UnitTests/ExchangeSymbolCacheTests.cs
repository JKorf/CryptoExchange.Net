using CryptoExchange.Net.SharedApis;
using NUnit.Framework;
using System;
using System.Linq;

namespace CryptoExchange.Net.UnitTests
{
    [TestFixture()]
    public class ExchangeSymbolCacheTests
    {
        private SharedSpotSymbol[] CreateTestSymbols()
        {
            return new[]
            {
                new SharedSpotSymbol("BTC", "USDT", "BTCUSDT", true, TradingMode.Spot),
                new SharedSpotSymbol("ETH", "USDT", "ETHUSDT", true, TradingMode.Spot),
                new SharedSpotSymbol("BTC", "EUR", "BTCEUR", true, TradingMode.Spot),
                new SharedSpotSymbol("ETH", "BTC", "ETHBTC", true, TradingMode.Spot),
                new SharedSpotSymbol("XRP", "USDT", "XRPUSDT", false, TradingMode.Spot)
            };
        }

        private SharedSpotSymbol[] CreateFuturesSymbols()
        {
            return new[]
            {
                new SharedSpotSymbol("BTC", "USDT", "BTCUSDT-PERP", true, TradingMode.PerpetualLinear),
                new SharedSpotSymbol("ETH", "USDT", "ETHUSDT-PERP", true, TradingMode.PerpetualLinear)
            };
        }

        [Test]
        public void UpdateSymbolInfo_NewTopic_Should_AddToCache()
        {
            // arrange
            var topicId = "NewExchange";
            var symbols = CreateTestSymbols();

            // act
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, symbols);
            var hasCached = ExchangeSymbolCache.HasCached(topicId);

            // assert
            Assert.That(hasCached, Is.True);
        }

        [Test]
        public void UpdateSymbolInfo_Should_StoreAllSymbols()
        {
            // arrange
            var topicId = "ExchangeWithSymbols";
            var symbols = CreateTestSymbols();

            // act
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, symbols);

            // assert
            Assert.That(ExchangeSymbolCache.SupportsSymbol(topicId, "BTCUSDT"), Is.True);
            Assert.That(ExchangeSymbolCache.SupportsSymbol(topicId, "ETHUSDT"), Is.True);
            Assert.That(ExchangeSymbolCache.SupportsSymbol(topicId, "BTCEUR"), Is.True);
            Assert.That(ExchangeSymbolCache.SupportsSymbol(topicId, "ETHBTC"), Is.True);
            Assert.That(ExchangeSymbolCache.SupportsSymbol(topicId, "XRPUSDT"), Is.True);
        }

        [Test]
        public void UpdateSymbolInfo_CalledTwiceWithinAnHour_Should_NotUpdate()
        {
            // arrange
            var topicId = "ExchangeNoUpdate";
            var initialSymbols = new[]
            {
                new SharedSpotSymbol("BTC", "USDT", "BTCUSDT", true, TradingMode.Spot)
            };
            var updatedSymbols = new[]
            {
                new SharedSpotSymbol("BTC", "USDT", "BTCUSDT", true, TradingMode.Spot),
                new SharedSpotSymbol("ETH", "USDT", "ETHUSDT", true, TradingMode.Spot)
            };

            // act
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, initialSymbols);
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, updatedSymbols);

            // assert - should still have only the initial symbol since less than 60 minutes passed
            Assert.That(ExchangeSymbolCache.SupportsSymbol(topicId, "BTCUSDT"), Is.True);
            // The second update should not have been applied
            Assert.That(ExchangeSymbolCache.SupportsSymbol(topicId, "ETHUSDT"), Is.False);
        }

        [Test]
        public void UpdateSymbolInfo_WithEmptyArray_Should_CreateEmptyCache()
        {
            // arrange
            var topicId = "EmptyExchange";
            var symbols = Array.Empty<SharedSpotSymbol>();

            // act
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, symbols);
            var hasCached = ExchangeSymbolCache.HasCached(topicId);

            // assert
            Assert.That(hasCached, Is.False);
        }

        [Test]
        public void HasCached_NonExistentTopic_Should_ReturnFalse()
        {
            // arrange
            var nonExistentTopic = "NonExistent_" + Guid.NewGuid();

            // act
            var result = ExchangeSymbolCache.HasCached(nonExistentTopic);

            // assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void HasCached_ExistingTopicWithSymbols_Should_ReturnTrue()
        {
            // arrange
            var topicId = "ExchangeWithData";
            var symbols = CreateTestSymbols();
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, symbols);

            // act
            var result = ExchangeSymbolCache.HasCached(topicId);

            // assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void HasCached_ExistingTopicWithNoSymbols_Should_ReturnFalse()
        {
            // arrange
            var topicId = "ExchangeNoData";
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, Array.Empty<SharedSpotSymbol>());

            // act
            var result = ExchangeSymbolCache.HasCached(topicId);

            // assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void SupportsSymbol_ByName_ExistingSymbol_Should_ReturnTrue()
        {
            // arrange
            var topicId = "ExchangeSupports";
            var symbols = CreateTestSymbols();
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, symbols);

            // act
            var result = ExchangeSymbolCache.SupportsSymbol(topicId, "BTCUSDT");

            // assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void SupportsSymbol_ByName_NonExistingSymbol_Should_ReturnFalse()
        {
            // arrange
            var topicId = "ExchangeNoSupport";
            var symbols = CreateTestSymbols();
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, symbols);

            // act
            var result = ExchangeSymbolCache.SupportsSymbol(topicId, "LINKUSDT");

            // assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void SupportsSymbol_ByName_NonExistentTopic_Should_ReturnFalse()
        {
            // arrange
            var nonExistentTopic = "NonExistent_" + Guid.NewGuid();

            // act
            var result = ExchangeSymbolCache.SupportsSymbol(nonExistentTopic, "BTCUSDT");

            // assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void SupportsSymbol_BySharedSymbol_ExistingSymbol_Should_ReturnTrue()
        {
            // arrange
            var topicId = "ExchangeSharedSymbol";
            var symbols = CreateTestSymbols();
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, symbols);
            var sharedSymbol = new SharedSymbol(TradingMode.Spot, "BTC", "USDT");

            // act
            var result = ExchangeSymbolCache.SupportsSymbol(topicId, sharedSymbol);

            // assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void SupportsSymbol_BySharedSymbol_NonExistingSymbol_Should_ReturnFalse()
        {
            // arrange
            var topicId = "ExchangeNoSharedSymbol";
            var symbols = CreateTestSymbols();
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, symbols);
            var sharedSymbol = new SharedSymbol(TradingMode.Spot, "LINK", "USDT");

            // act
            var result = ExchangeSymbolCache.SupportsSymbol(topicId, sharedSymbol);

            // assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void SupportsSymbol_BySharedSymbol_DifferentTradingMode_Should_ReturnFalse()
        {
            // arrange
            var topicId = "ExchangeDifferentMode";
            var symbols = CreateTestSymbols();
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, symbols);
            var sharedSymbol = new SharedSymbol(TradingMode.PerpetualLinear, "BTC", "USDT");

            // act
            var result = ExchangeSymbolCache.SupportsSymbol(topicId, sharedSymbol);

            // assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void SupportsSymbol_BySharedSymbol_NonExistentTopic_Should_ReturnFalse()
        {
            // arrange
            var nonExistentTopic = "NonExistent_" + Guid.NewGuid();
            var sharedSymbol = new SharedSymbol(TradingMode.Spot, "BTC", "USDT");

            // act
            var result = ExchangeSymbolCache.SupportsSymbol(nonExistentTopic, sharedSymbol);

            // assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void GetSymbolsForBaseAsset_ExistingBaseAsset_Should_ReturnMatchingSymbols()
        {
            // arrange
            var topicId = "ExchangeBaseAsset";
            var symbols = CreateTestSymbols();
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, symbols);

            // act
            var result = ExchangeSymbolCache.GetSymbolsForBaseAsset(topicId, "BTC");

            // assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.EqualTo(2));
            Assert.That(result.Any(x => x.QuoteAsset == "USDT"), Is.True);
            Assert.That(result.Any(x => x.QuoteAsset == "EUR"), Is.True);
        }

        [Test]
        public void GetSymbolsForBaseAsset_CaseInsensitive_Should_ReturnMatchingSymbols()
        {
            // arrange
            var topicId = "ExchangeCaseInsensitive";
            var symbols = CreateTestSymbols();
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, symbols);

            // act
            var result = ExchangeSymbolCache.GetSymbolsForBaseAsset(topicId, "btc");

            // assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.EqualTo(2));
        }

        [Test]
        public void GetSymbolsForBaseAsset_NonExistingBaseAsset_Should_ReturnEmptyArray()
        {
            // arrange
            var topicId = "ExchangeNoBaseAsset";
            var symbols = CreateTestSymbols();
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, symbols);

            // act
            var result = ExchangeSymbolCache.GetSymbolsForBaseAsset(topicId, "LINK");

            // assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.EqualTo(0));
        }

        [Test]
        public void GetSymbolsForBaseAsset_NonExistentTopic_Should_ReturnEmptyArray()
        {
            // arrange
            var nonExistentTopic = "NonExistent_" + Guid.NewGuid();

            // act
            var result = ExchangeSymbolCache.GetSymbolsForBaseAsset(nonExistentTopic, "BTC");

            // assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.EqualTo(0));
        }

        [Test]
        public void ParseSymbol_ExistingSymbol_Should_ReturnSharedSymbol()
        {
            // arrange
            var topicId = "ExchangeParse";
            var symbols = CreateTestSymbols();
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, symbols);

            // act
            var result = ExchangeSymbolCache.ParseSymbol(topicId, "BTCUSDT");

            // assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.BaseAsset, Is.EqualTo("BTC"));
            Assert.That(result.QuoteAsset, Is.EqualTo("USDT"));
            Assert.That(result.TradingMode, Is.EqualTo(TradingMode.Spot));
            Assert.That(result.SymbolName, Is.EqualTo("BTCUSDT"));
        }

        [Test]
        public void ParseSymbol_NonExistingSymbol_Should_ReturnNull()
        {
            // arrange
            var topicId = "ExchangeNoParse";
            var symbols = CreateTestSymbols();
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, symbols);

            // act
            var result = ExchangeSymbolCache.ParseSymbol(topicId, "LINKUSDT");

            // assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void ParseSymbol_NullSymbolName_Should_ReturnNull()
        {
            // arrange
            var topicId = "ExchangeNullSymbol";
            var symbols = CreateTestSymbols();
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, symbols);

            // act
            var result = ExchangeSymbolCache.ParseSymbol(topicId, null);

            // assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void ParseSymbol_NonExistentTopic_Should_ReturnNull()
        {
            // arrange
            var nonExistentTopic = "NonExistent_" + Guid.NewGuid();

            // act
            var result = ExchangeSymbolCache.ParseSymbol(nonExistentTopic, "BTCUSDT");

            // assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void MultipleTopics_Should_MaintainSeparateData()
        {
            // arrange
            var topic1 = "Exchange1";
            var topic2 = "Exchange2";
            var symbols1 = new[]
            {
                new SharedSpotSymbol("BTC", "USDT", "BTCUSDT", true, TradingMode.Spot)
            };
            var symbols2 = new[]
            {
                new SharedSpotSymbol("ETH", "USDT", "ETHUSDT", true, TradingMode.Spot)
            };

            // act
            ExchangeSymbolCache.UpdateSymbolInfo(topic1, symbols1);
            ExchangeSymbolCache.UpdateSymbolInfo(topic2, symbols2);

            // assert
            Assert.That(ExchangeSymbolCache.SupportsSymbol(topic1, "BTCUSDT"), Is.True);
            Assert.That(ExchangeSymbolCache.SupportsSymbol(topic1, "ETHUSDT"), Is.False);
            Assert.That(ExchangeSymbolCache.SupportsSymbol(topic2, "ETHUSDT"), Is.True);
            Assert.That(ExchangeSymbolCache.SupportsSymbol(topic2, "BTCUSDT"), Is.False);
        }

        [Test]
        public void UpdateSymbolInfo_WithDifferentTradingModes_Should_StoreCorrectly()
        {
            // arrange
            var topicId = "ExchangeMixedModes";
            var spotSymbols = CreateTestSymbols();
            var futuresSymbols = CreateFuturesSymbols();
            var allSymbols = spotSymbols.Concat(futuresSymbols).ToArray();

            // act
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, allSymbols);

            // assert
            var spotSymbol = new SharedSymbol(TradingMode.Spot, "BTC", "USDT");
            var futuresSymbol = new SharedSymbol(TradingMode.PerpetualLinear, "BTC", "USDT");

            Assert.That(ExchangeSymbolCache.SupportsSymbol(topicId, spotSymbol), Is.True);
            Assert.That(ExchangeSymbolCache.SupportsSymbol(topicId, futuresSymbol), Is.True);
        }

        [Test]
        public void GetSymbolsForBaseAsset_Should_ReturnAllTradingModes()
        {
            // arrange
            var topicId = "ExchangeAllModes";
            var spotSymbols = CreateTestSymbols();
            var futuresSymbols = CreateFuturesSymbols();
            var allSymbols = spotSymbols.Concat(futuresSymbols).ToArray();
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, allSymbols);

            // act
            var result = ExchangeSymbolCache.GetSymbolsForBaseAsset(topicId, "BTC");

            // assert
            Assert.That(result.Length, Is.GreaterThanOrEqualTo(2));
            Assert.That(result.Any(x => x.TradingMode == TradingMode.Spot), Is.True);
            Assert.That(result.Any(x => x.TradingMode == TradingMode.PerpetualLinear), Is.True);
        }

        [Test]
        public void GetSymbolsForBaseAsset_WithMultipleMatchingSymbols_Should_ReturnAll()
        {
            // arrange
            var topicId = "ExchangeMultiple";
            var symbols = new[]
            {
                new SharedSpotSymbol("ETH", "USDT", "ETHUSDT", true, TradingMode.Spot),
                new SharedSpotSymbol("ETH", "BTC", "ETHBTC", true, TradingMode.Spot),
                new SharedSpotSymbol("ETH", "EUR", "ETHEUR", true, TradingMode.Spot)
            };
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, symbols);

            // act
            var result = ExchangeSymbolCache.GetSymbolsForBaseAsset(topicId, "ETH");

            // assert
            Assert.That(result.Length, Is.EqualTo(3));
            Assert.That(result.All(x => x.BaseAsset == "ETH"), Is.True);
        }

    }
}