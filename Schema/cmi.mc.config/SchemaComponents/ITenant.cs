namespace cmi.mc.config.SchemaComponents
{
    public interface ITenant
    {
        string Name { get; }
        bool IsEnabled(App app);
        void Enable(App app, bool ensureDependencies = false);
        object GetConfigurationProperty(App app, string aspectPath);
        T GetConfigurationProperty<T>(App app, string aspectPath);
        void SetConfigurationProperty(App app, string aspectPath, object value, bool ensureDependencies = false);
    }
}