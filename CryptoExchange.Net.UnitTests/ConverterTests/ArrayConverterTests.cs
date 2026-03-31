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
    }

    [JsonConverter(typeof(ArrayConverter<Test>))]
    public record Test
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
    public record Test2
    {
        [ArrayProperty(0)]
        public int Prop21 { get; set; }
        [ArrayProperty(1)]
        public string Prop22 { get; set; }
    }

    public record Test3
    {
        [JsonPropertyName("prop31")]
        public int Prop31 { get; set; }
        [JsonPropertyName("prop32")]
        public string Prop32 { get; set; }
    }

}
