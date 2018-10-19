using System;
using System.Collections.Generic;
using System.Linq;
using cmi.mc.config.ModelComponents;
using cmi.mc.config.ModelContract;
using NUnit.Framework;

namespace cmi.mc.config.Tests
{
    [TestFixture]
    public class SimpleAspectTests
    {
        [Test]
        public void Should_HaveInitialPropertyValues_When_ConstructObject()
        {
            var s = new SimpleAspect<string>("name", "default", AxSupport.R17);

            Assert.That(s.AxSupport, Is.EqualTo(AxSupport.R17));
            Assert.That(s.Name, Is.EqualTo("name"));
            Assert.That(s.GetDefaultValue(), Is.TypeOf<string>());
            Assert.That(s.GetDefaultValue(), Is.EqualTo("default"));
        }

        [Test]
        public void Should_NotAcceptNullValues_When_TestValue()
        {
            var s = new SimpleAspect<string>("name", "default", AxSupport.R17) { IsRequired = true};
            var s2 = new SimpleAspect<int>("name", 13, AxSupport.R17);

            void D() => s.TestValue(null);
            void D2() => s2.TestValue(null);
            Assert.Throws(typeof(ValueValidationException), D);
            Assert.Throws(typeof(ValueValidationException), D2);
        }

        [Test]
        public void Should_NotAcceptValueOfOtherType_When_TestValue()
        {
            var s = new SimpleAspect<string>("name", "default", AxSupport.R17) { IsRequired = true };

            void D() => s.TestValue(15);
            Assert.Throws(typeof(ValueValidationException), D);
        }

        [Test]
        public void Should_NotAcceptValue_When_PlatformIsUnsupported()
        {
            var s = new SimpleAspect<string>("name", "default", AxSupport.R17);

            void D() => s.TestValue("test", null, Platform.App);
            Assert.Throws(typeof(ValueValidationException), D);
        }
    }
}
