using System.Collections.Generic;
using cmi.mc.config.AspectDependencies;

namespace cmi.mc.config.SchemaComponents
{
    public interface IAspect
    {
        string Name { get; }
        IReadOnlyList<IAspectDependency> Dependencies { get; }
        IAspect Parent { get; set; }
        IAspect Root { get; }
        void AddDepenency(IAspectDependency dependency);
        string GetAspectPath();
        List<IAspect> GetParents();
        IEnumerable<IAspect> Traverse();
    }
}