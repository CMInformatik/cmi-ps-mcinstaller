using System;
using System.Linq;
using cmi.mc.config.ModelContract;
using Newtonsoft.Json.Linq;

namespace cmi.mc.config.Extensions
{
    internal static class JPropertyChildExtensions
    {
        public static bool HasChildProperty(this JProperty prop, IAspect aspect)
        {
            if (aspect == null) throw new ArgumentNullException(nameof(aspect));
            return HasChildProperty(prop, aspect.Name);
        }

        public static bool HasChildProperty(this JProperty prop, Enum en)
        {
            if (en == null) throw new ArgumentNullException(nameof(en));
            return HasChildProperty(prop, en.ToConfigurationName());
        }

        public static bool HasChildProperty(this JProperty prop, string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return prop.Value.Children<JProperty>().Any(p => p.Name.Equals(name));
        }

        public static JProperty GetChildProperty(this JProperty prop, IAspect aspect)
        {
            if (aspect == null) throw new ArgumentNullException(nameof(aspect));
            return GetChildProperty(prop, aspect.Name);
        }

        public static JProperty GetChildProperty(this JProperty prop, Enum en)
        {
            if (en == null) throw new ArgumentNullException(nameof(en));
            return GetChildProperty(prop, en.ToConfigurationName());
        }

        public static JProperty GetChildProperty(this JProperty prop, string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return prop.Value.Children<JProperty>().SingleOrDefault(p => p.Name.Equals(name));
        }
    }
}
