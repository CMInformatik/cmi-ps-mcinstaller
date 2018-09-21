using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using cmi.mc.config.AspectDependencies;
using cmi.mc.config.SchemaComponents;

namespace cmi.mc.config.AspectDecorators
{

    public class TenantSpecificUriDecorator : ISimpleAspect
    {
        private readonly ISimpleAspect _cap;
        private readonly string _tenantPlaceholder;

        /// <param name="simpleAspect">Aspect to decorate.</param>
        /// <param name="tenantPlaceholder">When found in the uri, this string is replaced with the tenant name.</param>
        public TenantSpecificUriDecorator(ISimpleAspect simpleAspect, string tenantPlaceholder = "{tenantname}")
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

        public object GetDefaultValue(ITenant tenant = null)
        {
            var defaultValue = _cap.GetDefaultValue(tenant);
            var uri = defaultValue as Uri;
            if (tenant == null || tenant.ServiceBaseUrl == null || uri == null)
            {
                return defaultValue;
            }
            var tenantSpecific = new Uri(new Uri(tenant.ServiceBaseUrl.GetLeftPart(UriPartial.Authority)), uri.AbsolutePath);
            return _tenantPlaceholder != null ? new Uri(tenantSpecific.ToString().Replace(_tenantPlaceholder, tenant.Name)) : tenantSpecific;
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
        public IAspect AddDependency(IAspectDependency dependency) => _cap.AddDependency(dependency);
        public IAspect AddDependency(params IAspectDependency[] dependency) => _cap.AddDependency(dependency);
        public string GetAspectPath() => _cap.GetAspectPath();
        public List<IAspect> GetParents() => _cap.GetParents();
        public IEnumerable<IAspect> Traverse() => _cap.Traverse();
        public bool IsRequired
        {
            get => _cap.IsRequired;
            set => _cap.IsRequired = value;
        }
        public Type Type => _cap.Type;
        public AxSupport AxSupport => _cap.AxSupport;
        public IReadOnlyList<ValidateArgumentsAttribute> ValidationAttributes => _cap.ValidationAttributes;
        public void TestValue(object value, ITenant tenant = null) => _cap.TestValue(value, tenant);
        public void AddValidationAttribute(ValidateArgumentsAttribute validator) =>
            _cap.AddValidationAttribute(validator);
        #endregion
    }
}
