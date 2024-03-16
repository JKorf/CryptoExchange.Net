using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Converters.JsonNet;
using Newtonsoft.Json;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchange.Net.UnitTests
{
    [TestFixture()]
    public class JsonNetConverterTests
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
            var output = JsonConvert.DeserializeObject<TimeObject>($"{{ \"time\": \"{input}\" }}");
            Assert.That(output.Time == (expectNull ? null: new DateTime(2021, 05, 12, 0, 0, 0, DateTimeKind.Utc)));
        }

        [TestCase(1620777600.000)]
        [TestCase(1620777600000d)]
        public void TestDateTimeConverterDouble(double input)
        {
            var output = JsonConvert.DeserializeObject<TimeObject>($"{{ \"time\": {input} }}");
            Assert.That(output.Time == new DateTime(2021, 05, 12, 0, 0, 0, DateTimeKind.Utc));
        }

        [TestCase(1620777600)]
        [TestCase(1620777600000)]
        [TestCase(1620777600000000)]
        [TestCase(1620777600000000000)]
        [TestCase(0, true)]
        public void TestDateTimeConverterLong(long input, bool expectNull = false)
        {
            var output = JsonConvert.DeserializeObject<TimeObject>($"{{ \"time\": {input} }}");
            Assert.That(output.Time == (expectNull ? null : new DateTime(2021, 05, 12, 0, 0, 0, DateTimeKind.Utc)));
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
            var output = JsonConvert.DeserializeObject<TimeObject>($"{{ \"time\": null }}");
            Assert.That(output.Time == null);
        }

        [TestCase(TestEnum.One, "1")]
        [TestCase(TestEnum.Two, "2")]
        [TestCase(TestEnum.Three, "three")]
        [TestCase(TestEnum.Four, "Four")]
        [TestCase(null, null)]
        public void TestEnumConverterNullableGetStringTests(TestEnum? value, string expected)
        {
            var output = EnumConverter.GetString(value);
            Assert.That(output == expected);
        }

        [TestCase(TestEnum.One, "1")]
        [TestCase(TestEnum.Two, "2")]
        [TestCase(TestEnum.Three, "three")]
        [TestCase(TestEnum.Four, "Four")]
        public void TestEnumConverterGetStringTests(TestEnum value, string expected)
        {
            var output = EnumConverter.GetString(value);
            Assert.That(output == expected);
        }

        [TestCase("1", TestEnum.One)]
        [TestCase("2", TestEnum.Two)]
        [TestCase("3", TestEnum.Three)]
        [TestCase("three", TestEnum.Three)]
        [TestCase("Four", TestEnum.Four)]
        [TestCase("four", TestEnum.Four)]
        [TestCase("Four1", null)]
        [TestCase(null, null)]
        public void TestEnumConverterNullableDeserializeTests(string value, TestEnum? expected)
        {
            var val = value == null ? "null" : $"\"{value}\"";
            var output = JsonConvert.DeserializeObject<EnumObject>($"{{ \"Value\": {val} }}");
            Assert.That(output.Value == expected);
        }

        [TestCase("1", TestEnum.One)]
        [TestCase("2", TestEnum.Two)]
        [TestCase("3", TestEnum.Three)]
        [TestCase("three", TestEnum.Three)]
        [TestCase("Four", TestEnum.Four)]
        [TestCase("four", TestEnum.Four)]
        [TestCase("Four1", TestEnum.One)]
        [TestCase(null, TestEnum.One)]
        public void TestEnumConverterNotNullableDeserializeTests(string value, TestEnum? expected)
        {
            var val = value == null ? "null" : $"\"{value}\"";
            var output = JsonConvert.DeserializeObject<NotNullableEnumObject>($"{{ \"Value\": {val} }}");
            Assert.That(output.Value == expected);
        }

        [TestCase("1", true)]
        [TestCase("true", true)]
        [TestCase("yes", true)]
        [TestCase("y", true)]
        [TestCase("on", true)]
        [TestCase("-1", false)]
        [TestCase("0", false)]
        [TestCase("n", false)]
        [TestCase("no", false)]
        [TestCase("false", false)]
        [TestCase("off", false)]
        [TestCase("", null)]
        public void TestBoolConverter(string value, bool? expected)
        {
            var val = value == null ? "null" : $"\"{value}\"";
            var output = JsonConvert.DeserializeObject<BoolObject>($"{{ \"Value\": {val} }}");
            Assert.That(output.Value == expected);
        }

        [TestCase("1", true)]
        [TestCase("true", true)]
        [TestCase("yes", true)]
        [TestCase("y", true)]
        [TestCase("on", true)]
        [TestCase("-1", false)]
        [TestCase("0", false)]
        [TestCase("n", false)]
        [TestCase("no", false)]
        [TestCase("false", false)]
        [TestCase("off", false)]
        [TestCase("", false)]
        public void TestBoolConverterNotNullable(string value, bool expected)
        {
            var val = value == null ? "null" : $"\"{value}\"";
            var output = JsonConvert.DeserializeObject<NotNullableBoolObject>($"{{ \"Value\": {val} }}");
            Assert.That(output.Value == expected);
        }
    }

    public class TimeObject
    {
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime? Time { get; set; }
    }

    public class EnumObject
    {
        public TestEnum? Value { get; set; }
    }

    public class NotNullableEnumObject
    {
        public TestEnum Value { get; set; }
    }

    public class BoolObject
    {
        [JsonConverter(typeof(BoolConverter))]
        public bool? Value { get; set; }
    }

    public class NotNullableBoolObject
    {
        [JsonConverter(typeof(BoolConverter))]
        public bool Value { get; set; }
    }

    [JsonConverter(typeof(EnumConverter))]
    public enum TestEnum
    {
        [Map("1")]
        One,
        [Map("2")]
        Two,
        [Map("three", "3")]
        Three,
        Four
    }
}
