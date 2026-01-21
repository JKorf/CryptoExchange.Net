namespace CryptoExchange.Net.Objects
{
    /// <summary>
    /// Information on the platform
    /// </summary>
    public record PlatformInfo
    {
        /// <summary>
        /// Platform id
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// Display name
        /// </summary>
        public string DisplayName { get; }
        /// <summary>
        /// Logo
        /// </summary>
        public string Logo { get; }
        /// <summary>
        /// Url to main application
        /// </summary>
        public string Url { get; }
        /// <summary>
        /// Urls to the API documentation
        /// </summary>
        public string[] ApiDocsUrl { get; }
        /// <summary>
        /// Platform type
        /// </summary>
        public PlatformType PlatformType { get; }
        /// <summary>
        /// Centralization type
        /// </summary>
        public CentralizationType CentralizationType { get; }

        /// <summary>
        /// ctor
        /// </summary>
        public PlatformInfo(string id, string displayName, string logo, string url, string[] apiDocsUrl, PlatformType platformType, CentralizationType centralizationType)
        {
            Id = id;
            DisplayName = displayName;
            Logo = logo;
            Url = url;
            ApiDocsUrl = apiDocsUrl;
            PlatformType = platformType;
            CentralizationType = centralizationType;
        }
    }
}
