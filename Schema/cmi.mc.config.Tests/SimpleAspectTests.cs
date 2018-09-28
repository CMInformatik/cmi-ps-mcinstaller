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
            var s = new SimpleAspect("name", typeof(string), "default", AxSupport.R17);

            Assert.That(s.AxSupport, Is.EqualTo(AxSupport.R17));
            Assert.That(s.Name, Is.EqualTo("name"));
            Assert.That(s.GetDefaultValue(), Is.TypeOf<string>());
            Assert.That(s.GetDefaultValue(), Is.EqualTo("default"));
        }
    }
}
