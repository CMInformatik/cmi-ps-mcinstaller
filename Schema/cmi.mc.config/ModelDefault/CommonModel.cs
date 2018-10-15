using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cmi.mc.config.ModelComponents;
using cmi.mc.config.ModelComponents.Decorators;
using cmi.mc.config.ModelComponents.Dependencies;
using cmi.mc.config.ModelContract;
using FluentValidation;

namespace cmi.mc.config.ModelDefault
{
    internal static class CommonModel
    {
        private class Validator<T> : AbstractValidator<T>{}

        public static AppSection GetModel(Uri defaultServiceUrl)
        {
            var app = new AppSection(App.Common);
            var model = new List<IAspect>();

            var account = new ComplexAspect("account", ConfigControlAttribute.Private);
            account.AddAspect(new SimpleAspect<bool>("changePassword", false));
            account.AddAspect(new SimpleAspect<bool>("resetPassword", true));

            var service = new ComplexAspect("service", ConfigControlAttribute.Private);
            service.AddAspect(new SimpleAspect<bool>("allowDokumenteOpenExternal", true));
            service.AddAspect(new SimpleAspect<bool>("allowDokumenteAddNewVersion", false));
            service.AddAspect(new SimpleAspect<bool>("allowDokumenteAddNew", false));
            service.AddAspect(new SimpleAspect<bool>("supportsDokumenteDelete", false));
            service.AddAspect(new SimpleAspect<bool>("supportsPrivate", false, AxSupport.R17));
            service.AddAspect(new SimpleAspect<bool>("supportsSaveSettings", false, AxSupport.R18));

            model.Add(account);
            model.Add(service);
            model.Add(GetApi(defaultServiceUrl));
            model.Add(GetAppDirModel(defaultServiceUrl));
            model.Add(GetSecurity());
            model.Add(GetUi());
            model.Add(GetLanguages());
            app.AddAspect(model);
            return app;
        }

        private static IAspect GetApi(Uri defaultServiceUrl)
        {
            var api = new ComplexAspect("api");
            api.AddAspect(
                new TenantSpecificUriDecorator(
                    new SimpleAspect<Uri>("server", new Uri(defaultServiceUrl, "mobileclients")) { IsRequired = true }
                ));
            api.AddAspect(
                new DefaultValueDecorator(
                    new SimpleAspect<string>("public", "/proxy/pub") { IsRequired = true },
                    $"/proxy/{DefaultValueDecorator.TenantNamePlaceholder}pub",
                    true
                ));
            api.AddAspect(
                new DefaultValueDecorator(
                    new SimpleAspect<string>("private", "/proxy/pri") { IsRequired = true },
                    $"/proxy/{DefaultValueDecorator.TenantNamePlaceholder}pri",
                    true
                ));
            return api;
        }

        private static IAspect GetLanguages()
        {
            string[] langKeys = {"de", "fr"};

            var langValidator = new Validator<string[]>();
            langValidator.RuleFor(a => a)
                .Must(langs => langs.All(el => langKeys.Contains(el)))
                .WithMessage($"Only this values are allowed: {string.Join(",", langKeys)}");
            var defaultValidator = new Validator<string>();
            defaultValidator.RuleFor(l => l)
                .Must(d => langKeys.Contains(d))
                .WithMessage($"Only this values are allowed: {string.Join(",", langKeys)}");

            var languages = new ComplexAspect("languages");
            languages.AddAspect(new SimpleAspect<string[]>("supports", new[] { langKeys.First()}, AxSupport.R16_1, langValidator));
            languages.AddAspect(new SimpleAspect<string>("default", langKeys.First(), AxSupport.R16_1, defaultValidator));

            return languages;
        }

        private static IAspect GetUi()
        {
            var viewerVal = new Validator<string>();
            viewerVal.RuleFor(s => s).Matches("^$|^pdfjs$|^browser$").WithMessage("Only empty string, 'pdfjs' or 'browser' is allowed");
            var editorVal = new Validator<string>();
            editorVal.RuleFor(s => s).Matches("^$|^pdftools$").WithMessage("Only empty string or 'pdftools' is allowed");

            var ui = new ComplexAspect("ui", ConfigControlAttribute.Private);
            var pdf = new ComplexAspect("pdf");
            pdf.AddAspect(new SimpleAspect<bool>("inTabs", false) {IsPlatformSpecific = true});
            pdf.AddAspect(new SimpleAspect<string>("viewer", String.Empty, AxSupport.R16_1, viewerVal) { IsPlatformSpecific = true });
            pdf.AddAspect(new SimpleAspect<string>("editor", String.Empty, AxSupport.R18, editorVal) { IsPlatformSpecific = true });

            ui.AddAspect(pdf);
            return ui;
        }

        private static IAspect GetSecurity()
        {
            var postiveIntVal = new Validator<int>();
            postiveIntVal.RuleFor(i => i).InclusiveBetween(0, Int32.MaxValue);

            var security = new ComplexAspect("security");

            var pinCode = new ComplexAspect("PinCode", ConfigControlAttribute.Private);
            // Abhaenigkeiten zwischen required und optional?
            pinCode.AddAspect(new SimpleAspect<bool>("required", false));
            pinCode.AddAspect(new SimpleAspect<bool>("optional", false));
            pinCode.AddAspect(new SimpleAspect<int>("lockTimeout", 15, AxSupport.R16_1, postiveIntVal));

            var session = new ComplexAspect("session");
            session.AddAspect(new SimpleAspect<bool>("allowMulti", false));
            session.AddAspect(new SimpleAspect<int>("timeout", 30, AxSupport.R16_1, postiveIntVal));

            var offlineData = new ComplexAspect("offlineData", ConfigControlAttribute.Private);
            offlineData.AddAspect(new SimpleAspect<bool>("allow", false));

            var allowRememberMe = new SimpleAspect<bool>("allowRememberMe", false);
            allowRememberMe.SetDefaultValue(Platform.App, true);

            security.AddAspect(session);
            security.AddAspect(pinCode);
            security.AddAspect(offlineData);
            security.AddAspect(allowRememberMe);
            security.AddAspect(new SimpleAspect<bool>("defaultRememberMe", false));

            foreach(var secAspect in security.Traverse().OfType<ISimpleAspect>())
            {
                secAspect.IsPlatformSpecific = true;
            }
            return security;
        }

        private static IAspect GetAppDirModel(Uri defaultServiceUrl)
        {
            // dossierbrowser
            var dbDir = new ComplexAspect(App.Dossierbrowser.ToConfigurationName());
            dbDir.AddDependency(new AppDependency(App.Dossierbrowser));
            dbDir.AddAspect(
                new TenantSpecificUriDecorator(
                    new SimpleAspect<Uri>("web", new Uri(defaultServiceUrl, $"{App.Dossierbrowser.ToConfigurationName()}/tenantname"))
                ));
            dbDir.AddAspect(
                new SimpleAspect<string>("app", "cmidossierbrowser://"),
                new SimpleAspect<string>("dossierDetail", "/Abstr/{GUID}"));

            // sitzungsvorbereitung
            var svDir = new ComplexAspect(App.Sitzungsvorbereitung.ToConfigurationName());
            svDir.AddDependency(new AppDependency(App.Sitzungsvorbereitung));
            svDir.AddAspect(
                new TenantSpecificUriDecorator(
                    new SimpleAspect<Uri>("web", new Uri(defaultServiceUrl, $"{App.Sitzungsvorbereitung.ToConfigurationName()}/tenantname"))
                ));
            svDir.AddAspect(
                new SimpleAspect<string>("app", "cmisitzungsvorbereitung://"),
                new SimpleAspect<string>("sitzungDetail", "/{Gremium}/{Jahr}/{GUID}"),
                new SimpleAspect<string>("traktandumDetail", "/{Gremium}/{Jahr}/{GUID}/T/{TraktandumGUID}"));

            // zusammenarbeit dritte
            var zdDir = new ComplexAspect(App.Zusammenarbeitdritte.ToConfigurationName());
            zdDir.AddDependency(new AppDependency(App.Zusammenarbeitdritte));
            zdDir.AddAspect(
                new TenantSpecificUriDecorator(
                    new SimpleAspect<Uri>("web", new Uri(defaultServiceUrl, $"{App.Zusammenarbeitdritte.ToConfigurationName()}/tenantname"))
                ));
            zdDir.AddAspect(
                new SimpleAspect<string>("app", "cmisitzungsvorbereitung://"),
                new SimpleAspect<string>("aktivitaetDetail", "/Aktivitaet/{GUID}")
            );

            return new ComplexAspect("appDirectory").AddAspect(dbDir, svDir, zdDir);
        }
    }
}
