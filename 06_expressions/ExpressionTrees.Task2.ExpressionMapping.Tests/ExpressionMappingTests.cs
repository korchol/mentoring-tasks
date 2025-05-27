using ExpressionTrees.Task2.ExpressionMapping.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpressionTrees.Task2.ExpressionMapping.Tests
{
    [TestClass]
    public class ExpressionMappingTests
    {
        [TestMethod]
        public void Should_Map_Properties_With_Same_Names_And_Types()
        {
            // Arrange
            var mapGenerator = new MappingGenerator();
            var mapper = mapGenerator.Generate<Foo, Bar>();

            var source = new Foo { Id = 1, Name = "Test" };

            // Act
            var result = mapper.Map(source);

            // Assert
            Assert.AreEqual(source.Id, result.Id);
            Assert.AreEqual(source.Name, result.Name);
        }

        [TestMethod]
        public void Should_Ignore_Extra_Properties_In_Source()
        {
            // Arrange
            var mapGenerator = new MappingGenerator();
            var mapper = mapGenerator.Generate<Foo, Bar>();

            var source = new Foo { Id = 1, Name = "Test" };

            // Act
            var result = mapper.Map(source);

            // Assert
            Assert.AreEqual(source.Id, result.Id);
            Assert.AreEqual(source.Name, result.Name);
        }

        [TestMethod]
        public void Should_Ignore_Extra_Properties_In_Destination()
        {
            // Arrange
            var mapGenerator = new MappingGenerator();
            var mapper = mapGenerator.Generate<Bar, Foo>();

            var source = new Bar { Id = 1, Name = "Test" };

            // Act
            var result = mapper.Map(source);

            // Assert
            Assert.AreEqual(source.Id, result.Id);
            Assert.AreEqual(source.Name, result.Name);
        }
    }
}
