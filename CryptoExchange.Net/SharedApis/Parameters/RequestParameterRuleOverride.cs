using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request parameter rule override
    /// </summary>
    /// <param name="ParameterName">The name of the parameter</param>
    /// <param name="Support">The support level of the parameter</param>
    /// <param name="Description">A description of the parameter</param>
    public record RequestParameterRuleOverride(
        string ParameterName,
        RequestParameterSupport Support,
        string? Description = null);

    /// <summary>
    /// Request parameter rule override factory
    /// </summary>
    /// <typeparam name="TRequest">The type of the request</typeparam>
    public static class RequestParameterRuleOverride<TRequest>
        where TRequest : SharedRequest
    {
        /// <summary>
        /// Required parameter rule override
        /// </summary>
        public static RequestParameterRuleOverride Required<TValue>(
            Expression<Func<TRequest, TValue>> selector,
            string? description = null)
            => Create(selector, RequestParameterSupport.Required, description);

        /// <summary>
        /// Optional parameter rule override
        /// </summary>
        public static RequestParameterRuleOverride Optional<TValue>(
            Expression<Func<TRequest, TValue>> selector,
            string? description = null)
            => Create(selector, RequestParameterSupport.Optional, description);

        /// <summary>
        /// Not supported parameter rule override
        /// </summary>
        public static RequestParameterRuleOverride NotSupported<TValue>(
            Expression<Func<TRequest, TValue>> selector,
            string? description = null)
            => Create(selector, RequestParameterSupport.NotSupported, description);

        private static RequestParameterRuleOverride Create<TValue>(
            Expression<Func<TRequest, TValue>> selector,
            RequestParameterSupport support,
            string? description)
        {
            var body = selector.Body is UnaryExpression unary
                && unary.NodeType == ExpressionType.Convert
                    ? unary.Operand
                    : selector.Body;

            if (body is not MemberExpression member
                || member.Member is not PropertyInfo property
                || member.Expression != selector.Parameters[0])
            {
                throw new ArgumentException(
                    "Selector must select a direct request property",
                    nameof(selector));
            }

            return new RequestParameterRuleOverride(
                property.Name,
                support,
                description);
        }
    }
}
