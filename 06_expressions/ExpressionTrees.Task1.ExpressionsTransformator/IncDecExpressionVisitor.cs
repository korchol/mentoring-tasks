using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpressionTrees.Task1.ExpressionsTransformer
{
    public class IncDecExpressionVisitor : ExpressionVisitor
    {
        private readonly Dictionary<string, object> _parameterReplace;

        public IncDecExpressionVisitor(Dictionary<string, object> parameterReplace)
        {
            _parameterReplace = parameterReplace;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var newBody = Visit(node.Body);

            return Expression.Lambda(newBody, new ParameterExpression[0]);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.Right is ConstantExpression constant && constant.Value is int value && value == 1)
            {
                if (node.NodeType == ExpressionType.Add)
                {
                    return Expression.Increment(Visit(node.Left));
                }
                if (node.NodeType == ExpressionType.Subtract)
                {
                    return Expression.Decrement(Visit(node.Left));
                }
            }

            return base.VisitBinary(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (_parameterReplace.TryGetValue(node.Name, out var replacementValue))
            {
                return Expression.Constant(replacementValue, node.Type);
            }

            return base.VisitParameter(node);
        }
    }
}
