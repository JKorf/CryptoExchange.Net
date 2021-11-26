using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Attributes
{
    public class MapAttribute : Attribute
    {
        public string[] Values { get; set; }
        public MapAttribute(params string[] maps)
        {
            Values = maps;
        }
    }
}
