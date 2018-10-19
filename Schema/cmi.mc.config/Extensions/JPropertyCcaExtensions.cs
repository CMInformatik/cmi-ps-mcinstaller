using System;
using System.Diagnostics;
using System.Linq;
using cmi.mc.config.ModelContract;
using Newtonsoft.Json.Linq;

namespace cmi.mc.config.Extensions
{
    internal static class JPropertyCcaExtensions
    {
        /// <summary>
        /// Sets the default <see cref="ConfigControlAttribute"/> for the given <see cref="IComplexAspect"/>,
        /// if no <see cref="ConfigControlAttribute"/> is currently set.
        /// Does nothing if a <see cref="ConfigControlAttribute"/> is already set. 
        /// </summary>
        /// <param name="configPart">The configuration part the <see cref="aspect"/> belongs to.</param>
        /// <param name="aspect">The aspect whose default <see cref="ConfigControlAttribute"/> is to be set.</param>
        public static void SetDefaultCCa(this JProperty configPart, IComplexAspect aspect)
        {
            Debug.Assert(configPart != null);
            if (aspect == null) throw new ArgumentNullException(nameof(aspect));
            if (aspect.DefaultCca == ConfigControlAttribute.NotSet) return; // no default cca defined      
            if (configPart.Value.Children<JProperty>().Any(p => McSymbols.CcaNames.Contains(p.Name))) return; // a cca is already set
            configPart.Value[aspect.DefaultCca.ToConfigurationName()] = JToken.FromObject(true);
        }

        /// <summary>
        /// Gets the <see cref="ConfigControlAttribute"/> applied to the given configuration part.
        /// </summary>
        /// <param name="configPart">The configuration part.</param>
        /// <returns>The applied <see cref="ConfigControlAttribute"/> or <seealso cref="ConfigControlAttribute.NotSet"/></returns>
        /// <exception cref="InvalidConfigurationException">When the configuration part contains invalid <see cref="ConfigControlAttribute"/> informations.</exception>
        public static ConfigControlAttribute GetCCa(this JProperty configPart)
        {
            Debug.Assert(configPart != null);
            var ccas = configPart.Value?.Children<JProperty>().Where(p => McSymbols.CcaNames.Contains(p.Name)).ToList();

            if (ccas == null || !ccas.Any()) return ConfigControlAttribute.NotSet;
            if (ccas.Count > 1)
            {
                throw new InvalidConfigurationException($"{configPart.Path} has several {nameof(ConfigControlAttribute)}s", null, configPart.Path);
            }
            VerfiyCcaPropertyValue(ccas.First());
            return McSymbols.GetCca(ccas.First().Name);
        }

        /// <summary>
        /// Verfies that the given property contains a valid <see cref="ConfigControlAttribute"/> value.
        /// </summary>
        private static void VerfiyCcaPropertyValue(JProperty jProperty)
        {
            Debug.Assert(jProperty != null);
            if (!(jProperty.Value is JValue) || jProperty.Value.Type != JTokenType.Boolean)
            {
                throw new InvalidConfigurationException($"{jProperty.Path} does not contain a boolean value.", null, jProperty.Path);
            }
            if (((JValue) jProperty.Value).Value as bool? != true)
            {
                 throw new InvalidConfigurationException($"{jProperty.Path} must be a boolean with value 'true' to be a valid {nameof(ConfigControlAttribute)}.", null, jProperty.Path);
            }
        }
    }
}
