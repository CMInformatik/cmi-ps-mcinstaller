using System;
using cmi.mc.config.ModelContract;
using cmi.mc.config.ModelContract.Components;
using cmi.mc.config.ModelContract.Exceptions;
using cmi.mc.config.ModelImpl;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace cmi.mc.config.Tests.ModelImpl
{
    [TestFixture()]
    public class AppDirAspectTests
    {
        [Test()]
        public void Should_WebUrlWithTenantName_When_ReturnDefaultValue()
        {
            var tenantMock = new Mock<ITenant>();
            tenantMock.Setup(t => t.ServiceBaseUrl).Returns(new Uri("https://my.uri.ch:500/"));
            tenantMock.Setup(t => t.Name).Returns("mytenant");

            foreach (var app in McSymbols.Apps)
            {
                var appDir = new AppDirAspect(app);
                var defaultValue = appDir.GetDefaultValue(tenantMock.Object);
                var uri = (((JObject) defaultValue).Property("web").Value as JValue)?.Value;
                Assert.That(uri?.ToString(), Is.EqualTo($"https://my.uri.ch:500/{app.ToConfigurationName()}/mytenant") );
            }
        }

        [Test()]
        public void Should_AcceptDefaultValue_When_TestValue()
        {
            var tenantMock = new Mock<ITenant>();
            tenantMock.Setup(t => t.ServiceBaseUrl).Returns(new Uri("https://my.uri.ch:500/"));
            tenantMock.Setup(t => t.Name).Returns("mytenant");

            foreach (var app in McSymbols.Apps)
            {
                var appDir = new AppDirAspect(app);
                var defaultValue = appDir.GetDefaultValue(tenantMock.Object);
                appDir.TestValue(defaultValue, tenantMock.Object);
            }
        }

        [Test()]
        public void Should_NotAcceptNonDefaultValue_When_TestValue()
        {
            var tenantMock = new Mock<ITenant>();
            tenantMock.Setup(t => t.ServiceBaseUrl).Returns(new Uri("https://my.uri.ch:500/"));
            tenantMock.Setup(t => t.Name).Returns("mytenant");

            foreach (var app in McSymbols.Apps)
            {
                var appDir = new AppDirAspect(app);
                var defaultValue = appDir.GetDefaultValue(tenantMock.Object);
                var jobject = (JObject) defaultValue;
                jobject["web"] = new Uri("https://some.ch/modification");
              
                void D() => appDir.TestValue(defaultValue, tenantMock.Object);
                Assert.Throws(typeof(ValueValidationException), D);
            }
        }
    }
}