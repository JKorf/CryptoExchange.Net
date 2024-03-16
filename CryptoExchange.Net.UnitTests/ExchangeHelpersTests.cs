using CryptoExchange.Net.Objects;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System.Globalization;

namespace CryptoExchange.Net.UnitTests
{
    [TestFixture()]
    public class ExchangeHelpersTests
    {
        [TestCase(0.1, 1, 0.4, 0.4)]
        [TestCase(0.1, 1, 0.1, 0.1)]
        [TestCase(0.1, 1, 1, 1)]
        [TestCase(0.1, 1, 0.99, 0.99)]
        [TestCase(0.1, 1, 0.09, 0.1)]
        [TestCase(0.1, 1, 1.01, 1)]
        public void ClampValueTests(decimal min, decimal max, decimal input, decimal expected)
        {
            var result = ExchangeHelpers.ClampValue(min, max, input);
            Assert.That(expected == result);
        }

        [TestCase(0.1, 1, 0.1, RoundingType.Down, 0.4, 0.4)]
        [TestCase(0.1, 1, 0.1, RoundingType.Down, 0.1, 0.1)]
        [TestCase(0.1, 1, 0.1, RoundingType.Down, 1, 1)]
        [TestCase(0.1, 1, 0.1, RoundingType.Down, 0.99, 0.9)]
        [TestCase(0.1, 1, 0.1, RoundingType.Down, 0.09, 0.1)]
        [TestCase(0.1, 1, 0.1, RoundingType.Down, 1.01, 1)]
        [TestCase(0.1, 1, 0.01, RoundingType.Down, 0.532, 0.53)]
        [TestCase(0.1, 1, 0.0001, RoundingType.Down, 0.532, 0.532)]
        [TestCase(0.1, 1, 0.0001, RoundingType.Closest, 0.532, 0.532)]
        [TestCase(0.1, 1, 0.0001, RoundingType.Down, 0.5516592, 0.5516)]
        [TestCase(0.1, 1, 0.0001, RoundingType.Closest, 0.5516592, 0.5517)]
        public void AdjustValueStepTests(decimal min, decimal max, decimal? step, RoundingType roundingType, decimal input, decimal expected)
        {
            var result = ExchangeHelpers.AdjustValueStep(min, max, step, roundingType, input);
            Assert.That(expected == result);
        }

        [TestCase(0.1, 1, 2, RoundingType.Closest, 0.4, 0.4)]
        [TestCase(0.1, 1, 2, RoundingType.Closest, 0.1, 0.1)]
        [TestCase(0.1, 1, 2, RoundingType.Closest, 1, 1)]
        [TestCase(0.1, 1, 2, RoundingType.Down, 0.555, 0.55)]
        [TestCase(0.1, 1, 2, RoundingType.Closest, 0.555, 0.56)]
        [TestCase(0, 100, 5, RoundingType.Closest, 23.125987, 23.126)]
        [TestCase(0, 100, 5, RoundingType.Down, 23.125987, 23.125)]
        [TestCase(0, 100, 8, RoundingType.Down, 0.145647985948, 0.14564798)]
        [TestCase(0, 100, 8, RoundingType.Closest, 0.145647985948, 0.14564799)]
        public void AdjustValuePrecisionTests(decimal min, decimal max, int? precision, RoundingType roundingType, decimal input, decimal expected)
        {
            var result = ExchangeHelpers.AdjustValuePrecision(min, max, precision, roundingType, input);
            Assert.That(expected == result);
        }

        [TestCase(5, 0.1563158, 0.15631)]
        [TestCase(5, 12.1789258, 12.17892)]
        [TestCase(2, 12.1789258, 12.17)]
        [TestCase(8, 156146.1247, 156146.1247)]
        [TestCase(8, 50, 50)]
        public void RoundDownTests(int decimalPlaces, decimal input, decimal expected)
        {
            var result = ExchangeHelpers.RoundDown(input, decimalPlaces);
            Assert.That(expected == result);
        }

        [TestCase(0.1234560000, "0.123456")]
        [TestCase(794.1230130600, "794.12301306")]
        public void NormalizeTests(decimal input, string expected)
        {
            var result = ExchangeHelpers.Normalize(input);
            Assert.That(expected == result.ToString(CultureInfo.InvariantCulture));
        }
    }
}
