using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Expressions.Task3.E3SQueryProvider
{
    public class ExpressionToFtsRequestTranslator : ExpressionVisitor
    {
        readonly StringBuilder _resultStringBuilder;

        public ExpressionToFtsRequestTranslator()
        {
            _resultStringBuilder = new StringBuilder();
        }

        public string Translate(Expression exp)
        {
            Visit(exp);

            return _resultStringBuilder.ToString();
        }

        #region protected methods

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(string))
            {
                var memberExpression = node.Object as MemberExpression;
                if (memberExpression == null)
                    throw new NotSupportedException($"Method '{node.Method.Name}' is not supported on non-member expressions.");

                Visit(memberExpression);
                _resultStringBuilder.Append("(");

                var argumentExpression = node.Arguments[0] as ConstantExpression;
                if (argumentExpression == null)
                    throw new NotSupportedException($"Argument for method '{node.Method.Name}' must be a constant.");

                switch (node.Method.Name)
                {
                    case "StartsWith":
                        _resultStringBuilder.Append(argumentExpression.Value);
                        _resultStringBuilder.Append("*");
                        break;

                    case "EndsWith":
                        _resultStringBuilder.Append("*");
                        _resultStringBuilder.Append(argumentExpression.Value);
                        break;

                    case "Contains":
                        _resultStringBuilder.Append("*");
                        _resultStringBuilder.Append(argumentExpression.Value);
                        _resultStringBuilder.Append("*");
                        break;

                    case "Equals":
                        _resultStringBuilder.Append(argumentExpression.Value);
                        break;

                    default:
                        throw new NotSupportedException($"Method '{node.Method.Name}' is not supported.");
                }

                _resultStringBuilder.Append(")");
                return node;
            }
            if (node.Method.DeclaringType == typeof(Queryable)
                && node.Method.Name == "Where")
            {
                var predicate = node.Arguments[1];
                Visit(predicate);

                return node;
            }
            return base.VisitMethodCall(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.AndAlso:
                    _resultStringBuilder.Append("{\"statements\": [");
                    _resultStringBuilder.Append("{\"query\":\"");
                    Visit(node.Left);
                    _resultStringBuilder.Append("\"},");
                    _resultStringBuilder.Append("{\"query\":\"");
                    Visit(node.Right);
                    _resultStringBuilder.Append("\"}]}");
                    break;

                case ExpressionType.Equal:
                    if (node.Left.NodeType == ExpressionType.Constant && node.Right.NodeType == ExpressionType.MemberAccess)
                    {
                        Visit(node.Right);
                        _resultStringBuilder.Append("(");
                        Visit(node.Left);
                        _resultStringBuilder.Append(")");
                    }
                    else if (node.Left.NodeType == ExpressionType.MemberAccess && node.Right.NodeType == ExpressionType.Constant)
                    {
                        Visit(node.Left);
                        _resultStringBuilder.Append("(");
                        Visit(node.Right);
                        _resultStringBuilder.Append(")");
                    }
                    break;

                default:
                    throw new NotSupportedException($"Operation '{node.NodeType}' is not supported");
            };

            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            _resultStringBuilder.Append(node.Member.Name).Append(":");

            return base.VisitMember(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            _resultStringBuilder.Append(node.Value);

            return node;
        }

        #endregion
    }
}
