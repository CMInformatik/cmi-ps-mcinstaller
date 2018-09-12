using System;
using System.Collections.Generic;

namespace cmi.mc.config.SchemaComponents
{
    public abstract class Element
    {
        public readonly string Name;
        public readonly IList<IElementDependency> Dependencies = new List<IElementDependency>();

        protected Element(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
