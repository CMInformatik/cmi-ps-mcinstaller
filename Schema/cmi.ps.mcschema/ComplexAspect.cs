using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cmi.ps.mcschema
{
    public class ComplexAspect : Aspect
    {
        public readonly IDictionary<string, Aspect> Aspects = new Dictionary<string, Aspect>();

        public ComplexAspect(string name, ConfigControlAttribute defaultCca = ConfigControlAttribute.NotSet) : base(name, defaultCca) { }

        public void AddAspect(Aspect aspect)
        {
            if (aspect == null)
            {
                throw new ArgumentNullException(nameof(aspect));
            }
            if (aspect.Parent != null)
            {
                throw new ArgumentException($"Aspect already has a parent ({aspect.Name})");
            }
            aspect.Parent = this;
            Aspects.Add(aspect.Name, aspect);
        }

        public override IEnumerable<Aspect> Traverse()
        {
            yield return this;
            foreach (var item in Aspects.Values)
            {
                foreach (var child in item.Traverse())
                {
                    yield return child;
                }
            }
        }
    }
}
