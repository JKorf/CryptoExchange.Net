using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using CryptoExchange.Net;
using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.UnitTests
{
    internal class UriSerializationTests
    {
        [Test]
        public void CreateParamString_SerializesBasicValuesCorrectly()
        {
            var parameters = new Dictionary<string, object>()
            {
                { "a", "1" },
                { "b", 2 },
                { "c", true }
            };

            var parameterString = parameters.CreateParamString(false, ArrayParametersSerialization.Array);

            Assert.That(parameterString, Is.EqualTo("a=1&b=2&c=True"));
        }

        [Test]
        public void CreateParamString_SerializesArrayValuesCorrectly()
        {
            var parameters = new Dictionary<string, object>()
            {
                { "a", new [] { "1", "2" } },
            };

            var parameterString = parameters.CreateParamString(false, ArrayParametersSerialization.Array);

            Assert.That(parameterString, Is.EqualTo("a[]=1&a[]=2"));
        }

        [Test]
        public void CreateParamStringEncoded_SerializesArrayValuesCorrectly()
        {
            var parameters = new Dictionary<string, object>()
            {
                { "a", new [] { "1+2", "2+3" } },
            };

            var parameterString = parameters.CreateParamString(true, ArrayParametersSerialization.Array);

            Assert.That(parameterString, Is.EqualTo("a[]=1%2B2&a[]=2%2B3"));
        }

        [Test]
        public void CreateParamString_SerializesJsonArrayValuesCorrectly()
        {
            var parameters = new Dictionary<string, object>()
            {
                { "a", new [] { "1", "2" } },
            };

            var parameterString = parameters.CreateParamString(false, ArrayParametersSerialization.JsonArray);

            Assert.That(parameterString, Is.EqualTo("a=[1,2]"));
        }

        [Test]
        public void CreateParamStringEncoded_SerializesJsonArrayValuesCorrectly()
        {
            var parameters = new Dictionary<string, object>()
            {
                { "a", new [] { "1+2", "2+3" } },
            };

            var parameterString = parameters.CreateParamString(true, ArrayParametersSerialization.JsonArray);

            Assert.That(parameterString, Is.EqualTo("a=[1%2B2,2%2B3]"));
        }

        [Test]
        public void CreateParamString_SerializesMultipleValuesArrayCorrectly()
        {
            var parameters = new Dictionary<string, object>()
            {
                { "a", new [] { "1", "2" } },
            };

            var parameterString = parameters.CreateParamString(false, ArrayParametersSerialization.MultipleValues);

            Assert.That(parameterString, Is.EqualTo("a=1&a=2"));
        }

        [Test]
        public void CreateParamStringEncoded_SerializesMultipleValuesArrayCorrectly()
        {
            var parameters = new Dictionary<string, object>()
            {
                { "a", new [] { "1+2", "2+3" } },
            };

            var parameterString = parameters.CreateParamString(true, ArrayParametersSerialization.MultipleValues);

            Assert.That(parameterString, Is.EqualTo("a=1%2B2&a=2%2B3"));
        }
    }
}
