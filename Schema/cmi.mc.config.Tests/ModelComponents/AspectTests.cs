using System;
using System.Linq;
using cmi.mc.config.ModelComponents;
using cmi.mc.config.ModelContract;
using NUnit.Framework;

namespace cmi.mc.config.Tests
{
    [TestFixture]
    public class AspectTests
    {
        private static readonly ConfigurationModel TestModel = new ConfigurationModel();
        private static ISimpleAspect Leaf = null;

        [OneTimeSetUp]
        public static void ClassInit()
        {
            var simple1 = new SimpleAspect<bool>("simple1", true);
            var complex1 = new ComplexAspect("complex1");
            var complex2 = new ComplexAspect("complex2");
            var complex3 = new ComplexAspect("complex3");
            var complex4 = new ComplexAspect("complex4");
            var complex5 = new ComplexAspect("complex5");
            complex1.AddAspect(complex2);
            complex2.AddAspect(complex3);
            complex3.AddAspect(complex4);
            complex4.AddAspect(complex5);
            complex5.AddAspect(simple1);
            ((AppSection)TestModel[App.Common]).AddAspect(complex1);
            Leaf = simple1;
        }
    }
}
