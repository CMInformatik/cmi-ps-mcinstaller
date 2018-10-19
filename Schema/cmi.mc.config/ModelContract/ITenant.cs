using System;

namespace cmi.mc.config.ModelContract
{
    public interface ITenant
    {
        string Name { get; }
        Uri ServiceBaseUrl { get; }
        bool Has(App app);
        bool Has(App app, string aspectPath, Platform platform = Platform.Unspecified);
        void Add(App app, bool ensureDependencies = false);
        void Remove(App app);
        void Remove(App app, string aspectPath);
        void Remove(App app, string aspectPath, Platform platform);
        object Get(App app, string aspectPath, Platform platform = Platform.Unspecified);
        T Get<T>(App app, string aspectPath, Platform platform = Platform.Unspecified);

        /// <summary>
        /// Sets a configuration property to the specified value.
        /// </summary>
        /// <param name="app">The app of the property.</param>
        /// <param name="aspectPath">Path of the property.</param>
        /// <param name="value">Value of the property.</param>
        /// <param name="ensureDependencies">Set dependencies to the required values.</param>
        /// <param name="platform">Set the property platform specific.</param>
        void Set(App app, string aspectPath, object value, bool ensureDependencies = false, Platform platform = Platform.Unspecified);

        void Set(App app, string aspectPath, bool ensureDependencies = false, Platform platform = Platform.Unspecified);

        void Validate(AxSupport axVersion);
    }
}