using System;
using cmi.mc.config.ModelContract;
using cmi.mc.config.ModelContract.Components;
using cmi.mc.config.ModelImpl;
using cmi.mc.config.ModelImpl.Decorators;
using cmi.mc.config.ModelImpl.Dependencies;

namespace cmi.mc.config.DefaultSchema
{
    internal static class ZdSchema
    {
        public static AppAspect GetModel(AppAspect commonApp, Uri defaultServiceUrl)
        {
            if(commonApp == null) throw new ArgumentNullException(nameof(commonApp));
            if(commonApp.App != App.Common) throw new ArgumentException("Is not a common app section", nameof(commonApp));

            var allowDokumenteAddNewVersion = commonApp["service"]?["allowDokumenteAddNewVersion"] as ISimpleAspect;
            var allowDokumenteAddNew = commonApp["service"]?["allowDokumenteAddNew"] as ISimpleAspect;
            var supportsDokumenteDelete = commonApp["service"]?["supportsDokumenteDelete"] as ISimpleAspect;

            var app = new AppAspect(App.Zusammenarbeitdritte);
            app.AddDependency(new AppDependency(App.Common));
            app.AddDependency(new SimpleAspectDependency(App.Common, allowDokumenteAddNewVersion, true));
            app.AddDependency(new SimpleAspectDependency(App.Common, allowDokumenteAddNew, true));
            app.AddDependency(new SimpleAspectDependency(App.Common, supportsDokumenteDelete, true));

            var appDir = commonApp["appDirectory"][App.Zusammenarbeitdritte.ToConfigurationName()] as ISimpleAspect;
            app.AddDependency(new SimpleAspectDependency(App.Common, appDir));

            var boot = new ComplexAspect("boot").AddAspect(
                new TenantSpecificUriDecorator(
                    new SimpleAspect<Uri>("settings",
                        new Uri(defaultServiceUrl,
                            $"{App.Zusammenarbeitdritte.ToConfigurationName()}/proxy/tenantname{McSymbols.GetAppShortcut(App.Zusammenarbeitdritte)}")))
            );
            app.AddAspect(boot);

            return app;
        }
    }
}
