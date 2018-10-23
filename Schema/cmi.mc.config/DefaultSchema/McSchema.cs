using System;
using cmi.mc.config.ModelContract;
using cmi.mc.config.ModelImpl;
using cmi.mc.config.ModelImpl.Decorators;

namespace cmi.mc.config.DefaultSchema
{
    internal static class McSchema
    {
        public static AppSection GetModel(AppSection commonSection, Uri defaultServiceUrl)
        {
            if (commonSection == null) throw new ArgumentNullException(nameof(commonSection));
            if (commonSection.App != App.Common) throw new ArgumentException("Is not a common app section", nameof(commonSection));

            var app = new AppSection(App.Mobileclients);

            var api = new ComplexAspect("api");
            api.AddAspect(
                new TenantSpecificUriDecorator(
                    new SimpleAspect<Uri>("server", new Uri(defaultServiceUrl, App.Mobileclients.ToConfigurationName())) { IsRequired = true }
                ));
            api.AddAspect(
                new DefaultValueDecorator(
                    new SimpleAspect<string>("public", "/tenantname") { IsRequired = true },
                    $"/{DefaultValueDecorator.TenantNamePlaceholder}",
                    true
                ));
            api.AddAspect(
                new DefaultValueDecorator(
                    new SimpleAspect<string>("private", "/tenantname") { IsRequired = true },
                    $"/{DefaultValueDecorator.TenantNamePlaceholder}",
                    true
                ));

            app.AddAspect(api);
            app.AddAspect(new DefaultValueDecorator(
                new SimpleAspect<string>("info", "Mobile Clients"), $"Mobile Clients {DefaultValueDecorator.TenantNamePlaceholder}")
            );

            return app;
        }
    }
}
