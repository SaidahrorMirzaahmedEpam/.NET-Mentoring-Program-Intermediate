using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ExpressionTrees.Task1.ExpressionsTransformer
{
    /// <summary>
    /// Transforms a lambda expression by replacing the parameters whose names are listed
    /// in the supplied map with constant values. Parameters that get replaced are also
    /// removed from the lambda's parameter list.
    /// </summary>
    public class ParameterToConstantVisitor : ExpressionVisitor
    {
        private readonly IReadOnlyDictionary<string, object> _replacements;

        /// <param name="replacements">Pairs of &lt;parameter name : value to substitute&gt;.</param>
        public ParameterToConstantVisitor(IReadOnlyDictionary<string, object> replacements)
        {
            _replacements = replacements ?? throw new ArgumentNullException(nameof(replacements));
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node.Name != null && _replacements.TryGetValue(node.Name, out var value))
            {
                // Keep the parameter's declared type so the produced tree stays well-typed.
                return Expression.Constant(value, node.Type);
            }

            return base.VisitParameter(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var body = Visit(node.Body);

            // Drop the parameters that have been turned into constants.
            var remainingParameters = node.Parameters
                .Where(p => p.Name == null || !_replacements.ContainsKey(p.Name))
                .ToArray();

            return Expression.Lambda(body, remainingParameters);
        }
    }
}
