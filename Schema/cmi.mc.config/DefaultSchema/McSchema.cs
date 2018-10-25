using System;
using cmi.mc.config.ModelContract;
using cmi.mc.config.ModelImpl;
using cmi.mc.config.ModelImpl.Decorators;

namespace cmi.mc.config.DefaultSchema
{
    internal static class McSchema
    {
        public static AppAspect GetModel(AppAspect commonApp, Uri defaultServiceUrl)
        {
            if (commonApp == null) throw new ArgumentNullException(nameof(commonApp));
            if (commonApp.App != App.Common) throw new ArgumentException("Is not a common app aspect", nameof(commonApp));

            var app = new AppAspect(App.Mobileclients);

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
            app.AddAspect(new ComplexAspect("boot", ConfigControlAttribute.Internal).AddAspect(new SpecialBootSetting(defaultServiceUrl)));
            app.AddAspect(new DefaultValueDecorator(
                new SimpleAspect<string>("info", "Mobile Clients"), $"Mobile Clients {DefaultValueDecorator.TenantNamePlaceholder}")
            );

            return app;
        }
    }
}
