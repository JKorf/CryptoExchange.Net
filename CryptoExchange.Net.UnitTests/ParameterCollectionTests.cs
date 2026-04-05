using CryptoExchange.Net.Objects;
using CryptoExchange.Net.UnitTests.ConverterTests;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.UnitTests
{
    internal class ParameterCollectionTests
    {
        [Test]
        public void AddingBasicValue_SetValueCorrectly()
        {
            var parameters = new ParameterCollection();
            parameters.Add("test", "value");
            Assert.That(parameters["test"], Is.EqualTo("value"));
        }

        [Test]
        public void AddingBasicNullValue_ThrowExecption()
        {
            var parameters = new ParameterCollection();
            Assert.Throws<ArgumentNullException>(() =>  parameters.Add("test", null!));
        }

        [Test]
        public void AddingOptionalBasicValue_SetValueCorrectly()
        {
            var parameters = new ParameterCollection();
            parameters.AddOptional("test", "value");
            Assert.That(parameters["test"], Is.EqualTo("value"));
        }

        [Test]
        public void AddingOptionalBasicNullValue_DoesntSetValue()
        {
            var parameters = new ParameterCollection();
            parameters.AddOptional("test", null);
            Assert.That(parameters.ContainsKey("test"), Is.False);
        }

        [Test]
        public void AddingDecimalValueAsString_SetValueCorrectly()
        {
            var parameters = new ParameterCollection();
            parameters.AddString("test", 0.1m);
            Assert.That(parameters["test"], Is.EqualTo("0.1"));
        }

        [Test]
        public void AddingOptionalDecimalValueAsString_SetValueCorrectly()
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalString("test", 0.1m);
            Assert.That(parameters["test"], Is.EqualTo("0.1"));
        }

        [Test]
        public void AddingOptionalDecimalNullValueAsString_DoesntSetValue()
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalString("test", (decimal?)null);
            Assert.That(parameters.ContainsKey("test"), Is.False);
        }

        [Test]
        public void AddingIntValueAsString_SetValueCorrectly()
        {
            var parameters = new ParameterCollection();
            parameters.AddString("test", 1);
            Assert.That(parameters["test"], Is.EqualTo("1"));
        }

        [Test]
        public void AddingOptionalIntValueAsString_SetValueCorrectly()
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalString("test", 1);
            Assert.That(parameters["test"], Is.EqualTo("1"));
        }

        [Test]
        public void AddingOptionalIntNullValueAsString_DoesntSetValue()
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalString("test", (int?)null);
            Assert.That(parameters.ContainsKey("test"), Is.False);
        }

        [Test]
        public void AddingLongValueAsString_SetValueCorrectly()
        {
            var parameters = new ParameterCollection();
            parameters.AddString("test", 1L);
            Assert.That(parameters["test"], Is.EqualTo("1"));
        }

        [Test]
        public void AddingOptionalLongValueAsString_SetValueCorrectly()
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalString("test", 1L);
            Assert.That(parameters["test"], Is.EqualTo("1"));
        }

        [Test]
        public void AddingOptionalLongNullValueAsString_DoesntSetValue()
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalString("test", (long?)null);
            Assert.That(parameters.ContainsKey("test"), Is.False);
        }

        [Test]
        public void AddingMillisecondTimestamp_SetValueCorrectly()
        {
            var parameters = new ParameterCollection();
            parameters.AddMilliseconds("test", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.That(parameters["test"], Is.EqualTo(1735689600000));
        }

        [Test]
        public void AddingOptionalMillisecondTimestamp_SetValueCorrectly()
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalMilliseconds("test", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.That(parameters["test"], Is.EqualTo(1735689600000));
        }

        [Test]
        public void AddingOptionalMillisecondNullValue_DoesntSetValue()
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalMilliseconds("test", null);
            Assert.That(parameters.ContainsKey("test"), Is.False);
        }

        [Test]
        public void AddingMillisecondTimestampString_SetValueCorrectly()
        {
            var parameters = new ParameterCollection();
            parameters.AddMillisecondsString("test", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.That(parameters["test"], Is.EqualTo("1735689600000"));
        }

        [Test]
        public void AddingOptionalMillisecondTimestampString_SetValueCorrectly()
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalMillisecondsString("test", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.That(parameters["test"], Is.EqualTo("1735689600000"));
        }

        [Test]
        public void AddingOptionalMillisecondStringNullValue_DoesntSetValue()
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalMillisecondsString("test", null);
            Assert.That(parameters.ContainsKey("test"), Is.False);
        }

        [Test]
        public void AddingSecondTimestamp_SetValueCorrectly()
        {
            var parameters = new ParameterCollection();
            parameters.AddSeconds("test", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.That(parameters["test"], Is.EqualTo(1735689600));
        }

        [Test]
        public void AddingOptionalSecondTimestamp_SetValueCorrectly()
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalSeconds("test", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.That(parameters["test"], Is.EqualTo(1735689600));
        }

        [Test]
        public void AddingSecondNullValue_DoesntSetValue()
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalSeconds("test", null);
            Assert.That(parameters.ContainsKey("test"), Is.False);
        }

        [Test]
        public void AddingSecondTimestampString_SetValueCorrectly()
        {
            var parameters = new ParameterCollection();
            parameters.AddSecondsString("test", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.That(parameters["test"], Is.EqualTo("1735689600"));
        }

        [Test]
        public void AddingOptionalSecondTimestampString_SetValueCorrectly()
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalSecondsString("test", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.That(parameters["test"], Is.EqualTo("1735689600"));
        }

        [Test]
        public void AddingSecondStringNullValue_DoesntSetValue()
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalSecondsString("test", null);
            Assert.That(parameters.ContainsKey("test"), Is.False);
        }

        [Test]
        public void AddingEnum_SetValueCorrectly()
        {
            var parameters = new ParameterCollection();
            parameters.AddEnum("test", TestEnum.Two);
            Assert.That(parameters["test"], Is.EqualTo("2"));
        }

        [Test]
        public void AddingOptionalEnum_SetValueCorrectly()
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalEnum("test", (TestEnum?)TestEnum.Two);
            Assert.That(parameters["test"], Is.EqualTo("2"));
        }

        [Test]
        public void AddingOptionalEnumNullValue_DoesntSetValue()
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalEnum("test", (TestEnum?)null);
            Assert.That(parameters.ContainsKey("test"), Is.False);
        }

        [Test]
        public void AddingEnumAsInt_SetValueCorrectly()
        {
            var parameters = new ParameterCollection();
            parameters.AddEnumAsInt("test", TestEnum.Two);
            Assert.That(parameters["test"], Is.EqualTo(2));
        }

        [Test]
        public void AddingOptionalEnumAsInt_SetValueCorrectly()
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalEnumAsInt("test", (TestEnum?)TestEnum.Two);
            Assert.That(parameters["test"], Is.EqualTo(2));
        }

        [Test]
        public void AddingOptionalEnumAsIntNullValue_DoesntSetValue()
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalEnumAsInt("test", (TestEnum?)null);
            Assert.That(parameters.ContainsKey("test"), Is.False);
        }

        [Test]
        public void AddingCommaSeparated_SetValueCorrectly()
        {
            var parameters = new ParameterCollection();
            parameters.AddCommaSeparated("test", ["1", "2"]);
            Assert.That(parameters["test"], Is.EqualTo("1,2"));
        }

        [Test]
        public void AddingOptionalCommaSeparated_SetValueCorrectly()
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalCommaSeparated("test", ["1", "2"]);
            Assert.That(parameters["test"], Is.EqualTo("1,2"));
        }

        [Test]
        public void AddingOptionalCommaSeparatedNullValue_DoesntSetValue()
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalCommaSeparated("test", (string[]?)null);
            Assert.That(parameters.ContainsKey("test"), Is.False);
        }

        [Test]
        public void AddingCommaSeparatedEnum_SetValueCorrectly()
        {
            var parameters = new ParameterCollection();
            parameters.AddCommaSeparated("test", [TestEnum.Two, TestEnum.One]);
            Assert.That(parameters["test"], Is.EqualTo("2,1"));
        }

        [Test]
        public void AddingOptionalCommaSeparatedEnum_SetValueCorrectly()
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalCommaSeparated("test", [TestEnum.Two, TestEnum.One]);
            Assert.That(parameters["test"], Is.EqualTo("2,1"));
        }

        [Test]
        public void AddingOptionalCommaSeparatedEnumNullValue_DoesntSetValue()
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalCommaSeparated("test", (TestEnum[]?)null);
            Assert.That(parameters.ContainsKey("test"), Is.False);
        }

        [Test]
        public void AddingBoolString_SetValueCorrectly()
        {
            var parameters = new ParameterCollection();
            parameters.AddBoolString("test", true);
            Assert.That(parameters["test"], Is.EqualTo("true"));
        }

        [Test]
        public void AddingOptionalBoolString_SetValueCorrectly()
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalBoolString("test", true);
            Assert.That(parameters["test"], Is.EqualTo("true"));
        }

        [Test]
        public void AddingOptionalBoolStringNullValue_DoesntSetValue()
        {
            var parameters = new ParameterCollection();
            parameters.AddOptionalBoolString("test", null);
            Assert.That(parameters.ContainsKey("test"), Is.False);
        }
    }
}
