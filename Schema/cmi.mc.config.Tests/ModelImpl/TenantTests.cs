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
    [TestFixture]
    public class TenantTests
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

            var complex2 = new ComplexAspect("complex2", ConfigControlAttribute.Extend)
                .AddAspect(new SimpleAspect<string>("simple3", "simple3") { IsPlatformSpecific = true });

            var complex3 = new ComplexAspect("complex3").AddAspect(
                new SimpleAspect<int>("int", 1),
                new SimpleAspect<Uri>("uri", new Uri("https://uri.ch")),
                new SimpleAspect<string>("string", "string"),
                new SimpleAspect<string[]>("stringArray", new []{"1", "2"})
            );

            ((AppAspect)TestSchema[App.Common]).AddAspect(complex1, complex3);
            ((AppAspect)TestSchema[App.Dossierbrowser]).AddAspect(complex2);
        }

        #region service base url
        [Test]
        public void Should_UseUrlFromConfig_When_ServerPropertyIsSet()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{\"tenant1\": { \"common\":{ \"api\": { \"server\": \"https://my.service.ch:6000/mobileclients\"}}}}}", TestSchema);            
            Assert.AreEqual("https://my.service.ch:6000/", c["tenant1"].ServiceBaseUrl.ToString());
        }

        [Test]
        public void Should_UseKnownDefaultUrl_When_ServerPropertyIsNotSet()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{\"tenant1\": { \"common\":{ \"api\": {}}}}}", TestSchema);
            Assert.AreEqual(TestSchema.DefaultServiceUrl, c["tenant1"].ServiceBaseUrl);
        }

        [Test]
        public void Should_EnablesCommonApp_When_IsNotPresent()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{\"tenant1\": {}}}", TestSchema);
            Assert.That(c["tenant1"].Has(App.Common), Is.True);
            Assert.That(c["tenant1"].Get(App.Common, "api.server").ToString(), Is.EqualTo($"{TestSchema.DefaultServiceUrl}mobileclients"));
            Assert.That(c["tenant1"].Has(App.Common), Is.True);
        }

        #endregion

        #region Get/Set
        [Test]
        public void Should_RejectValue_When_ValueHasWrongType()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{ \"tenant1\": { \"common\":{}}}}", TestSchema);
            void D() => c["tenant1"].Set(App.Common, "complex1.simple1", new int());
            Assert.Throws(typeof(ValueValidationException), D);
        }

        [Test]
        public void Should_ExecuteValidators_When_SetConfigProperty()
        {
            int calls = 0;
            _validator.Setup(v => v.Validate(It.IsAny<object>())).Callback(()=> calls++);

            var c = JsonConfiguration.ReadFromString("{\"tenants\":{\"tenant1\": { \"common\":{}}}}", TestSchema);
            c["tenant1"].Set(App.Common, "complex1.simple1", "some string");
            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void Should_Throw_When_AppIsNotEnabled()
        {
            var c = JsonConfiguration.ReadFromString("{ \"tenants\":{\"tenant1\": { \"common\":{}}}}", TestSchema);
            void D() => c["tenant1"].Set(App.Dossierbrowser, "complex2.simple3", true);
            Assert.Throws(typeof(InvalidOperationException), D);
        }

        [Test]
        public void Should_Throw_When_AspectPathIsInvalid()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{\"tenant1\": { \"common\":{},  \"dossierbrowser\":{}}}}", TestSchema);
            void D() => c["tenant1"].Set(App.Dossierbrowser, "complex2.invalid3", "some string");
            Assert.Throws(typeof(KeyNotFoundException), D);
        }

        [Test]
        public void Should_ReturnPropertyValue_When_PropertyWasSetBefore()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{\"tenant1\": { \"common\":{},  \"dossierbrowser\":{}}}}", TestSchema);
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
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{\"tenant1\": { \"common\":{},  \"dossierbrowser\":{ \"complex2\": { \"simple3\": \"old\" }}}}}", TestSchema);
            c["tenant1"].Set(App.Dossierbrowser, "complex2.simple3", "new");
            var result = c["tenant1"].Get<string>(App.Dossierbrowser, "complex2.simple3");
            Assert.AreEqual("new", result);
        }

        [Test]
        public void Should_SetDefaultCca_When_NotCCaIsSet()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{\"tenant1\": { \"common\":{},  \"dossierbrowser\":{}}}}", TestSchema);
            c["tenant1"].Set(App.Dossierbrowser, "complex2.simple3", "some string");
            var cca = (JValue)JObject.Parse(c.ToString()).SelectTokens("$.tenants.tenant1.dossierbrowser.complex2._extend").Single();
            Assert.IsTrue(((bool)cca.Value));
        }

        [Test]
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
            c["tenant1"].Set(App.Dossierbrowser, "complex2.simple3", "some string");

            var o = JObject.Parse(c.ToString());
            var replace = (JValue)o.SelectTokens("$.tenants.tenant1.dossierbrowser.complex2._replace").Single();
            var extend = (JValue) o.SelectTokens("$.tenants.tenant1.dossierbrowser.complex2._extend").SingleOrDefault();

            Assert.IsNull(extend);
            Assert.IsTrue((bool)replace.Value);
        }

        [Test]
        public void Should_SetPropertyPlatformSpecific_When_PlatformIsSpecificed()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{\"tenant1\": { \"common\":{},  \"dossierbrowser\":{}}}}", TestSchema);
            c["tenant1"].Set(App.Dossierbrowser, "complex2.simple3", "some string", false, Platform.App);
            var cca = (JValue)JObject.Parse(c.ToString()).SelectTokens("$.tenants.tenant1.dossierbrowser.complex2.app.simple3").Single();
            Assert.That(((string)cca.Value), Is.EqualTo("some string"));
        }

        [Test]
        public void Should_SetPropertyPlatformUnspecific_When_PlatformIsNotSpecificed()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{\"tenant1\": { \"common\":{},  \"dossierbrowser\":{ \"complex2\": { \"app\": { \"simple3\": \"a\" } }}}}}", TestSchema);
            c["tenant1"].Set(App.Dossierbrowser, "complex2.simple3", "some string", false, Platform.Unspecified);
            var val = (JValue)JObject.Parse(c.ToString()).SelectTokens("$.tenants.tenant1.dossierbrowser.complex2.simple3").Single();
            Assert.That(((string)val.Value), Is.EqualTo("some string"));
        }

        [Test]
        public void Should_NotSetPropertyPlatformUnspecific_When_UnspecificValueIsSame()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{\"tenant1\": { \"common\":{},  \"dossierbrowser\":{ \"complex2\": { \"app\": { \"simple3\": \"b\" }, \"simple3\": \"a\" }}}}}", TestSchema);
            c["tenant1"].Set(App.Dossierbrowser, "complex2.simple3", "a", false, Platform.App);
            var val1 = (JValue)JObject.Parse(c.ToString()).SelectTokens("$.tenants.tenant1.dossierbrowser.complex2.simple3").Single();
            var val2 = JObject.Parse(c.ToString()).SelectTokens("$.tenants.tenant1.dossierbrowser.complex2.app.simple3").SingleOrDefault();
            Assert.That(((string)val1.Value), Is.EqualTo("a"));
            Assert.That(val2, Is.Null);
        }

        [Test]
        public void Should_RemovePlatformUnspecific_When_AllPlatformHaveSameValue()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{\"tenant1\": { \"common\":{},  \"dossierbrowser\":{ \"complex2\": { \"app\": { \"simple3\": \"a\" } }}}}}", TestSchema);
            c["tenant1"].Set(App.Dossierbrowser, "complex2.simple3", "a", false, Platform.Web);
            var cca = (JValue)JObject.Parse(c.ToString()).SelectTokens("$.tenants.tenant1.dossierbrowser.complex2.simple3").Single();
            Assert.That(((string)cca.Value), Is.EqualTo("a"));
        }

        [Test]
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
            var appVal =  c["tenant1"].Get(App.Dossierbrowser, "complex2.simple3", Platform.App);
            var webVal = c["tenant1"].Get(App.Dossierbrowser, "complex2.simple3", Platform.Web);
            var unspecVal = c["tenant1"].Get(App.Dossierbrowser, "complex2.simple3", Platform.Unspecified);
            
            Assert.That(appVal, Is.EqualTo("p_app"));
            Assert.That(webVal, Is.EqualTo("p_web"));
            Assert.That(unspecVal, Is.EqualTo("p_unspec"));
        }

        [Test]
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
                c["tenant1"].Set(App.Common, aspect.GetAspectPath());
                var value = c["tenant1"].Get(App.Common, aspect.GetAspectPath());
                Assert.That(value, Is.TypeOf(aspect.Type));
                Assert.That(value, Is.EqualTo(aspect.GetDefaultValue()));
            }
        }

        #endregion

        #region has

        [Test]
        public void Should_ReturnTrue_When_RequestedPlatformSpecificPropertyIsSet()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{ \"tenant1\": { \"common\":{},  \"dossierbrowser\":{ \"complex2\": { \"app\": { \"simple3\": \"a\" } } }}}}", TestSchema);
            var result = c["tenant1"].Has(App.Dossierbrowser, "complex2.simple3", Platform.App);
            Assert.That(result, Is.True);
        }

        [Test]
        public void Should_ReturnTrue_When_UnspecificPlatformPropertyIsSetAndSpecificRequested()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{ \"tenant1\": { \"common\":{},  \"dossierbrowser\":{ \"complex2\": { \"simple3\": \"a\" }}}}}", TestSchema);
            var result = c["tenant1"].Has(App.Dossierbrowser, "complex2.simple3", Platform.App);
            Assert.That(result, Is.True);
        }

        [Test]
        public void Should_ReturnTrue_When_UnspecificPlatformPropertyIsSet()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{ \"tenant1\": { \"common\":{},  \"dossierbrowser\":{ \"complex2\": { \"simple3\": \"a\" }}}}}", TestSchema);
            var result = c["tenant1"].Has(App.Dossierbrowser, "complex2.simple3");
            Assert.That(result, Is.True);
        }

        [Test]
        public void Should_ReturnFalse_When_PropertyIsNotSet()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{ \"tenant1\": { \"common\":{},  \"dossierbrowser\":{ \"complex2\": { }}}}}", TestSchema);
            var result = c["tenant1"].Has(App.Dossierbrowser, "complex2.simple3");
            Assert.That(result, Is.False);
        }

        [Test]
        public void Should_ReturnTrue_When_SpecificPlatformPropertyIsSetAndUnspecificRequested()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{ \"tenant1\": { \"common\":{},  \"dossierbrowser\":{ \"complex2\": { \"app\": { \"simple3\": \"a\" }}}}}}", TestSchema);
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
                c["tenant1"].Remove(App.Dossierbrowser, "complex2.simple3", pl);

                foreach(var pl2 in (Platform[])Enum.GetValues(typeof(Platform)))
                {
                    var r= c["tenant1"].Get(App.Dossierbrowser, "complex2.simple3", pl2);
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
            c["tenant1"].Remove(App.Dossierbrowser, "complex2.simple3", Platform.Unspecified);

            var r1 = c["tenant1"].Get(App.Dossierbrowser, "complex2.simple3", Platform.Unspecified);
            var r2 = c["tenant1"].Get(App.Dossierbrowser, "complex2.simple3", Platform.App);
            var r3 = c["tenant1"].Get(App.Dossierbrowser, "complex2.simple3", Platform.Web);

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
            c["tenant1"].Remove(App.Dossierbrowser, "complex2.simple3");

            var r1 = c["tenant1"].Get(App.Dossierbrowser, "complex2.simple3", Platform.Unspecified);
            var r2 = c["tenant1"].Get(App.Dossierbrowser, "complex2.simple3", Platform.App);
            var r3 = c["tenant1"].Get(App.Dossierbrowser, "complex2.simple3", Platform.Web);

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
            c["tenant1"].Remove(App.Dossierbrowser, "complex2");

            var complex2 = JObject.Parse(c.ToString()).SelectTokens("$.tenants.tenant1.dossierbrowser.complex2").SingleOrDefault();
            var dossierbrowser = JObject.Parse(c.ToString()).SelectTokens("$.tenants.tenant1.dossierbrowser").SingleOrDefault();

            Assert.That(complex2, Is.Null);
            Assert.That(dossierbrowser, Is.Not.Null);
        }

        #endregion

        #region dependencies

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
            void D() => c["tenant1"].Set(App.Sitzungsvorbereitung, "service.allowDokumenteAnnotations", true, false);

            Assert.Throws(typeof(AspectDependencyNotFulfilledException), D);
            Assert.That(c["tenant1"].Get(App.Sitzungsvorbereitung, "service.allowDokumenteAnnotations"), Is.Null);
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
            c["tenant1"].Set(App.Sitzungsvorbereitung, "service.allowDokumenteAnnotations", true, true);

            Assert.That(c["tenant1"].Get(App.Sitzungsvorbereitung, "service.allowDokumenteAnnotations"), Is.True);
            Assert.That(c["tenant1"].Get(App.Common, "ui.pdf.editor"), Is.EqualTo("pdftools"));
        }

        #endregion

        #region region valiation

        [Test]
        public void TestValidation()
        {
            var model = new DefaultSchema.DefaultSchema(new Uri("http://mobileclients.webserver.ch"));
            var config = @"
{""tenants"":{""schwerzenwil"":{""common"":{""languages"":{""supports"":[""de""],""default"":""de""},""api"":{""server"":""http://mobileclients.webserver.ch/mobileclients"",""public"":""/proxy/schwerzenwilpub"",""private"":""/proxy/schwerzenwilpri""},""account"":{""_private"":true,""changePassword"":true},""security"":{""web"":{""allowRememberMe"":false,""defaultRememberMe"":false},""offlineData"":{""_private"":true,""app"":{""allow"":true}},""pinCode"":{""_private"":true,""lockTimeout"":15,""app"":{""require"":true}}},""service"":{""_private"":true,""supportsPrivate"":true,""supportsSaveSettings"":true,""allowDokumenteOpenExternal"":true},""ui"":{""_private"":true,""controls"":{""buttonSet"":true}},""appDirectory"":{""dossierbrowser"":{""web"":""http://mobileclients.webserver.ch/dossierbrowser/schwerzenwil/"",""app"":""cmidossierbrowser://"",""dossierDetail"":""/Abstr/{GUID}""},""sitzungsvorbereitung"":{""web"":""http://mobileclients.webserver.ch/sitzungsvorbereitung/schwerzenwil/"",""app"":""cmisitzungsvorbereitung://"",""sitzungDetail"":""/{Gremium}/{Jahr}/{GUID}"",""traktandumDetail"":""/{Gremium}/{Jahr}/{GUID}/T/{TraktandumGUID}""},""zusammenarbeitdritte"":{""web"":""http://mobileclients.webserver.ch/zusammenarbeitdritte/schwerzenwil/"",""aktivitaetDetail"":""/Aktivitaet/{GUID}""}}},""dossierbrowser"":{},""sitzungsvorbereitung"":{""service"":{""_extend"":true,""allowDokumenteAddNewVersion"":true,""allowDokumenteAddNew"":true,""allowDokumenteDelete"":true,""allowDokumenteAnnotations"":false,""supportsPersoenlicheDokumente"":true,""supportsFreigabe"":true,""supportsWortbegehren"":true,""supportsLatestHistory"":true,""supportsLatestHistoryMail"":true,""supportsGesamtPdf"":false},""ui"":{""_extend"":true,""pdf"":{""editor"":""pdftools"",""editorMaxOpenCount"":1,""app"":{""inTabs"":true},""web"":{""inTabs"":true}}}},""zusammenarbeitdritte"":{""service"":{""_extend"":true,""allowDokumenteAddNewVersion"":true,""allowDokumenteAddNew"":true,""allowDokumenteDelete"":true}}},""musterau"":{""common"":{""languages"":{""supports"":[""de""],""default"":""de""},""api"":{""server"":""http://mobileclients.webserver.ch/mobileclients"",""public"":""/proxy/musteraupub"",""private"":""/proxy/musteraupri""},""account"":{""_private"":true,""changePassword"":true},""security"":{""offlineData"":{""_private"":true,""app"":{""allow"":true}},""pinCode"":{""_private"":true,""lockTimeout"":15,""app"":{""require"":true}}},""service"":{""_private"":true,""supportsPrivate"":true,""supportsSaveSettings"":true,""allowDokumenteOpenExternal"":true},""ui"":{""_private"":true,""controls"":{""buttonSet"":true}},""appDirectory"":{""dossierbrowser"":{""web"":""http://mobileclients.webserver.ch/dossierbrowser/musterau/"",""app"":""cmidossierbrowser://"",""dossierDetail"":""/Abstr/{GUID}""},""sitzungsvorbereitung"":{""web"":""http://mobileclients.webserver.ch/sitzungsvorbereitung/musterau/"",""app"":""cmisitzungsvorbereitung://"",""sitzungDetail"":""/{Gremium}/{Jahr}/{GUID}"",""traktandumDetail"":""/{Gremium}/{Jahr}/{GUID}/T/{TraktandumGUID}""},""zusammenarbeitdritte"":{""web"":""http://mobileclients.webserver.ch/zusammenarbeitdritte/musterau/"",""aktivitaetDetail"":""/Aktivitaet/{GUID}""}}},""dossierbrowser"":{},""sitzungsvorbereitung"":{""service"":{""_extend"":true,""allowDokumenteAddNewVersion"":true,""allowDokumenteAddNew"":true,""allowDokumenteDelete"":true,""allowDokumenteAnnotations"":false,""supportsPersoenlicheDokumente"":true,""supportsFreigabe"":true,""supportsWortbegehren"":true,""supportsLatestHistory"":true,""supportsLatestHistoryMail"":true,""supportsGesamtPdf"":false},""ui"":{""_extend"":true,""pdf"":{""editor"":""pdftools"",""editorMaxOpenCount"":1,""app"":{""inTabs"":true},""web"":{""inTabs"":true}}}},""zusammenarbeitdritte"":{""service"":{""_extend"":true,""allowDokumenteAddNewVersion"":true,""allowDokumenteAddNew"":true,""allowDokumenteDelete"":true}}}}}
            ";

            var c = JsonConfiguration.ReadFromString(config, model);
            c["schwerzenwil"].Validate(AxSupport.R16_1);
        }

        [Test]
        public void Should_ReturnValidConfiguration_When_CreateNewConfigurationWithDefaults()
        {
            var model = new DefaultSchema.DefaultSchema(new Uri("http://mobileclients.webserver.ch"));
            var c = JsonConfiguration.New(model);
            c.AddTenant("test");
            c["test"].Set(App.Common, null, true);

            c["test"].Add(App.Dossierbrowser, true);
            c["test"].Set(App.Dossierbrowser, null, true);

            c["test"].Add(App.Sitzungsvorbereitung, true);
            c["test"].Set(App.Sitzungsvorbereitung, null, true);

            c["test"].Add(App.Zusammenarbeitdritte, true);
            c["test"].Set(App.Zusammenarbeitdritte, null, true);

            c["test"].Add(App.Mobileclients, true);
            c["test"].Set(App.Mobileclients, null, true);

            c["test"].Validate(AxSupport.R18);
        }

        #endregion

        [Test]
        public void Should_UpdateAppDirectory_When_AddRemoceApp()
        {
            var c = JsonConfiguration.ReadFromString("{\"tenants\":{\"tenant1\": { \"common\":{} }}}", TestSchema);
            c["tenant1"].Add(App.Dossierbrowser);

            var o = JObject.Parse(c.ToString());
            Assert.That(()=> o.SelectTokens("$.tenants.tenant1.common.appDirectory.dossierbrowser").Single(), Is.Not.Null);

            c["tenant1"].Remove(App.Dossierbrowser);

            var o2 = JObject.Parse(c.ToString());
            Assert.That(() => o2.SelectTokens("$.tenants.tenant1.common.appDirectory.dossierbrowser").SingleOrDefault(), Is.Null);
        }
    }
}
