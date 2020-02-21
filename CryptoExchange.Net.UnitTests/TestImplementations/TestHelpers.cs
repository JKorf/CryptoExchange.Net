using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace CryptoExchange.Net.UnitTests.TestImplementations
{
    public class TestHelpers
    {
        [ExcludeFromCodeCoverage]
        public static bool AreEqual<T>(T self, T to, params string[] ignore) where T : class
        {
            if (self != null && to != null)
            {
                var type = self.GetType();
                var ignoreList = new List<string>(ignore);
                foreach (var pi in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (ignoreList.Contains(pi.Name))
                    {
                        continue;
                    }

                    var selfValue = type.GetProperty(pi.Name).GetValue(self, null);
                    var toValue = type.GetProperty(pi.Name).GetValue(to, null);

                    if (pi.PropertyType.IsClass && !pi.PropertyType.Module.ScopeName.Equals("System.Private.CoreLib.dll"))
                    {
                        // Check of "CommonLanguageRuntimeLibrary" is needed because string is also a class
                        if (AreEqual(selfValue, toValue, ignore))
                        {
                            continue;
                        }

                        return false;
                    }

                    if (selfValue != toValue && (selfValue == null || !selfValue.Equals(toValue)))
                    {
                        return false;
                    }
                }

                return true;
            }

            return self == to;
        }
    }
}
