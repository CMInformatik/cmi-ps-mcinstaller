using System;
using cmi.mc.config.ModelContract;

namespace cmi.mc.config.ModelImpl.Dependencies
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

        public void Verify(ITenant tenant, App app)
        {
            if (!_requiresSpecificValue)
            {
                if (!tenant.Has(_app, _otherAspect.GetAspectPath()))
                {
                    throw new AspectDependencyNotFulfilledException($"The dependency {_otherAspect.GetAspectPath()} is not set");
                }
                return;
            }

            if (tenant == null) throw new ArgumentNullException(nameof(tenant));
            var currentValue = tenant.Get(_app, _otherAspect.GetAspectPath());

            if (currentValue == null && _value == null) return;
            if (currentValue == null || !currentValue.Equals(_value))
            {
                throw new AspectDependencyNotFulfilledException($"The dependency {_otherAspect.GetAspectPath()} does not have the required value of {_value}.");
            }
        }

        public void Ensure(ITenant tenant, App app)
        {
            try
            {
                Verify(tenant, app);
            }
            catch (AspectDependencyNotFulfilledException)
            {
                var value = _requiresSpecificValue ? _value : _otherAspect.GetDefaultValue(tenant);
                tenant.Set(_app, _otherAspect.GetAspectPath(), value, true);
            }
        }
    }
}
