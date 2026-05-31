using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Expressions.Task3.E3SQueryProvider
{
    public class ExpressionToFtsRequestTranslator : ExpressionVisitor
    {
        private readonly List<string> _statements = new List<string>();
        private StringBuilder _currentStatement = new StringBuilder();

        public IEnumerable<string> Translate(Expression exp)
        {
            Visit(exp);
            FlushCurrentStatement();
            return _statements;
        }

        #region protected methods

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable) && node.Method.Name == "Where")
            {
                Visit(node.Arguments[1]);
                return node;
            }

            if (node.Method.DeclaringType == typeof(string))
            {
                MemberExpression member;
                ConstantExpression constant;

                switch (node.Method.Name)
                {
                    case "Equals":
                        // instance.Equals(value)
                        member = (MemberExpression)node.Object;
                        constant = (ConstantExpression)node.Arguments[0];
                        Visit(member);
                        _currentStatement.Append("(");
                        Visit(constant);
                        _currentStatement.Append(")");
                        return node;

                    case "StartsWith":
                        member = (MemberExpression)node.Object;
                        constant = (ConstantExpression)node.Arguments[0];
                        Visit(member);
                        _currentStatement.Append("(");
                        Visit(constant);
                        _currentStatement.Append("*)");
                        return node;

                    case "EndsWith":
                        member = (MemberExpression)node.Object;
                        constant = (ConstantExpression)node.Arguments[0];
                        Visit(member);
                        _currentStatement.Append("(*");
                        Visit(constant);
                        _currentStatement.Append(")");
                        return node;

                    case "Contains":
                        member = (MemberExpression)node.Object;
                        constant = (ConstantExpression)node.Arguments[0];
                        Visit(member);
                        _currentStatement.Append("(*");
                        Visit(constant);
                        _currentStatement.Append("*)");
                        return node;
                }
            }

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Equal:
                    MemberExpression memberExpr;
                    ConstantExpression constExpr;

                    if (node.Left.NodeType == ExpressionType.MemberAccess && node.Right.NodeType == ExpressionType.Constant)
                    {
                        memberExpr = (MemberExpression)node.Left;
                        constExpr = (ConstantExpression)node.Right;
                    }
                    else if (node.Right.NodeType == ExpressionType.MemberAccess && node.Left.NodeType == ExpressionType.Constant)
                    {
                        memberExpr = (MemberExpression)node.Right;
                        constExpr = (ConstantExpression)node.Left;
                    }
                    else
                    {
                        throw new NotSupportedException($"Equality requires one member access and one constant operand: {node}");
                    }

                    Visit(memberExpr);
                    _currentStatement.Append("(");
                    Visit(constExpr);
                    _currentStatement.Append(")");
                    break;

                case ExpressionType.AndAlso:
                    Visit(node.Left);
                    FlushCurrentStatement();
                    Visit(node.Right);
                    break;

                default:
                    throw new NotSupportedException($"Operation '{node.NodeType}' is not supported");
            }

            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            _currentStatement.Append(node.Member.Name).Append(":");
            return base.VisitMember(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            _currentStatement.Append(node.Value);
            return node;
        }

        #endregion

        private void FlushCurrentStatement()
        {
            if (_currentStatement.Length > 0)
            {
                _statements.Add(_currentStatement.ToString());
                _currentStatement = new StringBuilder();
            }
        }
    }
}
