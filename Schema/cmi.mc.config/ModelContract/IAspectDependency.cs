namespace cmi.mc.config.ModelContract
{
    public interface IAspectDependency
    {
        void Verify(ITenant tenant, App app, IAspect aspect);
        void Ensure(ITenant tenant, App app, IAspect aspect);
    }
}
