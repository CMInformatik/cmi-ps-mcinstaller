using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using cmi.mc.config.ModelContract;
using cmi.mc.config.ModelContract.Components;
using Moq;
using NUnit.Framework;

namespace cmi.mc.config.Tests
{
    [TestFixture]
    public class JsonConfigurationTests
    {
        private static readonly DefaultSchema.DefaultSchema Schema = new DefaultSchema.DefaultSchema();

        private static string GetTestDataPath(string fileName)
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fileName);
        }

        #region object construction

        [Test]
        public void Should_ReturnInstance_When_ReadConfigurationFromFile()
        {
            var c = JsonConfiguration.ReadFromFile(GetTestDataPath("test.json"), Schema);
            Assert.That(c, Is.Not.Null);
            Assert.IsInstanceOf(typeof(JsonConfiguration), c);
        }

        [Test]
        public void Should_Throw_When_ConfigurationFileIsNotPresent()
        {
            void D() => JsonConfiguration.ReadFromFile(GetTestDataPath("notpresent.json"), Schema);
            Assert.Throws(typeof(FileNotFoundException), D);
        }

        [Test]
        public void Should_ReturnInstance_When_ReadConfigurationFromString()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{\"tenant1\":{},\"tenant2\":{}}}", Schema);
            Assert.IsInstanceOf(typeof(JsonConfiguration), c);
        }

        [Test]
        public void Should_Throw_When_JsonStringIsEmpty()
        {
            var testvalues = new[] { null, "", " " };
            foreach (var test in testvalues)
            {
                try
                {
                    var c = JsonConfiguration.ReadFromString(test, Schema);
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
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{\"tenant1\":{},\"tenant2\":{},\"tenant3\":{}}}", Schema);
            var tenants = c.Select(t => t.Name).ToList();
            CollectionAssert.AreEqual(new[] { "tenant1", "tenant2", "tenant3" }, tenants);
        }

        [Test]
        public void Should_ContainTenantWithName_When_TenantWithNameIsAdded()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{\"tenant1\":{},\"tenant2\":{},\"tenant3\":{}}}", Schema);
            c.AddTenant("tenant4");

            Assert.AreEqual("tenant4", c["tenant4"].Name);
            Assert.IsNotNull(c.Single(t => t.Name.Equals("tenant4")));
        }

        [Test]
        public void Should_ReturnExistingTenant_When_TenantWithDublicateNameIsAdded()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{\"tenant1\":{},\"tenant2\":{},\"tenant3\":{}}}", Schema);
            c.AddTenant("tenant3");

            Assert.AreEqual("tenant3", c["tenant3"].Name);
            Assert.IsNotNull(c.Single(t => t.Name.Equals("tenant3")));
            Assert.AreEqual(3, c.Count());
        }

        #endregion


        #region jpath build
        [Test]
        public void Should_ReturnJPathString_When_BuildJPathWithParameters()
        {
            var simpleAspect = new Mock<ISimpleAspect>();
            var parent = new Mock<ISimpleAspect>();
            parent.Setup(s => s.GetAspectPath()).Returns("test");
            parent.Setup(s => s.Parent).Returns((IAspect)null);
            parent.Setup(s => s.Name).Returns("test");
            simpleAspect.Setup(s => s.GetAspectPath()).Returns("test.path");
            simpleAspect.Setup(s => s.Parent).Returns(parent.Object);
            simpleAspect.Setup(s => s.Name).Returns("path");

            var testcases = new List<Tuple<Func<string>, string>>()
            {
                new Tuple<Func<string>, string>(
                    () => JsonConfiguration.BuildJPath("tenant1", App.Common, null, Platform.Unspecified), "$.tenants.tenant1.common"),
                new Tuple<Func<string>, string>(
                    () => JsonConfiguration.BuildJPath("tenant2", App.Common, null, Platform.App), "$.tenants.tenant2.common"),
                new Tuple<Func<string>, string>(
                    () => JsonConfiguration.BuildJPath("tenant3", App.Common, null, Platform.Web), "$.tenants.tenant3.common"),
                new Tuple<Func<string>, string>(
                    () => JsonConfiguration.BuildJPath("tenant4", App.Common, simpleAspect.Object, Platform.Unspecified), "$.tenants.tenant4.common.test.path"),
                new Tuple<Func<string>, string>(
                    () => JsonConfiguration.BuildJPath("tenant5", App.Common, simpleAspect.Object, Platform.App), "$.tenants.tenant5.common.test.app.path"),
                new Tuple<Func<string>, string>(
                    () => JsonConfiguration.BuildJPath("tenant6", App.Common, simpleAspect.Object, Platform.Web), "$.tenants.tenant6.common.test.web.path"),
                new Tuple<Func<string>, string>(
                    () => JsonConfiguration.BuildJPath("tenant7", App.Dossierbrowser, simpleAspect.Object, Platform.Unspecified), "$.tenants.tenant7.dossierbrowser.test.path"),
                new Tuple<Func<string>, string>(
                    () => JsonConfiguration.BuildJPath("tenant8", App.Dossierbrowser, parent.Object, Platform.App), "$.tenants.tenant8.dossierbrowser.test")
            };

            foreach (var test in testcases)
            {
                Assert.That(test.Item1.Invoke(), Is.EqualTo(test.Item2));
            }
        }

    }

    #endregion
}
