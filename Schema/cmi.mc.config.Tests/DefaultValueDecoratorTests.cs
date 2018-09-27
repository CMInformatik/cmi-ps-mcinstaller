using System;
using cmi.mc.config.AspectDecorators;
using cmi.mc.config.SchemaComponents;
using Moq;
using NUnit.Framework;

namespace cmi.mc.config.Tests
{
    [TestFixture]
    public class DefaultValueDecoratorTests
    {
        private static Mock<ISimpleAspect> GetAspectMock()
        {
            var aspect = new Mock<ISimpleAspect>();
            aspect.Setup(m => m.GetAspectPath()).Returns("mock");
            aspect.Setup(m => m.Name).Returns("mock");
            return aspect;
        }

        [Test]
        public void Should_ReplacePlaceholders_When_GetDefaultValue()
        {
            var aspect = GetAspectMock();
            aspect.Setup(a => a.GetDefaultValue(It.IsAny<ITenant>(), Platform.Unspecified)).Returns("aspect");
            aspect.Setup(a => a.Type).Returns(typeof(string));

            var decAspect = new DefaultValueDecorator(
                aspect.Object, 
                $"a.{DefaultValueDecorator.OriginalDefaultPlaceholder}.b.{DefaultValueDecorator.TenantNamePlaceholder}.c");

            var tenant = new Mock<ITenant>();
            tenant.Setup(t => t.Name).Returns("tenant");
            tenant.Setup(t => t.ServiceBaseUrl).Returns(new Uri("http://c.c"));
            var result = decAspect.GetDefaultValue(tenant.Object);

            Assert.That(result, Is.EqualTo("a.aspect.b.tenant.c"));
        }

        [Test]
        public void Should_FailTestValue_When_DefaultValueIsEnforcedAndDifferent()
        {
            var aspect = GetAspectMock();
            aspect.Setup(a => a.GetDefaultValue(It.IsAny<ITenant>(), Platform.Unspecified)).Returns("a");
            aspect.Setup(a => a.Type).Returns(typeof(string));

            var decAspect = new DefaultValueDecorator(aspect.Object,"a",true);

            Assert.That(()=>{ decAspect.TestValue("b"); }, Throws.ArgumentException);
        }

        [Test]
        public void Should_PassTestValue_When_DefaultValueIsEnforcedAndSame()
        {
            var aspect = GetAspectMock();
            aspect.Setup(a => a.GetDefaultValue(It.IsAny<ITenant>(), Platform.Unspecified)).Returns("a");
            aspect.Setup(a => a.Type).Returns(typeof(string));

            var decAspect = new DefaultValueDecorator(aspect.Object, "a", true);
            decAspect.TestValue("a");

            Assert.Pass();
        }

        [Test]
        public void Should_NotModifyDefaultValue_When_TenantIsNull()
        {
            var aspect = GetAspectMock();
            aspect.Setup(a => a.GetDefaultValue(It.IsAny<ITenant>(), Platform.Unspecified)).Returns("aspect");
            aspect.Setup(a => a.Type).Returns(typeof(string));

            var decAspect = new DefaultValueDecorator(
                aspect.Object,
                $"a.{DefaultValueDecorator.OriginalDefaultPlaceholder}.b.{DefaultValueDecorator.TenantNamePlaceholder}.c");

            var result = decAspect.GetDefaultValue();

            Assert.That(result, Is.EqualTo("aspect"));
        }
    }
}
