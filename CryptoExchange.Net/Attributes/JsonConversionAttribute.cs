using System;

namespace CryptoExchange.Net.Attributes
{
    /// <summary>
    /// Used for conversion in ArrayConverter
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class JsonConversionAttribute: Attribute
    {
    }
}
