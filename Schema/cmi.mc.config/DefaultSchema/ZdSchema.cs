using System;
using cmi.mc.config.ModelContract;
using cmi.mc.config.ModelImpl;
using cmi.mc.config.ModelImpl.Decorators;
using cmi.mc.config.ModelImpl.Dependencies;

namespace cmi.mc.config.DefaultSchema
{
    internal static class ZdSchema
    {
        public static AppSection GetModel(AppSection commonSection, Uri defaultServiceUrl)
        {
            if(commonSection == null) throw new ArgumentNullException(nameof(commonSection));
            if(commonSection.App != App.Common) throw new ArgumentException("Is not a common app section", nameof(commonSection));

            var allowDokumenteAddNewVersion = commonSection["service"]?["allowDokumenteAddNewVersion"] as ISimpleAspect;
            var allowDokumenteAddNew = commonSection["service"]?["allowDokumenteAddNew"] as ISimpleAspect;
            var supportsDokumenteDelete = commonSection["service"]?["supportsDokumenteDelete"] as ISimpleAspect;

            var app = new AppSection(App.Zusammenarbeitdritte);
            app.AddDependency(new AppDependency(App.Common));
            app.AddDependency(new SimpleAspectDependency(App.Common, allowDokumenteAddNewVersion, true));
            app.AddDependency(new SimpleAspectDependency(App.Common, allowDokumenteAddNew, true));
            app.AddDependency(new SimpleAspectDependency(App.Common, supportsDokumenteDelete, true));

            var appDir = commonSection["appDirectory"][App.Zusammenarbeitdritte.ToConfigurationName()] as ISimpleAspect;
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
