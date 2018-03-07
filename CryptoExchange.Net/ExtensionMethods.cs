using System.Collections.Generic;

namespace CryptoExchange.Net
{
    public static class ExtensionMethods
    {
        public static void AddParameter(this Dictionary<string, object> parameters, string key, string value)
        {
            parameters.Add(key, value);
        }

        public static void AddParameter(this Dictionary<string, object> parameters, string key, object value)
        {
            parameters.Add(key, value);
        }

        public static void AddOptionalParameter(this Dictionary<string, object> parameters, string key, object value)
        {
            if(value != null)
                parameters.Add(key, value);
        }

        public static void AddOptionalParameter(this Dictionary<string, string> parameters, string key, string value)
        {
            if (value != null)
                parameters.Add(key, value);
        }
    }
}
