using System;
using System.Diagnostics;
using cmi.mc.config.ModelContract;
using cmi.mc.config.ModelImpl;
using cmi.mc.config.ModelImpl.Dependencies;

namespace cmi.mc.config.McModel
{
    internal static class ZdModel
    {
        public static AppSection GetModel(AppSection commonSection)
        {
            if(commonSection == null) throw new ArgumentNullException(nameof(commonSection));
            if(commonSection.App != App.Common) throw new ArgumentException("Is not a common app section", nameof(commonSection));

            var allowDokumenteAddNewVersion = commonSection["service"]?["allowDokumenteAddNewVersion"] as ISimpleAspect;
            var allowDokumenteAddNew = commonSection["service"]?["allowDokumenteAddNew"] as ISimpleAspect;
            var supportsDokumenteDelete = commonSection["service"]?["supportsDokumenteDelete"] as ISimpleAspect;

            Debug.Assert(allowDokumenteAddNew != null);
            Debug.Assert(allowDokumenteAddNewVersion != null);
            Debug.Assert(supportsDokumenteDelete != null);

            var app = new AppSection(App.Zusammenarbeitdritte);
            app.AddDependency(new AppDependency(App.Common));
            app.AddDependency(new SimpleAspectDependency(App.Common, allowDokumenteAddNewVersion, true));
            app.AddDependency(new SimpleAspectDependency(App.Common, allowDokumenteAddNew, true));
            app.AddDependency(new SimpleAspectDependency(App.Common, supportsDokumenteDelete, true));
            return app;
        }
    }
}
