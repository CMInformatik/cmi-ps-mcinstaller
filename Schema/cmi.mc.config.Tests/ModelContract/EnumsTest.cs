using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cmi.mc.config.ModelContract;

namespace cmi.mc.config.Tests.ModelContract
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
