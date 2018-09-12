using System;
using System.Collections.Generic;
using System.Linq;

namespace cmi.mc.config.SchemaComponents
{
    public class AppSection : ComplexAspect
    {
        public readonly App App;

        public AppSection(App app) : base(app.ToConfigurationName(), ConfigControlAttribute.NotSet)
        {
            App = app;
        }

        public override void AddAspect(Aspect aspect)
        {
            if (aspect == null) throw new ArgumentNullException(nameof(aspect));
            AspectsInternal.Add(aspect.Name, aspect);
        }

        // for now, hide the app section in the aspect path.
        // prep for future to support the same aspects in different app sections.
        public override string GetAspectPath() => null;

        public override string ToString() => App.ToString();
    }
}
