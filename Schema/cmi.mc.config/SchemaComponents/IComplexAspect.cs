using System.Collections.Generic;

namespace cmi.mc.config.SchemaComponents
{
    public interface IComplexAspect : IAspect
    {
        ConfigControlAttribute DefaultCca { get; }
        IReadOnlyDictionary<string, IAspect> Aspects { get; }
        void AddAspect(IAspect aspect);
    }
}