using System;
using System.Linq;
using System.Management.Automation;
using cmi.mc.config.SchemaComponents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

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
            var complex1 = new ComplexAspect("complex1");
            simple1.ValidationAttributes.Add(Validator);
            complex1.AddAspect(simple1);
            complex1.AddAspect(simple2);

            var simple3 = new SimpleAspect("simple3", typeof(string), "simple3");
            var complex2 = new ComplexAspect("complex2", ConfigControlAttribute.Extend);
            complex2.AddAspect(simple3);

            TestModel[App.Common].AddAspect(complex1);
            TestModel[App.Dossierbrowser].AddAspect(complex2);
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
            var c = Configuration.ReadFromString("{ \"tenant1\": { \"common\":{}}}", TestModel);
            c["tenant1"].SetConfigurationProperty(App.Common, "complex1.simple1", "some string");
            Assert.AreEqual(Validator.Arguments, "some string");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Should_Throw_When_AppIsNotEnabled()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\": { \"common\":{}}}", TestModel);
            c["tenant1"].SetConfigurationProperty(App.Dossierbrowser, "complex2.simple3", true);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Should_Throw_When_AspectPathIsInvalid()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\": { \"common\":{},  \"dossierbrowser\":{}}}", TestModel);
            c["tenant1"].SetConfigurationProperty(App.Dossierbrowser, "complex2.invalid3", "some string");
        }

        [TestMethod]
        public void Should_ReturnPropertyValue_When_PropertyWasSetBefore()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\": { \"common\":{},  \"dossierbrowser\":{}}}", TestModel);
            c["tenant1"].SetConfigurationProperty(App.Dossierbrowser, "complex2.simple3", "some string");
            var result = c["tenant1"].GetConfigurationPropertyValue(App.Dossierbrowser, "complex2.simple3");
            var result2 = c["tenant1"].GetConfigurationPropertyValue<string>(App.Dossierbrowser, "complex2.simple3");
            Assert.IsInstanceOfType(result, typeof(string));
            Assert.AreEqual("some string", (string)result);
            Assert.AreEqual("some string", result2);
        }

        [TestMethod]
        public void Should_ReturnPropertyValue_When_OverrideExistingValue()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\": { \"common\":{},  \"dossierbrowser\":{ \"complex2\": { \"simple3\": \"old\" } }}}", TestModel);
            c["tenant1"].SetConfigurationProperty(App.Dossierbrowser, "complex2.simple3", "new");
            var result = c["tenant1"].GetConfigurationPropertyValue<string>(App.Dossierbrowser, "complex2.simple3");
            Assert.AreEqual("new", result);
        }

        [TestMethod]
        public void Should_SetDefaultCca_When_NotCCaIsSet()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\": { \"common\":{},  \"dossierbrowser\":{}}}", TestModel);
            c["tenant1"].SetConfigurationProperty(App.Dossierbrowser, "complex2.simple3", "some string");
            var cca = (JValue)JObject.Parse(c.ToString()).SelectTokens("$.tenant1.dossierbrowser.complex2._extend").Single();
            Assert.IsTrue(((bool)cca.Value));
        }

        [TestMethod]
        public void Should_DoesNotChangeCca_When_CCaIsSet()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\": { \"common\":{},  \"dossierbrowser\":{ \"complex2\": { \"_replace\": true } }}}", TestModel);
            c["tenant1"].SetConfigurationProperty(App.Dossierbrowser, "complex2.simple3", "some string");

            var o = JObject.Parse(c.ToString());
            var replace = (JValue)o.SelectTokens("$.tenant1.dossierbrowser.complex2._replace").Single();
            var extend = (JValue) o.SelectTokens("$.tenant1.dossierbrowser.complex2._extend").SingleOrDefault();

            Assert.IsNull(extend);
            Assert.IsTrue((bool)replace.Value);
        }
    }
}
