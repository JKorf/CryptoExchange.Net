using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Authentication
{
    public delegate Task<TCredentials?> CredentialResolver<TCredentials, TEnvironment>(string userIdentifier, TEnvironment? environment, CancellationToken ct);
}
