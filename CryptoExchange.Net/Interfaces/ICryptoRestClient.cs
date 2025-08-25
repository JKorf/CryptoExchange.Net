using System;

namespace CryptoExchange.Net.Interfaces;

/// <summary>
/// Client for accessing REST API's for different exchanges
/// </summary>
public interface ICryptoRestClient
{
    /// <summary>
    /// Try get 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T TryGet<T>(Func<T> createFunc);
}