using System;
using cmi.mc.config.SchemaComponents;

namespace cmi.mc.config.AspectDependencies
{
    public class SimpleAspectDependency : IAspectDependency
    {
        private readonly App _app;
        private readonly ISimpleAspect _otherAspect;
        private readonly object _value;
        private readonly bool _requiresSpecificValue;

        public SimpleAspectDependency(App app, ISimpleAspect aspect, object value)
        {
            _otherAspect = aspect ?? throw new ArgumentNullException(nameof(aspect));
            _app = app;
            _value = value;
            _otherAspect.TestValue(value);
            _requiresSpecificValue = true;
        }
        public SimpleAspectDependency(App app, ISimpleAspect aspect)
        {
            _otherAspect = aspect ?? throw new ArgumentNullException(nameof(aspect));
            _app = app;
            _value = null;
            _requiresSpecificValue = false;
        }

        public void Verify(ITenant tenant, App app, IAspect aspect)
        {
            if (!_requiresSpecificValue)
            {
                if (!tenant.HasConfigurationProperty(_app, _otherAspect.GetAspectPath()))
                {
                    throw new AspectDependencyNotFulfilled($"The dependency {_otherAspect.GetAspectPath()} is not set");
                }
                return;
            }

            if (tenant == null) throw new ArgumentNullException(nameof(tenant));
            var currentValue = tenant.GetConfigurationProperty(_app, _otherAspect.GetAspectPath());

            if (currentValue == null && _value == null) return;
            if (currentValue == null || !currentValue.Equals(_value))
            {
                throw new AspectDependencyNotFulfilled($"The dependency {_otherAspect.GetAspectPath()} does not have the required value of {_value}.");
            }
        }

        public void Ensure(ITenant tenant, App app, IAspect aspect)
        {
            try
            {
                Verify(tenant, app, aspect);
            }
            catch (AspectDependencyNotFulfilled)
            {
                var value = _requiresSpecificValue ? _value : _otherAspect.GetDefaultValue(tenant);
                tenant.SetConfigurationProperty(_app, _otherAspect.GetAspectPath(), value, true);
            }
        }
    }
}
