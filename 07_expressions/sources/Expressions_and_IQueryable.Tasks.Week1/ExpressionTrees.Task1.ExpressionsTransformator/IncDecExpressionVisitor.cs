using System.Linq.Expressions;

namespace ExpressionTrees.Task1.ExpressionsTransformer
{
    /// <summary>
    /// Transforms an expression tree by replacing binary expressions of the form
    /// <c>&lt;operand&gt; + 1</c> with an increment operation and <c>&lt;operand&gt; - 1</c>
    /// with a decrement operation.
    /// </summary>
    public class IncDecExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitBinary(BinaryExpression node)
        {
            // Transform the children first so that nested patterns (e.g. (a + 1) - 1) are handled too.
            var left = Visit(node.Left);
            var right = Visit(node.Right);

            if (right is ConstantExpression constant && IsOne(constant))
            {
                switch (node.NodeType)
                {
                    case ExpressionType.Add:
                    case ExpressionType.AddChecked:
                        return Expression.Increment(left);

                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractChecked:
                        return Expression.Decrement(left);
                }
            }

            return node.Update(left, node.Conversion, right);
        }

        private static bool IsOne(ConstantExpression constant)
        {
            switch (constant.Value)
            {
                case sbyte v: return v == 1;
                case byte v: return v == 1;
                case short v: return v == 1;
                case ushort v: return v == 1;
                case int v: return v == 1;
                case uint v: return v == 1;
                case long v: return v == 1;
                case ulong v: return v == 1;
                case float v: return v == 1;
                case double v: return v == 1;
                case decimal v: return v == 1;
                default: return false;
            }
        }
    }
}
