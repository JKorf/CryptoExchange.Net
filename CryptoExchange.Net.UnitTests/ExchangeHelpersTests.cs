using CryptoExchange.Net.Objects;
using NUnit.Framework;
using System.Globalization;

namespace CryptoExchange.Net.UnitTests
{
    [TestFixture()]
    public class ExchangeHelpersTests
    {
        [TestCase("0.1", "1", "0.4", "0.4")]
        [TestCase("0.1", "1", "0.1", "0.1")]
        [TestCase("0.1", "1", "1", "1")]
        [TestCase("0.1", "1", "0.99", "0.99")]
        [TestCase("0.1", "1", "0.09", "0.1")]
        [TestCase("0.1", "1", "1.01", "1")]
        public void ClampValueTests(string min, string max, string input, string expected)
        {
            var result = ExchangeHelpers.ClampValue(
                decimal.Parse(min, CultureInfo.InvariantCulture),
                decimal.Parse(max, CultureInfo.InvariantCulture),
                decimal.Parse(input, CultureInfo.InvariantCulture));
            Assert.That(expected == result.ToString(CultureInfo.InvariantCulture));
        }

        [TestCase("0.1", "1", "0.1", RoundingType.Down, "0.4", "0.4")]
        [TestCase("0.1", "1", "0.1", RoundingType.Down, "0.1", "0.1")]
        [TestCase("0.1", "1", "0.1", RoundingType.Down, "1", "1")]
        [TestCase("0.1", "1", "0.1", RoundingType.Down, "0.99", "0.9")]
        [TestCase("0.1", "1", "0.1", RoundingType.Down, "0.09", "0.1")]
        [TestCase("0.1", "1", "0.1", RoundingType.Down, "1.01", "1")]
        [TestCase("0.1", "1", "0.01", RoundingType.Down, "0.532", "0.53")]
        [TestCase("0.1", "1", "0.0001", RoundingType.Down, "0.532", "0.532")]
        [TestCase("0.1", "1", "0.0001", RoundingType.Closest, "0.532", "0.532")]
        [TestCase("0.1", "1", "0.0001", RoundingType.Down, "0.5516592", "0.5516")]
        [TestCase("0.1", "1", "0.0001", RoundingType.Closest, "0.5516592", "0.5517")]
        [TestCase("0", "1", "0.000000001", RoundingType.Closest, "0.0000097232", "0.000009723")]
        public void AdjustValueStepTests(string min, string max, string? step, RoundingType roundingType, string input, string expected)
        {
            var result = ExchangeHelpers.AdjustValueStep(
                decimal.Parse(min, CultureInfo.InvariantCulture),
                decimal.Parse(max, CultureInfo.InvariantCulture),
                step == null ? (decimal?)null : decimal.Parse(step, CultureInfo.InvariantCulture),
                roundingType,
                decimal.Parse(input, CultureInfo.InvariantCulture));
            Assert.That(expected == result.ToString(CultureInfo.InvariantCulture));
        }

        [TestCase("0.1", "1", 2, RoundingType.Closest, "0.4", "0.4")]
        [TestCase("0.1", "1", 2, RoundingType.Closest, "0.1", "0.1")]
        [TestCase("0.1", "1", 2, RoundingType.Closest, "1", "1")]
        [TestCase("0.1", "1", 2, RoundingType.Down, "0.555", "0.55")]
        [TestCase("0.1", "1", 2, RoundingType.Closest, "0.555", "0.56")]
        [TestCase("0", "100", 5, RoundingType.Closest, "23.125987", "23.126")]
        [TestCase("0", "100", 5, RoundingType.Down, "23.125987", "23.125")]
        [TestCase("0", "100", 8, RoundingType.Down, "0.145647985948", "0.14564798")]
        [TestCase("0", "100", 8, RoundingType.Closest, "0.145647985948", "0.14564799")]
        public void AdjustValuePrecisionTests(string min, string max, int? precision, RoundingType roundingType, string input, string expected)
        {
            var result = ExchangeHelpers.AdjustValuePrecision(
                decimal.Parse(min, CultureInfo.InvariantCulture),
                decimal.Parse(max, CultureInfo.InvariantCulture),
                precision,
                roundingType,
                decimal.Parse(input, CultureInfo.InvariantCulture));
            Assert.That(result, Is.EqualTo(decimal.Parse(expected, CultureInfo.InvariantCulture)));
        }

        [TestCase(5, "0.1563158", "0.15631")]
        [TestCase(5, "12.1789258", "12.17892")]
        [TestCase(2, "12.1789258", "12.17")]
        [TestCase(8, "156146.1247", "156146.1247")]
        [TestCase(8, "50", "50")]
        public void RoundDownTests(int decimalPlaces, string input, string expected)
        {
            var result = ExchangeHelpers.RoundDown(decimal.Parse(input, CultureInfo.InvariantCulture), decimalPlaces);
            Assert.That(result, Is.EqualTo(decimal.Parse(expected, CultureInfo.InvariantCulture)));
        }

        [TestCase("0.1234560000", "0.123456")]
        [TestCase("794.1230130600", "794.12301306")]
        public void NormalizeTests(string input, string expected)
        {
            var result = ExchangeHelpers.Normalize(decimal.Parse(input, CultureInfo.InvariantCulture));
            Assert.That(expected == result.ToString(CultureInfo.InvariantCulture));
        }

        [Test]
        [TestCase("123", "BKR", 32, true, "BKRJK123")]
        [TestCase("123", "BKR", 32, false, "123")]
        [TestCase("123123123123123123123123123123", "BKR", 32, true, "123123123123123123123123123123")] // 30
        [TestCase("12312312312312312312312312312", "BKR", 32, true, "12312312312312312312312312312")] // 27
        [TestCase("123123123123123123123123123", "BKR", 32, true, "BKRJK123123123123123123123123123")] // 25
        [TestCase(null, "BKR", 32, true, null)]
        public void ApplyBrokerIdTests(string clientOrderId, string brokerId, int maxLength, bool allowValueAdjustement, string expected)
        {
            var result = LibraryHelpers.ApplyBrokerId(clientOrderId, brokerId, maxLength, allowValueAdjustement);
            
            if (expected != null)
                Assert.That(result, Is.EqualTo(expected));
        }
    }
}
