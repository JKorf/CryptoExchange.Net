using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.TokenManagement
{
    /// <summary>
    /// Token scope
    /// </summary>
    public class TokenScope
    {
        /// <summary>
        /// Exchange name
        /// </summary>
        public string Exchange { get; }
        /// <summary>
        /// Environment name
        /// </summary>
        public string Environment { get; }
        /// <summary>
        /// Token type
        /// </summary>
        public string TokenType { get; }
        /// <summary>
        /// API key
        /// </summary>
        public string ApiKey { get; }
        /// <summary>
        /// Additional identifier
        /// </summary>
        public string? AdditionalIdentifier { get; }

        private readonly string _maskedKey = "";

        /// <summary>
        /// The scope identifier
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// ctor
        /// </summary>
        public TokenScope(
            string exchange,
            string environment,
            string tokenType,
            string apiKey,
            string? additionalIdentifier = null)
        {
            Exchange = exchange;
            Environment = environment;
            TokenType = tokenType;
            ApiKey = apiKey;
            AdditionalIdentifier = additionalIdentifier;

            Id = $"{Exchange}/{Environment}/{TokenType}/{ApiKey}/{AdditionalIdentifier}";

            if (apiKey.Length > 12)
                _maskedKey = apiKey.Substring(0, 3) + "***" + apiKey.Substring(apiKey.Length - 4, 3);
            else
                _maskedKey = "******";
        }


        /// <inheritdoc />
        public override string ToString() => Id;
    }
}
