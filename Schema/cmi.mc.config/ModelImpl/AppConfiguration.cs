using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using cmi.mc.config.Extensions;
using cmi.mc.config.ModelContract;
using cmi.mc.config.ModelContract.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace cmi.mc.config.ModelImpl
{
    public class AppConfiguration : ConfigurationManipulator, IAppConfiguration
    {
        public App App { get; }
        private readonly ITenant _tenant;

        internal AppConfiguration(ITenant tenant, JProperty configuration, ISchema schema) : base(configuration, schema)
        {
            _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
            App = McSymbols.GetApp(configuration.Name);
        }

        /// <inheritdoc />
        public object Get(string aspectPath, Platform platform = Platform.Unspecified)
        {
            var model = Schema.GetAspect<ISimpleAspect>(App, aspectPath);
            var xpath = JsonConfiguration.BuildJPath(_tenant.Name, App, model, platform);
            try
            {
                return Configuration.Root.SelectTokens(xpath).SingleOrDefault()?.ToObject(model.Type);
            }
            catch (JsonException e)
            {
                // ToDo: Create unit test
                // Interface defines InvalidCastException to be thrown when the value can not be converted. 
                throw new InvalidCastException($"Can not convert JSON value of {xpath} to {model.Type.FullName}. See the inner exception for more details.", e);
            }
        }

        /// <inheritdoc />
        public T Get<T>(string aspectPath, Platform platform = Platform.Unspecified)
        {
            var result = Get(aspectPath, platform);
            return (result != null) ? (T)result : default(T);
        }

        /// <inheritdoc />
        public bool Has(string aspectPath, Platform platform = Platform.Unspecified)
        {
            var model = Schema.GetAspect<ISimpleAspect>(App, aspectPath);

            // test for platform specific property
            var jPathPlatform = JsonConfiguration.BuildJPath(_tenant.Name, App, model, platform);
            var token = Configuration.Root.SelectTokens(jPathPlatform).SingleOrDefault();

            if (token != null || platform == Platform.Unspecified) return !(token is null);

            // test for platform unspecific property
            var jPathNoplatform = JsonConfiguration.BuildJPath(_tenant.Name, App, model, Platform.Unspecified);
            token = Configuration.Root.SelectTokens(jPathNoplatform).SingleOrDefault();
            return !(token is null);
        }

        /// <inheritdoc />
        public void Remove(string aspectPath)
        {
            RevertChangesOnFailure(() =>
            {
                foreach (var pl in McSymbols.Platforms)
                {
                    Remove(aspectPath, pl);
                }
            });
        }

        /// <inheritdoc />
        public void Remove(string aspectPath, Platform platform)
        {
            var model = Schema.GetAspect<IAspect>(App, aspectPath);
            var xpath = JsonConfiguration.BuildJPath(_tenant.Name, App, model, platform);
            var token = Configuration.Root.SelectTokens(xpath).SingleOrDefault();
            RevertChangesOnFailure(() => token?.Parent?.Remove());
        }

        public void Set(string aspectPath, object value, bool ensureDependencies = false, Platform platform = Platform.Unspecified)
        {
            var model = Schema.GetAspect<ISimpleAspect>(App, aspectPath);
            RevertChangesOnFailure(() => SetPropertyInternal(model, value, ensureDependencies, platform));
        }

        /// <inheritdoc />
        public void Set(string aspectPath, bool ensureDependencies = false)
        {
            var model = Schema.GetAspect<IAspect>(App, aspectPath);

            void TraverseAndSetDefault()
            {
                foreach (var aspect in model.Traverse().OfType<ISimpleAspect>())
                {
                    // if aspect is platform specific, set default value for all platforms.
                    var platformsToSet = aspect.IsPlatformSpecific ? McSymbols.Platforms : new[] { Platform.Unspecified };

                    foreach (var p in platformsToSet)
                    {
                        SetPropertyInternal(aspect, aspect.GetDefaultValue(_tenant, p), ensureDependencies, p);
                    }
                }
            }
            RevertChangesOnFailure(TraverseAndSetDefault);
        }

        private void SetPropertyInternal(IAspect aspect, object value, bool ensureDependencies, Platform platform)
        {
            Debug.Assert(aspect != null);
            Console.WriteLine($"Set property: app {App}, aspect {aspect?.GetAspectPath()}, value '{value}', platform {platform}");

            // test value
            if (aspect is ISimpleAspect simpleAspect) simpleAspect.TestValue(value, _tenant, platform);

            JProperty parentProperty;
            // get parent property
            if (aspect.Parent != null)
            {
                var xpath = JsonConfiguration.BuildJPath(_tenant.Name, App, aspect.Parent, Platform.Unspecified);
                // is parent present?
                if (Configuration.Root.SelectTokens(xpath).SingleOrDefault() == null)
                {
                    // create parent
                    SetPropertyInternal(aspect.Parent, new object(), ensureDependencies, platform);
                }
                // parent property should now exists
                parentProperty = (JProperty)Configuration.Root.SelectTokens(xpath).Single().Parent;
            }
            else
            {
                parentProperty = Configuration;
            }
            Debug.Assert(parentProperty != null);

            // create property
            SetPropertyValue(parentProperty, aspect, value, platform);

            // set default cca
            if (aspect is IComplexAspect ca) parentProperty.GetChildProperty(ca).SetDefaultCCa(ca);

            // test dependencies
            aspect.TestDependencies(_tenant, App, ensureDependencies);
        }

        private static void SetPropertyValue(JProperty parentProperty, IAspect aspect, object value,
    Platform platform)
        {
            Debug.Assert(parentProperty != null);
            Debug.Assert(aspect != null);
            if (aspect is IComplexAspect && platform != Platform.Unspecified)
            {
                // complex aspects can not be set platform specific, because it's not a leaf configuration property
                platform = Platform.Unspecified;
            }

            Console.WriteLine($"Set property value: parent {parentProperty?.Path}, aspect {aspect?.GetAspectPath()}, value '{value}', platform {platform}");

            if (platform == Platform.Unspecified)
            {
                // remove more specific platform properties
                foreach (var p in McSymbols.CertainPlatforms)
                {
                    (parentProperty.GetChildProperty(p)?.Value as JProperty)?
                        .GetChildProperty(aspect.Name)?.Remove();
                }
                // set value platform unspecific 
                parentProperty.Value[aspect.Name] = JToken.FromObject(value);
            }
            else
            {
                if (!parentProperty.HasChildProperty(platform))
                {
                    // add more specific platform property
                    parentProperty.Value[platform.ToConfigurationName()] = JToken.FromObject(new object());
                }
                // set value platform specific
                parentProperty.GetChildProperty(platform).Value[aspect.Name] = JToken.FromObject(value);
            }
            OptimizePlatformSpecificValues(parentProperty, aspect);
        }

        /// <summary>
        /// Reduce redundant platform specific configurations for the given <see cref="IAspect"/>.
        /// </summary>
        private static void OptimizePlatformSpecificValues(JProperty parentProperty, IAspect aspect)
        {
            Debug.Assert(parentProperty != null);
            Debug.Assert(aspect != null);

            var unspecificValue = (parentProperty.GetChildProperty(aspect.Name)?.Value as JValue)?.Value;
            var specificValues = McSymbols.CertainPlatforms.ToDictionary(p => p, p =>
                (parentProperty.GetChildProperty(p)?.GetChildProperty(aspect.Name)?.Value as JValue)?.Value);

            // If platform specific value equals unspecific value,
            // there is no need to set the property platform specific.
            // Then the platform specific value can be removed.
            if (unspecificValue != null)
            {
                foreach (var pl in specificValues.Where(x => unspecificValue.Equals(x.Value)).Select(x => x.Key))
                {
                    parentProperty.GetChildProperty(pl)?.GetChildProperty(aspect.Name)?.Remove();
                }
            }

            // If all platform specific values have the same value,
            // then they can be replaced with the platform unspecific value.
            if (unspecificValue == null && specificValues.Values.AllEqual(out var conjunctValue) && conjunctValue != null)
            {
                foreach (var pl in specificValues.Keys)
                {
                    parentProperty.GetChildProperty(pl)?.GetChildProperty(aspect.Name)?.Remove();
                }
                parentProperty.Value[aspect.Name] = JToken.FromObject(conjunctValue);
            }

            // If platform specific properties does not contain any child property, remove them.
            foreach (var platformJObject in McSymbols.CertainPlatforms
                .Select(p => parentProperty.GetChildProperty(p)?.Value as JObject)
                .Where(x=> x != null && x.Count == 0))
            {
                platformJObject.Parent.Remove();
            }

        }
    }
}
