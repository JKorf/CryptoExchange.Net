using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Sockets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CryptoExchange.Net.Objects.Sockets
{
    public class MessageInterpreterPipeline
    {
        public List<PreInspectCallback> PreInspectCallbacks { get; set; } = new List<PreInspectCallback>();
        public List<PostInspectCallback> PostInspectCallbacks { get; set; } = new List<PostInspectCallback>();
    }

    public class PreInspectCallback
    {
        public Func<Stream, PreInspectResult> Callback { get; set; }
    }

    public class PostInspectCallback
    {
        public List<string> TypeFields { get; set; } = new List<string>();
        public Func<Dictionary<string, string>, IDictionary<string, IMessageProcessor>, PostInspectResult> Callback { get; set; }
    }

    public class PreInspectResult
    {
        public bool Matched { get; set; }
        public string Identifier { get; set; }
    }

    public class PostInspectResult
    {
        public Type? Type { get; set; }
        public string Identifier { get; set; }
    }
}
