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
            var parameters = new Parameters(new ParameterSerializationSettings());
            parameters.Add("test", "value");
            Assert.That(parameters["test"], Is.EqualTo("value"));
        }

        [Test]
        public void AddingOptionalBasicNullValue_DoesntSetValue()
        {
            var parameters = new Parameters(new ParameterSerializationSettings());
            parameters.Add("test", null);
            Assert.That(parameters.ContainsKey("test"), Is.False);
        }

        [Test]
        public void AddingDecimalValueAsString_SetValueCorrectly()
        {
            var parameters = new Parameters(new ParameterSerializationSettings());
            parameters.Add("test", 0.1m, DecimalSerialization.String);
            Assert.That(parameters["test"], Is.EqualTo("0.1"));
        }

        [Test]
        public void AddingDecimalValueAsString2_SetValueCorrectly()
        {
            var parameters = new Parameters(new ParameterSerializationSettings()
            {
                Decimal = DecimalSerialization.String
            });
            parameters.Add("test", 0.1m);
            Assert.That(parameters["test"], Is.EqualTo("0.1"));
        }

        [Test]
        public void AddingOptionalIntNullValueAsString_DoesntSetValue()
        {
            var parameters = new Parameters(new ParameterSerializationSettings());
            parameters.Add("test", (int?)null);
            Assert.That(parameters.ContainsKey("test"), Is.False);
        }

        [Test]
        public void AddingLongValueAsString_SetValueCorrectly()
        {
            var parameters = new Parameters(new ParameterSerializationSettings());
            parameters.Add("test", 1L, IntegerSerialization.String);
            Assert.That(parameters["test"], Is.EqualTo("1"));
        }

        [Test]
        public void AddingOptionalLongValueAsString_SetValueCorrectly()
        {
            var parameters = new Parameters(new ParameterSerializationSettings()
            {
                Integer = IntegerSerialization.String
            });
            parameters.Add("test", 1L);
            Assert.That(parameters["test"], Is.EqualTo("1"));
        }

        [Test]
        public void AddingOptionalLongNullValueAsString_DoesntSetValue()
        {
            var parameters = new Parameters(new ParameterSerializationSettings());
            parameters.Add("test", (long?)null);
            Assert.That(parameters.ContainsKey("test"), Is.False);
        }

        [Test]
        public void AddingMillisecondTimestamp_SetValueCorrectly()
        {
            var parameters = new Parameters(new ParameterSerializationSettings());
            parameters.Add("test", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), DateTimeSerialization.MillisecondsNumber);
            Assert.That(parameters["test"], Is.EqualTo(1735689600000));
        }

        [Test]
        public void AddingOptionalMillisecondTimestamp_SetValueCorrectly()
        {
            var parameters = new Parameters(new ParameterSerializationSettings()
            {
                DateTimes = DateTimeSerialization.MillisecondsNumber
            });
            parameters.Add("test", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.That(parameters["test"], Is.EqualTo(1735689600000));
        }

        [Test]
        public void AddingOptionalMillisecondNullValue_DoesntSetValue()
        {
            var parameters = new Parameters(new ParameterSerializationSettings());
            parameters.Add("test", (DateTime?)null);
            Assert.That(parameters.ContainsKey("test"), Is.False);
        }

        [Test]
        public void AddingMillisecondTimestampString_SetValueCorrectly()
        {
            var parameters = new Parameters(new ParameterSerializationSettings());
            parameters.Add("test", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), DateTimeSerialization.MillisecondsString);
            Assert.That(parameters["test"], Is.EqualTo("1735689600000"));
        }

        [Test]
        public void AddingOptionalMillisecondTimestampString_SetValueCorrectly()
        {
            var parameters = new Parameters(new ParameterSerializationSettings()
            {
                DateTimes = DateTimeSerialization.MillisecondsString
            });
            parameters.Add("test", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.That(parameters["test"], Is.EqualTo("1735689600000"));
        }

        [Test]
        public void AddingSecondTimestamp_SetValueCorrectly()
        {
            var parameters = new Parameters(new ParameterSerializationSettings());
            parameters.Add("test", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), DateTimeSerialization.SecondsNumber);
            Assert.That(parameters["test"], Is.EqualTo(1735689600));
        }

        [Test]
        public void AddingOptionalSecondTimestamp_SetValueCorrectly()
        {
            var parameters = new Parameters(new ParameterSerializationSettings()
            {
                DateTimes = DateTimeSerialization.SecondsNumber
            });
            parameters.Add("test", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.That(parameters["test"], Is.EqualTo(1735689600));
        }

        [Test]
        public void AddingSecondTimestampString_SetValueCorrectly()
        {
            var parameters = new Parameters(new ParameterSerializationSettings());
            parameters.Add("test", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), DateTimeSerialization.SecondsString);
            Assert.That(parameters["test"], Is.EqualTo("1735689600"));
        }

        [Test]
        public void AddingOptionalSecondTimestampString_SetValueCorrectly()
        {
            var parameters = new Parameters(new ParameterSerializationSettings()
            {
                DateTimes = DateTimeSerialization.SecondsString
            });
            parameters.Add("test", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.That(parameters["test"], Is.EqualTo("1735689600"));
        }

        [Test]
        public void AddingEnum_SetValueCorrectly()
        {
            var parameters = new Parameters(new ParameterSerializationSettings());
            parameters.Add("test", TestEnum.Two);
            Assert.That(parameters["test"], Is.EqualTo("2"));
        }

        [Test]
        public void AddingOptionalEnum_SetValueCorrectly()
        {
            var parameters = new Parameters(new ParameterSerializationSettings());
            parameters.Add("test", (TestEnum?)TestEnum.Two);
            Assert.That(parameters["test"], Is.EqualTo("2"));
        }

        [Test]
        public void AddingOptionalEnumNullValue_DoesntSetValue()
        {
            var parameters = new Parameters(new ParameterSerializationSettings());
            parameters.Add("test", (TestEnum?)null);
            Assert.That(parameters.ContainsKey("test"), Is.False);
        }

        [Test]
        public void AddingEnumAsInt_SetValueCorrectly()
        {
            var parameters = new Parameters(new ParameterSerializationSettings());
            parameters.Add("test", TestEnum.Two, EnumSerialization.Number);
            Assert.That(parameters["test"], Is.EqualTo(2));
        }

        [Test]
        public void AddingOptionalEnumAsInt_SetValueCorrectly()
        {
            var parameters = new Parameters(new ParameterSerializationSettings()
            {
                Enum = EnumSerialization.Number
            });
            parameters.Add("test", TestEnum.Two);
            Assert.That(parameters["test"], Is.EqualTo(2));
        }

        [Test]
        public void AddingCommaSeparated_SetValueCorrectly()
        {
            var parameters = new Parameters(new ParameterSerializationSettings());
            parameters.AddCommaSeparated("test", ["1", "2"]);
            Assert.That(parameters["test"], Is.EqualTo("1,2"));
        }

        [Test]
        public void AddingOptionalCommaSeparatedNullValue_DoesntSetValue()
        {
            var parameters = new Parameters(new ParameterSerializationSettings());
            parameters.AddCommaSeparated("test", (string[]?)null);
            Assert.That(parameters.ContainsKey("test"), Is.False);
        }

        [Test]
        public void AddingCommaSeparatedEnum_SetValueCorrectly()
        {
            var parameters = new Parameters(new ParameterSerializationSettings());
            parameters.AddCommaSeparated("test", [TestEnum.Two, TestEnum.One]);
            Assert.That(parameters["test"], Is.EqualTo("2,1"));
        }

        [Test]
        public void AddingBoolString_SetValueCorrectly()
        {
            var parameters = new Parameters(new ParameterSerializationSettings());
            parameters.Add("test", true, BoolSerialization.String);
            Assert.That(parameters["test"], Is.EqualTo("true"));
        }

        [Test]
        public void AddingOptionalBoolString_SetValueCorrectly()
        {
            var parameters = new Parameters(new ParameterSerializationSettings()
            {
                Bool = BoolSerialization.String
            });
            parameters.Add("test", true);
            Assert.That(parameters["test"], Is.EqualTo("true"));
        }

        [Test]
        public void AddingOptionalBoolStringNullValue_DoesntSetValue()
        {
            var parameters = new Parameters(new ParameterSerializationSettings());
            parameters.Add("test", null);
            Assert.That(parameters.ContainsKey("test"), Is.False);
        }
    }
}
