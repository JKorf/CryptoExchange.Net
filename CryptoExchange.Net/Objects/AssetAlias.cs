namespace CryptoExchange.Net.Objects;

/// <summary>
/// An alias used by the exchange for an asset commonly known by another name
/// </summary>
public class AssetAlias
{
    /// <summary>
    /// The name of the asset on the exchange
    /// </summary>
    public string ExchangeAssetName { get; set; }
    /// <summary>
    /// The name of the asset as it's commonly known
    /// </summary>
    public string CommonAssetName { get; set; }

    /// <summary>
    /// ctor
    /// </summary>
    public AssetAlias(string exchangeName, string commonName)
    {
        ExchangeAssetName = exchangeName;
        CommonAssetName = commonName;
    }
}
