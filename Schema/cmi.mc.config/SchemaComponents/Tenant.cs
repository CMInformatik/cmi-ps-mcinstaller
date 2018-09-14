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
        }

        public bool IsEnabled(App app) => _configuration.HasChildProperty(app);

        public void Enable(App app, bool ensureDependencies = false)
        {
            // ToDO: Test dependencies
            if (!IsEnabled(app))
            {
                _configuration.Add(new JProperty(app.ToConfigurationName(), null));
            }
        }

        public object GetConfigurationProperty(App app, string aspectPath)
        {
            var model = _model.GetAspect(app, aspectPath);
            if (!(model is ISimpleAspect))
            {
                throw new InvalidOperationException($"{aspectPath} is not a simple aspect and can not be retrieved with this method.");
            }
            if (!IsEnabled(app)) return null;
            var xpath = $"$.{Name}.{app.ToConfigurationName()}.{aspectPath}";
            var token = _configuration.Root.SelectTokens(xpath).SingleOrDefault();
            if (token != null && !(token is JValue))
            {
                throw new InvalidDataException($"A json value ({nameof(JValue)}) was expected at path {xpath}, but a {token.GetType().Name} was found.");
            }
            return ((JValue)token)?.Value;
        }

        public T GetConfigurationProperty<T>(App app, string aspectPath)
        {
            var result = GetConfigurationProperty(app, aspectPath);
            return (result != null) ? (T)result : default(T);
        }

        public void SetConfigurationProperty(App app, string aspectPath, object value, bool ensureDependencies = false)
        {
            var model = _model.GetAspect(app, aspectPath);
            if (!(model is ISimpleAspect))
            {
                throw new InvalidOperationException($"{aspectPath} is not a simple aspect and can not be set with this method.");
            }

            var beforeChanges = (JProperty)_configuration.DeepClone();
            try
            {
                SetConfigurationPropertyInternal(app, model, value, ensureDependencies);
            }
            catch (Exception)
            {
                _configuration.Replace(beforeChanges);
                _configuration = beforeChanges;
                throw;
            }
        }

        private void SetConfigurationPropertyInternal(App app, IAspect aspect, object value, bool ensureDependencies)
        {
            Debug.Assert(aspect != null);
            // is app enabled
            if (!IsEnabled(app))
            {
                throw new InvalidOperationException($"App {app.ToString()} is not enabled for tenant {Name}");
            }
            // test value
            if (aspect is ISimpleAspect simpleAspect) simpleAspect.TestValue(value);
            // test dependencies
            if (aspect.Dependencies.Any())
            {
                foreach (var dep in aspect.Dependencies)
                {
                    if (ensureDependencies)
                    {
                        dep.Ensure(this);
                    }
                    else{
                        dep.Verify(this);
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
