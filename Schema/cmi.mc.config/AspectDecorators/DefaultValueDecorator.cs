using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using cmi.mc.config.AspectDependencies;
using cmi.mc.config.SchemaComponents;

namespace cmi.mc.config.AspectDecorators
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

        public object GetDefaultValue(ITenant tenant = null)
        {          
            if (tenant == null)
            {
                return _cap.GetDefaultValue();
            }
            Debug.Assert(tenant.ServiceBaseUrl != null);
            return _pattern
                .Replace(TenantNamePlaceholder, tenant.Name)
                .Replace(ServiceBaseUrlPlaceholder, tenant.ServiceBaseUrl.ToString())
                .Replace(OriginalDefaultPlaceholder, _cap.GetDefaultValue() as string);
        }

        public void TestValue(object value, ITenant tenant = null)
        {
            _cap.TestValue(value);
            if (!_enforceDefaultValue) return;

            var defaultValue = GetDefaultValue(tenant);
            if (defaultValue == null && value == null) return;
            if (value == null || !value.Equals(defaultValue))
            {
                throw new ArgumentException($"The value for this property must be '{defaultValue}'");
            }
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
        public IEnumerable<IAspect> Traverse() => _cap.Traverse();
        public bool IsRequired
        {
            get => _cap.IsRequired;
            set => _cap.IsRequired = value;
        }
        public Type Type => _cap.Type;
        public AxSupport AxSupport => _cap.AxSupport;
        public IReadOnlyList<ValidateArgumentsAttribute> ValidationAttributes => _cap.ValidationAttributes;
        public void AddValidationAttribute(ValidateArgumentsAttribute validator) =>
            _cap.AddValidationAttribute(validator);
#endregion
    }
}
