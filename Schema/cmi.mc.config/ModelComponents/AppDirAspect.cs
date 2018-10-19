using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cmi.mc.config.ModelComponents.Dependencies;
using cmi.mc.config.ModelContract;
using Newtonsoft.Json.Linq;

namespace cmi.mc.config.ModelComponents
{
    internal class AppDirAspect : Aspect, ISimpleAspect
    {
        private readonly JObject _appDirConfig;
        private readonly App _app;
        public AppDirAspect(App app) : base(app.ToConfigurationName())
        {
            DependenciesInteral.Add(new AppDependency(app));
            _app = app;
            switch (app)
            {
                case App.Dossierbrowser:
                    _appDirConfig = JObject.Parse(@"{""web"": """",""app"": ""cmidossierbrowser://"",""dossierDetail"": ""/Abstr/{GUID}""}");
                    break;
                case App.Sitzungsvorbereitung:
                    _appDirConfig = JObject.Parse(@"{""web"": """",""app"": ""cmisitzungsvorbereitung://"",""sitzungDetail"": ""/{Gremium}/{Jahr}/{GUID}"",""traktandumDetail"": ""/{Gremium}/{Jahr}/{GUID}/T/{TraktandumGUID}""}");
                    break;
                case App.Zusammenarbeitdritte:
                    _appDirConfig = JObject.Parse(@"{""web"": """", ""aktivitaetDetail"": ""/Aktivitaet/{GUID}""}");
                    break;
                default:
                    _appDirConfig = JObject.Parse("{\"web\": \"\"}");
                    break;
            }
        }

        private JObject GetTenantSpecific(ITenant tenant)
        {
            var config = (JObject)(_appDirConfig.DeepClone());
            if (tenant == null)
            {
                config["web"] = null;
                return config;
            }
            config["web"] = new Uri(new Uri(tenant.ServiceBaseUrl.GetLeftPart(UriPartial.Authority)), $"{_app.ToConfigurationName()}/{tenant.Name}");
            return config;
        }

        public override IEnumerable<IAspect> Traverse()
        {
            yield return this;
        }

        public bool IsRequired
        {
            get => false;
            set => throw new InvalidOperationException();
        }

        public Type Type => typeof(JObject);
        public AxSupport AxSupport => AxSupport.R16_1;
        public bool IsPlatformSpecific
        {
            get => false;
            set => throw new InvalidOperationException();
        }
        public void TestValue(object value, ITenant tenant = null, Platform platform = Platform.Unspecified)
        {
            var expected = GetTenantSpecific(tenant);
            if (!(value is JObject) || !JToken.DeepEquals((JObject)value, expected))
            {
                throw new ValueValidationException($"A value of type {nameof(JObject)} with content {expected.ToString()} is required", this, null);
            }
        }

        public object GetDefaultValue(ITenant tenant = null, Platform platform = Platform.Unspecified)
        {
            return GetTenantSpecific(tenant);
        }
    }
}
