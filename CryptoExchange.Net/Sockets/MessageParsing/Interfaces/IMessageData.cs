﻿using System;
using System.Collections.Generic;
using System.IO;

namespace CryptoExchange.Net.Sockets.MessageParsing.Interfaces
{
    /// <summary>
    /// Message accessor
    /// </summary>
    public interface IMessageAccessor
    {
        /// <summary>
        /// Is this a json message
        /// </summary>
        bool IsJson { get; }
        /// <summary>
        /// The underlying data object
        /// </summary>
        object? Underlying { get; }
        /// <summary>
        /// Load a stream message
        /// </summary>
        /// <param name="stream"></param>
        void Load(Stream stream);
        /// <summary>
        /// Get the type of node
        /// </summary>
        /// <returns></returns>
        NodeType? GetNodeType();
        /// <summary>
        /// Get the type of node
        /// </summary>
        /// <param name="path">Access path</param>
        /// <returns></returns>
        NodeType? GetNodeType(MessagePath path);
        /// <summary>
        /// Get the value of a path
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        T? GetValue<T>(MessagePath path);
        /// <summary>
        /// Get the values of an array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        List<T?>? GetValues<T>(MessagePath path);
        /// <summary>
        /// Deserialize the message into this type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        object Deserialize(Type type, MessagePath? path = null);
    }
}