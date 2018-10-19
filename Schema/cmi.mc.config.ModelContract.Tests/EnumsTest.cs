using System;
using System.Linq;
using NUnit.Framework;

namespace cmi.mc.config.ModelContract.Tests
{
    [TestFixture]
    public class EnumsTest
    {
        [Test]
        public void Should_CompareLowerAxVersionLower_When_CompareDiffrentAxVersions()
        {
            Assert.That(AxSupport.R16_1, Is.LessThan(AxSupport.R17));
            Assert.That(AxSupport.R17, Is.LessThan(AxSupport.R18));

            var testedOrder = new[] { AxSupport.R16_1, AxSupport.R17, AxSupport.R18};

            foreach (var axV in (AxSupport[])Enum.GetValues(typeof(AxSupport)))
            {
                Assert.That(testedOrder.Contains(axV), $"Please add compare-test for {axV}");
            }
        }
    }
}
