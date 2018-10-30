using System;
using System.Collections.Generic;
using cmi.mc.config.ModelContract.Exceptions;

namespace cmi.mc.config.ModelContract.Components
{
    /// <summary>
    /// Represents a tenant in a mobile client configuration.
    /// </summary>
    public interface ITenant
    {
        /// <summary>
        /// Name of the tenant.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Tenant unspecific base service uri.
        /// Tenant specific uris can be built based on this uri.
        /// </summary>
        Uri ServiceBaseUrl { get; }
        /// <summary>
        /// Determines if the specified <see cref="App"/> is enabled.
        /// </summary>
        /// <param name="app">The app.</param>
        /// <returns>True if the app is enabled, otherwise false.</returns>
        bool Has(App app);
        /// <summary>
        /// The configuration of the specified <see cref="App"/>.
        /// </summary>
        /// <param name="app">The app.</param>
        /// <returns>The app configuration.</returns>
        IAppConfiguration Get(App app);
        /// <summary>
        /// Enables the given app.
        /// </summary>
        /// <param name="app">The app to enable.</param>
        /// <param name="ensureDependencies">Apply foreign configuration changes to fulfill app dependencies.</param>
        /// <exception cref="AspectDependencyNotFulfilledException">When dependencies are not fulfilled to enable the app.</exception>
        void Add(App app, bool ensureDependencies = false);
        /// <summary>
        /// Removes the given app.
        /// </summary>
        /// <param name="app">The app to remove.</param>
        /// <exception cref="InvalidOperationException">When the app can not be removed.</exception>
        void Remove(App app);
        /// <summary>
        /// Validates the configuration against all known constrains.
        /// Throws exception when constrains are not fulfilled.
        /// </summary>
        /// <param name="axVersion">Minimal function level that must be met.</param>
        /// <exception cref="ConfigurationValidationException">When a constrain is not fulfilled.</exception>
        /// <exception cref="AggregateException">When one or more constrains are not fulfilled.</exception>
        void Validate(AxSupport axVersion);
        /// <summary>
        /// The configuration of the specified <see cref="App"/>.
        /// </summary>
        /// <param name="app">The app.</param>
        /// <returns>The app configuration.</returns>
        /// <exception cref="KeyNotFoundException">When the app is not enabled for the tenant.</exception>
        IAppConfiguration this[App app] { get; }
    }
}