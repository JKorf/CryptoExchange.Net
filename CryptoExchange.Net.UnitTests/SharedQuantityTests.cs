using CryptoExchange.Net.SharedApis;
using NUnit.Framework;
using System;

namespace CryptoExchange.Net.UnitTests
{
    [TestFixture()]
    public class SharedQuantityTests
    {
        [Test]
        public void SharedQuantityReference_IsZero_AllNull_Should_ReturnTrue()
        {
            // arrange
            var quantity = new SharedOrderQuantity(null, null, null);

            // act & assert
            Assert.That(quantity.IsZero, Is.True);
        }

        [Test]
        public void SharedQuantityReference_IsZero_AllZero_Should_ReturnTrue()
        {
            // arrange
            var quantity = new SharedOrderQuantity(0, 0, 0);

            // act & assert
            Assert.That(quantity.IsZero, Is.True);
        }

        [Test]
        public void SharedQuantityReference_IsZero_BaseAssetSet_Should_ReturnFalse()
        {
            // arrange
            var quantity = new SharedOrderQuantity(1.5m, null, null);

            // act & assert
            Assert.That(quantity.IsZero, Is.False);
        }

        [Test]
        public void SharedQuantityReference_IsZero_QuoteAssetSet_Should_ReturnFalse()
        {
            // arrange
            var quantity = new SharedOrderQuantity(null, 100m, null);

            // act & assert
            Assert.That(quantity.IsZero, Is.False);
        }

        [Test]
        public void SharedQuantityReference_IsZero_ContractsSet_Should_ReturnFalse()
        {
            // arrange
            var quantity = new SharedOrderQuantity(null, null, 10m);

            // act & assert
            Assert.That(quantity.IsZero, Is.False);
        }

        [Test]
        public void SharedQuantityReference_IsZero_NegativeValue_Should_ReturnTrue()
        {
            // arrange
            var quantity = new SharedOrderQuantity(-1m, 0, 0);

            // act & assert
            Assert.That(quantity.IsZero, Is.True);
        }

        [Test]
        public void SharedQuantity_DefaultConstructor_Should_SetAllPropertiesToNull()
        {
            // arrange & act
            var quantity = new SharedQuantity();

            // assert
            Assert.That(quantity.QuantityInBaseAsset, Is.Null);
            Assert.That(quantity.QuantityInQuoteAsset, Is.Null);
            Assert.That(quantity.QuantityInContracts, Is.Null);
            Assert.That(quantity.IsZero, Is.True);
        }

        [Test]
        public void SharedQuantity_Base_Should_SetBaseAssetQuantity()
        {
            // arrange
            var expectedQuantity = 1.5m;

            // act
            var quantity = SharedQuantity.Base(expectedQuantity);

            // assert
            Assert.That(quantity.QuantityInBaseAsset, Is.EqualTo(expectedQuantity));
            Assert.That(quantity.QuantityInQuoteAsset, Is.Null);
            Assert.That(quantity.QuantityInContracts, Is.Null);
        }

        [Test]
        public void SharedQuantity_Base_WithZero_Should_SetZeroQuantity()
        {
            // arrange & act
            var quantity = SharedQuantity.Base(0m);

            // assert
            Assert.That(quantity.QuantityInBaseAsset, Is.EqualTo(0m));
            Assert.That(quantity.IsZero, Is.True);
        }

        [Test]
        public void SharedQuantity_Base_WithLargeValue_Should_SetCorrectly()
        {
            // arrange
            var largeValue = 999999.123456789m;

            // act
            var quantity = SharedQuantity.Base(largeValue);

            // assert
            Assert.That(quantity.QuantityInBaseAsset, Is.EqualTo(largeValue));
        }

        [Test]
        public void SharedQuantity_Quote_Should_SetQuoteAssetQuantity()
        {
            // arrange
            var expectedQuantity = 100m;

            // act
            var quantity = SharedQuantity.Quote(expectedQuantity);

            // assert
            Assert.That(quantity.QuantityInBaseAsset, Is.Null);
            Assert.That(quantity.QuantityInQuoteAsset, Is.EqualTo(expectedQuantity));
            Assert.That(quantity.QuantityInContracts, Is.Null);
        }

        [Test]
        public void SharedQuantity_Quote_WithDecimal_Should_PreserveDecimals()
        {
            // arrange
            var expectedQuantity = 50.123456m;

            // act
            var quantity = SharedQuantity.Quote(expectedQuantity);

            // assert
            Assert.That(quantity.QuantityInQuoteAsset, Is.EqualTo(expectedQuantity));
        }

        [Test]
        public void SharedQuantity_Contracts_Should_SetContractQuantity()
        {
            // arrange
            var expectedQuantity = 10m;

            // act
            var quantity = SharedQuantity.Contracts(expectedQuantity);

            // assert
            Assert.That(quantity.QuantityInBaseAsset, Is.Null);
            Assert.That(quantity.QuantityInQuoteAsset, Is.Null);
            Assert.That(quantity.QuantityInContracts, Is.EqualTo(expectedQuantity));
        }

        [Test]
        public void SharedQuantity_Contracts_WithFractionalValue_Should_SetCorrectly()
        {
            // arrange
            var expectedQuantity = 2.5m;

            // act
            var quantity = SharedQuantity.Contracts(expectedQuantity);

            // assert
            Assert.That(quantity.QuantityInContracts, Is.EqualTo(expectedQuantity));
        }

        [Test]
        public void SharedQuantity_BaseFromQuote_Should_CalculateCorrectly()
        {
            // arrange
            var quoteQuantity = 100m;
            var price = 50m;
            var expectedBase = 2m; // 100 / 50 = 2

            // act
            var quantity = SharedQuantity.BaseFromQuote(quoteQuantity, price);

            // assert
            Assert.That(quantity.QuantityInBaseAsset, Is.EqualTo(expectedBase));
            Assert.That(quantity.QuantityInQuoteAsset, Is.Null);
            Assert.That(quantity.QuantityInContracts, Is.Null);
        }

        [Test]
        public void SharedQuantity_BaseFromQuote_WithCustomDecimals_Should_RoundCorrectly()
        {
            // arrange
            var quoteQuantity = 100m;
            var price = 3m;
            var decimalPlaces = 2;

            // act
            var quantity = SharedQuantity.BaseFromQuote(quoteQuantity, price, decimalPlaces);

            // assert
            // 100 / 3 = 33.333... should round to 33.33
            Assert.That(quantity.QuantityInBaseAsset, Is.EqualTo(33.33m));
        }

        [Test]
        public void SharedQuantity_BaseFromQuote_WithLotSize_Should_AdjustToLotSize()
        {
            // arrange
            var quoteQuantity = 100m;
            var price = 7m;
            var lotSize = 0.1m;

            // act
            var quantity = SharedQuantity.BaseFromQuote(quoteQuantity, price, 8, lotSize);

            // assert
            // 100 / 7 = 14.285714... should adjust to nearest 0.1 = 14.3
            Assert.That(quantity.QuantityInBaseAsset, Is.EqualTo(14.3m));
        }

        [Test]
        public void SharedQuantity_BaseFromQuote_WithHighPrecision_Should_HandleCorrectly()
        {
            // arrange
            var quoteQuantity = 1000m;
            var price = 0.00001m;
            var decimalPlaces = 8;

            // act
            var quantity = SharedQuantity.BaseFromQuote(quoteQuantity, price, decimalPlaces);

            // assert
            Assert.That(quantity.QuantityInBaseAsset, Is.GreaterThan(0));
        }

        [Test]
        public void SharedQuantity_QuoteFromBase_Should_CalculateCorrectly()
        {
            // arrange
            var baseQuantity = 2m;
            var price = 50m;
            var expectedQuote = 100m; // 2 * 50 = 100

            // act
            var quantity = SharedQuantity.QuoteFromBase(baseQuantity, price);

            // assert
            Assert.That(quantity.QuantityInBaseAsset, Is.EqualTo(expectedQuote));
            Assert.That(quantity.QuantityInQuoteAsset, Is.Null);
            Assert.That(quantity.QuantityInContracts, Is.Null);
        }

        [Test]
        public void SharedQuantity_QuoteFromBase_WithCustomDecimals_Should_RoundCorrectly()
        {
            // arrange
            var baseQuantity = 1.234567m;
            var price = 10m;
            var decimalPlaces = 2;

            // act
            var quantity = SharedQuantity.QuoteFromBase(baseQuantity, price, decimalPlaces);

            // assert
            // 1.234567 * 10 = 12.34567 should round to 12.35
            Assert.That(quantity.QuantityInBaseAsset, Is.EqualTo(12.35m));
        }

        [Test]
        public void SharedQuantity_QuoteFromBase_WithLotSize_Should_AdjustToLotSize()
        {
            // arrange
            var baseQuantity = 3.456m;
            var price = 10m;
            var lotSize = 1m;

            // act
            var quantity = SharedQuantity.QuoteFromBase(baseQuantity, price, 8, lotSize);

            // assert
            // 3.456 * 10 = 34.56 should adjust to nearest 1 = 35
            Assert.That(quantity.QuantityInBaseAsset, Is.EqualTo(35m));
        }

        [Test]
        public void SharedQuantity_QuoteFromBase_WithSmallValues_Should_HandleCorrectly()
        {
            // arrange
            var baseQuantity = 0.001m;
            var price = 0.1m;
            var decimalPlaces = 8;

            // act
            var quantity = SharedQuantity.QuoteFromBase(baseQuantity, price, decimalPlaces);

            // assert
            Assert.That(quantity.QuantityInBaseAsset, Is.EqualTo(0.0001m));
        }

        [Test]
        public void SharedQuantity_ContractsFromBase_Should_CalculateCorrectly()
        {
            // arrange
            var baseQuantity = 100m;
            var contractSize = 10m;
            var expectedContracts = 10m; // 100 / 10 = 10

            // act
            var quantity = SharedQuantity.ContractsFromBase(baseQuantity, contractSize);

            // assert
            Assert.That(quantity.QuantityInBaseAsset, Is.EqualTo(expectedContracts));
            Assert.That(quantity.QuantityInQuoteAsset, Is.Null);
            Assert.That(quantity.QuantityInContracts, Is.Null);
        }

        [Test]
        public void SharedQuantity_ContractsFromBase_WithCustomDecimals_Should_RoundCorrectly()
        {
            // arrange
            var baseQuantity = 100m;
            var contractSize = 3m;
            var decimalPlaces = 2;

            // act
            var quantity = SharedQuantity.ContractsFromBase(baseQuantity, contractSize, decimalPlaces);

            // assert
            // 100 / 3 = 33.333... should round to 33.33
            Assert.That(quantity.QuantityInBaseAsset, Is.EqualTo(33.33m));
        }

        [Test]
        public void SharedQuantity_ContractsFromBase_WithLotSize_Should_AdjustToLotSize()
        {
            // arrange
            var baseQuantity = 100m;
            var contractSize = 7m;
            var lotSize = 0.5m;

            // act
            var quantity = SharedQuantity.ContractsFromBase(baseQuantity, contractSize, 8, lotSize);

            // assert
            // 100 / 7 = 14.285714... should adjust to nearest 0.5 = 14.5
            Assert.That(quantity.QuantityInBaseAsset, Is.EqualTo(14.5m));
        }

        [Test]
        public void SharedQuantity_ContractsFromBase_WithFractionalContract_Should_HandleCorrectly()
        {
            // arrange
            var baseQuantity = 1m;
            var contractSize = 0.1m;

            // act
            var quantity = SharedQuantity.ContractsFromBase(baseQuantity, contractSize);

            // assert
            Assert.That(quantity.QuantityInBaseAsset, Is.EqualTo(10m));
        }

        [Test]
        public void SharedQuantity_ContractsFromQuote_Should_CalculateCorrectly()
        {
            // arrange
            var quoteQuantity = 1000m;
            var contractSize = 10m;
            var price = 50m;
            var expectedContracts = 2m; // 1000 / 50 / 10 = 2

            // act
            var quantity = SharedQuantity.ContractsFromQuote(quoteQuantity, contractSize, price);

            // assert
            Assert.That(quantity.QuantityInBaseAsset, Is.EqualTo(expectedContracts));
            Assert.That(quantity.QuantityInQuoteAsset, Is.Null);
            Assert.That(quantity.QuantityInContracts, Is.Null);
        }

        [Test]
        public void SharedQuantity_ContractsFromQuote_WithCustomDecimals_Should_RoundCorrectly()
        {
            // arrange
            var quoteQuantity = 100m;
            var contractSize = 3m;
            var price = 7m;
            var decimalPlaces = 2;

            // act
            var quantity = SharedQuantity.ContractsFromQuote(quoteQuantity, contractSize, price, decimalPlaces);

            // assert
            // 100 / 7 / 3 = 4.761904... should round to 4.76
            Assert.That(quantity.QuantityInBaseAsset, Is.EqualTo(4.76m));
        }

        [Test]
        public void SharedQuantity_ContractsFromQuote_WithLotSize_Should_AdjustToLotSize()
        {
            // arrange
            var quoteQuantity = 1000m;
            var contractSize = 7m;
            var price = 13m;
            var lotSize = 0.5m;

            // act
            var quantity = SharedQuantity.ContractsFromQuote(quoteQuantity, contractSize, price, 8, lotSize);

            // assert
            // 1000 / 13 / 7 = 10.989... should adjust to nearest 0.5 = 11.0
            Assert.That(quantity.QuantityInBaseAsset, Is.EqualTo(11.0m));
        }

        [Test]
        public void SharedQuantity_ContractsFromQuote_WithComplexValues_Should_CalculateCorrectly()
        {
            // arrange
            var quoteQuantity = 5000m;
            var contractSize = 0.01m;
            var price = 25000m;
            var decimalPlaces = 4;

            // act
            var quantity = SharedQuantity.ContractsFromQuote(quoteQuantity, contractSize, price, decimalPlaces);

            // assert
            // 5000 / 25000 / 0.01 = 20
            Assert.That(quantity.QuantityInBaseAsset, Is.EqualTo(20m));
        }

        [Test]
        public void SharedOrderQuantity_DefaultConstructor_Should_SetAllPropertiesToNull()
        {
            // arrange & act
            var quantity = new SharedOrderQuantity();

            // assert
            Assert.That(quantity.QuantityInBaseAsset, Is.Null);
            Assert.That(quantity.QuantityInQuoteAsset, Is.Null);
            Assert.That(quantity.QuantityInContracts, Is.Null);
            Assert.That(quantity.IsZero, Is.True);
        }

        [Test]
        public void SharedOrderQuantity_ParameterizedConstructor_Should_SetBaseAsset()
        {
            // arrange
            var baseAsset = 5m;

            // act
            var quantity = new SharedOrderQuantity(baseAssetQuantity: baseAsset);

            // assert
            Assert.That(quantity.QuantityInBaseAsset, Is.EqualTo(baseAsset));
            Assert.That(quantity.QuantityInQuoteAsset, Is.Null);
            Assert.That(quantity.QuantityInContracts, Is.Null);
        }

        [Test]
        public void SharedOrderQuantity_ParameterizedConstructor_Should_SetQuoteAsset()
        {
            // arrange
            var quoteAsset = 100m;

            // act
            var quantity = new SharedOrderQuantity(quoteAssetQuantity: quoteAsset);

            // assert
            Assert.That(quantity.QuantityInBaseAsset, Is.Null);
            Assert.That(quantity.QuantityInQuoteAsset, Is.EqualTo(quoteAsset));
            Assert.That(quantity.QuantityInContracts, Is.Null);
        }

        [Test]
        public void SharedOrderQuantity_ParameterizedConstructor_Should_SetContracts()
        {
            // arrange
            var contracts = 10m;

            // act
            var quantity = new SharedOrderQuantity(contractQuantity: contracts);

            // assert
            Assert.That(quantity.QuantityInBaseAsset, Is.Null);
            Assert.That(quantity.QuantityInQuoteAsset, Is.Null);
            Assert.That(quantity.QuantityInContracts, Is.EqualTo(contracts));
        }

        [Test]
        public void SharedOrderQuantity_ParameterizedConstructor_Should_SetAllValues()
        {
            // arrange
            var baseAsset = 1m;
            var quoteAsset = 50m;
            var contracts = 5m;

            // act
            var quantity = new SharedOrderQuantity(baseAsset, quoteAsset, contracts);

            // assert
            Assert.That(quantity.QuantityInBaseAsset, Is.EqualTo(baseAsset));
            Assert.That(quantity.QuantityInQuoteAsset, Is.EqualTo(quoteAsset));
            Assert.That(quantity.QuantityInContracts, Is.EqualTo(contracts));
            Assert.That(quantity.IsZero, Is.False);
        }

        [Test]
        public void SharedOrderQuantity_ParameterizedConstructor_WithNullValues_Should_HandleCorrectly()
        {
            // arrange & act
            var quantity = new SharedOrderQuantity(null, null, null);

            // assert
            Assert.That(quantity.QuantityInBaseAsset, Is.Null);
            Assert.That(quantity.QuantityInQuoteAsset, Is.Null);
            Assert.That(quantity.QuantityInContracts, Is.Null);
            Assert.That(quantity.IsZero, Is.True);
        }

        [Test]
        public void SharedQuantity_RecordEquality_SameValues_Should_BeEqual()
        {
            // arrange
            var quantity1 = SharedQuantity.Base(10m);
            var quantity2 = SharedQuantity.Base(10m);

            // act & assert
            Assert.That(quantity1, Is.EqualTo(quantity2));
        }

        [Test]
        public void SharedQuantity_RecordEquality_DifferentValues_Should_NotBeEqual()
        {
            // arrange
            var quantity1 = SharedQuantity.Base(10m);
            var quantity2 = SharedQuantity.Base(20m);

            // act & assert
            Assert.That(quantity1, Is.Not.EqualTo(quantity2));
        }

        [Test]
        public void SharedQuantity_RecordEquality_DifferentTypes_Should_NotBeEqual()
        {
            // arrange
            var quantity1 = SharedQuantity.Base(10m);
            var quantity2 = SharedQuantity.Quote(10m);

            // act & assert
            Assert.That(quantity1, Is.Not.EqualTo(quantity2));
        }

        [Test]
        public void SharedOrderQuantity_RecordEquality_SameValues_Should_BeEqual()
        {
            // arrange
            var quantity1 = new SharedOrderQuantity(5m, 100m, 2m);
            var quantity2 = new SharedOrderQuantity(5m, 100m, 2m);

            // act & assert
            Assert.That(quantity1, Is.EqualTo(quantity2));
        }

        [Test]
        public void SharedQuantity_BaseFromQuote_WithDefaultParameters_Should_UseDefaults()
        {
            // arrange
            var quoteQuantity = 100m;
            var price = 3m;

            // act
            var quantity = SharedQuantity.BaseFromQuote(quoteQuantity, price);

            // assert
            // Default decimalPlaces = 8, default lotSize = 0.00000001
            Assert.That(quantity.QuantityInBaseAsset, Is.Not.Null);
            Assert.That(quantity.QuantityInBaseAsset, Is.GreaterThan(0));
        }

        [Test]
        public void SharedQuantity_QuoteFromBase_WithDefaultParameters_Should_UseDefaults()
        {
            // arrange
            var baseQuantity = 1.234567m;
            var price = 10m;

            // act
            var quantity = SharedQuantity.QuoteFromBase(baseQuantity, price);

            // assert
            Assert.That(quantity.QuantityInBaseAsset, Is.Not.Null);
            Assert.That(quantity.QuantityInBaseAsset, Is.GreaterThan(0));
        }

        [Test]
        public void SharedQuantity_ContractsFromBase_WithDefaultParameters_Should_UseDefaults()
        {
            // arrange
            var baseQuantity = 100m;
            var contractSize = 3m;

            // act
            var quantity = SharedQuantity.ContractsFromBase(baseQuantity, contractSize);

            // assert
            Assert.That(quantity.QuantityInBaseAsset, Is.Not.Null);
            Assert.That(quantity.QuantityInBaseAsset, Is.GreaterThan(0));
        }

        [Test]
        public void SharedQuantity_ContractsFromQuote_WithDefaultParameters_Should_UseDefaults()
        {
            // arrange
            var quoteQuantity = 1000m;
            var contractSize = 10m;
            var price = 50m;

            // act
            var quantity = SharedQuantity.ContractsFromQuote(quoteQuantity, contractSize, price);

            // assert
            Assert.That(quantity.QuantityInBaseAsset, Is.Not.Null);
            Assert.That(quantity.QuantityInBaseAsset, Is.GreaterThan(0));
        }
    }
}