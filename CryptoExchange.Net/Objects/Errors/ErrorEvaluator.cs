using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Objects.Errors
{
    /// <summary>
    /// Error evaluator
    /// </summary>
    public class ErrorEvaluator
    {
        /// <summary>
        /// Error code
        /// </summary>
        public string[] ErrorCodes { get; set; }

        /// <summary>
        /// Evaluation callback for determining the error type
        /// </summary>
        public Func<string, string?, ErrorInfo> ErrorTypeEvaluator { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public ErrorEvaluator(string errorCode, Func<string, string?, ErrorInfo> errorTypeEvaluator)
        {
            ErrorCodes = [errorCode];
            ErrorTypeEvaluator = errorTypeEvaluator;
        }

        /// <summary>
        /// ctor
        /// </summary>
        public ErrorEvaluator(string[] errorCodes, Func<string, string?, ErrorInfo> errorTypeEvaluator)
        {
            ErrorCodes = errorCodes;
            ErrorTypeEvaluator = errorTypeEvaluator;
        }
    }
}
