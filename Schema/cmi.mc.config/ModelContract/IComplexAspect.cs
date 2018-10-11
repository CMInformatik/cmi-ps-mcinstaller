using System.Collections.Generic;

namespace cmi.mc.config.ModelContract
{
    public interface IComplexAspect : IAspect
    {
        ConfigControlAttribute DefaultCca { get; }
        IReadOnlyDictionary<string, IAspect> Aspects { get; }
        IComplexAspect AddAspect(IAspect aspect);
        IComplexAspect AddAspect(params IAspect[] aspect);
        IComplexAspect AddAspect(IEnumerable<IAspect> aspect);
    }
}