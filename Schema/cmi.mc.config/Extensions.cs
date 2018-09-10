using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace cmi.mc.config
{
    internal static class Extensions
    {
        public static bool HasChildProperty(this JProperty prop, Aspect aspect)
        {
            if (aspect == null) throw new ArgumentNullException(nameof(aspect));
            return HasChildProperty(prop, aspect.Name);
        }

        public static bool HasChildProperty(this JProperty prop, string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return prop.Value.Children<JProperty>().Any(p => p.Name.Equals(name));
        }

        public static JProperty GetChildProperty(this JProperty prop, Aspect aspect)
        {
            if(aspect == null) throw new ArgumentNullException(nameof(aspect));
            return GetChildProperty(prop, aspect.Name);
        }

        public static JProperty GetChildProperty(this JProperty prop, string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return prop.Value.Children<JProperty>().SingleOrDefault(p => p.Name.Equals(name));
        }
    }
}
