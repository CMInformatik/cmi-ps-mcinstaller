using System;

namespace cmi.mc.config.ModelContract
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    internal class InConfigurationName : Attribute
    {
        public string Name { get; set; }
        public InConfigurationName(string name)
        {
            if(String.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            Name = name;
        }

        public override string ToString() => Name;
    }

    public enum Platform
    {
        [InConfigurationName("web")]
        Web,
        [InConfigurationName("app")]
        App,
        Unspecified
    }

    public enum App
    {
        [InConfigurationName("common")]
        Common,
        [InConfigurationName("mobileclients")]
        Mobileclients,
        [InConfigurationName("dossierbrowser")]
        Dossierbrowser,
        [InConfigurationName("sitzungsvorbereitung")]
        Sitzungsvorbereitung,
        [InConfigurationName("zusammenarbeitdritte")]
        Zusammenarbeitdritte
    }

    public enum ConfigControlAttribute
    {
        [InConfigurationName("_extend")]
        Extend,
        [InConfigurationName("_replace")]
        Replace,
        [InConfigurationName("_remove")]
        Remove,
        [InConfigurationName("_internal")]
        Internal,
        [InConfigurationName("_private")]
        Private,
        NotSet
    }

    public enum AxSupport
    {
        R16_1,
        R17,
        R18
    }
}
