using System.Linq.Expressions;
using System.Text;

namespace LinqProvider
{
    public class SqlExpressionTranslator
    {
        public string Translate(Expression expression)
        {
            var builder = new StringBuilder("SELECT * FROM [dbo].[books] WHERE ");

            Visit(expression, builder);

            return builder.ToString();
        }

        private void Visit(Expression expression, StringBuilder builder)
        {
            if (expression is UnaryExpression unaryExpression)
            {
                Visit(unaryExpression.Operand, builder);
            }
            else if (expression is LambdaExpression lambdaExpression)
            {
                Visit(lambdaExpression.Body, builder);
            }
            else if (expression is MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Method.Name == "Where")
                {
                    var predicate = methodCallExpression.Arguments[1];
                    Visit(predicate, builder);
                }
                else
                {
                    throw new NotSupportedException($"Method '{methodCallExpression.Method.Name}' is not supported.");
                }
            }
            else if (expression is BinaryExpression binaryExpression)
            {
                Visit(binaryExpression.Left, builder);

                switch (binaryExpression.NodeType)
                {
                    case ExpressionType.AndAlso:
                        builder.Append(" AND ");
                        break;
                    case ExpressionType.Equal:
                        builder.Append(" = ");
                        break;
                    case ExpressionType.GreaterThan:
                        builder.Append(" > ");
                        break;
                    case ExpressionType.LessThan:
                        builder.Append(" < ");
                        break;
                }

                Visit(binaryExpression.Right, builder);
            }
            else if (expression is MemberExpression memberExpression)
            {
                builder.Append($"[{memberExpression.Member.Name}]");
            }
            else if (expression is ConstantExpression constantExpression)
            {
                if (constantExpression.Type == typeof(string))
                {
                    builder.Append($"'{constantExpression.Value}'");
                }
                else
                {
                    builder.Append(constantExpression.Value);
                }
            }
            else
            {
                throw new NotSupportedException($"Expression type '{expression.GetType()}' is not supported.");
            }
        }
    }
}
