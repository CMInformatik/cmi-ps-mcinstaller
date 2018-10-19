using NUnit.Framework;
using cmi.mc.config.ModelContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cmi.mc.config.ModelContract.Tests
{
    [TestFixture()]
    public class McConfigSymbolsTests
    {
        [Test()]
        public void Should_ReturnPlatformEnum_When_ConfigurationNameIsGiven()
        {
            foreach (var pl in McConfigSymbols.Platforms)
            {
                Assert.That(McConfigSymbols.GetPlatform(pl.ToConfigurationName()), Is.EqualTo(pl));
            }
        }

        [Test()]
        public void Should_Throw_When_UnkownCcaConfigurationNameIsGiven()
        {
            void D() => McConfigSymbols.GetCca("test1");
            Assert.Throws(typeof(ArgumentException), D);
        }

        [Test()]
        public void Should_ReturnCcaEnum_When_ConfigurationNameIsGiven()
        {
            foreach (var cca in McConfigSymbols.ConfigControlAttributes)
            {
                Assert.That(McConfigSymbols.GetCca(cca.ToConfigurationName()), Is.EqualTo(cca));
            }
        }

        [Test()]
        public void Should_Throw_When_UnkownPlatformConfigurationNameIsGiven()
        {
            void D() => McConfigSymbols.GetPlatform("test1");
            Assert.Throws(typeof(ArgumentException), D);
        }
    }
}