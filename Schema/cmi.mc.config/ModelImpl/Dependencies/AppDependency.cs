using System.Diagnostics;
using cmi.mc.config.ModelContract;

namespace cmi.mc.config.ModelImpl.Dependencies
{
    public class AppDependency : IAspectDependency
    {
        private readonly App _requiredApp;

        public AppDependency(App requiredApp)
        {
            _requiredApp = requiredApp;
        }

        public void Verify(ITenant tenant, App app)
        {
            Debug.Assert(tenant != null);
            if (!tenant.Has(_requiredApp))
            {
                throw new AspectDependencyNotFulfilledException(app, $"{_requiredApp} needs to be enabled for the requested configuration setting.");
            }
        }

        public void Ensure(ITenant tenant, App app)
        {
            Debug.Assert(tenant != null);
            if (!tenant.Has(_requiredApp))
            {
                tenant.Add(_requiredApp, true);
            }
        }
    }
}
