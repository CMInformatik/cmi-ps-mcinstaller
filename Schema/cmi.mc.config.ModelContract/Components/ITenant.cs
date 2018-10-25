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
        /// Determines if a setting is set.
        /// </summary>
        /// <param name="app">The app where the setting belongs to.</param>
        /// <param name="aspectPath">The jpaht-like path to the setting.</param>
        /// <param name="platform">Determines for a specific platform.</param>
        /// <returns>True if the setting is set, otherwise false.</returns>
        /// <exception cref="KeyNotFoundException">When an aspect with the given path could not be found.</exception>
        bool Has(App app, string aspectPath, Platform platform = Platform.Unspecified);
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
        /// Removes the specified setting for all platforms.
        /// </summary>
        /// <param name="app">The app where the setting belongs to.</param>
        /// <param name="aspectPath">The jpaht-like path to the setting.</param>
        /// <exception cref="KeyNotFoundException">When an aspect with the given path could not be found.</exception>
        void Remove(App app, string aspectPath);
        /// <summary>
        /// Removes the specified setting for a specific platform.
        /// </summary>
        /// <param name="app">The app where the setting belongs to.</param>
        /// <param name="aspectPath">The jpaht-like path to the setting.</param>
        /// <param name="platform">The platform.</param>
        /// <exception cref="KeyNotFoundException">When an aspect with the given path could not be found.</exception>
        void Remove(App app, string aspectPath, Platform platform);
        /// <summary>
        /// Gets the value of the specified setting.
        /// </summary>
        /// <param name="app">The app where the setting belongs to.</param>
        /// <param name="aspectPath">The jpaht-like path to the setting.</param>
        /// <param name="platform">The platform specific value.</param>
        /// <returns>The value of the setting or null when the setting is not set.</returns>
        /// <exception cref="KeyNotFoundException">When an aspect with the given path could not be found.</exception>
        object Get(App app, string aspectPath, Platform platform = Platform.Unspecified);
        /// <summary>
        /// Gets the value of the specified setting.
        /// </summary>
        /// <typeparam name="T">Type of the value</typeparam>
        /// <param name="app">The app where the setting belongs to.</param>
        /// <param name="aspectPath">The jpaht-like path to the setting.</param>
        /// <param name="platform">The platform specific value.</param>
        /// <returns>The value of the setting or the default value of the specified type when the setting is not set.</returns>
        /// <exception cref="KeyNotFoundException">When an aspect with the given path could not be found.</exception>
        T Get<T>(App app, string aspectPath, Platform platform = Platform.Unspecified);
        /// <summary>
        /// Sets a setting to the specified value.
        /// </summary>
        /// <param name="app">The app where the setting belongs to.</param>
        /// <param name="aspectPath">The jpaht-like path to the setting.</param>
        /// <param name="value">Value of the setting.</param>
        /// <param name="platform">Set the setting platform specific.</param>
        /// <param name="ensureDependencies">Apply foreign configuration changes to fulfill setting dependencies.</param>
        /// <exception cref="AspectDependencyNotFulfilledException">When dependencies are not fulfilled to support this setting.</exception>
        /// <exception cref="KeyNotFoundException">When an aspect with the given path could not be found.</exception>
        /// <exception cref="ValueValidationException">When the specified value is not valid for the setting.</exception>
        void Set(App app, string aspectPath, object value, bool ensureDependencies = false,
            Platform platform = Platform.Unspecified);
        /// <summary>
        /// Sets a setting to its default value.
        /// If a non-leaf path is specified, all leaf-childs will be set to its default value.
        /// </summary>
        /// <param name="app">The app where the setting belongs to.</param>
        /// <param name="aspectPath">The jpaht-like path to the setting.</param>
        /// <param name="platform">Set the setting platform specific.</param>
        /// <param name="ensureDependencies">Apply foreign configuration changes to fulfill setting dependencies.</param>
        /// <exception cref="KeyNotFoundException">When an aspect with the given path could not be found.</exception>
        void Set(App app, string aspectPath, bool ensureDependencies = false, Platform platform = Platform.Unspecified);
        /// <summary>
        /// Validates the configuration against all known constrains.
        /// Throws exception when constrains are not fulfilled.
        /// </summary>
        /// <param name="axVersion">Minimal function level that must be met.</param>
        /// <exception cref="ConfigurationValidationException">When a constrain is not fulfilled.</exception>
        /// <exception cref="AggregateException">When one or more constrains are not fulfilled.</exception>
        void Validate(AxSupport axVersion);
    }
}