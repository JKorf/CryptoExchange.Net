﻿using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Sockets
{
    /// <summary>
    /// Subscription base
    /// </summary>
    public abstract class Subscription
    {
        private bool _outputOriginalData;

        /// <summary>
        /// Logger
        /// </summary>
        protected readonly ILogger _logger;

        /// <summary>
        /// If the subscription is a private subscription and needs authentication
        /// </summary>
        public bool Authenticated { get; }

        public abstract List<string> Identifiers { get; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="apiClient"></param>
        /// <param name="authenticated"></param>
        public Subscription(ILogger logger, ISocketApiClient apiClient, bool authenticated)
        {
            _logger = logger;
            _outputOriginalData = apiClient.ApiOptions.OutputOriginalData ?? apiClient.ClientOptions.OutputOriginalData;
            Authenticated = authenticated;
        }

        /// <summary>
        /// Get the subscribe object to send when subscribing
        /// </summary>
        /// <returns></returns>
        public abstract object? GetSubRequest();
        /// <summary>
        /// Check if the message is the response to the subscribe request
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract (bool, CallResult?) MessageMatchesSubRequest(ParsedMessage message);

        /// <summary>
        /// Get the unsubscribe object to send when unsubscribing
        /// </summary>
        /// <returns></returns>
        public abstract object? GetUnsubRequest();
        /// <summary>
        /// Check if the message is the response to the unsubscribe request
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract (bool, CallResult?) MessageMatchesUnsubRequest(ParsedMessage message);

        /// <summary>
        /// Check if the message is an update for this subscription
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        //public abstract bool MessageMatchesEvent(ParsedMessage message);
        /// <summary>
        /// Handle the update message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract Task HandleEventAsync(DataEvent<ParsedMessage> message);

        ///// <summary>
        ///// Create a data event
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="obj"></param>
        ///// <param name="message"></param>
        ///// <param name="topic"></param>
        ///// <param name="type"></param>
        ///// <returns></returns>
        //protected virtual DataEvent<T> CreateDataEvent<T>(T obj, ParsedMessage message, string? topic = null, SocketUpdateType? type = null)
        //{
        //    string? originalData = null;
        //    if (_outputOriginalData)
        //        originalData = message.Get(ParsingUtils.GetString);

        //    return new DataEvent<T>(obj, topic, originalData, message.Timestamp, type);
        //}

        ///// <summary>
        ///// Deserialize the message to an object using Json.Net
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="message"></param>
        ///// <param name="settings"></param>
        ///// <returns></returns>
        //protected virtual Task<CallResult<T>> DeserializeAsync<T>(StreamMessage message, JsonSerializerSettings settings)
        //{
        //    var serializer = JsonSerializer.Create(settings);
        //    using var sr = new StreamReader(message.Stream, Encoding.UTF8, false, (int)message.Stream.Length, true);
        //    using var jsonTextReader = new JsonTextReader(sr);
        //    var result = serializer.Deserialize<T>(jsonTextReader);
        //    message.Stream.Position = 0;
        //    return Task.FromResult(new CallResult<T>(result!));
        //}
    }
}