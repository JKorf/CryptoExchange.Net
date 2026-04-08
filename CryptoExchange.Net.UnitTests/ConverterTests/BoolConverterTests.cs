using CryptoExchange.Net.Converters.SystemTextJson;
using NUnit.Framework;
using System.Text.Json;

namespace CryptoExchange.Net.UnitTests.ConverterTests
{
    public class BoolConverterTests
    {
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
            var output = JsonSerializer.Deserialize<STJBoolObject>($"{{ \"Value\": {val} }}", SerializerOptions.WithConverters(new TestSerializerContext()));
            Assert.That(output!.Value == expected);
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
            var output = JsonSerializer.Deserialize<NotNullableSTJBoolObject>($"{{ \"Value\": {val} }}", SerializerOptions.WithConverters(new TestSerializerContext()));
            Assert.That(output!.Value == expected);
        }
    }

    public class STJBoolObject
    {
        public bool? Value { get; set; }
    }

    public class NotNullableSTJBoolObject
    {
        public bool Value { get; set; }
    }
}
