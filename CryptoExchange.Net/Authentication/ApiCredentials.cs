namespace CryptoExchange.Net.Authentication
{
    /// <summary>
    /// Api credentials, used to sign requests accessing private endpoints
    /// </summary>
    public abstract class ApiCredentials
    {
        /// <summary>
        /// Validate the API credentials
        /// </summary>
        public abstract void Validate();

        /// <summary>
        /// Copy the credentials
        /// </summary>
        /// <returns></returns>
        public abstract ApiCredentials Copy();
    }
}
