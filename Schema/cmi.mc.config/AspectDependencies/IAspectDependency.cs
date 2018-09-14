using cmi.mc.config.SchemaComponents;

namespace cmi.mc.config.AspectDependencies
{
    public interface IAspectDependency
    {
        void Verify(ITenant tenant);
        void Ensure(ITenant tenant);
    }
}
