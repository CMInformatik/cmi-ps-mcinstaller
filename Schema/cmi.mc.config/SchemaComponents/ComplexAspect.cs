using System;
using System.Collections.Generic;

namespace cmi.mc.config.SchemaComponents
{
    public class ComplexAspect : Aspect, IComplexAspect
    {
        protected readonly Dictionary<string, IAspect> AspectsInternal = new Dictionary<string, IAspect>();
        public ConfigControlAttribute DefaultCca { get; }

        public IReadOnlyDictionary<string, IAspect> Aspects => AspectsInternal;

        public ComplexAspect(string name, ConfigControlAttribute defaultCca = ConfigControlAttribute.NotSet) : base(name)
        {
            DefaultCca = defaultCca;
        }

        public virtual void AddAspect(IAspect aspect)
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

        public override IEnumerable<IAspect> Traverse()
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
