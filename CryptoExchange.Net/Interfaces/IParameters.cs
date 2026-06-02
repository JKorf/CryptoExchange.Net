using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Interfaces
{
    /// <summary>
    /// Parameters
    /// </summary>
    public interface IParameters
    {
        /// <summary>
        /// The parameter dictionary
        /// </summary>
        IDictionary<string, object> Dictionary { get; }
        /// <summary>
        /// Body value
        /// </summary>
        object? BodyValue { get; }
    }
}
