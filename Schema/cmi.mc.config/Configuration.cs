using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using cmi.mc.config.Extensions;
using cmi.mc.config.ModelContract;
using cmi.mc.config.ModelImpl;
using Newtonsoft.Json.Linq;

namespace cmi.mc.config
{
    public class Configuration : IEnumerable<ITenant>
    {
        private readonly JProperty _configuration;
        private readonly ConfigurationModel _model;

        protected Configuration(JObject configuration, ConfigurationModel model)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (configuration.Type != JTokenType.Object)
            {
                throw new InvalidDataException($"The root token is not a json object.");
            }

            if(!configuration.ContainsKey("tenants")){
                throw new InvalidDataException($"The root json object does not have a property of name 'tenants'.");
            }

            _configuration = configuration.Property("tenants");
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        /// <summary>
        /// Builds the jpath for the specified tenant and aspect.
        /// </summary>
        /// <param name="platform">Build the jpath platform specific.</param>
        /// <returns></returns>
        public static string BuildJPath(string tenantName, App app, IAspect aspect, Platform platform)
        {
            if(string.IsNullOrWhiteSpace(tenantName)) throw new ArgumentNullException(nameof(tenantName));
            if(aspect == null) throw new ArgumentNullException(nameof(aspect));

            var jpath = new StringBuilder($"$.tenants.{tenantName}.{app.ToConfigurationName()}");

            if (aspect is ISimpleAspect simple)
            {
                jpath.Append($".{simple.Parent.GetAspectPath()}");
                if (platform != Platform.Unspecified) jpath.Append($".{platform.ToConfigurationName()}");
                jpath.Append($".{simple.Name}");
            }
            else
            {
                jpath.Append($".{aspect.GetAspectPath()}");
            }
            return jpath.ToString();
        }

        #region read and write json

        public static Configuration ReadFromFile(string path, ConfigurationModel model)
        {
            if(model == null) throw new ArgumentNullException(nameof(model));
            if (!System.IO.File.Exists(path)) throw new FileNotFoundException($"Could not find {path}", path);
            return new Configuration(JObject.Parse(System.IO.File.ReadAllText(path)), model);
        }

        public static Configuration New(ConfigurationModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            return new Configuration(JObject.Parse("{}"), model);
        }

        public static Configuration ReadFromString(string jsonString, ConfigurationModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (string.IsNullOrWhiteSpace(jsonString)) throw new ArgumentNullException(nameof(jsonString));
            return new Configuration(JObject.Parse(jsonString), model);
        }

        public void WriteToFile(string path, bool allowOverride = false)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
            if (!System.IO.File.Exists(path))
            {
                throw new InvalidOperationException($"Configuration {path} is already present and {nameof(allowOverride)} is set to {allowOverride.ToString()}.");
            }
            System.IO.File.WriteAllText(path,ToString());
        }

        public override string ToString() => _configuration.Root.ToString();

#endregion

        /// <summary>
        /// Adds an empty new tenant to the configuration.
        /// If a tenant with the same name is already present,
        /// the configuration will not be changed and the current tenant will be returned. 
        /// </summary>
        /// <param name="name">Name of the tenant</param>
        /// <returns>The new or the already present tenant</returns>
        public ITenant AddTenant(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (!_configuration.HasChildProperty(name))
            {
                _configuration.Value[name] = JToken.FromObject(new object());
            }
            return new Tenant(_configuration.GetChildProperty(name),_model);       
        }

        /// <summary>
        /// Indexer for tenants.
        /// </summary>
        /// <param name="name">Name of the tenant</param>
        /// <returns>The tenant</returns>
        public ITenant this[string name]
        {
            get
            {
                if (_configuration.HasChildProperty(name))
                {
                    return new Tenant(_configuration.GetChildProperty(name),_model);
                }
                throw new ArgumentOutOfRangeException(nameof(name), $"No tenant with name '{name}' was found.");
            }
        }

        public IEnumerator<ITenant> GetEnumerator()
        {
            return _configuration.Value.Children<JProperty>().Select(e => new Tenant(e,_model)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
