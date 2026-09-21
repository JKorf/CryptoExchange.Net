using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Converters.SystemTextJson;
using NUnit.Framework;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.UnitTests.ConverterTests
{
    public class ArrayConverterTests
    {
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
                TypeInfoResolver = new TestSerializerContext()
            };
            var serialized = JsonSerializer.Serialize(data);
            var deserialized = JsonSerializer.Deserialize<Test>(serialized);

            Assert.That(deserialized!.Prop1, Is.EqualTo(2));
            Assert.That(deserialized.Prop2, Is.Null);
            Assert.That(deserialized.Prop3, Is.EqualTo("123"));
            Assert.That(deserialized.Prop3Again, Is.EqualTo("123"));
            Assert.That(deserialized.Prop4, Is.Null);
            Assert.That(deserialized.Prop5!.Prop21, Is.EqualTo(3));
            Assert.That(deserialized.Prop5!.Prop22, Is.EqualTo("456"));
            Assert.That(deserialized.Prop6!.Prop31, Is.EqualTo(4));
            Assert.That(deserialized.Prop6.Prop32, Is.EqualTo("789"));
            Assert.That(deserialized.Prop7, Is.EqualTo(TestEnum.Two));
            Assert.That(deserialized.TestInternal!.Prop1, Is.EqualTo(10));
            Assert.That(deserialized.Prop8!.Prop31, Is.EqualTo(5));
            Assert.That(deserialized.Prop8.Prop32, Is.EqualTo("101"));
        }

        [TestCase("[\"4.803E-5\",\"9.623e-4\",\"0.0010025\"]", 0.00004803, 0.0009623, 0.0010025)]
        [TestCase("[\"81251.5\",\"1E+2\",\"-2.5E-3\"]", 81251.5, 100, -0.0025)]
        [TestCase("[4.803E-5,9.623e-4,0.0010025]", 0.00004803, 0.0009623, 0.0010025)]
        public void TestArrayConverterDecimalInScientificNotation(string json, decimal expected1, decimal expected2, decimal expected3)
        {
            // Some exchanges send small prices as strings in scientific notation, for example the
            // Kucoin futures kline stream: "candles":["1789830000","4.803E-5","4.807E-5",...]
            var deserialized = JsonSerializer.Deserialize<TestDecimal>(json);

            Assert.That(deserialized!.Prop1, Is.EqualTo(expected1));
            Assert.That(deserialized.Prop2, Is.EqualTo(expected2));
            Assert.That(deserialized.Prop3, Is.EqualTo(expected3));
        }
    }

    [JsonConverter(typeof(ArrayConverter<TestDecimal>))]
    public record TestDecimal
    {
        [ArrayProperty(0)]
        public decimal Prop1 { get; set; }
        [ArrayProperty(1)]
        public decimal? Prop2 { get; set; }
        [ArrayProperty(2)]
        public decimal Prop3 { get; set; }
    }

    [JsonConverter(typeof(ArrayConverter<Test>))]
    public record Test
    {
        [ArrayProperty(0)]
        public int Prop1 { get; set; }
        [ArrayProperty(1)]
        public int? Prop2 { get; set; }
        [ArrayProperty(2)]
        public string? Prop3 { get; set; }
        [ArrayProperty(2)]
        public string? Prop3Again { get; set; }
        [ArrayProperty(3)]
        public string? Prop4 { get; set; }
        [ArrayProperty(4)]
        public Test2? Prop5 { get; set; }
        [ArrayProperty(5)]
        public Test3? Prop6 { get; set; }
        [ArrayProperty(6), JsonConverter(typeof(EnumConverter<TestEnum>))]
        public TestEnum? Prop7 { get; set; }
        [ArrayProperty(7)]
        public Test? TestInternal { get; set; }
        [ArrayProperty(8), JsonConversion]
        public Test3? Prop8 { get; set; }
    }

    [JsonConverter(typeof(ArrayConverter<Test2>))]
    public record Test2
    {
        [ArrayProperty(0)]
        public int Prop21 { get; set; }
        [ArrayProperty(1)]
        public string? Prop22 { get; set; }
    }

    public record Test3
    {
        [JsonPropertyName("prop31")]
        public int Prop31 { get; set; }
        [JsonPropertyName("prop32")]
        public string? Prop32 { get; set; }
    }

}
