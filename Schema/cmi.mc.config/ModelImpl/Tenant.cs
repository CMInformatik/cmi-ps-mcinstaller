using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using cmi.mc.config.Extensions;
using cmi.mc.config.ModelContract;
using cmi.mc.config.ModelContract.Components;
using cmi.mc.config.ModelContract.Exceptions;
using Newtonsoft.Json.Linq;
using Exception = System.Exception;
using CValException = cmi.mc.config.ModelContract.Exceptions.ConfigurationValidationException;

namespace cmi.mc.config.ModelImpl
{
    internal class Tenant : ConfigurationManipulator, ITenant
    {
        public string Name => Configuration.Name;
        public Uri ServiceBaseUrl { get; private set; }

        internal Tenant(JProperty configuration, ISchema schema) : base(configuration, schema)
        {
            if (string.IsNullOrWhiteSpace(configuration.Name)) throw new ArgumentNullException(nameof(configuration.Name));
            if (Has(App.Common) && this[App.Common].Has("api.server"))
            {
                var uri = this[App.Common].Get<Uri>("api.server");
                ServiceBaseUrl = new Uri(uri.Scheme + "://" + uri.Authority);
            }
            else
            {
                ServiceBaseUrl = Schema.DefaultServiceUrl;
            }
            Debug.Assert(ServiceBaseUrl != null);
            if (!Has(App.Common)) RevertChangesOnFailure(() => Add(App.Common));
        }

        public bool Has(App app) => Configuration.HasChildProperty(app);
        public IAppConfiguration Get(App app) => (Has(app)) ? this[app] : null;

        public void Add(App app, bool ensureDependencies = false)
        {
            RevertChangesOnFailure(() =>
            {
                if (!Has(app))
                {
                    Configuration.Value[app.ToConfigurationName()] = JToken.FromObject(new object());
                    foreach (var aspect in Schema[app].Traverse().OfType<ISimpleAspect>().Where(a => a.IsRequired))
                    {
                        this[app].Set(aspect.GetAspectPath(), ensureDependencies);
                    }

                    // update app directory
                    try
                    {
                        var appDirAspect = Schema.GetAspect<IAspect>(App.Common,
                            $"appDirectory.{app.ToConfigurationName()}");
                        this[App.Common].Set(appDirAspect.GetAspectPath());
                    }
                    catch (KeyNotFoundException e)
                    {
                        // app does not have an appDirectory?
                        Console.WriteLine(e);
                    }
                }
                // test dependencies
                Schema[app].TestDependencies(this, app, ensureDependencies);
            });
        }

        public void Remove(App app)
        {
            if (!Has(app)) return;
            if (app is App.Common) throw new InvalidOperationException($"Can not remove required app {app}");
            RevertChangesOnFailure(() =>
            {
                var xpath = JsonConfiguration.BuildJPath(Name, app, null, Platform.Unspecified);
                var token = Configuration.Root.SelectTokens(xpath).Single();
                token.Parent.Remove();
                // update app directory
                this[App.Common].Remove($"appDirectory.{app.ToConfigurationName()}");
            });
        }

        #region validate
        /// <inheritdoc />
        public void Validate(AxSupport axVersion)
        {
            // common app is required
            if (!Has(App.Common))
            {
                throw new CValException($"The property {App.Common.ToConfigurationName()} is required, but was not found", null, null);
            }
            IList<Exception> problems = new List<Exception>();
            foreach (var app in McSymbols.Apps.Where(Has))
            {
                // validate JTokens
                foreach (var child in Configuration.GetChildProperty(app).Value.Children())
                {
                    ValidateInternal(child, app, Platform.Unspecified, string.Empty, axVersion, ref problems);
                }
                // required aspects must be present
                foreach (var aspect in Schema[app].Traverse().OfType<ISimpleAspect>().Where(a => a.IsRequired))
                {
                    if (!this[app].Has(aspect.GetAspectPath()))
                    {
                        problems.Add(new CValException($"{aspect.GetAspectPath()} is required, but was not found in the configuration.", aspect, null));
                    }
                }
            }
            if (problems.Any()) throw new AggregateException(problems);
        }

        public IAppConfiguration this[App app] => Has(app)
            ? new AppConfiguration(this, Configuration.GetChildProperty(app), Schema)
            : throw new KeyNotFoundException($"The app {app} is not enabled for tenant {Name}.");

        /// <summary>
        /// Validate complex aspect.
        /// </summary>
        private void ValidateInternal(JProperty jProperty, IComplexAspect aspect, Platform platform,
            ref IList<Exception> problems)
        {
            // complex aspects can not be platform specific
            if (platform != Platform.Unspecified)
            {
                throw new CValException($"{aspect.GetAspectPath()} can not be platform specific", aspect, jProperty.Path);
            }
            // the value must be a jobject
            if (jProperty.Value == null || jProperty.Value.Type != JTokenType.Object)
            {
                throw new CValException($"{aspect.GetAspectPath()} does not contain json object", aspect, jProperty.Path);
            }
            // cca with default value?
            if (jProperty.GetCCa() != aspect.DefaultCca)
            {
                problems.Add(new CValException(
                    $"{aspect.GetAspectPath()} is expected to have the {nameof(ConfigControlAttribute)} to be set to {aspect.DefaultCca.ToConfigurationName()}",
                    aspect, jProperty.Path
                ));
            }
        }

        /// <summary>
        /// Validate simple aspect.
        /// </summary>
        private void ValidateInternal(ISimpleAspect aspect, App app, Platform platform, AxSupport axVersion)
        {
            var value = this[app].Get(aspect.GetAspectPath(), platform);
            // is supported?
            if (aspect.AxSupport > axVersion)
            {
                throw new CValException($"{aspect.GetAspectPath()} is not supported in version {axVersion}", aspect, null);
            }
            // validate value
            if (value != null) aspect.TestValue(value, this, platform);
            // dependencies
            aspect.TestDependencies(this, app);
        }

        /// <summary>
        /// Validate <seealso cref="ConfigControlAttribute"/>.
        /// </summary>
        private void ValidateInternal(ConfigControlAttribute cca, ref IList<Exception> problems)
        {
            switch (cca)
            {
                case ConfigControlAttribute.Replace:
                case ConfigControlAttribute.Remove:
                    problems.Add(new CValException($"{nameof(ConfigControlAttribute)} with value {cca.ToConfigurationName()} is not supported by this tool.", null, null));
                    break;
                default:
                    return;
            }
        }

        /// <summary>
        /// Validate <seealso cref="JToken"/>.
        /// </summary>
        private void ValidateInternal(JToken config, App app, Platform platform, string path, AxSupport axVersion, ref IList<Exception> problems)
        {
            try
            {
                if (!(config is JProperty jProperty))
                {
                    throw new CValException($"{config.Path} is not a json property", null, config.Path);
                }

                // handle cca properties are handled with the complex properties
                if (McSymbols.CcaNames.Contains(jProperty.Name))
                {
                    ValidateInternal(McSymbols.GetCca(jProperty.Name), ref problems);
                    return;
                }

                // handle platform
                if (McSymbols.PlatformNames.Contains(jProperty.Name))
                {
                    if (!(jProperty.Value is JObject))
                    {
                        throw new CValException($"{config.Path} is expected to contain a json object ({nameof(JObject)}).", null, config.Path);
                    }
                    foreach (var child in jProperty.Value.Children())
                    {
                        ValidateInternal(child, app, McSymbols.GetPlatform(jProperty.Name), path, axVersion, ref problems);
                    }
                    return;
                }

                // handle aspect
                var aspectPath = string.IsNullOrWhiteSpace(path) ? jProperty.Name : $"{path}.{jProperty.Name}";
                IAspect model;
                try
                {
                    model = Schema.GetAspect(app, aspectPath);
                }
                catch (KeyNotFoundException e)
                {
                    // often the configuration was made in the wrong app section
                    // maybe the aspect path can be found in an other app
                    foreach (var otherApp in McSymbols.Apps)
                    {
                        if (otherApp == app) continue;
                        if (Schema.TryGetAspect(otherApp, aspectPath) != null)
                        {
                            throw new CValException($"{aspectPath} is not expected to be configured in app {app}. Move to {otherApp}.", null, config.Path, e);
                        }
                    }
                    throw;
                }

                Debug.Assert(aspectPath != null);
                Debug.Assert(model != null, $"{nameof(Schema.GetAspect)} should throw if aspect can not be found");

                switch (model)
                {
                    case IComplexAspect complexModel:
                        ValidateInternal(jProperty, complexModel, platform, ref problems);
                        foreach (var child in jProperty.Value.Children())
                        {
                            // handle childs
                            ValidateInternal(child, app, platform, aspectPath, axVersion, ref problems);
                        }

                        break;
                    case ISimpleAspect simpleModel:
                        ValidateInternal(simpleModel, app, platform, axVersion);
                        break;
                    default:
                        throw new NotImplementedException($"Validation for aspects of type {model?.GetType().FullName} is not implemented");
                }
            }
            catch (Exception e)
            {
                problems.Add(e);
            }
        }

        #endregion
    }
}
