using CryptoExchange.Net.Interfaces;
using System;

namespace CryptoExchange.Net.Clients;

/// <inheritdoc />
public class CryptoRestClient : CryptoBaseClient, ICryptoRestClient
{
    /// <summary>
    /// ctor
    /// </summary>
    public CryptoRestClient()
    {
    }

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="serviceProvider"></param>
    public CryptoRestClient(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}