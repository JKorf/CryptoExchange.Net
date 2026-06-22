using System;

namespace CryptoExchange.Net.TokenManagement
{
    /// <summary>
    /// Token status
    /// </summary>
    public enum TokenStatus
    {
        /// <summary>
        /// Valid
        /// </summary>
        Valid,
        /// <summary>
        /// Expired token
        /// </summary>
        Expired
    }

    /// <summary>
    /// Token info
    /// </summary>
    public class TokenInfo
    {
        /// <summary>
        /// The scope of the token
        /// </summary>
        public TokenScope Scope { get; }
        /// <summary>
        /// The user API key
        /// </summary>
        public string ApiKey { get; }
        /// <summary>
        /// The server token
        /// </summary>
        public string Token { get; }
        /// <summary>
        /// The timestamp the token was created
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// Token status
        /// </summary>
        public TokenStatus Status { get; set; }

        /// <summary>
        /// The time the token should be refreshed
        /// </summary>
        public DateTime NextRefreshTime { get; set; }
        /// <summary>
        /// The time until which the token is valid
        /// </summary>
        public DateTime ValidUntil { get; set; }


        /// <summary>
        /// The time the token is valid for
        /// </summary>
        public TimeSpan ValidTime { get; set; }
        /// <summary>
        /// The refresh interval
        /// </summary>
        public TimeSpan RefreshInterval { get; set; }
        /// <summary>
        /// How the token is retained after its last lease is released
        /// </summary>
        public TokenRetentionPolicy RetentionPolicy { get; set; }

        /// <summary>
        /// Expired event
        /// </summary>
        public event Action<TokenInfo>? Expired;

        /// <summary>
        /// ctor
        /// </summary>
        public TokenInfo(TokenScope scope, string token, TimeSpan refreshInterval, TimeSpan timeValid, TokenRetentionPolicy retentionPolicy)
        {
            Scope = scope;
            ApiKey = scope.ApiKey;
            Token = token;
            RefreshInterval = refreshInterval;
            ValidTime = timeValid;
            RetentionPolicy = retentionPolicy;
        }

        internal void MarkExpired()
        {
            Status = TokenStatus.Expired;
            NextRefreshTime = DateTime.MaxValue;
        }

        internal void InvokeExpired()
        {
            Expired?.Invoke(this);
        }

        internal void Refresh()
        {
            NextRefreshTime = DateTime.UtcNow.Add(RefreshInterval);
            ValidUntil = DateTime.UtcNow.Add(ValidTime);
        }
    }
}
