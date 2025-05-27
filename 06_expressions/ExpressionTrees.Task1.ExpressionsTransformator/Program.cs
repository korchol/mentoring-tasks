/*
 * Create a class based on ExpressionVisitor, which makes expression tree transformation:
 * 1. converts expressions like <variable> + 1 to increment operations, <variable> - 1 - into decrement operations.
 * 2. changes parameter values in a lambda expression to constants, taking the following as transformation parameters:
 *    - source expression;
 *    - dictionary: <parameter name: value for replacement>
 * The results could be printed in console or checked via Debugger using any Visualizer.
 */
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AgileObjects.ReadableExpressions;

namespace ExpressionTrees.Task1.ExpressionsTransformer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Expression Visitor for increment/decrement.");
            Console.WriteLine();

            var replaceDictionary = new Dictionary<string, object> { { "x", 10 } };
            var visitor = new IncDecExpressionVisitor(replaceDictionary);

            var expression = PrepareExpressionManually();

            Console.WriteLine("Original Expression:");
            Console.WriteLine(expression.ToReadableString());
            Console.WriteLine(expression.Body.NodeType);

            Console.WriteLine();

            var transformedExpression = (Expression<Func<int>>)visitor.Visit(expression);

            Console.WriteLine("Transformed Expression:");
            Console.WriteLine(transformedExpression.ToReadableString());
            Console.WriteLine(transformedExpression.Body.NodeType);

            Console.ReadLine();
        }

        private static LambdaExpression PrepareExpressionManually()
        {
            // left
            var left = Expression.Parameter(typeof(int), "x");

            // right
            var right = Expression.Constant(1);

            // method
            var adding = Expression.Add(left, right);

            return Expression.Lambda<Func<int, int>>(adding, left);
        }
    }
}
