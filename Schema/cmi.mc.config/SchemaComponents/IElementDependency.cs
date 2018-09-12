using Newtonsoft.Json.Linq;

namespace cmi.mc.config.SchemaComponents
{
    public interface IElementDependency
    {
        void Verify(JContainer data);
        void Ensure(JContainer data);
    }
}
