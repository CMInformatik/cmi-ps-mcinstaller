using System;

namespace cmi.mc.config.ModelContract
{
    /// <inheritdoc />
    /// <summary>
    ///     Name/Symbol of the enum value in the mobile client configuration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    internal class InConfigurationName : Attribute
    {
        /// <param name="name">Name of the enum value in a mobile client configuration.</param>
        public InConfigurationName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            Name = name;
        }

        /// <summary>
        /// Name of the enum value in a mobile client configuration.
        /// </summary>
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// Specifies the client platform in a mobile client configuration.
    /// </summary>
    public enum Platform
    {
        /// <summary>
        /// The context will only apply to browsers.
        /// </summary>
        [InConfigurationName("web")] Web,
        /// <summary>
        /// The context will only apply to apps (e.g. Andriod, IOS).
        /// </summary>
        [InConfigurationName("app")] App,
        /// <summary>
        /// The platform is unspecified and the context will apply to any platform.
        /// </summary>
        Unspecified
    }

    /// <summary>
    /// Specifies the application in a mobile client configuration.
    /// </summary>
    public enum App
    {
        [InConfigurationName("common")] Common,
        [InConfigurationName("mobileclients")] Mobileclients,

        [InConfigurationName("dossierbrowser")]
        Dossierbrowser,

        [InConfigurationName("sitzungsvorbereitung")]
        Sitzungsvorbereitung,

        [InConfigurationName("zusammenarbeitdritte")]
        Zusammenarbeitdritte
    }

    /// <summary>
    /// Specifies the value interpretation mode of leaf properties in a mobile client configuration.
    /// </summary>
    public enum ConfigControlAttribute
    {
        [InConfigurationName("_extend")] Extend,
        [InConfigurationName("_replace")] Replace,
        [InConfigurationName("_remove")] Remove,
        [InConfigurationName("_internal")] Internal,
        [InConfigurationName("_private")] Private,
        NotSet
    }

    /// <summary>
    ///     List of CMI Axioma releases with feature additions.
    ///     Lower Ax versions will compare lower to higher Ax versions (e.g. R16_1 &lt; R17 &lt; R18)
    /// </summary>
    public enum AxSupport
    {
        /// <summary>
        /// Version 16.1 and higher.
        /// </summary>
        R16_1 = 0,
        /// <summary>
        /// Version 17.0 and higher.
        /// </summary>
        R17 = 1,
        /// <summary>
        /// Version 18.0 and higher.
        /// </summary>
        R18 = 2
    }
}