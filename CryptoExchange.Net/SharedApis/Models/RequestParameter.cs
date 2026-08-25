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
    public static class RequestParameter<TRequest>
        where TRequest : SharedRequest
    {
        /// <summary>
        /// Describe a request property which is required by an implementation.
        /// </summary>
        public static ParameterDescription Required<TValue>(
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

            return new ParameterDescription(
                property.Name,
                property.PropertyType,
                description,
                exampleValue!);
        }
    }

    /// <summary>
    /// Creates descriptions for exchange-specific parameters.
    /// </summary>
    public static class ExchangeParameterDescription
    {
        /// <summary>
        /// Describe a required exchange-specific parameter.
        /// </summary>
        public static ParameterDescription Required<TValue>(
            string name,
            string description,
            TValue exampleValue,
            params string[] aliases)
            => Create(name, description, exampleValue, aliases);

        /// <summary>
        /// Describe an optional exchange-specific parameter.
        /// </summary>
        public static ParameterDescription Optional<TValue>(
            string name,
            string description,
            TValue exampleValue,
            params string[] aliases)
            => Create(name, description, exampleValue, aliases);

        private static ParameterDescription Create<TValue>(
            string name,
            string description,
            TValue exampleValue,
            string[] aliases)
        {
            var names = new string[aliases.Length + 1];
            names[0] = name;
            Array.Copy(aliases, 0, names, 1, aliases.Length);

            return new ParameterDescription(
                names,
                typeof(TValue),
                description,
                exampleValue!);
        }
    }
}
