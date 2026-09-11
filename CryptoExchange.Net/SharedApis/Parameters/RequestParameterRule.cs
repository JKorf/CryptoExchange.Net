using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Creates descriptions for properties on a shared request.
    /// </summary>
    public static class RequestParameterRule<TRequest>
        where TRequest : SharedRequest
    {
        /// <summary>
        /// Describe a request property which is required by an implementation.
        /// </summary>
        public static RequestParameterDescription Required<TValue>(
            Expression<Func<TRequest, TValue>> selector,
            string description,
            TValue exampleValue)
        {
            Expression body = selector.Body;

            if (body is UnaryExpression unary
                && unary.NodeType == ExpressionType.Convert)
            {
                body = unary.Operand;
            }

            if (body is not MemberExpression member
                || member.Member is not PropertyInfo property
                || member.Expression != selector.Parameters[0])
            {
                throw new ArgumentException(
                    "Selector must select a direct request property",
                    nameof(selector));
            }

            return new RequestParameterDescription(
                RequestParameterSupport.Required,
                property.Name,
                property.PropertyType,
                description,
                exampleValue!
                );
        }

        /// <summary>
        /// Describe a request property which is optional by an implementation.
        /// </summary>
        public static RequestParameterDescription Optional<TValue>(
            Expression<Func<TRequest, TValue>> selector,
            string description,
            TValue exampleValue)
        {
            Expression body = selector.Body;

            if (body is UnaryExpression unary
                && unary.NodeType == ExpressionType.Convert)
            {
                body = unary.Operand;
            }

            if (body is not MemberExpression member
                || member.Member is not PropertyInfo property
                || member.Expression != selector.Parameters[0])
            {
                throw new ArgumentException(
                    "Selector must select a direct request property",
                    nameof(selector));
            }

            return new RequestParameterDescription(
                RequestParameterSupport.Optional,
                property.Name,
                property.PropertyType,
                description,
                exampleValue!
                );
        }        
    }

    /// <summary>
    /// Creates descriptions for exchange-specific parameters.
    /// </summary>
    public static class ExchangeParameterRule
    {
        /// <summary>
        /// Describe a required exchange-specific parameter.
        /// </summary>
        public static ExchangeParameterDescription Required<TValue>(
            string name,
            string description,
            TValue exampleValue,
            params string[] aliases)
            => Create(ExchangeParameterRequirement.Required, name, description, exampleValue, aliases);

        /// <summary>
        /// Describe an optional exchange-specific parameter.
        /// </summary>
        public static ExchangeParameterDescription Optional<TValue>(
            string name,
            string description,
            TValue exampleValue,
            params string[] aliases)
            => Create(ExchangeParameterRequirement.Optional, name, description, exampleValue, aliases);

        private static ExchangeParameterDescription Create<TValue>(
            ExchangeParameterRequirement requirement,
            string name,
            string description,
            TValue exampleValue,
            string[] aliases)
        {
            return new ExchangeParameterDescription(
                requirement,
                name,
                aliases,
                typeof(TValue),
                description,
                exampleValue!);
        }
    }
}
