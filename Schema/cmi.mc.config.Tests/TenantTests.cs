﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using cmi.mc.config.SchemaComponents;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace cmi.mc.config.Tests
{
    [TestFixture]
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

        [SetUp]
        public void TestInit()
        {
            Validator.Arguments = null;
        }

        [OneTimeSetUp]
        public static void ClassInit()
        {
            var simple1 = new SimpleAspect("simple1", typeof(string), "simple1");
            var simple2 = new SimpleAspect("simple2", typeof(bool), true);
            var complex1 = new ComplexAspect("complex1");
            simple1.AddValidationAttribute(Validator);
            complex1.AddAspect(simple1);
            complex1.AddAspect(simple2);

            var simple3 = new SimpleAspect("simple3", typeof(string), "simple3");
            var complex2 = new ComplexAspect("complex2", ConfigControlAttribute.Extend);
            complex2.AddAspect(simple3);

            TestModel[App.Common].AddAspect(complex1);
            TestModel[App.Dossierbrowser].AddAspect(complex2);
        }

        #region service base url
        [Test]
        public void Should_UseUrlFromConfig_When_ServerPropertyIsSet()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\": { \"common\":{ \"api\": { \"server\": \"https://my.service.ch:6000/mobileclients\"}}}}", TestModel);            
            Assert.AreEqual("https://my.service.ch:6000/", c["tenant1"].ServiceBaseUrl.ToString());
        }

        [Test]
        public void Should_UseKnownDefaultUrl_When_ServerPropertyIsNotSet()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\": { \"common\":{ \"api\": {}}}}", TestModel);
            Assert.AreEqual(TestModel.DefaultServiceUrl, c["tenant1"].ServiceBaseUrl);
        }

        [Test]
        public void Should_EnablesCommonApp_When_IsNotPresent()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\": {}}", TestModel);
            Assert.That(c["tenant1"].Has(App.Common), Is.True);
            Assert.That(c["tenant1"].Get(App.Common, "api.server").ToString(), Is.EqualTo($"{TestModel.DefaultServiceUrl}mobileclients"));
            Assert.That(c["tenant1"].Has(App.Common), Is.True);
        }

        #endregion

        #region Get/Set
        [Test]
        public void Should_RejectValue_When_ValueHasWrongType()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\": { \"common\":{}}}", TestModel);
            void D() => c["tenant1"].Set(App.Common, "complex1.simple1", new int());
            Assert.Throws(typeof(ArgumentException), D);
        }

        [Test]
        public void Should_ExecuteValidators_When_SetConfigProperty()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\": { \"common\":{}}}", TestModel);
            c["tenant1"].Set(App.Common, "complex1.simple1", "some string");
            Assert.AreEqual(Validator.Arguments, "some string");
        }

        [Test]
        public void Should_Throw_When_AppIsNotEnabled()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\": { \"common\":{}}}", TestModel);
            void D() => c["tenant1"].Set(App.Dossierbrowser, "complex2.simple3", true);
            Assert.Throws(typeof(InvalidOperationException), D);
        }

        [Test]
        public void Should_Throw_When_AspectPathIsInvalid()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\": { \"common\":{},  \"dossierbrowser\":{}}}", TestModel);
            void D() => c["tenant1"].Set(App.Dossierbrowser, "complex2.invalid3", "some string");
            Assert.Throws(typeof(KeyNotFoundException), D);
        }

        [Test]
        public void Should_ReturnPropertyValue_When_PropertyWasSetBefore()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\": { \"common\":{},  \"dossierbrowser\":{}}}", TestModel);
            c["tenant1"].Set(App.Dossierbrowser, "complex2.simple3", "some string");
            var result = c["tenant1"].Get(App.Dossierbrowser, "complex2.simple3");
            var result2 = c["tenant1"].Get<string>(App.Dossierbrowser, "complex2.simple3");
            Assert.IsInstanceOf(typeof(string), result);
            Assert.AreEqual("some string", (string)result);
            Assert.AreEqual("some string", result2);
        }

        [Test]
        public void Should_ReturnPropertyValue_When_OverrideExistingValue()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\": { \"common\":{},  \"dossierbrowser\":{ \"complex2\": { \"simple3\": \"old\" } }}}", TestModel);
            c["tenant1"].Set(App.Dossierbrowser, "complex2.simple3", "new");
            var result = c["tenant1"].Get<string>(App.Dossierbrowser, "complex2.simple3");
            Assert.AreEqual("new", result);
        }

        [Test]
        public void Should_SetDefaultCca_When_NotCCaIsSet()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\": { \"common\":{},  \"dossierbrowser\":{}}}", TestModel);
            c["tenant1"].Set(App.Dossierbrowser, "complex2.simple3", "some string");
            var cca = (JValue)JObject.Parse(c.ToString()).SelectTokens("$.tenant1.dossierbrowser.complex2._extend").Single();
            Assert.IsTrue(((bool)cca.Value));
        }

        [Test]
        public void Should_DoesNotChangeCca_When_CCaIsSet()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\": { \"common\":{},  \"dossierbrowser\":{ \"complex2\": { \"_replace\": true } }}}", TestModel);
            c["tenant1"].Set(App.Dossierbrowser, "complex2.simple3", "some string");

            var o = JObject.Parse(c.ToString());
            var replace = (JValue)o.SelectTokens("$.tenant1.dossierbrowser.complex2._replace").Single();
            var extend = (JValue) o.SelectTokens("$.tenant1.dossierbrowser.complex2._extend").SingleOrDefault();

            Assert.IsNull(extend);
            Assert.IsTrue((bool)replace.Value);
        }

        [Test]
        public void Should_SetPropertyPlatformSpecific_When_PlatformIsSpecificed()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\": { \"common\":{},  \"dossierbrowser\":{}}}", TestModel);
            c["tenant1"].Set(App.Dossierbrowser, "complex2.simple3", "some string", false, Platform.App);
            var cca = (JValue)JObject.Parse(c.ToString()).SelectTokens("$.tenant1.dossierbrowser.complex2.app.simple3").Single();
            Assert.That(((string)cca.Value), Is.EqualTo("some string"));
        }

        [Test]
        public void Should_SetPropertyPlatformUnspecific_When_PlatformIsNotSpecificed()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\": { \"common\":{},  \"dossierbrowser\":{ \"complex2\": { \"app\": { \"simple3\": \"a\" } } }}}", TestModel);
            c["tenant1"].Set(App.Dossierbrowser, "complex2.simple3", "some string", false, Platform.Unspecified);
            var val = (JValue)JObject.Parse(c.ToString()).SelectTokens("$.tenant1.dossierbrowser.complex2.simple3").Single();
            Assert.That(((string)val.Value), Is.EqualTo("some string"));
        }

        [Test]
        public void Should_NotSetPropertyPlatformUnspecific_When_UnspecificValueIsSame()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\": { \"common\":{},  \"dossierbrowser\":{ \"complex2\": { \"app\": { \"simple3\": \"b\" }, \"simple3\": \"a\" }}}}", TestModel);
            c["tenant1"].Set(App.Dossierbrowser, "complex2.simple3", "a", false, Platform.App);
            var val1 = (JValue)JObject.Parse(c.ToString()).SelectTokens("$.tenant1.dossierbrowser.complex2.simple3").Single();
            var val2 = JObject.Parse(c.ToString()).SelectTokens("$.tenant1.dossierbrowser.complex2.app.simple3").SingleOrDefault();
            Assert.That(((string)val1.Value), Is.EqualTo("a"));
            Assert.That(val2, Is.Null);
        }

        [Test]
        public void Should_RemovePlatformUnspecific_When_AllPlatformHaveSameValue()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\": { \"common\":{},  \"dossierbrowser\":{ \"complex2\": { \"app\": { \"simple3\": \"a\" } } }}}", TestModel);
            c["tenant1"].Set(App.Dossierbrowser, "complex2.simple3", "a", false, Platform.Web);
            var cca = (JValue)JObject.Parse(c.ToString()).SelectTokens("$.tenant1.dossierbrowser.complex2.simple3").Single();
            Assert.That(((string)cca.Value), Is.EqualTo("a"));
        }

        [Test]
        public void Should_GetPropertyPlatformSpecific_When_PlatformIsSpecificed()
        {
            const string json = @"
            {
	            ""tenant1"": {
		            ""common"": {},
		            ""dossierbrowser"": {
			            ""complex2"" : {
				            ""app"" : {
					            ""simple3"" : ""p_app""
				            },
				            ""web"" : {
					            ""simple3"" : ""p_web""
				            },
				            ""simple3"": ""p_unspec""
			            }
		            }
	            }
            }";

            var c = Configuration.ReadFromString(json, TestModel);
            var appVal =  c["tenant1"].Get(App.Dossierbrowser, "complex2.simple3", Platform.App);
            var webVal = c["tenant1"].Get(App.Dossierbrowser, "complex2.simple3", Platform.Web);
            var unspecVal = c["tenant1"].Get(App.Dossierbrowser, "complex2.simple3", Platform.Unspecified);
            
            Assert.That(appVal, Is.EqualTo("p_app"));
            Assert.That(webVal, Is.EqualTo("p_web"));
            Assert.That(unspecVal, Is.EqualTo("p_unspec"));

        }

        #endregion

        #region has

        [Test]
        public void Should_ReturnTrue_When_RequestedPlatformSpecificPropertyIsSet()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\": { \"common\":{},  \"dossierbrowser\":{ \"complex2\": { \"app\": { \"simple3\": \"a\" } } }}}", TestModel);
            var result = c["tenant1"].Has(App.Dossierbrowser, "complex2.simple3", Platform.App);
            Assert.That(result, Is.True);
        }

        [Test]
        public void Should_ReturnTrue_When_UnspecificPlatformPropertyIsSetAndSpecificRequested()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\": { \"common\":{},  \"dossierbrowser\":{ \"complex2\": { \"simple3\": \"a\" }}}}", TestModel);
            var result = c["tenant1"].Has(App.Dossierbrowser, "complex2.simple3", Platform.App);
            Assert.That(result, Is.True);
        }

        [Test]
        public void Should_ReturnTrue_When_UnspecificPlatformPropertyIsSet()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\": { \"common\":{},  \"dossierbrowser\":{ \"complex2\": { \"simple3\": \"a\" }}}}", TestModel);
            var result = c["tenant1"].Has(App.Dossierbrowser, "complex2.simple3");
            Assert.That(result, Is.True);
        }

        [Test]
        public void Should_ReturnFalse_When_PropertyIsNotSet()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\": { \"common\":{},  \"dossierbrowser\":{ \"complex2\": { }}}}", TestModel);
            var result = c["tenant1"].Has(App.Dossierbrowser, "complex2.simple3");
            Assert.That(result, Is.False);
        }

        [Test]
        public void Should_ReturnTrue_When_SpecificPlatformPropertyIsSetAndUnspecificRequested()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\": { \"common\":{},  \"dossierbrowser\":{ \"complex2\": { \"app\": { \"simple3\": \"a\" } } }}}", TestModel);
            var result = c["tenant1"].Has(App.Dossierbrowser, "complex2.simple3");
            Assert.That(result, Is.False);
        }

        #endregion

        #region remove

        [Test]
        public void Should_RemovePropertyPlatformSpecific_When_PlatformIsSpecificed()
        {
            const string json = @"
            {
	            ""tenant1"": {
		            ""common"": {},
		            ""dossierbrowser"": {
			            ""complex2"" : {
				            ""app"" : {
					            ""simple3"" : ""p_app""
				            },
				            ""web"" : {
					            ""simple3"" : ""p_web""
				            },
				            ""simple3"": ""p_unspec""
			            }
		            }
	            }
            }";

            foreach (var pl in (Platform[])Enum.GetValues(typeof(Platform)))
            {
                var c = Configuration.ReadFromString(json, TestModel);
                c["tenant1"].Remove(App.Dossierbrowser, "complex2.simple3", pl);

                foreach(var pl2 in (Platform[])Enum.GetValues(typeof(Platform)))
                {
                    var r= c["tenant1"].Get(App.Dossierbrowser, "complex2.simple3", pl2);
                    Assert.That(r, pl == pl2 ? Is.Null : Is.Not.Null);
                }
            }
        }

        [Test]
        public void Should_RemoveComplexProperty_When_SelectComplexAspectToRemove()
        {
            const string json = @"
            {
	            ""tenant1"": {
		            ""common"": {},
		            ""dossierbrowser"": {
			            ""complex2"" : {
				            ""app"" : {
					            ""simple3"" : ""p_app""
				            },
				            ""web"" : {
					            ""simple3"" : ""p_web""
				            },
				            ""simple3"": ""p_unspec""
			            }
		            }
	            }
            }";

            var c = Configuration.ReadFromString(json, TestModel);
            c["tenant1"].Remove(App.Dossierbrowser, "complex2");

            var complex2 = JObject.Parse(c.ToString()).SelectTokens("$.tenant1.dossierbrowser.complex2").SingleOrDefault();
            var dossierbrowser = JObject.Parse(c.ToString()).SelectTokens("$.tenant1.dossierbrowser").SingleOrDefault();

            Assert.That(complex2, Is.Null);
            Assert.That(dossierbrowser, Is.Not.Null);
        }

        #endregion

        [Test]
        public void Should_UpdateAppDirectory_When_EnabledApp()
        {
            var c = Configuration.ReadFromString("{ \"tenant1\": { \"common\":{} }}", TestModel);
            c["tenant1"].Add(App.Dossierbrowser);

            var o = JObject.Parse(c.ToString());
            Assert.That(()=> o.SelectTokens("$.tenant1.common.appDirectory.dossierbrowser").Single(), Is.Not.Null);
        }
    }
}
