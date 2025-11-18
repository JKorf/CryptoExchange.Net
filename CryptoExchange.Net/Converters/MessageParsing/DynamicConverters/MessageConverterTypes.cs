using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.Converters.MessageParsing.DynamicConverters
{

    public class MessageEvaluator
    {
        public int Priority { get; set; }
        public bool ForceIfFound { get; set; }
        public MessageFieldReference[] Fields { get; set; }

        public Func<SearchResult, string> MessageIdentifier { get; set; }
        public Func<SearchResult, Type?> TypeIdentifier { get; set; }

        public bool Statisfied(SearchResult result)
        {
            foreach(var field in Fields)
            {
                if (!result.Contains(field.Name))
                    return false;
            }

            return true;
        }

        public MessageInfo ProduceMessageInfo(SearchResult result)
        {
            return new MessageInfo
            {
                DeserializationType = TypeIdentifier(result),
                Identifier = MessageIdentifier(result)
            };
        }
    }

    public class MessageFieldReference
    {
        public int Level { get; set; }
        public string Name { get; set; }
        public Type Type { get; set; }
    }

    public class SearchResult
    {
        public Dictionary<string, string>? _stringValues;
        public Dictionary<string, int>? _intValues;

        public int GetInt(string name) => _intValues[name];
        public string GetString(string name) => _stringValues[name];

        public void WriteInt(string name, int value)
        {
            _intValues ??= new();
            _intValues[name] = value;
        }
        public void WriteString(string name, string value)
        {
            _stringValues ??= new();
            _stringValues[name] = value;
        }

        public bool Contains(string name)
        {
            if (_intValues?.ContainsKey(name) == true)
                return true;
            if (_stringValues?.ContainsKey(name) == true)
                return true;

            return false;
        }

        public void Reset()
        {
            _intValues?.Clear();
            _stringValues?.Clear();
        }
    }

    public class MessageEvalutorFieldReference
    {
        public MessageFieldReference Field { get; set; }
        public MessageEvaluator? ForceEvaluator { get; set; }
    }
}
