using System;
using System.Linq;
using cmi.mc.config.ModelContract;
using cmi.mc.config.ModelImpl.Decorators;
using Moq;
using NUnit.Framework;

namespace cmi.mc.config.Tests.ModelImpl.Decorators
{
    [TestFixture]
    public class TenantSpecificUriDecoratorTests
    {
        private static Mock<ISimpleAspect> GetAspectMock()
        {
            var aspect = new Mock<ISimpleAspect>();
            aspect.Setup(m => m.GetAspectPath()).Returns("mock");
            aspect.Setup(m => m.Name).Returns("mock");
            aspect.Setup(a => a.Type).Returns(typeof(Uri));
            return aspect;
        }

        [Test]
        public void Should_ReplaceBaseUrl_When_GetDefaultValue()
        {
            var aspect = GetAspectMock();
            aspect.Setup(a => a.GetDefaultValue(It.IsAny<ITenant>(), Platform.Unspecified)).Returns(new Uri("https://m.ch/myapp"));

            var decAspect = new TenantSpecificUriDecorator(aspect.Object);

            var tenant = new Mock<ITenant>();
            tenant.Setup(t => t.Name).Returns("tenant");
            tenant.Setup(t => t.ServiceBaseUrl).Returns(new Uri("http://c.c"));
            var result = decAspect.GetDefaultValue(tenant.Object);

            Assert.That(result.ToString(), Is.EqualTo("http://c.c/myapp"));
        }

        [Test]
        public void Should_ReplaceTenantPlaceHolder_When_GetDefaultValue()
        {
            var aspect = GetAspectMock();
            aspect.Setup(a => a.GetDefaultValue(It.IsAny<ITenant>(), Platform.Unspecified)).Returns(new Uri("https://m.ch/myapp/{tenant}"));

            var decAspect = new TenantSpecificUriDecorator(aspect.Object, "{tenant}");

            var tenant = new Mock<ITenant>();
            tenant.Setup(t => t.Name).Returns("mytenant");
            tenant.Setup(t => t.ServiceBaseUrl).Returns(new Uri("http://c.c"));
            var result = decAspect.GetDefaultValue(tenant.Object);

            Assert.That(result.ToString(), Is.EqualTo("http://c.c/myapp/mytenant"));
        }

        [Test]
        public void Should_ReturnSelf_When_Traverse()
        {
            var aspect = GetAspectMock();
            var decAspect = new TenantSpecificUriDecorator(aspect.Object,"{tenant}");

            var result = decAspect.Traverse();

            var enumerable = result as IAspect[] ?? result.ToArray();
            Assert.That(enumerable.Count(), Is.EqualTo(1));
            Assert.That(enumerable.First(), Is.EqualTo(decAspect));
        }
    }
}
