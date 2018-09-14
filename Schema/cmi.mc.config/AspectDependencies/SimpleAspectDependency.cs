using System;
using cmi.mc.config.SchemaComponents;

namespace cmi.mc.config.AspectDependencies
{
    public class SimpleAspectDependency : IAspectDependency
    {
        private readonly App _app;
        private readonly ISimpleAspect _aspect;
        private readonly object _value;

        public SimpleAspectDependency(App app, ISimpleAspect aspect, object value)
        {
            _aspect = aspect ?? throw new ArgumentNullException(nameof(aspect));
            _app = app;
            _value = value;
            _aspect.TestValue(value);
        }

        private void VerifyInternal(object currentValue)
        {
            if (currentValue == null && _value == null) return;
            if (currentValue == null || !currentValue.Equals(_value))
            {
                throw new AspectDependencyNotFulfilled($"The dependency {_aspect.GetAspectPath()} does not have the required value of {_value}.");
            }
        }

        public void Verify(ITenant tenant)
        {
            if (tenant == null) throw new ArgumentNullException(nameof(tenant));
            var currentValue = tenant.GetConfigurationProperty(_app, _aspect.GetAspectPath());
            VerifyInternal(currentValue);
        }

        public void Ensure(ITenant tenant)
        {
            if (tenant == null) throw new ArgumentNullException(nameof(tenant));
            var currentValue = tenant.GetConfigurationProperty(_app, _aspect.GetAspectPath());
            try
            {
                VerifyInternal(currentValue);
            }
            catch (AspectDependencyNotFulfilled)
            {
                tenant.SetConfigurationProperty(_app, _aspect.GetAspectPath(), _value, true);
            }
        }
    }
}
