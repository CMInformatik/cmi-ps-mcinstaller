using System.Diagnostics;
using cmi.mc.config.ModelContract;

namespace cmi.mc.config.ModelComponents.Dependencies
{
    public class AppDependency : IAspectDependency
    {
        private readonly App _requiredApp;

        public AppDependency(App requiredApp)
        {
            _requiredApp = requiredApp;
        }

        public void Verify(ITenant tenant, App app, IAspect aspect)
        {
            Debug.Assert(tenant != null);
            Debug.Assert(aspect != null);
            if (!tenant.Has(_requiredApp))
            {
                throw new AspectDependencyNotFulfilledException($"{aspect.GetAspectPath()}:{_requiredApp} requires to be enabled when this property is set.");
            }
        }

        public void Ensure(ITenant tenant, App app, IAspect aspect)
        {
            Debug.Assert(tenant != null);
            Debug.Assert(aspect != null);
            if (!tenant.Has(_requiredApp))
            {
                tenant.Add(_requiredApp);
            }
        }
    }
}
