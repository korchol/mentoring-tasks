using System.Linq.Expressions;
using Microsoft.Data.SqlClient;

namespace LinqProvider
{
    public class CustomQueryProvider<T> : IQueryProvider
    {
        private readonly string _connectionString;

        public CustomQueryProvider(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new CustomEntitySet<TElement>(expression, this);
        }

        public object? Execute(Expression expression)
        {
            throw new NotImplementedException();
        }

        public TResult Execute<TResult>(Expression expression)
        {
            var translator = new SqlExpressionTranslator();
            string sqlQuery = translator.Translate(expression);

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(sqlQuery, connection);

                using (var reader = command.ExecuteReader())
                {
                    var results = new List<T>();

                    while (reader.Read())
                    {
                        results.Add(MapReaderToEntity(reader));
                    }

                    return (TResult)(object)results;
                }
            }
        }
        private T MapReaderToEntity(SqlDataReader reader)
        {
            var entity = Activator.CreateInstance<T>();

            foreach (var prop in typeof(T).GetProperties())
            {
                prop.SetValue(entity, reader[prop.Name] == DBNull.Value ? null : reader[prop.Name]);
            }

            return entity;
        }
    }
}
