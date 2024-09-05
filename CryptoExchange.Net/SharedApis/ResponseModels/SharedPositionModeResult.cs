using CryptoExchange.Net.SharedApis.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.ResponseModels
{
    public record SharedPositionModeResult
    {
        public SharedPositionMode PositionMode { get; set; }

        public SharedPositionModeResult(SharedPositionMode positionMode)
        {
            PositionMode = positionMode;
        }
    }
}
