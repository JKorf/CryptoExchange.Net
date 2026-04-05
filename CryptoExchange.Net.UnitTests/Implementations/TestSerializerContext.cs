using CryptoExchange.Net.UnitTests.ConverterTests;
using CryptoExchange.Net.UnitTests.Implementations;
using CryptoExchange.Net.UnitTests.TestImplementations;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.UnitTests
{
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(int))]
    [JsonSerializable(typeof(Dictionary<string, string>))]
    [JsonSerializable(typeof(IDictionary<string, string>))]
    [JsonSerializable(typeof(Dictionary<string, object>))]
    [JsonSerializable(typeof(IDictionary<string, object>))]
    [JsonSerializable(typeof(TestObject))]

    [JsonSerializable(typeof(TestSocketMessage))]
    [JsonSerializable(typeof(Test))]
    [JsonSerializable(typeof(Test2))]
    [JsonSerializable(typeof(Test3))]
    [JsonSerializable(typeof(NotNullableSTJBoolObject))]
    [JsonSerializable(typeof(STJBoolObject))]
    [JsonSerializable(typeof(NotNullableSTJEnumObject))]
    [JsonSerializable(typeof(STJEnumObject))]
    [JsonSerializable(typeof(STJDecimalObject))]
    [JsonSerializable(typeof(STJTimeObject))]
    internal partial class TestSerializerContext : JsonSerializerContext
    {
    }
}
