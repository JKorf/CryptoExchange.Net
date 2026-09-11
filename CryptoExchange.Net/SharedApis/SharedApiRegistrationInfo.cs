using System;

namespace CryptoExchange.Net.SharedApis
{
    internal sealed class SharedApiRegistrationInfo
    {
        public Type SharedApiType { get; }
        public Type[] BaseCapabilityTypes { get; }

        public SharedApiRegistrationInfo(
            Type sharedApiType,
            Type[] baseCapabilityTypes)
        {
            SharedApiType = sharedApiType;
            BaseCapabilityTypes = baseCapabilityTypes;
        }
    }
}
