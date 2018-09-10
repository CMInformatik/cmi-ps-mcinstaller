using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace cmi.mc.config
{
    public class Tenant
    {
        private JProperty _configuration;
        private readonly ConfigurationModel _model;
        private static readonly IDictionary<ConfigControlAttribute, string> _ccaLookup = new Dictionary<ConfigControlAttribute, string>()
        {
            {ConfigControlAttribute.Extend, "_extend"},
            {ConfigControlAttribute.Replace, "_replace"},
            {ConfigControlAttribute.Remove, "_remove"},
            {ConfigControlAttribute.Internal, "_internal"},
            {ConfigControlAttribute.Private, "_private"},
        };
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

        public bool IsEnabled(App app) => _configuration.HasChildProperty(app.ToString());

        public void Enable(App app, bool ensureDependencies = false)
        {
            // ToDO: Test dependencies
            if (!IsEnabled(app))
            {
                _configuration.Add(new JProperty(app.ToString(), null));
            }
        }

        public void SetConfigurationProperty(App app, string aspectPath, object value, bool ensureDependencies = false)
        {
            var model = _model.GetAspect(app, aspectPath);
            if (!(model is SimpleAspect))
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
                _configuration = beforeChanges;
                throw;
            }
        }

        private void SetConfigurationPropertyInternal(App app, Aspect aspect, object value, bool ensureDependencies)
        {
            Debug.Assert(aspect != null);
            // test value
            if (aspect is SimpleAspect simpleAspect) simpleAspect.TestValue(value);
            // enable app
            if (!IsEnabled(app)) Enable(app, ensureDependencies);
            // test dependencies
            if (aspect.Dependencies.Any())
            {
                foreach (var dep in aspect.Dependencies)
                {
                    try
                    {
                        dep.Verify(_configuration);
                    }
                    catch(Exception)
                    {
                        if (ensureDependencies) dep.Ensure(_configuration);
                        else throw;
                    }
                }
            }

            // create parent aspects
            var currentConfigPart = _configuration.GetChildProperty(app.ToString());
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
            SetDefaultCCa(aspect, currentConfigPart.GetChildProperty(aspect));
        }

        private static void SetDefaultCCa(Element element, JContainer configPart)
        {
            Debug.Assert(element != null);
            Debug.Assert(configPart != null);
            if (element.DefaultCca == ConfigControlAttribute.NotSet) return; // no default cca defined
            if (configPart.Children<JProperty>().Any(p => _ccaLookup.Values.Contains(p.Name))) return; // cca is already set
            configPart.Add(new JProperty(_ccaLookup[element.DefaultCca], true));
        }
    }
}
