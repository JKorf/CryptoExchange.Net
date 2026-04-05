using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Testing;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.UnitTests.ConverterTests
{
    public class EnumConverterTests
    {
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
            var output = JsonSerializer.Deserialize<STJEnumObject>($"{{ \"Value\": {val} }}", SerializerOptions.WithConverters(new TestSerializerContext()));
            Assert.That(output!.Value == expected);
        }

        [TestCase("1", TestEnum.One)]
        [TestCase("2", TestEnum.Two)]
        [TestCase("3", TestEnum.Three)]
        [TestCase("three", TestEnum.Three)]
        [TestCase("Four", TestEnum.Four)]
        [TestCase("four", TestEnum.Four)]
        [TestCase("Four1", (TestEnum)(-9))]
        [TestCase(null, (TestEnum)(-9))]
        public void TestEnumConverterNotNullableDeserializeTests(string value, TestEnum expected)
        {
            var val = value == null ? "null" : $"\"{value}\"";
            var output = JsonSerializer.Deserialize<NotNullableSTJEnumObject>($"{{ \"Value\": {val} }}");
            Assert.That(output!.Value == expected);
        }

        [Test]
        public void TestEnumConverterMapsUndefinedValueCorrectlyIfDefaultIsDefined()
        {
            var output = JsonSerializer.Deserialize<TestEnum2>($"\"TestUndefined\"");
            Assert.That((int)output == -99);
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

        [Test]
        public void TestEnumConverterParseNullOnNonNullableOnlyLogsOnce()
        {
            LibraryHelpers.StaticLogger = new TraceLogger();
            var listener = new EnumValueTraceListener();
            Trace.Listeners.Add(listener);
            EnumConverter<TestEnum>.Reset();
            try
            {
                Assert.Throws<Exception>(() =>
                {
                    var result = JsonSerializer.Deserialize<NotNullableSTJEnumObject>("{\"Value\": null}", SerializerOptions.WithConverters(new TestSerializerContext()));
                });

                Assert.DoesNotThrow(() =>
                {
                    var result2 = JsonSerializer.Deserialize<NotNullableSTJEnumObject>("{\"Value\": null}", SerializerOptions.WithConverters(new TestSerializerContext()));
                });
            }
            finally
            {
                Trace.Listeners.Remove(listener);
            }
        }
    }
    public class STJEnumObject
    {
        public TestEnum? Value { get; set; }
    }

    public class NotNullableSTJEnumObject
    {
        public TestEnum Value { get; set; }
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

    [JsonConverter(typeof(EnumConverter<TestEnum2>))]
    public enum TestEnum2
    {
        [Map("-9")]
        Minus9 = -9,
        [Map("1")]
        One,
        [Map("2")]
        Two,
        [Map("three", "3")]
        Three,
        Four
    }
}
