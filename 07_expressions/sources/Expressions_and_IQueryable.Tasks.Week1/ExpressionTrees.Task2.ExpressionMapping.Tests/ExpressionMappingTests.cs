using System;
using ExpressionTrees.Task2.ExpressionMapping.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpressionTrees.Task2.ExpressionMapping.Tests
{
    [TestClass]
    public class ExpressionMappingTests
    {
        [TestMethod]
        public void Generate_ReturnsNonNullDestinationInstance()
        {
            var mapGenerator = new MappingGenerator();
            var mapper = mapGenerator.Generate<Foo, Bar>();

            var res = mapper.Map(new Foo());

            Assert.IsNotNull(res);
            Assert.IsInstanceOfType(res, typeof(Bar));
        }

        [TestMethod]
        public void Map_CopiesPropertiesWithMatchingNameAndType()
        {
            var mapper = new MappingGenerator().Generate<Foo, Bar>();

            var source = new Foo { Id = 42, Name = "Joe" };
            var result = mapper.Map(source);

            Assert.AreEqual(42, result.Id);
            Assert.AreEqual("Joe", result.Name);
        }

        [TestMethod]
        public void Map_ConvertsMatchingNameWithWideningType()
        {
            // Foo.Age is int, Bar.Age is long -> conversion is inserted automatically.
            var mapper = new MappingGenerator().Generate<Foo, Bar>();

            var result = mapper.Map(new Foo { Age = 30 });

            Assert.AreEqual(30L, result.Age);
        }

        [TestMethod]
        public void Map_LeavesUnmatchedDestinationPropertiesAtDefault()
        {
            var mapper = new MappingGenerator().Generate<Foo, Bar>();

            var result = mapper.Map(new Foo { Salary = 100 });

            // No "Unmapped" / "SalaryText" / "BirthYear" source members in the default mapping.
            Assert.IsNull(result.Unmapped);
            Assert.IsNull(result.SalaryText);
            Assert.AreEqual(0, result.BirthYear);
        }

        [TestMethod]
        public void Map_UsesCustomMappingForDifferentNameAndType()
        {
            // Salary (double) -> SalaryText (string), BirthDate -> BirthYear (int).
            var mapper = new MappingGenerator<Foo, Bar>()
                .ForMember(dst => dst.SalaryText, src => src.Salary.ToString())
                .ForMember(dst => dst.BirthYear, src => src.BirthDate.Year)
                .Generate();

            var source = new Foo
            {
                Salary = 1234.5,
                BirthDate = new DateTime(1990, 5, 1),
            };
            var result = mapper.Map(source);

            Assert.AreEqual(source.Salary.ToString(), result.SalaryText);
            Assert.AreEqual(1990, result.BirthYear);
        }

        [TestMethod]
        public void Map_CustomMappingAndConventionCoexist()
        {
            var mapper = new MappingGenerator<Foo, Bar>()
                .ForMember(dst => dst.SalaryText, src => src.Salary.ToString())
                .Generate();

            var source = new Foo
            {
                Id = 7,
                Name = "Anna",
                Age = 25,
                Salary = 999,
            };
            var result = mapper.Map(source);

            // Convention-based members are still mapped alongside the custom one.
            Assert.AreEqual(7, result.Id);
            Assert.AreEqual("Anna", result.Name);
            Assert.AreEqual(25L, result.Age);
            Assert.AreEqual("999", result.SalaryText);
        }

        [TestMethod]
        public void ForMember_OverridesConventionForMatchingName()
        {
            // Id exists in both types; the explicit configuration must take precedence.
            var mapper = new MappingGenerator<Foo, Bar>()
                .ForMember(dst => dst.Id, src => src.Id * 10)
                .Generate();

            var result = mapper.Map(new Foo { Id = 5 });

            Assert.AreEqual(50, result.Id);
        }
    }
}
