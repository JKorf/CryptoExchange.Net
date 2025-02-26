using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    internal interface INullableConverter
    {
        JsonConverter CreateNullableConverter();
    }
}
