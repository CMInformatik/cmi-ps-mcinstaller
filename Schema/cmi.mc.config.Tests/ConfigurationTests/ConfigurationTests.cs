using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace cmi.mc.config.Tests.ConfigurationTests
{
    [TestFixture]
    public class ConfigurationTests
    {
        private static readonly ConfigurationModel Model = new ConfigurationModel();

        private static string GetTestDataPath(string fileName)
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ConfigurationTests", fileName);
        }

        #region object construction

        [Test]
        public void Should_ReturnInstance_When_ReadConfigurationFromFile()
        {
            var c = Configuration.ReadFromFile(GetTestDataPath("test.json"), Model);
            Assert.IsInstanceOf(typeof(Configuration), c);
        }

        [Test]
        public void Should_Throw_When_ConfigurationFileIsNotPresent()
        {
            void D() => Configuration.ReadFromFile(GetTestDataPath("notpresent.json"), Model);
            Assert.Throws(typeof(FileNotFoundException), D);
        }

        [Test]
        public void Should_ReturnInstance_When_ReadConfigurationFromString()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\":{},\"tenant2\":{}  }", Model);
            Assert.IsInstanceOf(typeof(Configuration), c);
        }

        [Test]
        public void Should_Throw_When_JsonStringIsEmpty()
        {
            var testvalues = new[] { null, "", " " };
            foreach (var test in testvalues)
            {
                try
                {
                    var c = Configuration.ReadFromString(test, Model);
                    Assert.Fail($"{nameof(ArgumentNullException)} expected, no exception was thrown");
                }
                catch (Exception e)
                {
                    if (!(e is ArgumentNullException))
                    {
                        Assert.Fail($"{nameof(ArgumentNullException)} expected, but {e.GetType().Name} was thrown");
                    }
                }
            }
        }

        #endregion

        #region tenant handling

        [Test]
        public void Should_ReturnTenantObjects_When_GetTenantIterator()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\":{},\"tenant2\":{},\"tenant3\":{} }", Model);
            var tenants = c.Select(t => t.Name).ToList();
            CollectionAssert.AreEqual(new[] { "tenant1", "tenant2", "tenant3" }, tenants);
        }

        [Test]
        public void Should_ContainTenantWithName_When_TenantWithNameIsAdded()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\":{},\"tenant2\":{},\"tenant3\":{} }", Model);
            c.AddTenant("tenant4");
            
            Assert.AreEqual("tenant4", c["tenant4"].Name);
            Assert.IsNotNull(c.Single(t => t.Name.Equals("tenant4")));
        }

        [Test]
        public void Should_ReturnExistingTenant_When_TenantWithDublicateNameIsAdded()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\":{},\"tenant2\":{},\"tenant3\":{} }", Model);
            c.AddTenant("tenant3");

            Assert.AreEqual("tenant3", c["tenant3"].Name);
            Assert.IsNotNull(c.Single(t => t.Name.Equals("tenant3")));
            Assert.AreEqual(3,c.Count());
        }

        #endregion

    }
}
