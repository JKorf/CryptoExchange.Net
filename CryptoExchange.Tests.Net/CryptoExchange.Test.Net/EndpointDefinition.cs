using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchange.Test.Net
{
    public class EndpointDefinition
    {
        public HttpMethod Method { get; set; }
        public string Path { get; set; }
        public string Json { get; set; }
    }
}
