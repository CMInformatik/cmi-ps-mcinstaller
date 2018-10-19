using System;

namespace cmi.mc.config.ModelContract
{
    /// <summary>
    ///     Name/Symbol of the enum value in the mobile client configuration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    internal class InConfigurationName : Attribute
    {
        public InConfigurationName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            Name = name;
        }

        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public enum Platform
    {
        [InConfigurationName("web")] Web,
        [InConfigurationName("app")] App,
        Unspecified
    }

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
        R16_1 = 0,
        R17 = 1,
        R18 = 2
    }
}