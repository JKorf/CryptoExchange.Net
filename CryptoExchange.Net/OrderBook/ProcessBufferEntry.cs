using System.Collections.Generic;

namespace CryptoExchange.Net.OrderBook
{
    public class ProcessBufferEntry
    {
        public long FirstSequence { get; set; }
        public long LastSequence { get; set; }
        public List<ProcessEntry> Entries { get; set; }

        public ProcessBufferEntry()
        {
            Entries = new List<ProcessEntry>();
        }
    }
}
