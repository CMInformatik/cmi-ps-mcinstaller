using System;
using System.Collections.Generic;
using cmi.mc.config.ModelContract;

namespace cmi.mc.config.ModelImpl.Decorators
{

    public class TenantSpecificUriDecorator : ISimpleAspect
    {
        private readonly ISimpleAspect _cap;
        private readonly string _tenantPlaceholder;

        /// <param name="simpleAspect">Aspect to decorate.</param>
        /// <param name="tenantPlaceholder">When found in the uri, this string is replaced with the tenant name.</param>
        public TenantSpecificUriDecorator(ISimpleAspect simpleAspect, string tenantPlaceholder = "tenantname")
        {
            _cap = simpleAspect ?? throw new ArgumentNullException(nameof(simpleAspect));

            if (_cap.Type != typeof(Uri))
            {
                throw new ArgumentException(
                    $"Only aspects with value type {typeof(Uri).Name} are supported. {_cap.Name} has type {_cap.Type?.Name}.",
                    nameof(simpleAspect));
            }

            _tenantPlaceholder = string.IsNullOrWhiteSpace(tenantPlaceholder) ? null : tenantPlaceholder;
        }

        public object GetDefaultValue(ITenant tenant = null, Platform platform = Platform.Unspecified)
        {
            var defaultValue = _cap.GetDefaultValue(tenant, platform);
            var uri = defaultValue as Uri;
            if (tenant == null || tenant.ServiceBaseUrl == null || uri == null)
            {
                return defaultValue;
            }
            var tenantSpecific = new Uri(new Uri(tenant.ServiceBaseUrl.GetLeftPart(UriPartial.Authority)), uri.AbsolutePath);
            return _tenantPlaceholder != null ? new Uri(tenantSpecific.ToString().Replace(_tenantPlaceholder, tenant.Name)) : tenantSpecific;
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
        public Type Type => _cap.Type;
        public AxSupport AxSupport => _cap.AxSupport;
        public bool IsPlatformSpecific
        {
            get => _cap.IsPlatformSpecific;
            set => _cap.IsPlatformSpecific = value;
        }
        public void TestValue(object value, ITenant tenant = null, Platform platform = Platform.Unspecified) => _cap.TestValue(value, tenant, platform);
        #endregion
    }
}
