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
