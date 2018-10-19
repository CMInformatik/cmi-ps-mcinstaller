﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using cmi.mc.config.ModelContract;
using Newtonsoft.Json.Linq;
using Exception = System.Exception;
using CValException = cmi.mc.config.ModelContract.ConfigurationValidationException;

namespace cmi.mc.config.ModelComponents
{
    internal class Tenant : ITenant
    {
        private JProperty _configuration;
        private readonly ConfigurationModel _model;

        public string Name => _configuration.Name;
        public Uri ServiceBaseUrl { get; private set; }

        internal Tenant(JProperty configuration, ConfigurationModel model)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _model = model ?? throw new ArgumentNullException(nameof(model));
            if (configuration.Type != JTokenType.Property)
            {
                throw new InvalidDataException($"The tenant {configuration.Name} is not a json property.");
            }
            if (string.IsNullOrWhiteSpace(configuration.Name))
            {
                throw new ArgumentNullException(nameof(configuration.Name));
            }

            if (Has(App.Common, "api.server"))
            {
                var uri = Get<Uri>(App.Common, "api.server");
                ServiceBaseUrl = new Uri(uri.Scheme + "://" + uri.Authority);
            }
            else
            {
                ServiceBaseUrl = _model.DefaultServiceUrl;
            }
            if (!Has(App.Common)) RevertChangesOnFailure(() => Add(App.Common));
        }

        private void RevertChangesOnFailure(Action action)
        {
            Debug.Assert(action != null);
            var beforeChanges = (JProperty)_configuration.DeepClone();
            try
            {
                action.Invoke();
            }
            catch (Exception)
            {
                _configuration.Replace(beforeChanges);
                _configuration = beforeChanges;
                throw;
            }
        }

        public bool Has(App app) => _configuration.HasChildProperty(app);

        public void Add(App app, bool ensureDependencies = false)
        {
            RevertChangesOnFailure(() => _model[app].TestDependencies(this, app, ensureDependencies));
            if (!Has(app))
            {
                RevertChangesOnFailure(() =>
                {
                    _configuration.Value[app.ToConfigurationName()] = JToken.FromObject(new object());
                    foreach (var aspect in _model[app].Traverse().OfType<ISimpleAspect>().Where(a => a.IsRequired))
                    {
                        Set(app, aspect.GetAspectPath(), ensureDependencies);
                    }
                    // update app directory
                    try
                    {
                        var appDirAspect = _model.GetAspect<IAspect>(App.Common, $"appDirectory.{app.ToConfigurationName()}");
                        Set(App.Common, appDirAspect.GetAspectPath());
                    }
                    catch (KeyNotFoundException)
                    {
                        // app does not have an appDirectory?
                    }
                });
            }
        }

        public void Remove(App app)
        {
            if (!Has(app)) return;
            RevertChangesOnFailure(() =>
            {
                _configuration.Value[app.ToConfigurationName()].Remove();
                // update app directory
                Remove(App.Common, $"appDirectory.{app.ToConfigurationName()}");
            });
        }

        public bool Has(App app, string aspectPath, Platform platform = Platform.Unspecified)
        {
            var model = _model.GetAspect<ISimpleAspect>(app, aspectPath);

            // test for platform specific property
            var jPathPlatform = Configuration.BuildJPath(Name, app, model, platform);
            var token = _configuration.Root.SelectTokens(jPathPlatform).SingleOrDefault();

            if (token != null || platform == Platform.Unspecified) return !(token is null);

            // test for platform unspecific property
            var jPathNoplatform = Configuration.BuildJPath(Name, app, model, Platform.Unspecified);
            token = _configuration.Root.SelectTokens(jPathNoplatform).SingleOrDefault();
            return !(token is null);
        }

        public void Remove(App app, string aspectPath, Platform platform)
        {
            var model = _model.GetAspect<IAspect>(app, aspectPath);
            if (!Has(app)) return;
            var xpath = Configuration.BuildJPath(Name, app, model, platform);
            var token = _configuration.Root.SelectTokens(xpath).SingleOrDefault();
            RevertChangesOnFailure(() => token?.Parent?.Remove());
        }

        public void Remove(App app, string aspectPath)
        {
            RevertChangesOnFailure(() =>
            {
                foreach (var pl in McConfigSymbols.Platforms)
                {
                    Remove(app, aspectPath, pl);
                }
            });
        }

        public object Get(App app, string aspectPath, Platform platform = Platform.Unspecified)
        {
            var model = _model.GetAspect<ISimpleAspect>(app, aspectPath);
            if (!Has(app)) return null;
            var xpath = Configuration.BuildJPath(Name, app, model, platform);
            var token = _configuration.Root.SelectTokens(xpath).SingleOrDefault();
            switch (token)
            {
                case null:
                    return null;
                case JArray _ when !model.Type.IsArray:
                    throw new InvalidCastException($"{model.GetAspectPath()} is not epected to contain a json array.");
            }
            return token.ToObject(model.Type);
        }

        public T Get<T>(App app, string aspectPath, Platform platform = Platform.Unspecified)
        {
            var result = Get(app, aspectPath);
            return (result != null) ? (T)result : default(T);
        }

        /// <summary>
        /// Sets a configuration property to its default value.
        /// If the property is a complex property, all child properties will be set to default values.
        /// </summary>
        /// <param name="app">The app of the property.</param>
        /// <param name="aspectPath">Path of the property.</param>
        /// <param name="ensureDependencies">Set dependencies the required values.</param>
        public void Set(App app, string aspectPath, bool ensureDependencies = false, Platform platform = Platform.Unspecified)
        {
            var model = _model.GetAspect<IAspect>(app, aspectPath);
            switch (model)
            {
                case ISimpleAspect simple:
                    var defaultValue = simple.GetDefaultValue(this, platform);
                    RevertChangesOnFailure(() => SetPropertyInternal(app, model, defaultValue, ensureDependencies, platform));
                    break;
                case IComplexAspect complex:
                    void Action()
                    {
                        foreach (var aspect in complex.Traverse().OfType<ISimpleAspect>())
                        {
                            SetPropertyInternal(app, aspect, aspect.GetDefaultValue(this, platform), ensureDependencies, platform);
                        }
                    }
                    RevertChangesOnFailure(Action);
                    break;
                default:
                    throw new NotSupportedException($"This method does not support aspect type {model.GetType().Name}.");
            }
        }

        public void Set(App app, string aspectPath, object value, bool ensureDependencies = false, Platform platform = Platform.Unspecified)
        {
            var model = _model.GetAspect<ISimpleAspect>(app, aspectPath);
            RevertChangesOnFailure(() => SetPropertyInternal(app, model, value, ensureDependencies, platform));
        }

        private void SetPropertyInternal(App app, IAspect aspect, object value, bool ensureDependencies, Platform platform)
        {
            Debug.Assert(aspect != null);
            // is app enabled
            if (!Has(app)) throw new InvalidOperationException($"RequiredApp {app.ToString()} is not enabled for tenant {Name}");
            // test value
            if (aspect is ISimpleAspect simpleAspect) simpleAspect.TestValue(value, this, platform);
            // test dependencies
            aspect.TestDependencies(this, app, ensureDependencies);

            JProperty parentProperty;
            // get parent property
            if (aspect.Parent != null)
            {
                var xpath = Configuration.BuildJPath(Name, app, aspect.Parent, Platform.Unspecified);
                // is parent present?
                if (_configuration.Root.SelectTokens(xpath).SingleOrDefault() == null)
                {
                    // create parent
                    SetPropertyInternal(app, aspect.Parent, new object(), ensureDependencies, platform);
                }
                // parent property should now exists
                parentProperty = (JProperty)_configuration.Root.SelectTokens(xpath).Single().Parent;
            }
            else
            {
                parentProperty = _configuration.GetChildProperty(app);
            }
            Debug.Assert(parentProperty != null);

            // create property
            SetPropertyValue(parentProperty, aspect, value, platform);

            // set default cca
            if (aspect is IComplexAspect ca) parentProperty.GetChildProperty(ca).SetDefaultCCa(ca);
        }

        private static void SetPropertyValue(JProperty parentProperty, IAspect aspect, object value,
            Platform platform)
        {
            Debug.Assert(parentProperty != null);
            Debug.Assert(aspect != null);

            if (aspect is ISimpleAspect)
            {
                if (platform != Platform.Unspecified)
                {
                    // If platform specific value equals unspecific value,
                    // there is no need to set the property platform specific.
                    if ((parentProperty.GetChildProperty(aspect.Name)?.Value as JValue)?.Value?.Equals(value) == true)
                    {
                        // remove the old platform specific value, if present
                        parentProperty.GetChildProperty(platform)?.GetChildProperty(aspect.Name)?.Remove();
                        return;
                    }

                    // Is a platform specific value really required? If all other platforms have the same value,
                    // the property can be set as platform unspecific.
                    var allPlatformsWithSameValue = McConfigSymbols.Platforms.ToList()
                        .Where(p => p != Platform.Unspecified && p != platform)
                        .All(p =>
                        {
                            var currentValue = (parentProperty.GetChildProperty(p)?.GetChildProperty(aspect.Name)?.Value as JValue)?.Value;
                            return currentValue != null && currentValue.Equals(value);
                        });
                    if (allPlatformsWithSameValue)
                    {
                        platform = Platform.Unspecified;
                    }
                }

                if (platform == Platform.Unspecified)
                {
                    // remove more specific platform properties
                    foreach (var p in McConfigSymbols.Platforms)
                    {
                        (parentProperty.GetChildProperty(p)?.Value as JProperty)?
                            .GetChildProperty(aspect.Name)?.Remove();
                    }
                }
                else
                {
                    if (!parentProperty.HasChildProperty(platform))
                    {
                        // add more specific platform property
                        parentProperty.Value[platform.ToConfigurationName()] = JToken.FromObject(new object());
                    }
                    // set parent config part to more specific platform
                    parentProperty = parentProperty.GetChildProperty(platform);
                }
            }

            // set property value
            if (parentProperty.HasChildProperty(aspect))
            {
                // property is already present. Overwriting
                parentProperty.GetChildProperty(aspect).Value = new JValue(value);
            }
            else
            {
                // adding new property
                parentProperty.Value[aspect.Name] = JToken.FromObject(value);
            }
        }

        #region validate
        /// <inheritdoc />
        public void Validate(AxSupport axVersion)
        {
            // common app is required
            if (!Has(App.Common))
            {
                throw new CValException($"The property {App.Common.ToConfigurationName()} is required, but was not found", null, null);
            }
            IList<Exception> problems = new List<Exception>();
            foreach (var app in McConfigSymbols.Apps.Where(Has))
            {
                // validate JTokens
                foreach (var child in _configuration.GetChildProperty(app).Value.Children())
                {
                    ValidateInternal(child, app, Platform.Unspecified, string.Empty, axVersion, ref problems);
                }
                // required aspects must be present
                foreach (var aspect in _model[app].Traverse().OfType<ISimpleAspect>().Where(a => a.IsRequired))
                {
                    if (!Has(app, aspect.GetAspectPath()))
                    {
                        problems.Add(new CValException($"{aspect.GetAspectPath()} is required, but was not found in the configuration.", aspect, null));
                    }
                }
            }
            if (problems.Any()) throw new AggregateException(problems);
        }

        /// <summary>
        /// Validate complex aspect.
        /// </summary>
        private void ValidateInternal(JProperty jProperty, IComplexAspect aspect, Platform platform,
            ref IList<Exception> problems)
        {
            // complex aspects can not be platform specific
            if (platform != Platform.Unspecified)
            {
                throw new CValException($"{aspect.GetAspectPath()} can not be platform specific", aspect, jProperty);
            }
            // the value must be a jobject
            if (jProperty.Value == null || jProperty.Value.Type != JTokenType.Object)
            {
                throw new CValException($"{aspect.GetAspectPath()} does not contain json object", aspect, jProperty);
            }
            // cca with default value?
            if (jProperty.GetCCa() != aspect.DefaultCca)
            {
                problems.Add(new CValException(
                    $"{aspect.GetAspectPath()} is expected to have the {nameof(ConfigControlAttribute)} to be set to {aspect.DefaultCca.ToConfigurationName()}",
                    aspect, jProperty
                ));
            }
        }

        /// <summary>
        /// Validate simple aspect.
        /// </summary>
        private void ValidateInternal(ISimpleAspect aspect, App app, Platform platform, AxSupport axVersion)
        {
            var value = Get(app, aspect.GetAspectPath(), platform);
            // is supported?
            if (aspect.AxSupport > axVersion)
            {
                throw new CValException($"{aspect.GetAspectPath()} is not supported in version {axVersion}", aspect, null);
            }
            // validate value
            if (value != null) aspect.TestValue(value, this, platform);
            // dependencies
            aspect.TestDependencies(this, app);
        }

        /// <summary>
        /// Validate <seealso cref="ConfigControlAttribute"/>.
        /// </summary>
        private void ValidateInternal(ConfigControlAttribute cca, ref IList<Exception> problems)
        {
            switch (cca)
            {
                case ConfigControlAttribute.Replace:
                case ConfigControlAttribute.Remove:
                    problems.Add(new CValException($"{nameof(ConfigControlAttribute)} with value {cca.ToConfigurationName()} is not supported by this tool.", null, null));
                    break;
                default:
                    return;
            }
        }

        /// <summary>
        /// Validate <seealso cref="JToken"/>.
        /// </summary>
        private void ValidateInternal(JToken config, App app, Platform platform, string path, AxSupport axVersion, ref IList<Exception> problems)
        {
            try
            {
                if (!(config is JProperty jProperty))
                {
                    throw new CValException($"{config.Path} is not a json property", null, config);
                }

                // handle cca properties are handled with the complex properties
                if (McConfigSymbols.CcaNames.Contains(jProperty.Name))
                {
                    ValidateInternal(McConfigSymbols.GetCca(jProperty.Name), ref problems);
                    return;
                }

                // handle platform
                if (McConfigSymbols.PlatformNames.Contains(jProperty.Name))
                {
                    if (!(jProperty.Value is JObject))
                    {
                        throw new CValException($"{config.Path} is expected to contain a json object ({nameof(JObject)}).", null, config);
                    }
                    foreach (var child in jProperty.Value.Children())
                    {
                        ValidateInternal(child, app, McConfigSymbols.GetPlatform(jProperty.Name), path, axVersion, ref problems);
                    }
                    return;
                }

                // handle aspect
                var aspectPath = string.IsNullOrWhiteSpace(path) ? jProperty.Name : $"{path}.{jProperty.Name}";
                IAspect model;
                try
                {
                    model = _model.GetAspect(app, aspectPath);
                }
                catch (KeyNotFoundException e)
                {
                    // often the configuration was made in the wrong app section
                    // maybe the aspect path can be found in an other app
                    foreach (var otherApp in McConfigSymbols.Apps)
                    {
                        if (otherApp == app) continue;
                        if (_model.TryGetAspect(otherApp, aspectPath) != null)
                        {
                            throw new CValException($"{aspectPath} is not expected to be configured in app {app}. Move to {otherApp}.", null, e, config);
                        }
                    }
                    throw;
                }

                Debug.Assert(aspectPath != null);
                Debug.Assert(model != null, $"{nameof(_model.GetAspect)} should throw if aspect can not be found");

                switch (model)
                {
                    case IComplexAspect complexModel:
                        ValidateInternal(jProperty, complexModel, platform, ref problems);
                        foreach (var child in jProperty.Value.Children())
                        {
                            // handle childs
                            ValidateInternal(child, app, platform, aspectPath, axVersion, ref problems);
                        }

                        break;
                    case ISimpleAspect simpleModel:
                        ValidateInternal(simpleModel, app, platform, axVersion);
                        break;
                    default:
                        throw new NotImplementedException($"Validation for aspects of type {model?.GetType().FullName} is not implemented");
                }
            }
            catch (Exception e)
            {
                problems.Add(e);
            }
        }

        #endregion
    }
}
