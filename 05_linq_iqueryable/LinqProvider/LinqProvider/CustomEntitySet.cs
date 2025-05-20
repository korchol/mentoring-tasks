using System.Collections;
using System.Linq.Expressions;

namespace LinqProvider
{
    public class CustomEntitySet<T> : IQueryable<T>
    {
        private readonly Expression _expression;
        private readonly IQueryProvider _provider;

        public CustomEntitySet(string connectionString)
        {
            _expression = Expression.Constant(this); // TODO: Initialize with the current instance
            _provider = new CustomQueryProvider<T>(connectionString);
        }

        public CustomEntitySet(Expression expression, IQueryProvider provider)
        {
            _expression = expression;
            _provider = provider;
        }

        public Type ElementType => typeof(T);

        public Expression Expression => _expression;

        public IQueryProvider Provider => _provider;

        public IEnumerator<T> GetEnumerator()
        {
            var results = Provider.Execute<IEnumerable<T>>(Expression);
            return results.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
