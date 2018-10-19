using NUnit.Framework;
using System;

namespace cmi.mc.config.ModelContract.Tests
{
    [TestFixture()]
    public class McSymbolsTests
    {
        [Test()]
        public void Should_ReturnPlatformEnum_When_ConfigurationNameIsGiven()
        {
            foreach (var pl in McSymbols.Platforms)
            {
                Assert.That(McSymbols.GetPlatform(pl.ToConfigurationName()), Is.EqualTo(pl));
            }
        }

        [Test()]
        public void Should_Throw_When_UnkownCcaConfigurationNameIsGiven()
        {
            void D() => McSymbols.GetCca("test1");
            Assert.Throws(typeof(ArgumentException), D);
        }

        [Test()]
        public void Should_ReturnCcaEnum_When_ConfigurationNameIsGiven()
        {
            foreach (var cca in McSymbols.ConfigControlAttributes)
            {
                Assert.That(McSymbols.GetCca(cca.ToConfigurationName()), Is.EqualTo(cca));
            }
        }

        [Test()]
        public void Should_Throw_When_UnkownPlatformConfigurationNameIsGiven()
        {
            void D() => McSymbols.GetPlatform("test1");
            Assert.Throws(typeof(ArgumentException), D);
        }
    }
}