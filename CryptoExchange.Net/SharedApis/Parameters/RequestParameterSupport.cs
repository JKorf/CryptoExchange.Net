using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request parameter support
    /// </summary>
    public enum RequestParameterSupport
    {
        /// <summary>
        /// Required parameter
        /// </summary>
        Required,
        /// <summary>
        /// Optional parameter
        /// </summary>
        Optional,
        /// <summary>
        /// Not supported parameter
        /// </summary>
        NotSupported
    }
}
