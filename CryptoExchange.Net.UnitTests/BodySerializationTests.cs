using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using CryptoExchange.Net;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Converters.SystemTextJson;

namespace CryptoExchange.Net.UnitTests
{
    internal class BodySerializationTests
    {
        [Test]
        public void ToFormData_SerializesBasicValuesCorrectly()
        {
            var parameters = new Dictionary<string, object>()
            {
                { "a", "1" },
                { "b", 2 },
                { "c", true }
            };

            var parameterString = parameters.ToFormData();

            Assert.That(parameterString, Is.EqualTo("a=1&b=2&c=True"));
        }

        [Test]
        public void JsonSerializer_SerializesBasicValuesCorrectly()
        {
            var serializer = new SystemTextJsonMessageSerializer(SerializerOptions.WithConverters(new TestSerializerContext()));
            var parameters = new Dictionary<string, object>()
            {
                { "a", "1" },
                { "b", 2 },
                { "c", true }
            };

            var parameterString = serializer.Serialize(parameters);
            Assert.That(parameterString, Is.EqualTo("{\"a\":\"1\",\"b\":2,\"c\":true}"));
        }
    }
}
