using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cmi.mc.config.SchemaComponents;

namespace cmi.mc.config
{
    public class ConfigurationModel : IReadOnlyDictionary<App, AppSection>
    {
        private readonly IDictionary<App, AppSection> _internal = new Dictionary<App, AppSection>();
        public readonly Uri DefaultServiceUrl = new Uri("https://mobile.cmiaxioma.ch/mobileclients/");

        public ConfigurationModel()
        {
            foreach (var appValue in System.Enum.GetValues(typeof(App)))
            {
                _internal.Add((App)appValue, new AppSection((App)appValue));
            }
            // minimal model
            var api = new ComplexAspect("api");
            api.AddAspect(new SimpleAspect("server", typeof(Uri), new Uri(DefaultServiceUrl, "mobileclients")) { IsRequired = true });
            api.AddAspect(new SimpleAspect("public", typeof(string), null) { IsRequired = true });
            api.AddAspect(new SimpleAspect("private", typeof(string), null) { IsRequired = true });
            _internal[App.Common].AddAspect(api);
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
                        currentAspect = ((IComplexAspect) currentAspect).Aspects[part];
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
            if (!(currentAspect is IAspect))
            {
                throw new KeyNotFoundException($"{app} does not have a aspect path of {aspectPath}");
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
