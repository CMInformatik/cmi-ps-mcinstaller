using System;
using cmi.mc.config.ModelContract;

namespace cmi.mc.config.ModelImpl
{
    internal class AppSection : ComplexAspect
    {
        public App App { get; }

        public AppSection(App app) : base(app.ToConfigurationName(), ConfigControlAttribute.NotSet)
        {
            App = app;
        }

        public override IComplexAspect AddAspect(IAspect aspect)
        {
            if (aspect == null) throw new ArgumentNullException(nameof(aspect));
            AspectsInternal.Add(aspect.Name, aspect);
            return this;
        }

        // for now, hide the app section in the aspect path.
        // prep for future to support the same aspects in different app sections.
        public override string GetAspectPath() => null;

        public override string ToString() => App.ToString();
    }
}
