using System;
using System.Collections.Generic;
using System.Linq;
using cmi.mc.config.ModelContract;
using cmi.mc.config.ModelContract.Components;
using cmi.mc.config.ModelContract.Exceptions;

namespace cmi.mc.config.ModelImpl
{
    /// <inheritdoc cref="ISimpleAspect"/>
    /// <summary>
    /// Exceptional handling for $.tenants.tenant.mobileclients.boot.settings.
    /// The correct value depends on the presents or non-presents of other apps.
    /// This is not mappable with the standard model implementation.
    /// This class only handles this specific aspect of the mobile clients configuration.
    /// </summary>
    internal class SpecialBootSetting : Aspect, ISimpleAspect
    {
        private readonly Uri _defaultServiceUrl;

        public SpecialBootSetting(Uri defaultServiceUrl) : base("setting")
        {
            _defaultServiceUrl = defaultServiceUrl ?? throw new ArgumentNullException(nameof(defaultServiceUrl));
        }

        private Uri GetDefaultValueInternal(ITenant tenant)
        {
            var baseUri = tenant?.ServiceBaseUrl ?? _defaultServiceUrl;

            // Look for any other app that is currently enabled.
            var otherApp = McSymbols.Apps.Where(a => a!= App.Mobileclients).FirstOrDefault(a => (tenant?.Has(a) ?? true));
            var suffix = McSymbols.GetAppShortcut(otherApp);

            // Mobileclients must use one of the other apps url the receive the settings.
            return new Uri(baseUri, $"{App.Mobileclients.ToConfigurationName()}/proxy/{tenant?.Name}{suffix}");
        }

        public void TestValue(object value, ITenant tenant = null, Platform platform = Platform.Unspecified)
        {
            if (!(value is Uri))
            {
                throw new ValueValidationException($"{value?.GetType().FullName} is not convertable to type {typeof(Uri).FullName}", this, value);
            }

            var defaultValue = GetDefaultValueInternal(tenant);
            if (!value.Equals(defaultValue))
            {
                throw new ValueValidationException($"{GetAspectPath()} is expected to be {defaultValue}.", this, value);
            }
        }

        public object GetDefaultValue(ITenant tenant = null, Platform platform = Platform.Unspecified) => GetDefaultValueInternal(tenant);

        public bool IsRequired
        {
            get => false;
            set => throw new InvalidOperationException();
        }
        public Type Type => typeof(Uri);
        public AxSupport AxSupport => AxSupport.R16_1;
        public bool IsPlatformSpecific
        {
            get => false;
            set => throw new InvalidOperationException();
        }
        public override IEnumerable<IAspect> Traverse()
        {
            yield return this;
        }
    }
}
