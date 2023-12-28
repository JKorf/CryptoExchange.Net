//using CryptoExchange.Net.Interfaces;
//using CryptoExchange.Net.Objects;
//using CryptoExchange.Net.Objects.Sockets;
//using CryptoExchange.Net.Sockets;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Net.WebSockets;
//using System.Text;

//namespace CryptoExchange.Net.Converters
//{
//    /// <summary>
//    /// Socket message converter
//    /// </summary>
//    public abstract class SocketConverter
//    {

//        public abstract MessageInterpreterPipeline InterpreterPipeline { get; }

//        /// <inheritdoc />
//        public BaseParsedMessage? ReadJson(WebSocketMessageType websocketMessageType, Stream stream, SocketListenerManager listenerManager, bool outputOriginalData)
//        {
//            // Start reading the data
//            // Once we reach the properties that identify the message we save those in a dict
//            // Once all id properties have been read callback to see what the deserialization type should be
//            // Deserialize to the correct type

//            if (InterpreterPipeline.PreProcessCallback != null)
//                stream = InterpreterPipeline.PreProcessCallback(websocketMessageType, stream);

//            var accessor = new JTokenAccessor(stream);
//            if (accessor == null)
//                return null;

//            var streamIdentity = InterpreterPipeline.GetStreamIdentifier(accessor);
//            if (streamIdentity == null)
//                return null;

//            var typeIdentity = InterpreterPipeline.GetTypeIdentifier(accessor);
//            var typeResult = listenerManager.IdToType(streamIdentity, typeIdentity);
//            if (typeResult == null)
//                return null;

//            var idInstance = accessor.Instantiate(typeResult);
//            if (outputOriginalData)
//                idInstance.OriginalData = idInstance.OriginalData;

//            idInstance.StreamIdentifier = streamIdentity;
//            idInstance.TypeIdentifier = typeIdentity;
//            idInstance.Parsed = true;
//            return idInstance;
//        }
//    }
//}
