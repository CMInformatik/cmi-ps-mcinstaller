using cmi.mc.config.SchemaComponents;

namespace cmi.mc.config.AspectDependencies
{
    public interface IAspectDependency
    {
        void Verify(ITenant tenant, App app, IAspect aspect);
        void Ensure(ITenant tenant, App app, IAspect aspect);
    }
}
