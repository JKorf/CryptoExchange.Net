using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.Objects.Errors
{
    /// <summary>
    /// Error mapping collection
    /// </summary>
    public class ErrorMapping
    {
        private Dictionary<string, ErrorEvaluator> _evaluators = new Dictionary<string, ErrorEvaluator>();
        private Dictionary<string, ErrorInfo> _directMapping = new Dictionary<string, ErrorInfo>();

        /// <summary>
        /// ctor
        /// </summary>
        public ErrorMapping(ErrorInfo[] errorMappings, ErrorEvaluator[]? errorTypeEvaluators = null)
        {
            foreach (var item in errorMappings)
            {
                if (!item.ErrorCodes.Any())
                    throw new Exception("Error codes can't be null in error mapping");

                foreach(var code in item.ErrorCodes!)
                    _directMapping.Add(code, item);
            }

            if (errorTypeEvaluators == null)
                return;

            foreach (var item in errorTypeEvaluators)
            {
                foreach(var code in item.ErrorCodes)
                    _evaluators.Add(code, item);
            }
        }

        /// <summary>
        /// Get error info for an error code
        /// </summary>
        public ErrorInfo GetErrorInfo(string code, string? message)
        {
            if (_directMapping.TryGetValue(code!, out var info))
                return info with { Message = message };
            
            if (_evaluators.TryGetValue(code!, out var eva))
                return eva.ErrorTypeEvaluator.Invoke(code!, message) with { Message = message };

            return ErrorInfo.Unknown with { Message = message };
        }
    }
}
