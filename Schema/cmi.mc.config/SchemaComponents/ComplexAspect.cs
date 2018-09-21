using System;
using System.Collections.Generic;
using System.Linq;

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

        public virtual IComplexAspect AddAspect(IAspect aspect)
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
            return this;
        }

        public IComplexAspect AddAspect(params IAspect[] aspect)
        {
            var exceptions = new List<Exception>();
            if (aspect == null) return this;
            foreach (var a in aspect)
            {
                try
                {
                    AddAspect(a);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }
            return exceptions.Any() ? throw new AggregateException(exceptions) : this;
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
