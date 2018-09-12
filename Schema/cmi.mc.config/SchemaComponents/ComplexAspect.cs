using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace cmi.mc.config.SchemaComponents
{
    public class ComplexAspect : Aspect
    {
        protected readonly IDictionary<string, Aspect> AspectsInternal = new Dictionary<string, Aspect>();
        public readonly ConfigControlAttribute DefaultCca;

        public IReadOnlyDictionary<string, Aspect> Aspects => new ReadOnlyDictionary<string, Aspect>(AspectsInternal);

        public ComplexAspect(string name, ConfigControlAttribute defaultCca = ConfigControlAttribute.NotSet) : base(name)
        {
            DefaultCca = defaultCca;
        }

        public virtual void AddAspect(Aspect aspect)
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
            AspectsInternal.Add(aspect.Name, aspect);
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
