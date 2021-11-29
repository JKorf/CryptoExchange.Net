using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Requests;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CryptoExchange.Net
{
    /// <summary>
    /// Base rest client
    /// </summary>
    public abstract class SubClient
    {
        public AuthenticationProvider? AuthenticationProvider { get; }
        protected string BaseAddress { get; }

        public SubClient(SubClientOptions options, AuthenticationProvider? authProvider)
        {
            AuthenticationProvider = authProvider;
            BaseAddress = options.BaseAddress;
        }

    }
}
