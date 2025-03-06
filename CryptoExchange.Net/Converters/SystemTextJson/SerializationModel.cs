using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    /// <summary>
    /// Attribute to mark a model as json serializable. Used for AOT compilation.
    /// </summary>
    [AttributeUsage(System.AttributeTargets.Class | AttributeTargets.Enum | System.AttributeTargets.Interface)]
    public class SerializationModelAttribute : Attribute
    {
    }
}
