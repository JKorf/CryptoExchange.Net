using NUnit.Framework;
using CryptoExchange.Net.SharedApis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CryptoExchange.Net.UnitTests
{
    [TestFixture]
    internal class SharedParametersTests
    {
        [Test]
        public void AllSharedAPIOptions_ShouldListAllDefaultParametersInRequestParameterRules()
        {
            var failures = new List<string>();

            var optionsTypes = typeof(CapabilityOptions).Assembly
                .GetTypes()
                .Where(x => x.IsClass
                            && !x.IsAbstract
                            && typeof(CapabilityOptions).IsAssignableFrom(x))
                .OrderBy(x => x.FullName)
                .ToArray();

            foreach (var optionsType in optionsTypes)
            {
                var genericOptionsType = GetGenericOptionsType(optionsType);
                if (genericOptionsType == null)
                {
                    if (optionsType == typeof(ClosePositionOptions))
                        continue;

                    failures.Add($"{optionsType.Name}: unable to determine request type");
                    continue;
                }

                var requestType = genericOptionsType.GetGenericArguments()[0];
                var defaultRulesField = GetDefaultParameterRulesField(optionsType);
                if (defaultRulesField == null)
                {
                    failures.Add($"{optionsType.Name}: no _defaultParameterRules field found");
                    continue;
                }

                var rules = (RequestParameterDescription[]?)defaultRulesField.GetValue(null);
                if (rules == null)
                {
                    failures.Add($"{optionsType.Name}: _defaultParameterRules is null");
                    continue;
                }

                var constructorParameterNames = requestType
                    .GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                    .SelectMany(x => x.GetParameters())
                    .Where(x => !string.Equals(
                        x.Name,
                        "exchangeParameters",
                        StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.Name!)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var parameterProperties = requestType
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(x => x.DeclaringType == requestType
                                || constructorParameterNames.Contains(x.Name))
                    .Where(x => x.Name != nameof(SharedRequest.ExchangeParameters))
                    .Select(x => x.Name)
                    .OrderBy(x => x)
                    .ToArray();

                var ruleNames = rules
                    .Select(x => x.Name)
                    .ToHashSet(StringComparer.Ordinal);

                foreach (var parameterProperty in parameterProperties)
                {
                    if (!ruleNames.Contains(parameterProperty))
                    {
                        failures.Add(
                            $"{optionsType.Name}: request property " +
                            $"{requestType.Name}.{parameterProperty} has no default request parameter rule");
                    }
                }
            }

            Assert.That(
                failures,
                Is.Empty,
                $"Missing default request parameter rules:{Environment.NewLine}" +
                string.Join(Environment.NewLine, failures));
        }

        [Test]
        public void RequiredExchangeParameter_ShouldAcceptNameOrAnyAlias()
        {
            const string exchange = "TestExchange";
            var options = new GetTickerOptions(exchange)
            {
                ExchangeParameterRules =
                [
                    ExchangeParameterRule.Required(
                        "Parameter",
                        "Test parameter",
                        1,
                        "Alias1",
                        "Alias2")
                ]
            };

            var missingResult = options.ValidateRequest(null, null, [TradingMode.Spot]);
            var nameResult = options.ValidateRequest(
                new ExchangeParameters(new ExchangeParameter(exchange, "Parameter", 1)),
                null,
                [TradingMode.Spot]);
            var firstAliasResult = options.ValidateRequest(
                new ExchangeParameters(new ExchangeParameter(exchange, "Alias1", 1)),
                null,
                [TradingMode.Spot]);
            var secondAliasResult = options.ValidateRequest(
                new ExchangeParameters(new ExchangeParameter(exchange, "Alias2", 1)),
                null,
                [TradingMode.Spot]);

            Assert.Multiple(() =>
            {
                Assert.That(missingResult, Is.Not.Null);
                Assert.That(nameResult, Is.Null);
                Assert.That(firstAliasResult, Is.Null);
                Assert.That(secondAliasResult, Is.Null);
            });
        }

        [Test]
        public void RequiredExchangeParameterWithoutAliases_ShouldBeRequired()
        {
            const string exchange = "TestExchange";
            var options = new GetTickerOptions(exchange)
            {
                ExchangeParameterRules =
                [
                    ExchangeParameterRule.Required(
                        "Parameter",
                        "Test parameter",
                        1)
                ]
            };

            var result = options.ValidateRequest(null, null, [TradingMode.Spot]);

            Assert.That(result, Is.Not.Null);
        }

        private static Type? GetGenericOptionsType(Type optionsType)
        {
            for (var type = optionsType; type != null; type = type.BaseType)
            {
                if (type.IsGenericType
                    && type.GetGenericTypeDefinition() == typeof(CapabilityOptions<,>))
                    return type;
            }

            return null;
        }

        private static FieldInfo? GetDefaultParameterRulesField(Type optionsType)
        {
            for (var type = optionsType; type != null; type = type.BaseType)
            {
                var field = type.GetField(
                    "_defaultParameterRules",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (field != null)
                    return field;
            }

            return null;
        }
    }
}
