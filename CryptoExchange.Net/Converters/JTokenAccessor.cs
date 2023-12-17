using CryptoExchange.Net.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.Converters
{
    internal class JTokenAccessor : IMessageAccessor
    {
        private readonly JToken _token;
        private Dictionary<string, JToken?> _cache = new Dictionary<string, JToken?>();

        public JTokenAccessor(JToken token)
        {
            _token = token;
        }

        public int? GetArrayIntValue(string? key, int index)
        {
            var accessToken = key == null ? _token : GetToken(key);
            if (accessToken == null || accessToken is not JArray arr)
                return null;
            return arr[index].Value<int>();
        }

        public string? GetArrayStringValue(string? key, int index)
        {
            var accessToken = key == null ? _token : GetToken(key);
            if (accessToken == null || accessToken is not JArray arr)
                return null;

            if (arr.Count <= index)
                return null;

            if (arr[index].Type != JTokenType.String)
                return null;

            return arr[index].Value<string>();
        }

        public int? GetCount(string key)
        {
            var accessToken = GetToken(key);
            return accessToken.Count();
        }

        public int? GetIntValue(string key)
        {
            var accessToken = GetToken(key);
            return accessToken?.Value<int>();
        }

        public string? GetStringValue(string key)
        {
            var accessToken = GetToken(key);
            if (accessToken?.Type == JTokenType.Object)
                return ((JObject)accessToken).Properties().First().Name;

            return accessToken?.ToString();
        }

        public bool IsObject(string? key) => _token.Type == JTokenType.Object;
        public bool IsArray(IEnumerable<int> indexes)
        {
            var item = _token;
            foreach(var index in indexes)
            {
                if (item.Type != JTokenType.Array)
                    return false;

                var arr = ((JArray)item);
                if (arr.Count <= index)
                    return false;

                item = arr[index];
            }

            return item.Type == JTokenType.Array;
        }

        private JToken? GetToken(string key)
        {
            if (_cache.TryGetValue(key, out var token))
                return token;

            var splitTokens = key.Split(new char[] { ':' });
            var accessToken = _token;
            foreach (var splitToken in splitTokens)
            {
                if (accessToken.Type == JTokenType.Array)
                    return null;

                accessToken = accessToken[splitToken];

                if (accessToken == null)
                    break;
            }

            _cache.Add(key, accessToken);
            return accessToken;
        }
    }
}
