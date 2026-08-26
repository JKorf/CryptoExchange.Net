using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Endpoint options
    /// </summary>
    [Obsolete("Use CapabilityOptions instead")]
    public abstract class EndpointOptions : CapabilityOptions
    {
        /// <summary>
        /// Endpoint name
        /// </summary>
        [Obsolete("Use OperationName instead")]
        public string EndpointName
        {
            get => OperationName;
            set => OperationName = value;
        }

        /// <summary>
        /// ctor
        /// </summary>
        protected EndpointOptions(
            string exchange,
            string operationName,
            bool needsAuthentication)
            : base(exchange, operationName, needsAuthentication)
        {
        }
    }
}
