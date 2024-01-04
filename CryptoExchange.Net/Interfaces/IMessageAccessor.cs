using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Interfaces
{
    public interface IMessageAccessor
    {
        bool IsObject(string? key);
        bool IsArray(IEnumerable<int> indexes);
        bool IsEmptyArray(IEnumerable<int> indexes);
        string? GetStringValue(string key);
        int? GetIntValue(string key);
        public int? GetCount(string key);
        public int? GetArrayIntValue(string? key, int index);
        public string? GetArrayStringValue(string? key, int index);

        public BaseParsedMessage Instantiate(Type type);
    }
}
