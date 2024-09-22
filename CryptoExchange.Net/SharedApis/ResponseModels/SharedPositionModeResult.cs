using CryptoExchange.Net.SharedApis.Enums;

namespace CryptoExchange.Net.SharedApis.ResponseModels
{
    /// <summary>
    /// Position mode result
    /// </summary>
    public record SharedPositionModeResult
    {
        /// <summary>
        /// The current position mode
        /// </summary>
        public SharedPositionMode PositionMode { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedPositionModeResult(SharedPositionMode positionMode)
        {
            PositionMode = positionMode;
        }
    }
}
