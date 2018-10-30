using System;
using System.Collections.Generic;
using System.Linq;
using cmi.mc.config.ModelContract;
using cmi.mc.config.ModelContract.Components;
using cmi.mc.config.ModelContract.Exceptions;
using cmi.mc.config.ModelImpl;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace cmi.mc.config.Tests.ModelImpl
{
    [TestFixture()]
    public class AppConfigurationTests
    {
        private static readonly DefaultSchema.DefaultSchema TestSchema = new DefaultSchema.DefaultSchema();
        private static Mock<IValidator<string>> _validator;

        [OneTimeSetUp]
        public static void ClassInit()
        {
            _validator = new Mock<IValidator<string>>();
            var sum = new Mock<ValidationResult>();
            sum.Setup(s => s.IsValid).Returns(true);
            _validator.Setup(v => v.Validate(It.IsAny<object>())).Returns(sum.Object);

            var complex1 = new ComplexAspect("complex1").AddAspect(
                new SimpleAspect<string>("simple1", "simple1", AxSupport.R16_1, _validator.Object) { IsPlatformSpecific = true },
                new SimpleAspect<bool>("simple2", true) { IsPlatformSpecific = true });

            var simple3 = new SimpleAspect<string>("simple3", "simple3") {IsPlatformSpecific = true};
            simple3.SetDefaultValue(Platform.App, "simple3app");
            simple3.SetDefaultValue(Platform.Web, "simple3web");
            var complex2 = new ComplexAspect("complex2", ConfigControlAttribute.Extend).AddAspect(simple3);

            var complex3 = new ComplexAspect("complex3").AddAspect(
                new SimpleAspect<int>("int", 1),
                new SimpleAspect<Uri>("uri", new Uri("https://uri.ch")),
                new SimpleAspect<string>("string", "string"),
                new SimpleAspect<string[]>("stringArray", new[] { "1", "2" })
            );

            ((AppAspect)TestSchema[App.Common]).AddAspect(complex1, complex3);
            ((AppAspect)TestSchema[App.Dossierbrowser]).AddAspect(complex2);
        }


        #region Get/Set
        [Test]
        [Category("Set")]
        public void Should_RejectValue_When_ValueHasWrongType()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{ \"tenant1\": { \"common\":{}}}}", TestSchema);
            void D() => c["tenant1"][App.Common].Set("complex1.simple1", new int());
            Assert.Throws(typeof(ValueValidationException), D);
        }

        [Test]
        [Category("Set")]
        public void Should_ExecuteValidators_When_SetConfigProperty()
        {
            int calls = 0;
            _validator.Setup(v => v.Validate(It.IsAny<object>())).Callback(() => calls++);

            var c = JsonConfiguration.ReadFromString("{\"tenants\":{\"tenant1\": { \"common\":{}}}}", TestSchema);
            c["tenant1"][App.Common].Set("complex1.simple1", "some string");
            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        [Category("Set")]
        public void Should_Throw_When_AppIsNotEnabled()
        {
            var c = JsonConfiguration.ReadFromString("{ \"tenants\":{\"tenant1\": { \"common\":{}}}}", TestSchema);
            void D() => c["tenant1"][App.Dossierbrowser].Set("complex2.simple3", true);
            Assert.Throws(typeof(KeyNotFoundException), D);
        }

        [Test]
        [Category("Set")]
        public void Should_Throw_When_AspectPathIsInvalid()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{\"tenant1\": { \"common\":{},  \"dossierbrowser\":{}}}}", TestSchema);
            void D() => c["tenant1"][App.Dossierbrowser].Set("complex2.invalid3", "some string");
            Assert.Throws(typeof(KeyNotFoundException), D);
        }

        [Test]
        [Category("Set")]
        [Category("Get")]
        public void Should_ReturnPropertyValue_When_PropertyWasSetBefore()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{\"tenant1\": { \"common\":{},  \"dossierbrowser\":{}}}}", TestSchema);
            c["tenant1"][App.Dossierbrowser].Set("complex2.simple3", "some string");
            var result = c["tenant1"][App.Dossierbrowser].Get("complex2.simple3");
            var result2 = c["tenant1"][App.Dossierbrowser].Get<string>("complex2.simple3");
            Assert.IsInstanceOf(typeof(string), result);
            Assert.AreEqual("some string", (string)result);
            Assert.AreEqual("some string", result2);
        }

        [Test]
        [Category("Set")]
        [Category("Get")]
        public void Should_ReturnPropertyValue_When_OverrideExistingValue()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{\"tenant1\": { \"common\":{},  \"dossierbrowser\":{ \"complex2\": { \"simple3\": \"old\" }}}}}", TestSchema);
            c["tenant1"][App.Dossierbrowser].Set("complex2.simple3", "new");
            var result = c["tenant1"][App.Dossierbrowser].Get<string>("complex2.simple3");
            Assert.AreEqual("new", result);
        }

        [Test]
        [Category("Set")]
        public void Should_SetDefaultCca_When_NotCCaIsSet()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{\"tenant1\": { \"common\":{},  \"dossierbrowser\":{}}}}", TestSchema);
            c["tenant1"][App.Dossierbrowser].Set("complex2.simple3", "some string");
            var cca = (JValue)JObject.Parse(c.ToString()).SelectTokens("$.tenants.tenant1.dossierbrowser.complex2._extend").Single();
            Assert.IsTrue(((bool)cca.Value));
        }

        [Test]
        [Category("Set")]
        public void Should_DoesNotChangeCca_When_CCaIsSet()
        {
            var c = JsonConfiguration.ReadFromString(@"{
	            ""tenants"": {
		            ""tenant1"": {
			            ""common"": {},
			            ""dossierbrowser"": {
				            ""complex2"": {
					            ""_replace"": true
				            }
			            }
		            }
	            }
            }", TestSchema);
            c["tenant1"][App.Dossierbrowser].Set("complex2.simple3", "some string");

            var o = JObject.Parse(c.ToString());
            var replace = (JValue)o.SelectTokens("$.tenants.tenant1.dossierbrowser.complex2._replace").Single();
            var extend = (JValue)o.SelectTokens("$.tenants.tenant1.dossierbrowser.complex2._extend").SingleOrDefault();

            Assert.IsNull(extend);
            Assert.IsTrue((bool)replace.Value);
        }

        [Test]
        [Category("Set")]
        public void Should_SetPropertyPlatformSpecific_When_PlatformIsSpecificed()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{\"tenant1\": { \"common\":{},  \"dossierbrowser\":{}}}}", TestSchema);
            c["tenant1"][App.Dossierbrowser].Set("complex2.simple3", "some string", false, Platform.App);
            var cca = (JValue)JObject.Parse(c.ToString()).SelectTokens("$.tenants.tenant1.dossierbrowser.complex2.app.simple3").Single();
            Assert.That(((string)cca.Value), Is.EqualTo("some string"));
        }

        [Test]
        [Category("Set")]
        public void Should_SetPropertyPlatformUnspecific_When_PlatformIsNotSpecificed()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{\"tenant1\": { \"common\":{},  \"dossierbrowser\":{ \"complex2\": { \"app\": { \"simple3\": \"a\" } }}}}}", TestSchema);
            c["tenant1"][App.Dossierbrowser].Set("complex2.simple3", "some string", false, Platform.Unspecified);
            var val = (JValue)JObject.Parse(c.ToString()).SelectTokens("$.tenants.tenant1.dossierbrowser.complex2.simple3").Single();
            Assert.That(((string)val.Value), Is.EqualTo("some string"));
        }

        [Test]
        [Category("Set")]
        public void Should_NotSetPropertyPlatformUnspecific_When_UnspecificValueIsSame()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{\"tenant1\": { \"common\":{},  \"dossierbrowser\":{ \"complex2\": { \"app\": { \"simple3\": \"b\" }, \"simple3\": \"a\" }}}}}", TestSchema);
            c["tenant1"][App.Dossierbrowser].Set("complex2.simple3", "a", false, Platform.App);
            var val1 = (JValue)JObject.Parse(c.ToString()).SelectTokens("$.tenants.tenant1.dossierbrowser.complex2.simple3").Single();
            var val2 = JObject.Parse(c.ToString()).SelectTokens("$.tenants.tenant1.dossierbrowser.complex2.app.simple3").SingleOrDefault();
            Assert.That(((string)val1.Value), Is.EqualTo("a"));
            Assert.That(val2, Is.Null);
        }

        [Test]
        [Category("Set")]
        public void Should_RemovePlatformUnspecific_When_AllPlatformHaveSameValue()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{\"tenant1\": { \"common\":{},  \"dossierbrowser\":{ \"complex2\": { \"app\": { \"simple3\": \"a\" } }}}}}", TestSchema);
            c["tenant1"][App.Dossierbrowser].Set("complex2.simple3", "a", false, Platform.Web);
            var cca = (JValue)JObject.Parse(c.ToString()).SelectTokens("$.tenants.tenant1.dossierbrowser.complex2.simple3").Single();
            Assert.That(((string)cca.Value), Is.EqualTo("a"));
        }

        [Test]
        [Category("Get")]
        public void Should_GetPropertyPlatformSpecific_When_PlatformIsSpecificed()
        {
            const string json = @"
            {
	            ""tenants"":{
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
	            }}
            }";

            var c = JsonConfiguration.ReadFromString(json, TestSchema);
            var appVal = c["tenant1"][App.Dossierbrowser].Get("complex2.simple3", Platform.App);
            var webVal = c["tenant1"][App.Dossierbrowser].Get("complex2.simple3", Platform.Web);
            var unspecVal = c["tenant1"][App.Dossierbrowser].Get("complex2.simple3", Platform.Unspecified);

            Assert.That(appVal, Is.EqualTo("p_app"));
            Assert.That(webVal, Is.EqualTo("p_web"));
            Assert.That(unspecVal, Is.EqualTo("p_unspec"));
        }

        [Test]
        [Category("Set")]
        [Category("Get")]
        public void Should_ReturnDefaultValue_When_SetAndGetValue()
        {
            const string json = @"
            {
	            ""tenants"":{
                ""tenant1"": {
		            ""common"": {
			            ""complex3"" : {}
		            }
	            }}
            }";

            var c = JsonConfiguration.ReadFromString(json, TestSchema);

            foreach (var aspect in TestSchema[App.Common]["complex3"].Traverse().OfType<ISimpleAspect>())
            {
                c["tenant1"][App.Common].Set(aspect.GetAspectPath());
                var value = c["tenant1"][App.Common].Get(aspect.GetAspectPath());
                Assert.That(value, Is.TypeOf(aspect.Type));
                Assert.That(value, Is.EqualTo(aspect.GetDefaultValue()));
            }
        }

        [Test]
        [Category("Set")]
        public void Should_SetValuePlatformSpecific_When_SetDefault()
        {
            const string json = @"
            {
	            ""tenants"":{
                ""tenant1"": {
		            ""common"": {},
                    ""dossierbrowser"": {}
	            }}
            }";

            var c = JsonConfiguration.ReadFromString(json, TestSchema);
            c["tenant1"][App.Dossierbrowser].Set("complex2.simple3");

            var web = (JValue)JObject.Parse(c.ToString()).SelectTokens("$.tenants.tenant1.dossierbrowser.complex2.web.simple3").Single();
            var app = (JValue)JObject.Parse(c.ToString()).SelectTokens("$.tenants.tenant1.dossierbrowser.complex2.app.simple3").Single();
            Assert.That(((string)app.Value), Is.Not.Null);
            Assert.That(((string)web.Value), Is.Not.Null);
        }

        [Test]
        [Category("Set")]
        public void Should_RemoveSamePlatformSpecificValue_When_SetValue()
        {
            const string json = @"
            {
	            ""tenants"":{
                ""tenant1"": {
		            ""common"": {},
                    ""dossierbrowser"": {}
	            }}
            }";

            var c = JsonConfiguration.ReadFromString(json, TestSchema);
            c["tenant1"][App.Dossierbrowser].Set("complex2.simple3", "test", false, Platform.App);
            c["tenant1"][App.Dossierbrowser].Set("complex2.simple3", "test", false, Platform.Web);

            var web = (JValue)JObject.Parse(c.ToString()).SelectTokens("$.tenants.tenant1.dossierbrowser.complex2.web.simple3").SingleOrDefault();
            var app = (JValue)JObject.Parse(c.ToString()).SelectTokens("$.tenants.tenant1.dossierbrowser.complex2.app.simple3").SingleOrDefault();
            var unspec = (JValue)JObject.Parse(c.ToString()).SelectTokens("$.tenants.tenant1.dossierbrowser.complex2.simple3").SingleOrDefault();
            Assert.That(web, Is.Null);
            Assert.That(app, Is.Null);
            Assert.That(unspec, Is.Not.Null);
        }

        #endregion

        #region Remove

        [Test]
        public void Should_RemovePropertyPlatformSpecific_When_PlatformIsSpecificed()
        {
            const string json = @"
            {
                ""tenants"": {
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
	            }
            }";

            foreach (var pl in (Platform[])Enum.GetValues(typeof(Platform)))
            {
                var c = JsonConfiguration.ReadFromString(json, TestSchema);
                c["tenant1"][App.Dossierbrowser].Remove("complex2.simple3", pl);

                foreach (var pl2 in (Platform[])Enum.GetValues(typeof(Platform)))
                {
                    var r = c["tenant1"][App.Dossierbrowser].Get("complex2.simple3", pl2);
                    Assert.That(r, pl == pl2 ? Is.Null : Is.Not.Null);
                }
            }
        }

        [Test]
        public void Should_NotRemovePropertyPlatformSpecific_When_PlatformIsNotSpecificed()
        {
            const string json = @"
            {
                ""tenants"": {	            
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
                }
            }";

            var c = JsonConfiguration.ReadFromString(json, TestSchema);
            c["tenant1"][App.Dossierbrowser].Remove("complex2.simple3", Platform.Unspecified);

            var r1 = c["tenant1"][App.Dossierbrowser].Get("complex2.simple3", Platform.Unspecified);
            var r2 = c["tenant1"][App.Dossierbrowser].Get("complex2.simple3", Platform.App);
            var r3 = c["tenant1"][App.Dossierbrowser].Get("complex2.simple3", Platform.Web);

            Assert.That(r1, Is.Null);
            Assert.That(r2, Is.Not.Null);
            Assert.That(r3, Is.Not.Null);
        }

        [Test]
        public void Should_RemoveAllPropertyValues_When_NoPlatformIsGiven()
        {
            const string json = @"
            {
	            ""tenants"": {
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
	            }}
            }";

            var c = JsonConfiguration.ReadFromString(json, TestSchema);
            c["tenant1"][App.Dossierbrowser].Remove("complex2.simple3");

            var r1 = c["tenant1"][App.Dossierbrowser].Get("complex2.simple3", Platform.Unspecified);
            var r2 = c["tenant1"][App.Dossierbrowser].Get("complex2.simple3", Platform.App);
            var r3 = c["tenant1"][App.Dossierbrowser].Get("complex2.simple3", Platform.Web);

            Assert.That(r1, Is.Null);
            Assert.That(r2, Is.Null);
            Assert.That(r3, Is.Null);
        }

        [Test]
        public void Should_RemoveComplexProperty_When_SelectComplexAspectToRemove()
        {
            const string json = @"
            {
	            ""tenants"": {
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
	            }}
            }";

            var c = JsonConfiguration.ReadFromString(json, TestSchema);
            c["tenant1"][App.Dossierbrowser].Remove("complex2");

            var complex2 = JObject.Parse(c.ToString()).SelectTokens("$.tenants.tenant1.dossierbrowser.complex2").SingleOrDefault();
            var dossierbrowser = JObject.Parse(c.ToString()).SelectTokens("$.tenants.tenant1.dossierbrowser").SingleOrDefault();

            Assert.That(complex2, Is.Null);
            Assert.That(dossierbrowser, Is.Not.Null);
        }

        #endregion

        #region Dependencies

        [Test]
        public void Should_Throw_When_DependencyIsNotFullfilled()
        {
            const string json = @"
            {
                ""tenants"": {	            
                    ""tenant1"": {
		                ""common"": {},
		                ""sitzungsvorbereitung"": {
		                }
	                }   
                }
            }";

            var c = JsonConfiguration.ReadFromString(json, TestSchema);
            void D() => c["tenant1"][App.Sitzungsvorbereitung].Set("service.allowDokumenteAnnotations", true, false);

            Assert.Throws(typeof(AspectDependencyNotFulfilledException), D);
            Assert.That(c["tenant1"][App.Sitzungsvorbereitung].Get("service.allowDokumenteAnnotations"), Is.Null);
        }

        [Test]
        public void Should_EnsuresDependency_When_EnsureDependencyIsSet()
        {
            const string json = @"
            {
                ""tenants"": {	            
                    ""tenant1"": {
		                ""common"": {},
		                ""sitzungsvorbereitung"": {
		                }
	                }   
                }
            }";

            var c = JsonConfiguration.ReadFromString(json, TestSchema);
            c["tenant1"][App.Sitzungsvorbereitung].Set("service.allowDokumenteAnnotations", true, true);

            Assert.That(c["tenant1"][App.Sitzungsvorbereitung].Get("service.allowDokumenteAnnotations"), Is.True);
            Assert.That(c["tenant1"][App.Common].Get("ui.pdf.editor"), Is.EqualTo("pdftools"));
        }

        #endregion

        #region Has

        [Test]
        public void Should_ReturnTrue_When_RequestedPlatformSpecificPropertyIsSet()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{ \"tenant1\": { \"common\":{},  \"dossierbrowser\":{ \"complex2\": { \"app\": { \"simple3\": \"a\" } } }}}}", TestSchema);
            var result = c["tenant1"][App.Dossierbrowser].Has("complex2.simple3", Platform.App);
            Assert.That(result, Is.True);
        }

        [Test]
        public void Should_ReturnTrue_When_UnspecificPlatformPropertyIsSetAndSpecificRequested()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{ \"tenant1\": { \"common\":{},  \"dossierbrowser\":{ \"complex2\": { \"simple3\": \"a\" }}}}}", TestSchema);
            var result = c["tenant1"][App.Dossierbrowser].Has("complex2.simple3", Platform.App);
            Assert.That(result, Is.True);
        }

        [Test]
        public void Should_ReturnTrue_When_UnspecificPlatformPropertyIsSet()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{ \"tenant1\": { \"common\":{},  \"dossierbrowser\":{ \"complex2\": { \"simple3\": \"a\" }}}}}", TestSchema);
            var result = c["tenant1"][App.Dossierbrowser].Has("complex2.simple3");
            Assert.That(result, Is.True);
        }

        [Test]
        public void Should_ReturnFalse_When_PropertyIsNotSet()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{ \"tenant1\": { \"common\":{},  \"dossierbrowser\":{ \"complex2\": { }}}}}", TestSchema);
            var result = c["tenant1"][App.Dossierbrowser].Has("complex2.simple3");
            Assert.That(result, Is.False);
        }

        [Test]
        public void Should_ReturnTrue_When_SpecificPlatformPropertyIsSetAndUnspecificRequested()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{ \"tenant1\": { \"common\":{},  \"dossierbrowser\":{ \"complex2\": { \"app\": { \"simple3\": \"a\" }}}}}}", TestSchema);
            var result = c["tenant1"][App.Dossierbrowser].Has("complex2.simple3");
            Assert.That(result, Is.False);
        }

        #endregion
    }
}