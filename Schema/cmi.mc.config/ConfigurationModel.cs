using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using cmi.mc.config.ModelComponents;
using cmi.mc.config.ModelComponents.Decorators;
using cmi.mc.config.ModelComponents.Dependencies;
using cmi.mc.config.ModelContract;
using FluentValidation;

namespace cmi.mc.config
{
    public class ConfigurationModel : IReadOnlyDictionary<App, AppSection>
    {
        private readonly IDictionary<App, AppSection> _internal = new Dictionary<App, AppSection>();
        public Uri DefaultServiceUrl { get; private set; } = new Uri("https://mobile.cmiaxioma.ch");

        /// provide prameterless constructor
        public ConfigurationModel() : this(null){}

        public ConfigurationModel(Uri defaultServiceUrl = null)
        {
            if (defaultServiceUrl != null)
            {
                DefaultServiceUrl = new Uri(defaultServiceUrl.Scheme + "://" + defaultServiceUrl.Authority);
            }

            foreach (var appValue in System.Enum.GetValues(typeof(App)))
            {
                _internal.Add((App) appValue, new AppSection((App) appValue));
            }

            // minimal model
            var api = new ComplexAspect("api");
            api.AddAspect(
                new TenantSpecificUriDecorator(
                    new SimpleAspect<Uri>("server", new Uri(DefaultServiceUrl, "mobileclients")) {IsRequired = true}
                ));
            api.AddAspect(
                new DefaultValueDecorator(
                    new SimpleAspect<string>("public", null) {IsRequired = true},
                    $"/proxy/{DefaultValueDecorator.TenantNamePlaceholder}pub",
                    true
                ));
            api.AddAspect(
                new DefaultValueDecorator(
                    new SimpleAspect<string>("private", null) {IsRequired = true},
                    $"/proxy/{DefaultValueDecorator.TenantNamePlaceholder}pri",
                    true
                ));

            _internal[App.Common].AddAspect(api);
            BuildAppDirModel();
        }

        private void BuildAppDirModel()
        {
            // dossierbrowser
            var dbDir = new ComplexAspect(App.Dossierbrowser.ToConfigurationName());
            dbDir.AddDependency(new AppDependency(App.Dossierbrowser));
            dbDir.AddAspect(
                new TenantSpecificUriDecorator(
                    NotNullAspect("web", new Uri(DefaultServiceUrl, $"{App.Dossierbrowser.ToConfigurationName()}/tenantname"))
                ));
            dbDir.AddAspect(
                NotNullAspect("app", "cmidossierbrowser://"),
                NotNullAspect("dossierDetail", "/Abstr/{GUID}"));

            // sitzungsvorbereitung
            var svDir = new ComplexAspect(App.Sitzungsvorbereitung.ToConfigurationName());
            svDir.AddDependency(new AppDependency(App.Sitzungsvorbereitung));
            svDir.AddAspect(
                new TenantSpecificUriDecorator(
                    NotNullAspect("web", new Uri(DefaultServiceUrl, $"{App.Sitzungsvorbereitung.ToConfigurationName()}/tenantname"))
                ));
            svDir.AddAspect(
                NotNullAspect("app", "cmisitzungsvorbereitung://"),
                NotNullAspect("sitzungDetail", "/{Gremium}/{Jahr}/{GUID}"),
                NotNullAspect("traktandumDetail", "/{Gremium}/{Jahr}/{GUID}/T/{TraktandumGUID}"));

            // zusammenarbeit dritte
            var zdDir = new ComplexAspect(App.Zusammenarbeitdritte.ToConfigurationName());
            zdDir.AddDependency(new AppDependency(App.Zusammenarbeitdritte));
            zdDir.AddAspect(
                new TenantSpecificUriDecorator(
                    NotNullAspect("web", new Uri(DefaultServiceUrl, $"{App.Zusammenarbeitdritte.ToConfigurationName()}/tenantname"))
                ));
            zdDir.AddAspect(
                NotNullAspect("app", "cmisitzungsvorbereitung://"),
                NotNullAspect("aktivitaetDetail", "/Aktivitaet/{GUID}")
            );

            _internal[App.Common].AddAspect(new ComplexAspect("appDirectory").AddAspect(dbDir, svDir, zdDir));
        }

        class Validator<T> : AbstractValidator<T>
        {
        }

        private static ISimpleAspect NotNullAspect(string name, string value)
        {
            var val = new Validator<string>();
            val.RuleFor(s => s).NotEmpty();
            return new SimpleAspect<string>(name, value, AxSupport.R16_1, val);
        }

        private static ISimpleAspect NotNullAspect(string name, Uri value)
        {
            var val = new Validator<Uri>();
            val.RuleFor(u => u.AbsolutePath).NotEmpty();
            return new SimpleAspect<Uri>(name, value, AxSupport.R16_1, val);
        }

        public T GetAspect<T>(App app, string aspectPath) where T : IAspect
        {
            var r = GetAspect(app, aspectPath);
            if (!(r is T))
            {
                throw new InvalidOperationException($"{aspectPath} is not a {typeof(T).Name}.");
            }
            return (T) r;
        }

        public IAspect GetAspect(App app, string aspectPath)
        {
            Aspect.ThrowIfInvalidAspectPath(aspectPath);
            var parts = aspectPath.Split('.');
            IAspect currentAspect = this[app];
            foreach (var part in parts)
            {
                if (currentAspect is IComplexAspect)
                {
                    try
                    {
                        currentAspect = ((IComplexAspect)currentAspect).Aspects[part];
                    }
                    catch (KeyNotFoundException e)
                    {
                        throw new KeyNotFoundException($"{app} does not have a aspect path of {aspectPath}", e);
                    }
                }
                else
                {
                    throw new KeyNotFoundException($"{app} does not have a aspect path of {aspectPath}");
                }
            }
            return (IAspect)currentAspect;
        }

        #region IReadOnlyDictionary impl.
        public AppSection this[App key] => _internal[key];

        public IEnumerable<App> Keys => _internal.Keys;

        public IEnumerable<AppSection> Values => _internal.Values;

        public int Count => _internal.Count;

        public bool ContainsKey(App key)
        {
            return _internal.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<App, AppSection>> GetEnumerator()
        {
            return _internal.GetEnumerator();
        }

        public bool TryGetValue(App key, out AppSection value)
        {
            return _internal.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _internal.GetEnumerator();
        }


        #endregion
    }
}
