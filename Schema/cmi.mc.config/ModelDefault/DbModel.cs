using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cmi.mc.config.ModelComponents;
using cmi.mc.config.ModelComponents.Dependencies;
using cmi.mc.config.ModelContract;

namespace cmi.mc.config.ModelDefault
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
