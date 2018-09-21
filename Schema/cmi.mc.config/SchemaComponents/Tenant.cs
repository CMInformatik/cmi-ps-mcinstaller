using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace cmi.mc.config.SchemaComponents
{
    public class Tenant : ITenant
    {
        private JProperty _configuration;
        private readonly ConfigurationModel _model;
        private static readonly IEnumerable<string> CcaNames = Enum.GetValues(typeof(ConfigControlAttribute)).Cast<ConfigControlAttribute>().Select(e => e.ToConfigurationName());

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

            if (HasConfigurationProperty(App.Common, "api.server"))
            {
                var uri = GetConfigurationProperty<Uri>(App.Common, "api.server");
                ServiceBaseUrl = new Uri(uri.Scheme + "://" + uri.Authority);
            }
            else
            {
                ServiceBaseUrl = _model.DefaultServiceUrl;
            }            
            if(!IsEnabled(App.Common)) RevertChangesOnFailure(() => Enable(App.Common));
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

        public bool IsEnabled(App app) => _configuration.HasChildProperty(app);

        public void Enable(App app, bool ensureDependencies = false)
        {
            RevertChangesOnFailure(() =>
            {
                foreach (var dep in _model[app].Dependencies)
                {
                    if (ensureDependencies)
                    {
                        dep.Ensure(this, app, _model[app]);
                    }
                    else
                    {
                        dep.Verify(this,  app, _model[app]);
                    }
                }
            });
            if (!IsEnabled(app))
            {
                RevertChangesOnFailure(() =>
                {
                    _configuration.Value[app.ToConfigurationName()] = JToken.FromObject(new object());
                    foreach (var aspect in _model[app].Traverse().OfType<ISimpleAspect>().Where(a => a.IsRequired))
                    {
                        SetConfigurationProperty(app, aspect.GetAspectPath(), ensureDependencies);
                    }
                    // update app directory
                    try
                    {
                        var appDirAspect = _model.GetAspect<IComplexAspect>(App.Common, $"appDirectory.{app.ToConfigurationName()}");
                        foreach(var aspect in appDirAspect.Traverse().OfType<ISimpleAspect>()) {
                            SetConfigurationProperty(App.Common, aspect.GetAspectPath());
                        }
                    }
                    catch (KeyNotFoundException)
                    {
                        // app does not have appDirectory?
                    }
                });
            }
        }

        public void Disable(App app)
        {
            if (!IsEnabled(app)) return;
            RevertChangesOnFailure(() =>
            {
                _configuration.Value[app.ToConfigurationName()].Remove();
                // update app directory
                RemoveConfigurationProperty(App.Common, $"appDirectory.{app.ToConfigurationName()}");
            });
        }

        public bool HasConfigurationProperty(App app, string aspectPath)
        {
            var model = _model.GetAspect<ISimpleAspect>(app, aspectPath);
            var xpath = $"$.{Name}.{app.ToConfigurationName()}.{model.GetAspectPath()}";
            var token = _configuration.Root.SelectTokens(xpath).SingleOrDefault();
            return !(token is null);
        }

        public void RemoveConfigurationProperty(App app, string aspectPath)
        {
            var model = _model.GetAspect<IAspect>(app, aspectPath);
            if (!IsEnabled(app)) return;

            var xpath = $"$.{Name}.{app.ToConfigurationName()}.{model.GetAspectPath()}";
            var token = _configuration.Root.SelectTokens(xpath).SingleOrDefault();
            if (token != null && !(token is JValue))
            {
                throw new InvalidDataException($"A json value ({nameof(JValue)}) was expected at path {xpath}, but a {token.GetType().Name} was found.");
            }
            token?.Remove();
        }

        public object GetConfigurationProperty(App app, string aspectPath)
        {
            var model = _model.GetAspect<ISimpleAspect>(app, aspectPath);
            if (!IsEnabled(app)) return null;
            var xpath = $"$.{Name}.{app.ToConfigurationName()}.{model.GetAspectPath()}";
            var token = _configuration.Root.SelectTokens(xpath).SingleOrDefault();
            if (token != null && !(token is JValue))
            {
                throw new InvalidDataException($"A json value ({nameof(JValue)}) was expected at path {xpath}, but a {token.GetType().Name} was found.");
            }
            return (token as JValue)?.Value;
        }

        public T GetConfigurationProperty<T>(App app, string aspectPath)
        {
            var result = GetConfigurationProperty(app, aspectPath);

            // no implicit cast from string to uri, but json reader delivers string, when uri is expected
            if (typeof(T) == typeof(Uri) && result is string s)
            {
                result = new Uri(s);
            }

            return (result != null) ? (T)result : default(T);
        }

        /// <summary>
        /// Sets a configuration property to its default value.
        /// </summary>
        /// <param name="app">The app of the property.</param>
        /// <param name="aspectPath">Path of the property.</param>
        /// <param name="ensureDependencies">Set dependencies the required values.</param>
        public void SetConfigurationProperty(App app, string aspectPath, bool ensureDependencies = false)
        {
            var model = _model.GetAspect<ISimpleAspect>(app, aspectPath);
            RevertChangesOnFailure(() => SetConfigurationPropertyInternal(app, model, model.GetDefaultValue(this), ensureDependencies));
        }

        /// <summary>
        /// Sets a configuration property to the specified value.
        /// </summary>
        /// <param name="app">The app of the property.</param>
        /// <param name="aspectPath">Path of the property.</param>
        /// <param name="value">Value of the property</param>
        /// <param name="ensureDependencies">Set dependencies the required values.</param>
        public void SetConfigurationProperty(App app, string aspectPath, object value, bool ensureDependencies = false)
        {
            var model = _model.GetAspect<ISimpleAspect>(app, aspectPath);
            RevertChangesOnFailure(() => SetConfigurationPropertyInternal(app, model, value, ensureDependencies));
        }

        private void SetConfigurationPropertyInternal(App app, IAspect aspect, object value, bool ensureDependencies)
        {
            Debug.Assert(aspect != null);
            // is app enabled
            if (!IsEnabled(app))
            {
                throw new InvalidOperationException($"RequiredApp {app.ToString()} is not enabled for tenant {Name}");
            }
            // test value
            if (aspect is ISimpleAspect simpleAspect) simpleAspect.TestValue(value, this);
            // test dependencies
            if (aspect.Dependencies.Any())
            {
                foreach (var dep in aspect.Dependencies)
                {
                    if (ensureDependencies)
                    {
                        dep.Ensure(this, app, aspect);
                    }
                    else
                    {
                        dep.Verify(this, app, aspect);
                    }
                }
            }

            // create parent aspects
            var currentConfigPart = _configuration.GetChildProperty(app);
            Debug.Assert(currentConfigPart != null);
            if (aspect.Parent != null)
            {
                foreach (var parentAspect in aspect.GetParents())
                {
                    if (!currentConfigPart.HasChildProperty(parentAspect))
                    {
                        SetConfigurationPropertyInternal(app, parentAspect, new object(), ensureDependencies);
                    }

                    currentConfigPart = currentConfigPart.GetChildProperty(parentAspect);
                    Debug.Assert(currentConfigPart != null);
                }
            }

            // create aspect
            var property = currentConfigPart.GetChildProperty(aspect);
            if (property != null)
            {
                // property is already present. Overwriting
                property.Value = new JValue(value);
            }
            else
            {
                // adding new property
                currentConfigPart.Value[aspect.Name] = JToken.FromObject(value);
            }
            // set default cca
            if (aspect is IComplexAspect complexAspect)
            {
                SetDefaultCCa(complexAspect, currentConfigPart.GetChildProperty(complexAspect));
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
    }
}
