namespace CryptoExchange.Net.Authentication
{
    /// <summary>
    /// Api credentials, used to sign requests accessing private endpoints
    /// </summary>
    public abstract class ApiCredentials
    {
        /// <summary>
        /// Copy the credentials
        /// </summary>
        /// <returns></returns>
        public abstract ApiCredentials Copy();
    }
}
