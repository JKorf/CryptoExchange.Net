using CryptoExchange.Net.SharedApis;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CryptoExchange.Net.UnitTests
{
    [TestFixture]
    internal class SharedCapabilitiesTests
    {
        [Test]
        public void AllSharedApiCapabilities_ShouldBeListedInSharedCapabilities()
        {
            var markerTypes = new[]
            {
                typeof(ISharedApiCapability),
                typeof(ISharedRest),
                typeof(ISharedSocket),
                typeof(ISharedSubscription)
            };

            var expectedCapabilities = typeof(ISharedApiCapability).Assembly
                .GetTypes()
                .Where(x => x.IsInterface
                            && typeof(ISharedApiCapability).IsAssignableFrom(x)
                            && !markerTypes.Contains(x))
                .OrderBy(x => x.FullName)
                .ToArray();

            var listedCapabilities = GetTypeAndNestedTypes(typeof(SharedCapabilities))
                .SelectMany(x => x.GetProperties(
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
                .SelectMany(x => x.PropertyType.GetGenericArguments())
                .Where(x => x.IsInterface
                            && typeof(ISharedApiCapability).IsAssignableFrom(x))
                .ToArray();

            var missingCapabilities = expectedCapabilities
                .Except(listedCapabilities)
                .Select(x => x.Name)
                .ToArray();

            var unknownCapabilities = listedCapabilities
                .Except(expectedCapabilities)
                .Select(x => x.Name)
                .ToArray();

            var duplicateCapabilities = listedCapabilities
                .GroupBy(x => x)
                .Where(x => x.Count() > 1)
                .Select(x => x.Key.Name)
                .ToArray();

            Assert.Multiple(() =>
            {
                Assert.That(
                    missingCapabilities,
                    Is.Empty,
                    $"Capabilities missing from SharedCapabilities: " +
                    string.Join(", ", missingCapabilities));

                Assert.That(
                    unknownCapabilities,
                    Is.Empty,
                    $"Unknown capabilities listed in SharedCapabilities: " +
                    string.Join(", ", unknownCapabilities));

                Assert.That(
                    duplicateCapabilities,
                    Is.Empty,
                    $"Capabilities listed multiple times in SharedCapabilities: " +
                    string.Join(", ", duplicateCapabilities));
            });
        }

        private static IEnumerable<Type> GetTypeAndNestedTypes(Type type)
        {
            yield return type;

            foreach (var nestedType in type.GetNestedTypes(BindingFlags.Public))
            {
                foreach (var result in GetTypeAndNestedTypes(nestedType))
                    yield return result;
            }
        }
    }
}
