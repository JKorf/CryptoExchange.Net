using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CryptoExchange.Net.Testing.Exceptions
{
    internal class MissingPropertyException : Exception
    {
        public MissingPropertyException(string method, string objName, string propName, string value) 
            : base($"{method}: Missing property `{propName}` on `{objName}`, value: {value.Substring(0, Math.Min(50, value.Length))}")
        {
        }
    }
}
