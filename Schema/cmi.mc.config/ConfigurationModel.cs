using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cmi.mc.config
{
    public class ConfigurationModel : IReadOnlyDictionary<App, AppSection>
    {
        private readonly IDictionary<App, AppSection> _internal = new Dictionary<App, AppSection>();

        public ConfigurationModel()
        {
            foreach (var appValue in System.Enum.GetValues(typeof(App)))
            {
                _internal.Add((App)appValue, new AppSection((App)appValue));
            }
        }

        public Aspect GetAspect(App app, string aspectPath)
        {
            Aspect.ThrowIfInvalidAspectPath(aspectPath);
            var parts = aspectPath.Split('.');
            Element currentElement = this[app];
            foreach (var part in parts)
            {
                switch (currentElement)
                {
                    case AppSection section when section.Aspects.ContainsKey(part):
                        currentElement = ((AppSection) currentElement).Aspects[part];
                        break;
                    case ComplexAspect aspect when aspect.Aspects.ContainsKey(part):
                        currentElement = ((ComplexAspect)currentElement).Aspects[part];
                        break;
                    default:
                        throw new KeyNotFoundException($"{app} does not have a aspect path of {aspectPath}");
                }
            }
            if (!(currentElement is Aspect))
            {
                throw new KeyNotFoundException($"{app} does not have a aspect path of {aspectPath}");
            }
            return (Aspect)currentElement;
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
