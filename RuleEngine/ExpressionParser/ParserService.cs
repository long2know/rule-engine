using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using RuleEngine.Interfaces;

namespace RuleEngine.Domain.ExpressionParser
{
	public class ParserService : IParser
	{
		public object Parse<T>(string expression, T model)
		{
			var parameter = Expression.Parameter(typeof(T), "Request");
			var exp = DynamicExpression.ParseLambda(new[] { parameter }, null, expression);
			return exp.Compile().DynamicInvoke(model);
		}

		public V Parse<T, V>(string expression, T model)
		{
			var parameter = Expression.Parameter(typeof(T), "Request");
			var exp = DynamicExpression.ParseLambda(new[] { parameter }, null, expression);
			return (V)exp.Compile().DynamicInvoke(model);
		}

		/// <summary>
		/// Parse with no specific model
		/// </summary>
		/// <param name="expression"></param>
		/// <returns></returns>
		public object Parse(string expression)
		{
			var parameter = Expression.Parameter(typeof(object), "none");
			var exp = DynamicExpression.ParseLambda(new[] { parameter }, null, expression);
			return exp.Compile().DynamicInvoke(new object());
		}

		public V Parse<V>(string expression)
		{
			var parameter = Expression.Parameter(typeof(object), "none");
			var exp = DynamicExpression.ParseLambda(new[] { parameter }, null, expression);
			return (V)exp.Compile().DynamicInvoke(new object());
		}
	}
}
