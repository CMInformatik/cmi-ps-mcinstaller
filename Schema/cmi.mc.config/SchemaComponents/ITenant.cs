using System;

namespace cmi.mc.config.SchemaComponents
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
        void Set(App app, string aspectPath, object value, bool ensureDependencies = false, Platform platform = Platform.Unspecified);

        void Set(App app, string aspectPath, bool ensureDependencies = false, Platform platform = Platform.Unspecified);
    }
}