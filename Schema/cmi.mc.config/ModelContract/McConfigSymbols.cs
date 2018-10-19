using System;
using System.Collections.Generic;
using System.Linq;

namespace cmi.mc.config.ModelContract
{
    /// <summary>
    /// Helper class to deal with the symbols in mobile client configuration. 
    /// </summary>
    public static class McConfigSymbols
    {
        private static readonly List<string> _reservedWords;
        private static readonly Dictionary<string, Platform> ConfigurationNameToPlatform;
        private static readonly Dictionary<string, ConfigControlAttribute> ConfigurationNameToCca;
        private static readonly Dictionary<string, App> ConfigurationNameToApp;

        static McConfigSymbols() {
            _reservedWords = new List<string>();
            _reservedWords.AddRange(PlatformNames);
            _reservedWords.AddRange(CcaNames);

            ConfigurationNameToPlatform = new Dictionary<string, Platform>();
            foreach (var platform in Platforms)
            {
                ConfigurationNameToPlatform.Add(platform.ToConfigurationName(), platform);
            }

            ConfigurationNameToCca = new Dictionary<string, ConfigControlAttribute>();
            foreach (var cca in ConfigControlAttributes)
            {
                ConfigurationNameToCca.Add(cca.ToConfigurationName(), cca);
            }

            ConfigurationNameToApp = new Dictionary<string, App>();
            foreach (var app in Apps)
            {
                ConfigurationNameToApp.Add(app.ToConfigurationName(), app);
            }
        }

        /// <summary>
        /// Returns the configuration name of an enum value, as it's used in the mobile client configuration.
        /// </summary>
        /// <param name="en">Th enum value</param>
        /// <returns>The configuration name of the enum value or the name of the enum value if no configuration name is defined.</returns>
        public static string ToConfigurationName(this Enum en)
        {
            var memInfo = en.GetType().GetMember(en.ToString());
            if (!memInfo.Any()) return en.ToString();
            var attrs = memInfo[0].GetCustomAttributes(typeof(InConfigurationName), false);
            return attrs.Any() ? ((InConfigurationName)attrs[0]).Name : en.ToString();
        }

        /// <summary>
        /// List of all <seealso cref="Platform"/> configuration names.
        /// </summary>
        public static IEnumerable<string> PlatformNames { get; } = Enum.GetValues(typeof(Platform))
            .Cast<Platform>().Select(e => e.ToConfigurationName());

        /// <summary>
        /// List of all <seealso cref="Platform"/>s.
        /// </summary>
        public static IList<Platform> Platforms { get; } = (Platform[]) Enum.GetValues(typeof(Platform));

        /// <summary>
        /// List of all <seealso cref="ConfigControlAttribute"/> configuration names.
        /// </summary>
        public static IEnumerable<string> CcaNames { get; } = Enum.GetValues(typeof(ConfigControlAttribute))
            .Cast<ConfigControlAttribute>().Select(e => e.ToConfigurationName());

        /// <summary>
        /// List of all <seealso cref="ConfigControlAttribute"/>s.
        /// </summary>
        public static IList<ConfigControlAttribute> ConfigControlAttributes { get; } = (ConfigControlAttribute[])Enum.GetValues(typeof(ConfigControlAttribute));

        /// <summary>
        /// List of all <seealso cref="App"/> configuration names.
        /// </summary>
        public static IEnumerable<string> AppNames { get; } = Enum.GetValues(typeof(App))
            .Cast<App>().Select(e => e.ToConfigurationName());

        /// <summary>
        /// List of all <seealso cref="App"/>s.
        /// </summary>
        public static IList<App> Apps { get; } = (App[])Enum.GetValues(typeof(App));

        /// <summary>
        /// List of all words that are reserved for special meaning and can not be used as aspect names.
        /// </summary>
        public static IEnumerable<string> ReservedWords => _reservedWords;

        /// <summary>
        /// Lookups up the configuration name to a <see cref="ConfigControlAttribute"/>.
        /// </summary>
        /// <param name="configurationName">The configuration name</param>
        /// <returns>The <see cref="ConfigControlAttribute"/> value</returns>
        /// <exception cref="ArgumentException">When there is no such configuration name</exception>
        public static ConfigControlAttribute GetCca(string configurationName)
        {
            if (ConfigurationNameToCca.ContainsKey(configurationName))
            {
                return ConfigurationNameToCca[configurationName];
            }
            throw new ArgumentException($"{configurationName} is not a configuration name for any {nameof(ConfigControlAttribute)}", nameof(configurationName));
        }

        /// <summary>
        /// Lookups up the configuration name to a <see cref="Platform"/>.
        /// </summary>
        /// <param name="configurationName">The configuration name</param>
        /// <returns>The <see cref="Platform"/> value</returns>
        /// <exception cref="ArgumentException">When there is no such configuration name</exception>
        public static Platform GetPlatform(string configurationName)
        {
            if (ConfigurationNameToPlatform.ContainsKey(configurationName))
            {
                return ConfigurationNameToPlatform[configurationName];
            }
            throw new ArgumentException($"{configurationName} is not a configuration name for any {nameof(Platform)}", nameof(configurationName));
        }

        /// <summary>
        /// Lookups up the configuration name to an <see cref="App"/>.
        /// </summary>
        /// <param name="configurationName">The configuration name</param>
        /// <returns>The <see cref="App"/> value</returns>
        /// <exception cref="ArgumentException">When there is no such configuration name</exception>
        public static App GetApp(string configurationName)
        {
            if (ConfigurationNameToApp.ContainsKey(configurationName))
            {
                return ConfigurationNameToApp[configurationName];
            }
            throw new ArgumentException($"{configurationName} is not a configuration name for any {nameof(App)}", nameof(configurationName));
        }
    }
}
