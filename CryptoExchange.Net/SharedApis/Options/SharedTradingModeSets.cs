namespace CryptoExchange.Net.SharedApis
{
    internal static class SharedTradingModeSets
    {
        internal static TradingMode[] Spot { get; } =
        [
            TradingMode.Spot
        ];

        internal static TradingMode[] Futures { get; } =
        [
            TradingMode.PerpetualLinear,
            TradingMode.DeliveryLinear,
            TradingMode.PerpetualInverse,
            TradingMode.DeliveryInverse
        ];

        internal static TradingMode[] Perpetual { get; } =
        [
            TradingMode.PerpetualLinear,
            TradingMode.PerpetualInverse
        ];
    }
}
