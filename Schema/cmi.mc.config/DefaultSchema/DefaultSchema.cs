using System;
using System.Collections;
using System.Collections.Generic;
using cmi.mc.config.ModelContract;
using cmi.mc.config.ModelImpl;

namespace cmi.mc.config.DefaultSchema
{
    /// <summary>
    /// Represents the default mobile client configuration model.
    /// </summary>
    public class DefaultSchema : ISchema
    {
        private readonly IDictionary<App, IComplexAspect> _internal = new Dictionary<App, IComplexAspect>();

        /// <inheritdoc />
        public Uri DefaultServiceUrl { get; private set; } = new Uri("https://mobile.cmiaxioma.ch");

        /// <inheritdoc />
        /// Provide prameterless constructor.
        public DefaultSchema() : this(null){}

        /// <inheritdoc />
        /// <param name="defaultServiceUrl">The default base url for the mobile client service.
        /// Currently, everything behind the uri authority will be cut.</param>
        public DefaultSchema(Uri defaultServiceUrl = null)
        {
            if (defaultServiceUrl != null)
            {
                DefaultServiceUrl = new Uri(defaultServiceUrl.Scheme + "://" + defaultServiceUrl.Authority);
            }

            // apps with default model
            _internal.Add(App.Common, CommonSchema.GetModel(DefaultServiceUrl));
            _internal.Add(App.Zusammenarbeitdritte, ZdSchema.GetModel(_internal[App.Common] as AppSection));
            _internal.Add(App.Dossierbrowser, DbSchema.GetModel(_internal[App.Common] as AppSection));
            _internal.Add(App.Sitzungsvorbereitung, SvSchema.GetModel(_internal[App.Common] as AppSection));

            // add remaining apps without model
            foreach (var appValue in System.Enum.GetValues(typeof(App)))
            {
                if (!_internal.ContainsKey((App) appValue))
                {
                    _internal.Add((App)appValue, new AppSection((App)appValue));
                }    
            }
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public T GetAspect<T>(App app, string aspectPath) where T : IAspect
        {
            var r = GetAspect(app, aspectPath);
            if (!(r is T))
            {
                throw new InvalidOperationException($"{aspectPath} is not a {typeof(T).Name}.");
            }
            return (T)r;
        }
        
        /// <inheritdoc />
        public IAspect TryGetAspect(App app, string aspectPath)
        {
            try
            {
                return GetAspect(app, aspectPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }
        
        #region IReadOnlyDictionary impl.
        /// <inheritdoc />
        public IComplexAspect this[App key] => _internal[key];
        /// <inheritdoc />
        public IEnumerable<App> Keys => _internal.Keys;
        /// <inheritdoc />
        public IEnumerable<IComplexAspect> Values => _internal.Values;
        /// <inheritdoc />
        public int Count => _internal.Count;
        /// <inheritdoc />
        public bool ContainsKey(App key) => _internal.ContainsKey(key);
        /// <inheritdoc />
        public IEnumerator<KeyValuePair<App, IComplexAspect>> GetEnumerator() => _internal.GetEnumerator();
        /// <inheritdoc />
        public bool TryGetValue(App key, out IComplexAspect value) => _internal.TryGetValue(key, out value);
        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => _internal.GetEnumerator();
        #endregion
    }
}
