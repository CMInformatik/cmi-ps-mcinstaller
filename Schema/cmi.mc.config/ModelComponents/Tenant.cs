using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using cmi.mc.config.ModelContract;
using Newtonsoft.Json.Linq;

namespace cmi.mc.config.ModelComponents
{
    internal class Tenant : ITenant
    {
        private JProperty _configuration;
        private readonly ConfigurationModel _model;
        private static readonly IEnumerable<string> CcaNames = Enum.GetValues(typeof(ConfigControlAttribute)).Cast<ConfigControlAttribute>().Select(e => e.ToConfigurationName());
        private static readonly IList<Platform> Platforms = (Platform[])Enum.GetValues(typeof(Platform));

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
            if(!Has(App.Common)) RevertChangesOnFailure(() => Add(App.Common));
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
            RevertChangesOnFailure(() =>
            {
                foreach (var dep in _model[app].Dependencies)
                {
                    if (ensureDependencies)
                    {
                        dep.Ensure(this, app);
                    }
                    else
                    {
                        dep.Verify(this, app);
                    }
                }
            });
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
                        var appDirAspect = _model.GetAspect<IComplexAspect>(App.Common, $"appDirectory.{app.ToConfigurationName()}");
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
            var jPathPlatform = BuildJPath(_configuration, app, model, platform);    
            var token = _configuration.Root.SelectTokens(jPathPlatform).SingleOrDefault();

            if (token != null || platform == Platform.Unspecified) return !(token is null);

            // test for platform unspecific property
            var jPathNoplatform = BuildJPath(_configuration, app, model, Platform.Unspecified);
            token = _configuration.Root.SelectTokens(jPathNoplatform).SingleOrDefault();
            return !(token is null);
        }

        public void Remove(App app, string aspectPath, Platform platform)
        {
            var model = _model.GetAspect<IAspect>(app, aspectPath);
            if (!Has(app)) return;
            var xpath = BuildJPath(_configuration, app, model, platform);
            var token = _configuration.Root.SelectTokens(xpath).SingleOrDefault();
            RevertChangesOnFailure(() =>
            {
                token?.Parent?.Remove();
            });
        }

        public void Remove(App app, string aspectPath)
        {
            RevertChangesOnFailure(() =>
            {
                foreach (var pl in Platforms)
                {
                    Remove(app, aspectPath, pl);
                }
            });
        }

        public object Get(App app, string aspectPath, Platform platform = Platform.Unspecified)
        {
            var model = _model.GetAspect<ISimpleAspect>(app, aspectPath);
            if (!Has(app)) return null;
            var xpath = BuildJPath(_configuration, app, model, platform);
            var token = _configuration.Root.SelectTokens(xpath).SingleOrDefault();
            if (token != null && !(token is JValue))
            {
                throw new InvalidDataException($"A json value ({nameof(JValue)}) was expected at path {xpath}, but a {token.GetType().Name} was found.");
            }
            return (token as JValue)?.Value;
        }

        public T Get<T>(App app, string aspectPath, Platform platform = Platform.Unspecified)
        {
            var result = Get(app, aspectPath);

            // no implicit cast from string to uri, but json reader delivers string, when uri is expected
            if (typeof(T) == typeof(Uri) && result is string s)
            {
                result = new Uri(s);
            }

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
            if (!Has(app))
            {
                throw new InvalidOperationException($"RequiredApp {app.ToString()} is not enabled for tenant {Name}");
            }
            // test value
            if (aspect is ISimpleAspect simpleAspect) simpleAspect.TestValue(value, this, platform);
            // test dependencies
            if (aspect.Dependencies.Any())
            {
                foreach (var dep in aspect.Dependencies)
                {
                    if (ensureDependencies)
                    {
                        dep.Ensure(this, app);
                    }
                    else
                    {
                        dep.Verify(this, app);
                    }
                }
            }

            JProperty parentConfigPart;
            // get parent property
            if (aspect.Parent != null)
            {
                var xpath = $"$.{_configuration.Path}.{app.ToConfigurationName()}.{aspect.Parent.GetAspectPath()}";
                // is parent present?
                if (_configuration.Root.SelectTokens(xpath).SingleOrDefault() == null)
                {
                    // create parent
                    SetPropertyInternal(app, aspect.Parent, new object(), ensureDependencies, platform);
                }
                // parent property should now exists
                parentConfigPart = (JProperty)_configuration.Root.SelectTokens(xpath).Single().Parent;
            }
            else
            {
                parentConfigPart = _configuration.GetChildProperty(app);
            }
            Debug.Assert(parentConfigPart != null);

            // create property
            SetPropertyValue(parentConfigPart, aspect, value, platform);

            // set default cca
            if (aspect is IComplexAspect complexAspect)
            {
                SetDefaultCCa(complexAspect, parentConfigPart.GetChildProperty(complexAspect));
            }
        }

        private static void SetDefaultCCa(IComplexAspect aspect, JProperty configPart)
        {
            Debug.Assert(aspect != null);
            Debug.Assert(configPart != null);
            if (aspect.DefaultCca == ConfigControlAttribute.NotSet) return; // no default cca defined      
            if (configPart.Value.Children<JProperty>().Any(p => CcaNames.Contains(p.Name))) return; // a cca is already set
            configPart.Value[aspect.DefaultCca.ToConfigurationName()] = JToken.FromObject(true);
        }

        private static void SetPropertyValue(JProperty parentConfigPart, IAspect aspect, object value,
            Platform platform)
        {
            Debug.Assert(parentConfigPart != null);
            Debug.Assert(aspect != null);

            if (aspect is ISimpleAspect)
            {
                if (platform != Platform.Unspecified)
                {
                    // If platform specific value equals unspecific value,
                    // there is no need to set the property platform specific.
                    if ((parentConfigPart.GetChildProperty(aspect.Name)?.Value as JValue)?.Value?.Equals(value) == true)
                    {
                        // remove the old platform specific value, if present
                        parentConfigPart.GetChildProperty(platform)?.GetChildProperty(aspect.Name)?.Remove();
                        return;
                    }

                    // Is a platform specific value really required? If all other platforms have the same value,
                    // the property can be set as platform unspecific.
                    var allPlatformsWithSameValue = Platforms.ToList()
                        .Where(p => p != Platform.Unspecified && p != platform)
                        .All(p =>
                        {
                            var currentValue = (parentConfigPart.GetChildProperty(p)?.GetChildProperty(aspect.Name)?.Value as JValue)?.Value;
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
                    foreach (var p in Platforms)
                    {
                        (parentConfigPart.GetChildProperty(p)?.Value as JProperty)?
                            .GetChildProperty(aspect.Name)?.Remove();
                    }
                }
                else
                {
                    if (!parentConfigPart.HasChildProperty(platform))
                    {
                        // add more specific platform property
                        parentConfigPart.Value[platform.ToConfigurationName()] = JToken.FromObject(new object());
                    }
                    // set parent config part to more specific platform
                    parentConfigPart = parentConfigPart.GetChildProperty(platform);
                }
            }

            // set property value
            if (parentConfigPart.HasChildProperty(aspect))
            {
                // property is already present. Overwriting
                parentConfigPart.GetChildProperty(aspect).Value = new JValue(value);
            }
            else
            {
                // adding new property
                parentConfigPart.Value[aspect.Name] = JToken.FromObject(value);
            }
        }

        private static string BuildJPath(JToken tenantConfiguration, App app, IAspect aspect, Platform platform)
        {
            Debug.Assert(tenantConfiguration != null);
            Debug.Assert(aspect != null);
            var jpath = new StringBuilder($"$.{tenantConfiguration.Path}");
            jpath.Append($".{app.ToConfigurationName()}");

            if (aspect is ISimpleAspect simple)
            {
                jpath.Append($".{simple.Parent.GetAspectPath()}");
                if (platform != Platform.Unspecified)
                {
                    jpath.Append($".{platform.ToConfigurationName()}");
                }
                jpath.Append($".{simple.Name}");
            }
            else
            {
                jpath.Append($".{aspect.GetAspectPath()}");
            }
            return jpath.ToString();
        }
    }
}
