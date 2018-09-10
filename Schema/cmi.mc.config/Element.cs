using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cmi.mc.config
{
    public abstract class Element
    {
        public readonly ConfigControlAttribute DefaultCca;
        public readonly string Name;
        public readonly IList<IElementDependency> Dependencies = new List<IElementDependency>();

        protected Element(string name, ConfigControlAttribute defaultCca = ConfigControlAttribute.NotSet)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            Name = name;
            DefaultCca = defaultCca;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
