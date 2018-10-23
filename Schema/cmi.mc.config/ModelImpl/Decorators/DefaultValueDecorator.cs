using System;
using System.Collections.Generic;
using System.Diagnostics;
using cmi.mc.config.ModelContract;

namespace cmi.mc.config.ModelImpl.Decorators
{
    public class DefaultValueDecorator : ISimpleAspect
    {
        private readonly ISimpleAspect _cap;
        private readonly string _pattern;
        private readonly bool _enforceDefaultValue;

        public DefaultValueDecorator(ISimpleAspect simpleAspect, string enhancePattern = "{defaultvalue}", bool enforceDefaultValue = false)
        {
            _cap = simpleAspect ?? throw new ArgumentNullException(nameof(simpleAspect));
            _pattern = enhancePattern;
            _enforceDefaultValue = enforceDefaultValue;

            if (_cap.Type != typeof(string))
            {
                throw new ArgumentException(
                    $"Only aspects with value type {typeof(string).Name} are supported. {_cap.Name} has type {_cap.Type?.Name}.", 
                    nameof(simpleAspect));
            }
        }

        public static string TenantNamePlaceholder => "{tenantname}";
        public static string OriginalDefaultPlaceholder => "{defaultvalue}";

        public static string ServiceBaseUrlPlaceholder => "{servicebaseurl}";

        public object GetDefaultValue(ITenant tenant = null, Platform platform = Platform.Unspecified)
        {          
            if (tenant == null)
            {
                return _cap.GetDefaultValue(null, platform);
            }
            Debug.Assert(tenant.ServiceBaseUrl != null);
            return _pattern
                .Replace(TenantNamePlaceholder, tenant.Name)
                .Replace(ServiceBaseUrlPlaceholder, tenant.ServiceBaseUrl.ToString())
                .Replace(OriginalDefaultPlaceholder, _cap.GetDefaultValue(tenant, platform) as string);
        }

        public void TestValue(object value, ITenant tenant = null, Platform platform = Platform.Unspecified)
        {
            _cap.TestValue(value, tenant, platform);
            if (!_enforceDefaultValue) return;

            var defaultValue = GetDefaultValue(tenant, platform);
            if (defaultValue == null && value == null) return;
            if (value == null || !value.Equals(defaultValue))
            {
                throw new ValueValidationException($"The value for {GetAspectPath()} must be '{defaultValue}'", this, value);
            }
        }
        public IEnumerable<IAspect> Traverse()
        {
            yield return this;
        }

        #region unchanged behavior
        public string Name => _cap.Name;
        public IReadOnlyList<IAspectDependency> Dependencies => _cap.Dependencies;
        public IAspect Parent
        {
            get => _cap.Parent;
            set => _cap.Parent = value;
        }
        public IAspect Root => _cap.Root;
        public string GetAspectPath() => _cap.GetAspectPath();
        public IAspect this[string name] => _cap[name];
        public bool IsRequired
        {
            get => _cap.IsRequired;
            set => _cap.IsRequired = value;
        }
        public bool IsPlatformSpecific
        {
            get => _cap.IsPlatformSpecific;
            set => _cap.IsPlatformSpecific = value;
        }
        public Type Type => _cap.Type;
        public AxSupport AxSupport => _cap.AxSupport;
#endregion
    }
}
