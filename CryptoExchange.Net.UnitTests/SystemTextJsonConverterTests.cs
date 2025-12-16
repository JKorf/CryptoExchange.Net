using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Converters.SystemTextJson;
using System.Text.Json;
using NUnit.Framework;
using System;
using System.Text.Json.Serialization;
using CryptoExchange.Net.Converters;
using CryptoExchange.Net.SharedApis;

namespace CryptoExchange.Net.UnitTests
{
    [TestFixture()]
    public class SystemTextJsonConverterTests
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
            Assert.That(output.Time == (expectNull ? null: new DateTime(2021, 05, 12, 0, 0, 0, DateTimeKind.Utc)));
        }

        [TestCase(1620777600.000)]
        [TestCase(1620777600000d)]
        public void TestDateTimeConverterDouble(double input)
        {
            var output = JsonSerializer.Deserialize<STJTimeObject>($"{{ \"time\": {input} }}");
            Assert.That(output.Time == new DateTime(2021, 05, 12, 0, 0, 0, DateTimeKind.Utc));
        }

        [TestCase(1620777600)]
        [TestCase(1620777600000)]
        [TestCase(1620777600000000)]
        [TestCase(1620777600000000000)]
        [TestCase(0, true)]
        public void TestDateTimeConverterLong(long input, bool expectNull = false)
        {
            var output = JsonSerializer.Deserialize<STJTimeObject>($"{{ \"time\": {input} }}");
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
            var output = JsonSerializer.Deserialize<STJTimeObject>($"{{ \"time\": null }}");
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
            var output = JsonSerializer.Deserialize<STJEnumObject>($"{{ \"Value\": {val} }}", SerializerOptions.WithConverters(new SerializationContext()));
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
            var output = JsonSerializer.Deserialize<NotNullableSTJEnumObject>($"{{ \"Value\": {val} }}");
            Assert.That(output.Value == expected);
        }

        [TestCase("1", TestEnum.One)]
        [TestCase("2", TestEnum.Two)]
        [TestCase("3", TestEnum.Three)]
        [TestCase("three", TestEnum.Three)]
        [TestCase("Four", TestEnum.Four)]
        [TestCase("four", TestEnum.Four)]
        [TestCase("Four1", null)]
        [TestCase(null, null)]
        public void TestEnumConverterParseStringTests(string value, TestEnum? expected)
        {
            var result = EnumConverter.ParseString<TestEnum>(value);
            Assert.That(result == expected);
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
            var output = JsonSerializer.Deserialize<STJBoolObject>($"{{ \"Value\": {val} }}", SerializerOptions.WithConverters(new SerializationContext()));
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
            var output = JsonSerializer.Deserialize<NotNullableSTJBoolObject>($"{{ \"Value\": {val} }}", SerializerOptions.WithConverters(new SerializationContext()));
            Assert.That(output.Value == expected);
        }

        [TestCase("1", 1)]
        [TestCase("1.1", 1.1)]
        [TestCase("-1.1", -1.1)]
        [TestCase(null, null)]
        [TestCase("", null)]
        [TestCase("null", null)]
        [TestCase("nan", null)]
        [TestCase("1E+2", 100)]
        [TestCase("1E-2", 0.01)]
        [TestCase("Infinity", 999)] // 999 is workaround for not being able to specify decimal.MinValue
        [TestCase("-Infinity", -999)] // -999 is workaround for not being able to specify decimal.MaxValue
        [TestCase("80228162514264337593543950335", 999)] // 999 is workaround for not being able to specify decimal.MaxValue
        [TestCase("-80228162514264337593543950335", -999)] // -999 is workaround for not being able to specify decimal.MaxValue
        public void TestDecimalConverterString(string value, decimal? expected)
        {
            var result = JsonSerializer.Deserialize<STJDecimalObject>("{ \"test\": \""+ value + "\"}");
            Assert.That(result.Test, Is.EqualTo(expected == -999 ? decimal.MinValue : expected == 999 ? decimal.MaxValue: expected));
        }

        [TestCase("1", 1)]
        [TestCase("1.1", 1.1)]
        [TestCase("-1.1", -1.1)]
        [TestCase("null", null)]
        [TestCase("1E+2", 100)]
        [TestCase("1E-2", 0.01)]
        [TestCase("80228162514264337593543950335", -999)] // -999 is workaround for not being able to specify decimal.MaxValue
        public void TestDecimalConverterNumber(string value, decimal? expected)
        {
            var result = JsonSerializer.Deserialize<STJDecimalObject>("{ \"test\": " + value + "}");
            Assert.That(result.Test, Is.EqualTo(expected == -999 ? decimal.MaxValue : expected));
        }

        [Test()]
        public void TestArrayConverter()
        {
            var data = new Test()
            {
                Prop1 = 2,
                Prop2 = null,
                Prop3 = "123",
                Prop3Again = "123",
                Prop4 = null,
                Prop5 = new Test2
                {
                    Prop21 = 3,
                    Prop22 = "456"
                },
                Prop6 = new Test3
                {
                    Prop31 = 4,
                    Prop32 = "789"
                },
                Prop7 = TestEnum.Two,
                TestInternal = new Test
                {
                    Prop1 = 10
                },
                Prop8 = new Test3
                {
                    Prop31 = 5,
                    Prop32 = "101"
                },
            };

            var options = new JsonSerializerOptions()
            {
                TypeInfoResolver = new SerializationContext()
            };
            var serialized = JsonSerializer.Serialize(data);
            var deserialized = JsonSerializer.Deserialize<Test>(serialized);

            Assert.That(deserialized.Prop1, Is.EqualTo(2));
            Assert.That(deserialized.Prop2, Is.Null);
            Assert.That(deserialized.Prop3, Is.EqualTo("123"));
            Assert.That(deserialized.Prop3Again, Is.EqualTo("123"));
            Assert.That(deserialized.Prop4, Is.Null);
            Assert.That(deserialized.Prop5.Prop21, Is.EqualTo(3));
            Assert.That(deserialized.Prop5.Prop22, Is.EqualTo("456"));
            Assert.That(deserialized.Prop6.Prop31, Is.EqualTo(4));
            Assert.That(deserialized.Prop6.Prop32, Is.EqualTo("789"));
            Assert.That(deserialized.Prop7, Is.EqualTo(TestEnum.Two));
            Assert.That(deserialized.TestInternal.Prop1, Is.EqualTo(10));
            Assert.That(deserialized.Prop8.Prop31, Is.EqualTo(5));
            Assert.That(deserialized.Prop8.Prop32, Is.EqualTo("101"));
        }

        [TestCase(TradingMode.Spot, "ETH", "USDT", null)]
        [TestCase(TradingMode.PerpetualLinear, "ETH", "USDT", null)]
        [TestCase(TradingMode.DeliveryLinear, "ETH", "USDT", 1748432430)]
        public void TestSharedSymbolConversion(TradingMode tradingMode, string baseAsset, string quoteAsset, int? deliverTime)
        {
            DateTime? time = deliverTime == null ? null : DateTimeConverter.ParseFromDouble(deliverTime.Value);
            var symbol = new SharedSymbol(tradingMode, baseAsset, quoteAsset, time);

            var serialized = JsonSerializer.Serialize(symbol);
            var restored = JsonSerializer.Deserialize<SharedSymbol>(serialized);

            Assert.That(restored.TradingMode, Is.EqualTo(symbol.TradingMode));
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

            Assert.That(restored.QuantityInBaseAsset, Is.EqualTo(symbol.QuantityInBaseAsset));
            Assert.That(restored.QuantityInQuoteAsset, Is.EqualTo(symbol.QuantityInQuoteAsset));
            Assert.That(restored.QuantityInContracts, Is.EqualTo(symbol.QuantityInContracts));
        }
    }

    public class STJDecimalObject
    {
        [JsonConverter(typeof(DecimalConverter))]
        [JsonPropertyName("test")]
        public decimal? Test { get; set; }
    }

    public class STJTimeObject
    {
        [JsonConverter(typeof(DateTimeConverter))]
        [JsonPropertyName("time")]
        public DateTime? Time { get; set; }
    }

    public class STJEnumObject
    {
        public TestEnum? Value { get; set; }
    }

    public class NotNullableSTJEnumObject
    {
        public TestEnum Value { get; set; }
    }

    public class STJBoolObject
    {
        public bool? Value { get; set; }
    }

    public class NotNullableSTJBoolObject
    {
        public bool Value { get; set; }
    }

    [JsonConverter(typeof(ArrayConverter<Test>))]
    record Test
    {
        [ArrayProperty(0)]
        public int Prop1 { get; set; }
        [ArrayProperty(1)]
        public int? Prop2 { get; set; }
        [ArrayProperty(2)]
        public string Prop3 { get; set; }
        [ArrayProperty(2)]
        public string Prop3Again { get; set; }
        [ArrayProperty(3)]
        public string Prop4 { get; set; }
        [ArrayProperty(4)]
        public Test2 Prop5 { get; set; }
        [ArrayProperty(5)]
        public Test3 Prop6 { get; set; }
        [ArrayProperty(6), JsonConverter(typeof(EnumConverter<TestEnum>))]
        public TestEnum? Prop7 { get; set; }
        [ArrayProperty(7)]
        public Test TestInternal { get; set; }
        [ArrayProperty(8), JsonConversion]
        public Test3 Prop8 { get; set; }
    }

    [JsonConverter(typeof(ArrayConverter<Test2>))]
    record Test2
    {
        [ArrayProperty(0)]
        public int Prop21 { get; set; }
        [ArrayProperty(1)]
        public string Prop22 { get; set; }
    }

    record Test3
    {
        [JsonPropertyName("prop31")]
        public int Prop31 { get; set; }
        [JsonPropertyName("prop32")]
        public string Prop32 { get; set; }
    }

    [JsonConverter(typeof(EnumConverter<TestEnum>))]
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

    [JsonSerializable(typeof(Test))]
    [JsonSerializable(typeof(Test2))]
    [JsonSerializable(typeof(Test3))]
    [JsonSerializable(typeof(NotNullableSTJBoolObject))]
    [JsonSerializable(typeof(STJBoolObject))]
    [JsonSerializable(typeof(NotNullableSTJEnumObject))]
    [JsonSerializable(typeof(STJEnumObject))]
    [JsonSerializable(typeof(STJDecimalObject))]
    [JsonSerializable(typeof(STJTimeObject))]
    internal partial class SerializationContext : JsonSerializerContext
    {
    }
}
