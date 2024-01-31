using CryptoExchange.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchange.Net.UnitTests.TestImplementations.Sockets
{
    internal class TestQuery : Query<object>
    {
        public override HashSet<string> ListenerIdentifiers { get; set; }

        public TestQuery(string identifier, object request, bool authenticated, int weight = 1) : base(request, authenticated, weight)
        {
            ListenerIdentifiers = new HashSet<string> { identifier };
        }
    }
}
