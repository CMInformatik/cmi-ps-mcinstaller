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
            aspect.Setup(a => a.GetDefaultValue(It.IsAny<ITenant>())).Returns("aspect");
            aspect.Setup(a => a.Type).Returns(typeof(string));

            var decAspect = new DefaultValueDecorator(
                aspect.Object, 
                $"a.{DefaultValueDecorator.OriginalDefaultPlaceholder}.b.{DefaultValueDecorator.TenantNamePlaceholder}.c");

            var tenant = new Mock<ITenant>();
            tenant.Setup(t => t.Name).Returns("tenant");
            var result = decAspect.GetDefaultValue(tenant.Object);

            Assert.That(result, Is.EqualTo("a.aspect.b.tenant.c"));
        }
    }
}
