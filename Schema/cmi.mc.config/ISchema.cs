using System;
using System.Collections.Generic;
using cmi.mc.config.ModelContract;
using cmi.mc.config.ModelContract.Components;

namespace cmi.mc.config
{
    /// <summary>
    /// Represents a mobile client configuration schema.
    /// </summary>
    public interface ISchema : IReadOnlyDictionary<App, IComplexAspect>
    {
        /// <summary>
        /// The default base url for the mobile client service.
        /// </summary>
        Uri DefaultServiceUrl { get; }

        /// <summary>
        /// Returns the <see cref="IAspect"/> for the specified aspect path.
        /// </summary>
        /// <param name="app">App where the aspect belongs to.</param>
        /// <param name="aspectPath">JPath-like path of the aspect.</param>
        /// <returns>The aspect.</returns>
        /// <exception cref="KeyNotFoundException">When an aspect with the given path could not be found.</exception>
        IAspect GetAspect(App app, string aspectPath);

        /// <summary>
        /// Returns the <see cref="IAspect"/> for the specified aspect path.
        /// </summary>
        /// <param name="app">App where the aspect belongs to.</param>
        /// <param name="aspectPath">JPath-like path of the aspect.</param>
        /// <returns>The aspect.</returns>
        /// <exception cref="KeyNotFoundException">When an aspect with the given path could not be found.</exception>
        T GetAspect<T>(App app, string aspectPath) where T : IAspect;

        /// <summary>
        /// Returns the <see cref="IAspect"/> for the specified aspect path.
        /// </summary>
        /// <param name="app">App where the aspect belongs to.</param>
        /// <param name="aspectPath">JPath-like path of the aspect.</param>
        /// <returns>The aspect or null, when the aspect can not be found.</returns>
        IAspect TryGetAspect(App app, string aspectPath);
    }
}