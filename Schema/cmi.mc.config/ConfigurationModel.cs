using System;
using System.Collections;
using System.Collections.Generic;
using cmi.mc.config.ModelComponents;
using cmi.mc.config.ModelContract;
using cmi.mc.config.ModelDefault;

namespace cmi.mc.config
{
    public class ConfigurationModel : IReadOnlyDictionary<App, IComplexAspect>
    {
        private readonly IDictionary<App, IComplexAspect> _internal = new Dictionary<App, IComplexAspect>();
        public Uri DefaultServiceUrl { get; private set; } = new Uri("https://mobile.cmiaxioma.ch");

        /// provide prameterless constructor
        public ConfigurationModel() : this(null){}

        public ConfigurationModel(Uri defaultServiceUrl = null)
        {
            if (defaultServiceUrl != null)
            {
                DefaultServiceUrl = new Uri(defaultServiceUrl.Scheme + "://" + defaultServiceUrl.Authority);
            }

            // apps with default model
            _internal.Add(App.Common, CommonModel.GetModel(DefaultServiceUrl));
            _internal.Add(App.Zusammenarbeitdritte, ZdModel.GetModel(_internal[App.Common] as AppSection));
            _internal.Add(App.Dossierbrowser, DbModel.GetModel(_internal[App.Common] as AppSection));
            _internal.Add(App.Sitzungsvorbereitung, SvModel.GetModel(_internal[App.Common] as AppSection));

            // add remaining apps without model
            foreach (var appValue in System.Enum.GetValues(typeof(App)))
            {
                if (!_internal.ContainsKey((App) appValue))
                {
                    _internal.Add((App)appValue, new AppSection((App)appValue));
                }    
            }
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
        public IComplexAspect this[App key] => _internal[key];

        public IEnumerable<App> Keys => _internal.Keys;

        public IEnumerable<IComplexAspect> Values => _internal.Values;

        public int Count => _internal.Count;

        public bool ContainsKey(App key) => _internal.ContainsKey(key);

        public IEnumerator<KeyValuePair<App, IComplexAspect>> GetEnumerator() => _internal.GetEnumerator();

        public bool TryGetValue(App key, out IComplexAspect value) => _internal.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => _internal.GetEnumerator();

        #endregion
    }
}
