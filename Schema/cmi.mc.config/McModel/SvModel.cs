using System;
using cmi.mc.config.ModelContract;
using cmi.mc.config.ModelImpl;
using cmi.mc.config.ModelImpl.Dependencies;

namespace cmi.mc.config.McModel
{
    internal static class SvModel
    {
        public static AppSection GetModel(AppSection commonSection)
        {
            if (commonSection == null) throw new ArgumentNullException(nameof(commonSection));
            if (commonSection.App != App.Common) throw new ArgumentException("Is not a common app section", nameof(commonSection));

            var app = new AppSection(App.Sitzungsvorbereitung);
            var service = new ComplexAspect("service", ConfigControlAttribute.Extend);

            service.AddAspect(new SimpleAspect<bool>("supportsPersoenlicheDokumente", false));
            service.AddAspect(new SimpleAspect<bool>("supportsFreigabe", false));
            service.AddAspect(new SimpleAspect<bool>("supportsWortbegehren", true));
            service.AddAspect(new SimpleAspect<bool>("supportsLatestHistory", true, AxSupport.R17));

            var saveSettingDep = new SimpleAspectDependency(
                App.Common, 
                commonSection["service"]["supportsSaveSettings"] as ISimpleAspect, true);
            var persoenlicheDokumenteDep = new SimpleAspectDependency(
                App.Sitzungsvorbereitung, 
                service["supportsPersoenlicheDokumente"] as ISimpleAspect, true);
            var freigabeDep = new SimpleAspectDependency(
                App.Sitzungsvorbereitung,
                service["supportsFreigabe"] as ISimpleAspect, true);
            var pdfToolDep = new SimpleAspectDependency(
                App.Common,
                commonSection["ui"]["pdf"]["editor"] as ISimpleAspect, "pdftools");

            service.AddAspect(new SimpleAspect<bool>("supportsLatestHistoryMail", true, AxSupport.R18).AddDependency(saveSettingDep));
            service.AddAspect(new SimpleAspect<bool>("supportsPrintOnDemand", false, AxSupport.R18));
            service.AddAspect(new SimpleAspect<bool>("supportsGesamtPdf", false, AxSupport.R18).AddDependency(persoenlicheDokumenteDep));
            service.AddAspect(new SimpleAspect<bool>("allowDokumenteCopyAsPersoenlich", true, AxSupport.R18).AddDependency(persoenlicheDokumenteDep));
            service.AddAspect(new SimpleAspect<bool>("allowFreigabeToSachbearbeiter", true, AxSupport.R18).AddDependency(freigabeDep));
            service.AddAspect(new SimpleAspect<bool>("allowDokumenteAnnotations", false, AxSupport.R18).AddDependency(pdfToolDep));

            app.AddAspect(service);
            app.AddDependency(new AppDependency(App.Common));
            return app;
        }
    }
}
