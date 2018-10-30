using System;
using System.Collections.Generic;
using cmi.mc.config.ModelContract.Exceptions;

namespace cmi.mc.config.ModelContract.Components
{
    /// <summary>
    /// Represents the configuration of an <see cref="cmi.mc.config.ModelContract.App"/> in the mobile client configuration.
    /// </summary>
    public interface IAppConfiguration
    {
        /// <summary>
        /// The <see cref="cmi.mc.config.ModelContract.App"/> context in which the operations will be executed.
        /// </summary>
        App App { get; }
        /// <summary>
        /// Determines if a setting is set.
        /// </summary>
        /// <param name="aspectPath">The jpaht-like path to the setting.</param>
        /// <param name="platform">Determines for a specific platform.</param>
        /// <returns>True if the setting is set, otherwise false.</returns>
        /// <exception cref="KeyNotFoundException">When an aspect with the given path could not be found.</exception>
        bool Has(string aspectPath, Platform platform = Platform.Unspecified);
        /// <summary>
        /// Removes the specified setting for all platforms.
        /// </summary>
        /// <param name="aspectPath">The jpaht-like path to the setting.</param>
        /// <exception cref="KeyNotFoundException">When an aspect with the given path could not be found.</exception>
        void Remove(string aspectPath);
        /// <summary>
        /// Removes the specified setting for a specific platform.
        /// </summary>
        /// <param name="aspectPath">The jpaht-like path to the setting.</param>
        /// <param name="platform">The platform.</param>
        /// <exception cref="KeyNotFoundException">When an aspect with the given path could not be found.</exception>
        void Remove(string aspectPath, Platform platform);
        /// <summary>
        /// Gets the value of the specified setting.
        /// </summary>
        /// <param name="aspectPath">The jpaht-like path to the setting.</param>
        /// <param name="platform">The platform specific value.</param>
        /// <returns>The value of the setting or null when the setting is not set.</returns>
        /// <exception cref="KeyNotFoundException">When an aspect with the given path could not be found.</exception>
        /// <exception cref="InvalidCastException">When the value of the setting can not be casted to the aspect type.</exception>
        object Get(string aspectPath, Platform platform = Platform.Unspecified);
        /// <summary>
        /// Gets the value of the specified setting.
        /// </summary>
        /// <typeparam name="T">Type of the value</typeparam>
        /// <param name="aspectPath">The jpaht-like path to the setting.</param>
        /// <param name="platform">The platform specific value.</param>
        /// <returns>The value of the setting or the default value of the specified type when the setting is not set.</returns>
        /// <exception cref="KeyNotFoundException">When an aspect with the given path could not be found.</exception>
        /// <exception cref="InvalidCastException">When the value of the setting can not be casted to the aspect type.</exception>
        T Get<T>(string aspectPath, Platform platform = Platform.Unspecified);
        /// <summary>
        /// Sets a setting to the specified value.
        /// </summary>
        /// <param name="aspectPath">The jpaht-like path to the setting.</param>
        /// <param name="value">Value of the setting.</param>
        /// <param name="platform">Set the setting platform specific.</param>
        /// <param name="ensureDependencies">Apply foreign configuration changes to fulfill setting dependencies.</param>
        /// <exception cref="AspectDependencyNotFulfilledException">When dependencies are not fulfilled to support this setting.</exception>
        /// <exception cref="KeyNotFoundException">When an aspect with the given path could not be found.</exception>
        /// <exception cref="ValueValidationException">When the specified value is not valid for the setting.</exception>
        void Set(string aspectPath, object value, bool ensureDependencies = false,
            Platform platform = Platform.Unspecified);
        /// <summary>
        /// Sets a setting to its default value.
        /// If a non-leaf path is specified, all leaf-childs will be set to its default value.
        /// </summary>
        /// <param name="aspectPath">The jpaht-like path to the setting.</param>
        /// <param name="ensureDependencies">Apply foreign configuration changes to fulfill setting dependencies.</param>
        /// <exception cref="KeyNotFoundException">When an aspect with the given path could not be found.</exception>
        void Set(string aspectPath, bool ensureDependencies = false);
    }
}