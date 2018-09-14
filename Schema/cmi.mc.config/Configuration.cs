using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cmi.mc.config.SchemaComponents;
using Newtonsoft.Json.Linq;

namespace cmi.mc.config
{
    public class Configuration : IEnumerable<ITenant>
    {
        private readonly JObject _configuration;
        private readonly ConfigurationModel _model;

        protected Configuration(JObject configuration, ConfigurationModel model)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (configuration.Type != JTokenType.Object)
            {
                throw new InvalidDataException($"The root token is not a json object.");
            }

            _configuration = configuration;
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        #region read and write json

        public static Configuration ReadFromFile(string path, ConfigurationModel model)
        {
            if(model == null) throw new ArgumentNullException(nameof(model));
            if (!System.IO.File.Exists(path)) throw new FileNotFoundException($"Could not find {path}", path);
            return new Configuration(JObject.Parse(System.IO.File.ReadAllText(path)), model);
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

        public override string ToString() => _configuration.ToString();

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
            if (!_configuration.ContainsKey(name))
            {
                _configuration.Add(name, JToken.FromObject(new object()));
            }
            return new Tenant(_configuration.Property(name),_model);       
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
                if (_configuration.ContainsKey(name))
                {
                    return new Tenant(_configuration.Property(name),_model);
                }
                throw new ArgumentOutOfRangeException(nameof(name));
            }
        }

        public IEnumerator<ITenant> GetEnumerator()
        {
            return _configuration.Properties().Select(e => new Tenant(e,_model)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
