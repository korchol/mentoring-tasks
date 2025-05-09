using Microsoft.Data.SqlClient;
using LinqProvider.Models;

namespace LinqProvider.Test
{
    public class LinqProviderTests
    {
        private readonly string _connectionString = 
            "Server=EPPLGDAW0502\\MSSQLSERVER01;Database=BooksDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public LinqProviderTests()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                EnsureTableExists(connection);
                PopulateDatabaseIfEmpty(connection);
            }
        }

        [Fact]
        public void TestSimpleQuery()
        {
            // Arrange
            var bookSet = new CustomEntitySet<Book>(_connectionString);

            // Act
            // SELECT * FROM [dbo].[Books] WHERE Price > 20
            var result = bookSet.Where(b => b.Price > 20).ToList();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count > 0);
            Assert.True(result[0].Price > 20);
        }

        [Fact]
        public void TestComplexQuery()
        {
            // Arrange
            var bookSet = new CustomEntitySet<Book>(_connectionString);

            // Act
            // SELECT * FROM [dbo].[Books] WHERE Price > 20 AND Genre = 'Fantasy'
            var result = bookSet.Where(b => b.Price > 20 && b.Genre == "Fantasy").ToList();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count > 0);
            Assert.True(result[0].Price > 20);
            Assert.True(result[0].Genre == "Fantasy");
        }


        private void EnsureTableExists(SqlConnection connection)
        {
            // Check if the Books table exists and create it if it doesn't
            var command = new SqlCommand(
                @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Books' AND xtype='U')
                  CREATE TABLE Books (
                      Id INT IDENTITY(1,1) PRIMARY KEY,
                      Title NVARCHAR(255) NOT NULL,
                      Author NVARCHAR(255) NOT NULL,
                      Price DECIMAL(10,2) NOT NULL,
                      Genre NVARCHAR(255) NOT NULL
                  );",
                connection);

            command.ExecuteNonQuery();
        }

        private void PopulateDatabaseIfEmpty(SqlConnection connection)
        {
            // Check if the Books table is empty
            var checkCommand = new SqlCommand("SELECT COUNT(*) FROM Books", connection);
            int rowCount = (int)checkCommand.ExecuteScalar();

            if (rowCount == 0)
            {
                // Insert dummy data
                var populateCommand = new SqlCommand(
                    @"INSERT INTO Books (Title, Author, Price, Genre)
                      VALUES
                      ('The Hobbit', 'J.R.R. Tolkien', 15.99, 'Fantasy'),
                      ('1984', 'George Orwell', 8.99, 'Dystopian'),
                      ('To Kill a Mockingbird', 'Harper Lee', 10.99, 'Fiction'),
                      ('The Catcher in the Rye', 'J.D. Salinger', 9.99, 'Fiction'),
                      ('A Game of Thrones', 'George R.R. Martin', 25.99, 'Fantasy'),
                      ('The Great Gatsby', 'F. Scott Fitzgerald', 12.99, 'Classics'),
                      ('Pride and Prejudice', 'Jane Austen', 7.99, 'Romance');",
                    connection);

                populateCommand.ExecuteNonQuery();
            }
        }
    }
}