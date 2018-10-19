using cmi.mc.config.ModelContract;
using cmi.mc.config.ModelImpl;
using cmi.mc.config.ModelImpl.Dependencies;

namespace cmi.mc.config.McModel
{
    internal static class DbModel
    {
        public static AppSection GetModel(AppSection commonSection)
        {
            var app = new AppSection(App.Dossierbrowser);
            var service = new ComplexAspect("service", ConfigControlAttribute.Extend);
            service.AddAspect(new SimpleAspect<bool>("allowDokumenteCheckIn", false));
            service.AddAspect(new SimpleAspect<bool>("allowDokumenteDetails", false));
            service.AddAspect(new SimpleAspect<bool>("allowSearchForKontakte", false));
            service.AddAspect(new SimpleAspect<bool>("supportsDokumenteVersions", false));
            service.AddAspect(new SimpleAspect<bool>("supportsDetailsSearch", false));
            app.AddAspect(service);
            app.AddDependency(new AppDependency(App.Common));
            return app;
        }
    }
}
