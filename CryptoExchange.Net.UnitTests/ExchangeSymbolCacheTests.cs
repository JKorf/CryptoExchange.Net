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
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, "Env", null, symbols);
            var hasCached = ExchangeSymbolCache.HasCached(topicId, "Env", null);

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
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, "Env", null, symbols);

            // assert
            Assert.That(ExchangeSymbolCache.SupportsSymbol(topicId, "Env", null, "BTCUSDT"), Is.True);
            Assert.That(ExchangeSymbolCache.SupportsSymbol(topicId, "Env", null, "ETHUSDT"), Is.True);
            Assert.That(ExchangeSymbolCache.SupportsSymbol(topicId, "Env", null, "BTCEUR"), Is.True);
            Assert.That(ExchangeSymbolCache.SupportsSymbol(topicId, "Env", null, "ETHBTC"), Is.True);
            Assert.That(ExchangeSymbolCache.SupportsSymbol(topicId, "Env", null, "XRPUSDT"), Is.True);
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
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, "Env", null, initialSymbols);
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, "Env", null, updatedSymbols);

            // assert - should still have only the initial symbol since less than 60 minutes passed
            Assert.That(ExchangeSymbolCache.SupportsSymbol(topicId, "Env", null, "BTCUSDT"), Is.True);
            // The second update should not have been applied
            Assert.That(ExchangeSymbolCache.SupportsSymbol(topicId, "Env", null, "ETHUSDT"), Is.False);
        }

        [Test]
        public void UpdateSymbolInfo_WithEmptyArray_Should_CreateEmptyCache()
        {
            // arrange
            var topicId = "EmptyExchange";
            var symbols = Array.Empty<SharedSpotSymbol>();

            // act
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, "Env", null, symbols);
            var hasCached = ExchangeSymbolCache.HasCached(topicId, "Env", null);

            // assert
            Assert.That(hasCached, Is.False);
        }

        [Test]
        public void HasCached_NonExistentTopic_Should_ReturnFalse()
        {
            // arrange
            var nonExistentTopic = "NonExistent_" + Guid.NewGuid();

            // act
            var result = ExchangeSymbolCache.HasCached(nonExistentTopic, "Env", null);

            // assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void HasCached_ExistingTopicWithSymbols_Should_ReturnTrue()
        {
            // arrange
            var topicId = "ExchangeWithData";
            var symbols = CreateTestSymbols();
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, "Env", null, symbols);

            // act
            var result = ExchangeSymbolCache.HasCached(topicId, "Env", null);

            // assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void HasCached_ExistingTopicWithNoSymbols_Should_ReturnFalse()
        {
            // arrange
            var topicId = "ExchangeNoData";
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, "Env", null, Array.Empty<SharedSpotSymbol>());

            // act
            var result = ExchangeSymbolCache.HasCached(topicId, "Env", null);

            // assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void SupportsSymbol_ByName_ExistingSymbol_Should_ReturnTrue()
        {
            // arrange
            var topicId = "ExchangeSupports";
            var symbols = CreateTestSymbols();
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, "Env", null, symbols);

            // act
            var result = ExchangeSymbolCache.SupportsSymbol(topicId, "Env", null, "BTCUSDT");

            // assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void SupportsSymbol_ByName_NonExistingSymbol_Should_ReturnFalse()
        {
            // arrange
            var topicId = "ExchangeNoSupport";
            var symbols = CreateTestSymbols();
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, "Env", null, symbols);

            // act
            var result = ExchangeSymbolCache.SupportsSymbol(topicId, "Env", null, "LINKUSDT");

            // assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void SupportsSymbol_ByName_NonExistentTopic_Should_ReturnFalse()
        {
            // arrange
            var nonExistentTopic = "NonExistent_" + Guid.NewGuid();

            // act
            var result = ExchangeSymbolCache.SupportsSymbol(nonExistentTopic, "Env", null, "BTCUSDT");

            // assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void SupportsSymbol_BySharedSymbol_ExistingSymbol_Should_ReturnTrue()
        {
            // arrange
            var topicId = "ExchangeSharedSymbol";
            var symbols = CreateTestSymbols();
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, "Env", null, symbols);
            var sharedSymbol = new SharedSymbol(TradingMode.Spot, "BTC", "USDT");

            // act
            var result = ExchangeSymbolCache.SupportsSymbol(topicId, "Env", null, sharedSymbol);

            // assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void SupportsSymbol_BySharedSymbol_NonExistingSymbol_Should_ReturnFalse()
        {
            // arrange
            var topicId = "ExchangeNoSharedSymbol";
            var symbols = CreateTestSymbols();
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, "Env", null, symbols);
            var sharedSymbol = new SharedSymbol(TradingMode.Spot, "LINK", "USDT");

            // act
            var result = ExchangeSymbolCache.SupportsSymbol(topicId, "Env", null, sharedSymbol);

            // assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void SupportsSymbol_BySharedSymbol_DifferentTradingMode_Should_ReturnFalse()
        {
            // arrange
            var topicId = "ExchangeDifferentMode";
            var symbols = CreateTestSymbols();
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, "Env", null, symbols);
            var sharedSymbol = new SharedSymbol(TradingMode.PerpetualLinear, "BTC", "USDT");

            // act
            var result = ExchangeSymbolCache.SupportsSymbol(topicId, "Env", null, sharedSymbol);

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
            var result = ExchangeSymbolCache.SupportsSymbol(nonExistentTopic, "Env", null, sharedSymbol);

            // assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void GetSymbolsForBaseAsset_ExistingBaseAsset_Should_ReturnMatchingSymbols()
        {
            // arrange
            var topicId = "ExchangeBaseAsset";
            var symbols = CreateTestSymbols();
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, "Env", null, symbols);

            // act
            var result = ExchangeSymbolCache.GetSymbolsForBaseAsset(topicId, "Env", null, "BTC");

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
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, "Env", null, symbols);

            // act
            var result = ExchangeSymbolCache.GetSymbolsForBaseAsset(topicId, "Env", null, "btc");

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
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, "Env", null, symbols);

            // act
            var result = ExchangeSymbolCache.GetSymbolsForBaseAsset(topicId, "Env", null, "LINK");

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
            var result = ExchangeSymbolCache.GetSymbolsForBaseAsset(nonExistentTopic, "Env", null, "BTC");

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
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, "Env", null, symbols);

            // act
            var result = ExchangeSymbolCache.ParseSymbol(topicId, "Env", null, "BTCUSDT");

            // assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.BaseAsset, Is.EqualTo("BTC"));
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
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, "Env", null, symbols);

            // act
            var result = ExchangeSymbolCache.ParseSymbol(topicId, "Env", null, "LINKUSDT");

            // assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void ParseSymbol_NullSymbolName_Should_ReturnNull()
        {
            // arrange
            var topicId = "ExchangeNullSymbol";
            var symbols = CreateTestSymbols();
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, "Env", null, symbols);

            // act
            var result = ExchangeSymbolCache.ParseSymbol(topicId, "Env", null, null);

            // assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void ParseSymbol_NonExistentTopic_Should_ReturnNull()
        {
            // arrange
            var nonExistentTopic = "NonExistent_" + Guid.NewGuid();

            // act
            var result = ExchangeSymbolCache.ParseSymbol(nonExistentTopic, "Env", null, "BTCUSDT");

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
            ExchangeSymbolCache.UpdateSymbolInfo(topic1, "Env", null, symbols1);
            ExchangeSymbolCache.UpdateSymbolInfo(topic2, "Env", null, symbols2);

            // assert
            Assert.That(ExchangeSymbolCache.SupportsSymbol(topic1, "Env", null, "BTCUSDT"), Is.True);
            Assert.That(ExchangeSymbolCache.SupportsSymbol(topic1, "Env", null, "ETHUSDT"), Is.False);
            Assert.That(ExchangeSymbolCache.SupportsSymbol(topic2, "Env", null, "ETHUSDT"), Is.True);
            Assert.That(ExchangeSymbolCache.SupportsSymbol(topic2, "Env", null, "BTCUSDT"), Is.False);
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
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, "Env", null, allSymbols);

            // assert
            var spotSymbol = new SharedSymbol(TradingMode.Spot, "BTC", "USDT");
            var futuresSymbol = new SharedSymbol(TradingMode.PerpetualLinear, "BTC", "USDT");

            Assert.That(ExchangeSymbolCache.SupportsSymbol(topicId, "Env", null, spotSymbol), Is.True);
            Assert.That(ExchangeSymbolCache.SupportsSymbol(topicId, "Env", null, futuresSymbol), Is.True);
        }

        [Test]
        public void GetSymbolsForBaseAsset_Should_ReturnAllTradingModes()
        {
            // arrange
            var topicId = "ExchangeAllModes";
            var spotSymbols = CreateTestSymbols();
            var futuresSymbols = CreateFuturesSymbols();
            var allSymbols = spotSymbols.Concat(futuresSymbols).ToArray();
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, "Env", null, allSymbols);

            // act
            var result = ExchangeSymbolCache.GetSymbolsForBaseAsset(topicId, "Env", null, "BTC");

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
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, "Env", null, symbols);

            // act
            var result = ExchangeSymbolCache.GetSymbolsForBaseAsset(topicId, "Env", null, "ETH");

            // assert
            Assert.That(result.Length, Is.EqualTo(3));
            Assert.That(result.All(x => x.BaseAsset == "ETH"), Is.True);
        }

        [Test]
        public void GetSymbolsForBaseAsset_WithDifferentEnvironments_Should_ReturnNone()
        {
            // arrange
            var topicId = "Topic1";
            var spotSymbols = CreateTestSymbols();
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, "Live", null, spotSymbols);

            // act
            var result = ExchangeSymbolCache.GetSymbolsForBaseAsset(topicId, "Test", null, "BTC");

            // assert
            Assert.That(result.Length, Is.EqualTo(0));
        }

        [Test]
        public void GetSymbolsForBaseAsset_WithDifferentKey_Should_ReturnNone()
        {
            // arrange
            var topicId = "Topic2";
            var spotSymbols = CreateTestSymbols();
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, "Live", "1", spotSymbols);

            // act
            var result = ExchangeSymbolCache.GetSymbolsForBaseAsset(topicId, "Live", "2", "BTC");

            // assert
            Assert.That(result.Length, Is.EqualTo(0));
        }

        [Test]
        public void GetSymbolsForBaseAsset_WithSetKey_Should_ReturnNone()
        {
            // arrange
            var topicId = "Topic3";
            var spotSymbols = CreateTestSymbols();
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, "Live", null, spotSymbols);

            // act
            var result = ExchangeSymbolCache.GetSymbolsForBaseAsset(topicId, "Live", "2", "BTC");

            // assert
            Assert.That(result.Length, Is.EqualTo(0));
        }

        [Test]
        public void GetSymbolsForBaseAsset_WithNotSetKey_Should_ReturnNone()
        {
            // arrange
            var topicId = "Topic4";
            var spotSymbols = CreateTestSymbols();
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, "Live", "2", spotSymbols);

            // act
            var result = ExchangeSymbolCache.GetSymbolsForBaseAsset(topicId, "Live", null, "BTC");

            // assert
            Assert.That(result.Length, Is.EqualTo(2));
        }

        [Test]
        public void ParseSymbol_WithDifferentKey_Should_ReturnNull()
        {
            // arrange
            var topicId = "Topic5";
            var spotSymbols = CreateTestSymbols();
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, "Live", "1", spotSymbols);

            // act
            var result = ExchangeSymbolCache.ParseSymbol(topicId, "Live", "2", "BTCUSDT");

            // assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void ParseSymbol_WithSetKey_Should_ReturnNull()
        {
            // arrange
            var topicId = "Topic6";
            var spotSymbols = CreateTestSymbols();
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, "Live", null, spotSymbols);

            // act
            var result = ExchangeSymbolCache.ParseSymbol(topicId, "Live", "2", "BTCUSDT");

            // assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void ParseSymbol_WithNotSetKey_Should_ReturnNull()
        {
            // arrange
            var topicId = "Topic7";
            var spotSymbols = CreateTestSymbols();
            ExchangeSymbolCache.UpdateSymbolInfo(topicId, "Live", "1", spotSymbols);

            // act
            var result = ExchangeSymbolCache.ParseSymbol(topicId, "Live", null, "BTCUSDT");

            // assert
            Assert.That(result, Is.Not.Null);
        }
    }
}