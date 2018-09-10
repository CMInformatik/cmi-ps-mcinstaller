using System;
using System.Management.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace cmi.mc.config.Tests
{
    [TestClass]
    public class TenantTests
    {
        private static readonly ConfigurationModel TestModel = new ConfigurationModel();
        private static readonly ValidatorMock Validator = new ValidatorMock();

        private class ValidatorMock : ValidateArgumentsAttribute
        {
            public object Arguments { get; set; }

            protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
            {
                Arguments = arguments;
            }
        }

        [TestInitialize()]
        public void TestInit()
        {
            Validator.Arguments = null;
        }

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            var simple1 = new SimpleAspect("simple1", typeof(string), "simple1");
            var simple2 = new SimpleAspect("simple2", typeof(bool), true);
            var complex1 = new ComplexAspect("complex1", ConfigControlAttribute.NotSet);
            simple1.ValidationAttributes.Add(Validator);
            complex1.AddAspect(simple1);
            complex1.AddAspect(simple2);
            TestModel[App.Common].AddAspect(complex1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Should_RejectValue_When_ValueHasWrongType()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\": { \"Common\":{}}}", TestModel);
            c["tenant1"].SetConfigurationProperty(App.Common, "complex1.simple1", new int());
        }

        [TestMethod]
        public void Should_ExecuteValidators_When_SetConfigProperty()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\": { \"Common\":{}}}", TestModel);
            c["tenant1"].SetConfigurationProperty(App.Common, "complex1.simple1", "some string");
            Assert.AreEqual(Validator.Arguments, "some string");
        }
    }
}
