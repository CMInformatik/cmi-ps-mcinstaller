using NUnit.Framework;
using System.Collections.Generic;
using cmi.mc.config.Extensions;
using cmi.mc.config.ModelContract;
using Newtonsoft.Json.Linq;

namespace cmi.mc.config.Tests
{
    [TestFixture()]
    public class JPropertyCcaExtensionsTests
    {
        [Test()]
        public void Should_ThrowOnInvalidCca_When_GetCca()
        {
            List<string> invalidCCas = new List<string>()
            {
                @" { ""prop"": { ""_private"": false }}",
                @" { ""prop"": { ""_private"": ""string"" }}",
                @" { ""prop"": { ""_private"": {""prop2"": 15} }}",
                @" { ""prop"": { ""_private"": false, ""_extend"": false }}",
            };

            foreach (var invalidCCa in invalidCCas)
            {
                void D() => JObject.Parse(invalidCCa).Property("prop").GetCCa();
                Assert.Throws(typeof(InvalidConfigurationException), D, "Should not be a valid cca: "+invalidCCa);
            }
        }

        [Test()]
        public void Should_ReturnCCa_When_GetCca()
        {
            foreach (var cca in McSymbols.ConfigControlAttributes)
            {
                Assert.That(JObject.Parse($" {{ \"prop\": {{ \"{cca.ToConfigurationName()}\": true }}}}").Property("prop").GetCCa(), Is.EqualTo(cca));
            }
        }
    }
}