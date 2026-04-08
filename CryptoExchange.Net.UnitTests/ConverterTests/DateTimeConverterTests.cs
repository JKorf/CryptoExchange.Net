using CryptoExchange.Net.Converters.SystemTextJson;
using NUnit.Framework;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.UnitTests.ConverterTests
{
    public class DateTimeConverterTests
    {
        [TestCase("2021-05-12")]
        [TestCase("20210512")]
        [TestCase("210512")]
        [TestCase("1620777600.000")]
        [TestCase("1620777600000")]
        [TestCase("2021-05-12T00:00:00.000Z")]
        [TestCase("2021-05-12T00:00:00.000000000Z")]
        [TestCase("0.000000", true)]
        [TestCase("0", true)]
        [TestCase("", true)]
        [TestCase("  ", true)]
        public void TestDateTimeConverterString(string input, bool expectNull = false)
        {
            var output = JsonSerializer.Deserialize<STJTimeObject>($"{{ \"time\": \"{input}\" }}");
            Assert.That(output!.Time == (expectNull ? null : new DateTime(2021, 05, 12, 0, 0, 0, DateTimeKind.Utc)));
        }

        [TestCase(1620777600.000)]
        [TestCase(1620777600000d)]
        public void TestDateTimeConverterDouble(double input)
        {
            var output = JsonSerializer.Deserialize<STJTimeObject>($"{{ \"time\": {input} }}");
            Assert.That(output!.Time == new DateTime(2021, 05, 12, 0, 0, 0, DateTimeKind.Utc));
        }

        [TestCase(1620777600)]
        [TestCase(1620777600000)]
        [TestCase(1620777600000000)]
        [TestCase(1620777600000000000)]
        [TestCase(0, true)]
        public void TestDateTimeConverterLong(long input, bool expectNull = false)
        {
            var output = JsonSerializer.Deserialize<STJTimeObject>($"{{ \"time\": {input} }}");
            Assert.That(output!.Time == (expectNull ? null : new DateTime(2021, 05, 12, 0, 0, 0, DateTimeKind.Utc)));
        }

        [TestCase(1620777600)]
        [TestCase(1620777600.000)]
        public void TestDateTimeConverterFromSeconds(double input)
        {
            var output = DateTimeConverter.ConvertFromSeconds(input);
            Assert.That(output == new DateTime(2021, 05, 12, 0, 0, 0, DateTimeKind.Utc));
        }

        [Test]
        public void TestDateTimeConverterToSeconds()
        {
            var output = DateTimeConverter.ConvertToSeconds(new DateTime(2021, 05, 12, 0, 0, 0, DateTimeKind.Utc));
            Assert.That(output == 1620777600);
        }

        [TestCase(1620777600000)]
        [TestCase(1620777600000.000)]
        public void TestDateTimeConverterFromMilliseconds(double input)
        {
            var output = DateTimeConverter.ConvertFromMilliseconds(input);
            Assert.That(output == new DateTime(2021, 05, 12, 0, 0, 0, DateTimeKind.Utc));
        }

        [Test]
        public void TestDateTimeConverterToMilliseconds()
        {
            var output = DateTimeConverter.ConvertToMilliseconds(new DateTime(2021, 05, 12, 0, 0, 0, DateTimeKind.Utc));
            Assert.That(output == 1620777600000);
        }

        [TestCase(1620777600000000)]
        public void TestDateTimeConverterFromMicroseconds(long input)
        {
            var output = DateTimeConverter.ConvertFromMicroseconds(input);
            Assert.That(output == new DateTime(2021, 05, 12, 0, 0, 0, DateTimeKind.Utc));
        }

        [Test]
        public void TestDateTimeConverterToMicroseconds()
        {
            var output = DateTimeConverter.ConvertToMicroseconds(new DateTime(2021, 05, 12, 0, 0, 0, DateTimeKind.Utc));
            Assert.That(output == 1620777600000000);
        }

        [TestCase(1620777600000000000)]
        public void TestDateTimeConverterFromNanoseconds(long input)
        {
            var output = DateTimeConverter.ConvertFromNanoseconds(input);
            Assert.That(output == new DateTime(2021, 05, 12, 0, 0, 0, DateTimeKind.Utc));
        }

        [Test]
        public void TestDateTimeConverterToNanoseconds()
        {
            var output = DateTimeConverter.ConvertToNanoseconds(new DateTime(2021, 05, 12, 0, 0, 0, DateTimeKind.Utc));
            Assert.That(output == 1620777600000000000);
        }

        [TestCase()]
        public void TestDateTimeConverterNull()
        {
            var output = JsonSerializer.Deserialize<STJTimeObject>($"{{ \"time\": null }}");
            Assert.That(output!.Time == null);
        }
    }

    public class STJTimeObject
    {
        [JsonConverter(typeof(DateTimeConverter))]
        [JsonPropertyName("time")]
        public DateTime? Time { get; set; }
    }
}
