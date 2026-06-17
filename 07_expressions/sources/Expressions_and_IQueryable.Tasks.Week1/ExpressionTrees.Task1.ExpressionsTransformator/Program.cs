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

namespace ExpressionTrees.Task1.ExpressionsTransformer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Expression Visitor for increment/decrement.");
            Console.WriteLine();

            IncrementDecrementDemo();
            Console.WriteLine();
            ParameterToConstantDemo();

            Console.WriteLine();
            Console.WriteLine("Press <Enter> to exit...");
            Console.ReadLine();
        }

        // 1. <variable> + 1  ->  Increment(variable),  <variable> - 1  ->  Decrement(variable)
        private static void IncrementDecrementDemo()
        {
            Console.WriteLine("== Transformation 1: +1 / -1  ->  Increment / Decrement ==");

            var incDecVisitor = new IncDecExpressionVisitor();

            Expression<Func<int, int>> addOne = x => x + 1;
            Expression<Func<int, int>> subOne = x => x - 1;
            Expression<Func<int, int, int>> mixed = (a, b) => (a + 1) * (b - 1) - 1;

            foreach (var expression in new LambdaExpression[] { addOne, subOne, mixed })
            {
                var transformed = incDecVisitor.Visit(expression);
                Console.WriteLine($"  source     : {expression}");
                Console.WriteLine($"  transformed: {transformed}");
                Console.WriteLine();
            }

            // The transformed trees stay compilable and produce the same results.
            var compiled = (Func<int, int>)((LambdaExpression)incDecVisitor.Visit(addOne)).Compile();
            Console.WriteLine($"  sanity check: (x => Increment(x))(41) = {compiled(41)}");
        }

        // 2. Replace lambda parameters with constants taken from a <name : value> dictionary.
        private static void ParameterToConstantDemo()
        {
            Console.WriteLine("== Transformation 2: parameters -> constants ==");

            Expression<Func<int, int, int>> source = (a, b) => (a + b) * 2;

            var replacements = new Dictionary<string, object>
            {
                ["a"] = 5,
                ["b"] = 10,
            };

            var visitor = new ParameterToConstantVisitor(replacements);
            var transformed = visitor.Visit(source);

            Console.WriteLine($"  source     : {source}");
            Console.WriteLine("  replace    : a = 5, b = 10");
            Console.WriteLine($"  transformed: {transformed}");

            // Now the lambda has no parameters left, so it can be invoked without arguments.
            var compiled = (Func<int>)((LambdaExpression)transformed).Compile();
            Console.WriteLine($"  evaluated  : {compiled()}");
        }
    }
}
