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
            Assert.That(c["tenant1"][App.Common].Get("api.server").ToString(), Is.EqualTo($"{TestSchema.DefaultServiceUrl}mobileclients"));
            Assert.That(c["tenant1"].Has(App.Common), Is.True);
        }

        #endregion

        #region region Validation

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
            c["test"][App.Common].Set(null, true);

            c["test"].Add(App.Dossierbrowser, true);
            c["test"][App.Dossierbrowser].Set(null, true);

            c["test"].Add(App.Sitzungsvorbereitung, true);
            c["test"][App.Sitzungsvorbereitung].Set(null, true);

            c["test"].Add(App.Zusammenarbeitdritte, true);
            c["test"][App.Zusammenarbeitdritte].Set(null, true);

            c["test"].Add(App.Mobileclients, true);
            c["test"][App.Mobileclients].Set(null, true);

            c["test"].Validate(AxSupport.R18);
        }

        #endregion

        [Test]
        public void Should_UpdateAppDirectory_When_AddRemoveApp()
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
