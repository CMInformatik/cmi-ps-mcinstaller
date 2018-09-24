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
        IAspect AddDependency(IAspectDependency dependency);
        IAspect AddDependency(params IAspectDependency[] dependency);
        string GetAspectPath();
        IEnumerable<IAspect> Traverse();
    }
}