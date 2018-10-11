using System.Collections.Generic;

namespace cmi.mc.config.ModelContract
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
        IAspect this[string name] { get; }
    }
}