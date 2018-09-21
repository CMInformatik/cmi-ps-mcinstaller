using System;

namespace cmi.mc.config.SchemaComponents
{
    public interface ITenant
    {
        string Name { get; }
        Uri ServiceBaseUrl { get; }
        bool IsEnabled(App app);
        void Enable(App app, bool ensureDependencies = false);
        void Disable(App app);
        object GetConfigurationProperty(App app, string aspectPath);
        T GetConfigurationProperty<T>(App app, string aspectPath);
        void SetConfigurationProperty(App app, string aspectPath, object value, bool ensureDependencies = false);
        bool HasConfigurationProperty(App app, string aspectPath);

        void RemoveConfigurationProperty(App app, string aspectPath);
    }
}